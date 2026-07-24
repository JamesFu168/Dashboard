using Dashboard.Api.Contracts;
using Dashboard.Api.Data;
using Dashboard.Api.Domain;
using Dashboard.Api.Hubs;
using Dashboard.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Api.Controllers;

/// <summary>
/// 處理看板卡片 CRUD、狀態拖移、軟刪除與回收桶還原、每月自動結案管理之控制器 (Cards Controller)。
/// </summary>
[ApiController]
[Route("api/v1/cards")]
public sealed class CardsController(
    AppDbContext db,
    ICurrentUserService currentUser,
    CardAuthorizationService authorization,
    IHubContext<KanbanHub> hub) : ControllerBase
{
    /// <summary>
    /// 取得當前使用者權限可存取的卡片列表 (預設自動忽略已軟刪除 IsDeleted 與已結案 AutoClosed 之卡片)。
    /// </summary>
    /// <param name="viewMode">檢視模式 (organization: 組織視角, personal: 個人視角)</param>
    /// <returns>卡片 DTO 集合</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CardDto>>> GetCards([FromQuery] string? viewMode)
    {
        var cards = await authorization
            .GetAccessibleCardsQuery(db, currentUser.UserId, currentUser.DepartmentId, viewMode)
            .Include(card => card.Owner)
            .Include(card => card.Department)
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .OrderBy(card => card.Status)
            .ThenBy(card => card.SequenceOrder)
            .ToListAsync();

        return Ok(cards.Select(card => card.ToDto()).ToArray());
    }

    /// <summary>
    /// 取得單一卡片的詳細資訊與細項 Tasks。
    /// </summary>
    /// <param name="id">卡片 GUID</param>
    /// <returns>卡片詳細資料 DTO</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardDto>> GetCard(Guid id)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!CanView(card))
        {
            return Forbid();
        }

        return Ok(card.ToDto());
    }

    /// <summary>
    /// 建立新的看板卡片。
    /// 預設將 Owner 設定為當前登入者，並透過 SignalR 廣播異動。
    /// </summary>
    /// <param name="request">建立卡片請求 DTO</param>
    /// <returns>建立成功之卡片 DTO</returns>
    [HttpPost]
    public async Task<ActionResult<CardDto>> CreateCard(CreateCardRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var now = DateTimeProvider.TaiwanNow;
        var card = new Card
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = CardStatus.Plan,
            Scope = request.Scope,
            OwnerId = currentUser.UserId,
            DepartmentId = request.Scope == CardScope.Organization ? request.DepartmentId ?? currentUser.DepartmentId : null,
            DueDate = request.DueDate,
            SequenceOrder = request.SequenceOrder,
            DevOpsUrl = request.DevOpsUrl,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Cards.Add(card);
        await db.SaveChangesAsync();

        card = await GetCardWithDetails(card.Id) ?? card;
        await PublishAsync("CardCreated", card);

        return CreatedAtAction(nameof(GetCard), new { id = card.Id }, card.ToDto());
    }

    /// <summary>
    /// 編輯卡片標題、內容、範疇、到期日與 DevOps 連結 (需要 Owner 權限與樂觀鎖驗證)。
    /// </summary>
    /// <param name="id">卡片 GUID</param>
    /// <param name="request">更新卡片請求 DTO</param>
    /// <returns>更新後卡片 DTO</returns>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CardDto>> UpdateCard(Guid id, UpdateCardRequest request)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanEditCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (IsStale(request.UpdatedAt, card.UpdatedAt))
        {
            return Conflict("Card has changed since it was loaded.");
        }

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title cannot be empty.");
            }

            card.Title = request.Title.Trim();
        }

        card.Description = request.Description ?? card.Description;
        card.Scope = request.Scope ?? card.Scope;
        card.DepartmentId = card.Scope == CardScope.Organization
            ? request.DepartmentId ?? card.DepartmentId ?? currentUser.DepartmentId
            : null;
        card.DueDate = request.DueDate ?? card.DueDate;
        card.DevOpsUrl = request.DevOpsUrl ?? card.DevOpsUrl;
        card.UpdatedAt = DateTimeProvider.TaiwanNow;

        await db.SaveChangesAsync();
        await PublishAsync("CardUpdated", card);

        return Ok(card.ToDto());
    }

    /// <summary>
    /// 移動卡片至新欄位狀態或調整欄位內部排序 SequenceOrder (僅限 Owner 可操作)。
    /// </summary>
    /// <param name="id">卡片 GUID</param>
    /// <param name="request">移動卡片請求 DTO</param>
    /// <returns>更新後卡片 DTO</returns>
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<CardDto>> MoveCard(Guid id, MoveCardRequest request)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanMoveCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (IsStale(request.UpdatedAt, card.UpdatedAt))
        {
            return Conflict("Card has changed since it was loaded.");
        }

        card.Status = request.Status;
        card.SequenceOrder = request.SequenceOrder;
        card.UpdatedAt = DateTimeProvider.TaiwanNow;

        await db.SaveChangesAsync();
        await PublishAsync("CardMoved", card);

        return Ok(card.ToDto());
    }

    /// <summary>
    /// 標記軟刪除卡片 (`IsDeleted = true`, `DeletedAt = TaiwanNow`)，移至回收桶 (僅限 Owner)。
    /// 軟刪除後預設不顯示於看板上。
    /// </summary>
    /// <param name="id">卡片 GUID</param>
    /// <returns>無內容成功回應 (204 No Content)</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCard(Guid id)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanEditCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        card.IsDeleted = true;
        card.DeletedAt = DateTimeProvider.TaiwanNow;
        await db.SaveChangesAsync();
        await PublishAsync("CardDeleted", card);

        return NoContent();
    }

    /// <summary>
    /// 批次將前一個月已完成 (Status = Done，且最後更新時間小於本月1號) 之卡片自動更新為結案 (Status = AutoClosed)。
    /// 結案後之卡片預設將不會顯示於看板上。
    /// </summary>
    /// <returns>已自動結案之卡片列表與數量</returns>
    [HttpPost("auto-close-previous-month")]
    public async Task<ActionResult<IReadOnlyCollection<CardDto>>> AutoClosePreviousMonthCards()
    {
        var now = DateTimeProvider.TaiwanNow;
        var firstDayOfCurrentMonth = new DateTime(now.Year, now.Month, 1);

        // 搜尋上個月以前已 Done 且未刪除之卡片
        var doneCardsToClose = await db.Cards
            .Where(card => card.Status == CardStatus.Done && card.UpdatedAt < firstDayOfCurrentMonth)
            .Include(card => card.Owner)
            .Include(card => card.Department)
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .ToListAsync();

        foreach (var card in doneCardsToClose)
        {
            card.Status = CardStatus.AutoClosed;
            card.UpdatedAt = now;
        }

        await db.SaveChangesAsync();

        // 透過 SignalR 廣播每一張已結案卡片從看板移除
        foreach (var card in doneCardsToClose)
        {
            await PublishAsync("CardDeleted", card);
        }

        return Ok(doneCardsToClose.Select(card => card.ToDto()).ToArray());
    }

    /// <summary>
    /// 取得歷史已結案 (Status = AutoClosed) 之卡片列表 (供封存或歷史紀錄查詢)。
    /// </summary>
    /// <returns>已結案卡片 DTO 集合</returns>
    [HttpGet("closed")]
    public async Task<ActionResult<IReadOnlyCollection<CardDto>>> GetClosedCards()
    {
        var cards = await db.Cards
            .Where(card => card.Status == CardStatus.AutoClosed && (card.OwnerId == currentUser.UserId || card.DepartmentId == currentUser.DepartmentId))
            .Include(card => card.Owner)
            .Include(card => card.Department)
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .OrderByDescending(card => card.UpdatedAt)
            .ToListAsync();

        return Ok(cards.Select(card => card.ToDto()).ToArray());
    }

    /// <summary>
    /// 取得當前登入者已被軟刪除的卡片列表 (回收桶專用)。
    /// </summary>
    /// <returns>軟刪除卡片 DTO 集合</returns>
    [HttpGet("trash")]
    public async Task<ActionResult<IReadOnlyCollection<CardDto>>> GetTrash()
    {
        var cards = await db.Cards
            .IgnoreQueryFilters()
            .Where(card => card.OwnerId == currentUser.UserId && card.IsDeleted)
            .Include(card => card.Owner)
            .Include(card => card.Department)
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .OrderByDescending(card => card.DeletedAt)
            .ToListAsync();

        return Ok(cards.Select(card => card.ToDto()).ToArray());
    }

    /// <summary>
    /// 從回收桶還原指定之軟刪除卡片 (`IsDeleted = false`, `DeletedAt = null`)。
    /// </summary>
    /// <param name="id">卡片 GUID</param>
    /// <returns>還原後之卡片 DTO</returns>
    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<CardDto>> RestoreCard(Guid id)
    {
        var card = await GetCardWithDetails(id, ignoreQueryFilters: true);
        if (card is null || !card.IsDeleted)
        {
            return NotFound();
        }

        if (!authorization.CanEditCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        card.IsDeleted = false;
        card.DeletedAt = null;
        await db.SaveChangesAsync();
        await PublishAsync("CardCreated", card);

        return Ok(card.ToDto());
    }

    /// <summary>
    /// 從回收桶永久刪除卡片及其關聯之細項 Tasks。
    /// </summary>
    /// <param name="id">卡片 GUID</param>
    /// <returns>無內容成功回應 (204 No Content)</returns>
    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> PermanentlyDeleteCard(Guid id)
    {
        var card = await GetCardWithDetails(id, ignoreQueryFilters: true);
        if (card is null || !card.IsDeleted)
        {
            return NotFound();
        }

        if (!authorization.CanEditCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        db.Cards.Remove(card);
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 載入包含擁有者、部門與 Task 被指派者細節的卡片資料。
    /// </summary>
    private async Task<Card?> GetCardWithDetails(Guid id, bool ignoreQueryFilters = false)
    {
        var query = ignoreQueryFilters ? db.Cards.IgnoreQueryFilters() : db.Cards;

        return await query
            .Include(card => card.Owner)
            .Include(card => card.Department)
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .FirstOrDefaultAsync(card => card.Id == id);
    }

    /// <summary>
    /// 檢查當前登入者是否具備檢視卡片的權限。
    /// </summary>
    private bool CanView(Card card)
    {
        return card.OwnerId == currentUser.UserId
            || (card.Scope == CardScope.Organization
                && (card.DepartmentId == currentUser.DepartmentId
                    || card.Tasks.Any(task => task.AssigneeId == currentUser.UserId)));
    }

    /// <summary>
    /// 檢查請求中的 UpdatedAt 時間戳記是否過期 (樂觀鎖比對)。
    /// </summary>
    private static bool IsStale(DateTime? requestUpdatedAt, DateTime entityUpdatedAt)
    {
        return requestUpdatedAt is not null && requestUpdatedAt.Value != entityUpdatedAt;
    }

    /// <summary>
    /// 透過 SignalR 對卡片 Owner 與部門群組廣播即時事件。
    /// </summary>
    private async Task PublishAsync(string eventName, Card card)
    {
        await hub.Clients.User(card.OwnerId.ToString()).SendAsync(eventName, card.ToDto());

        if (card.DepartmentId is int departmentId)
        {
            await hub.Clients.Group(KanbanHub.GetDepartmentGroup(departmentId)).SendAsync(eventName, card.ToDto());
        }
    }
}
