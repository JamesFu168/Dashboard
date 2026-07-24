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
/// 處理卡片內部細項任務 (CardTask) 之新增、編輯、勾選完成、人員指派與刪除控制 (Tasks Controller)。
/// </summary>
[ApiController]
public sealed class TasksController(
    AppDbContext db,
    ICurrentUserService currentUser,
    CardAuthorizationService authorization,
    IHubContext<KanbanHub> hub) : ControllerBase
{
    /// <summary>
    /// 於指定卡片內新增細項 Task (僅限 Card Owner)。
    /// 若卡片範疇為 Personal，則自動預設被指派人為 Card Owner。
    /// </summary>
    /// <param name="cardId">目標卡片 GUID</param>
    /// <param name="request">新增 Task 請求 DTO</param>
    /// <returns>建立成功之 Task DTO</returns>
    [HttpPost("api/v1/cards/{cardId:guid}/tasks")]
    public async Task<ActionResult<CardTaskDto>> CreateTask(Guid cardId, CreateTaskRequest request)
    {
        var card = await GetCardWithTasks(cardId);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var now = DateTimeProvider.TaiwanNow;
        var task = new CardTask
        {
            Id = Guid.NewGuid(),
            CardId = card.Id,
            Title = request.Title.Trim(),
            AssigneeId = card.Scope == CardScope.Organization
                ? request.AssigneeId ?? currentUser.UserId
                : card.OwnerId,
            SequenceOrder = request.SequenceOrder,
            DueDate = request.DueDate,
            DevOpsUrl = request.DevOpsUrl,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.CardTasks.Add(task);
        card.UpdatedAt = now;
        await db.SaveChangesAsync();

        await PublishAsync("TaskUpdated", card.Id);
        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task.ToDto());
    }

    /// <summary>
    /// 取得單一 Task 詳情。
    /// </summary>
    /// <param name="taskId">任務細項 GUID</param>
    /// <returns>Task 詳情 DTO</returns>
    [HttpGet("api/v1/tasks/{taskId:guid}")]
    public async Task<ActionResult<CardTaskDto>> GetTask(Guid taskId)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        if (!CanView(task.Card!))
        {
            return Forbid();
        }

        return Ok(task.ToDto());
    }

    /// <summary>
    /// 編輯 Task 標題、排序、到期日與 DevOps 網址 (僅限 Card Owner)。
    /// </summary>
    /// <param name="taskId">任務細項 GUID</param>
    /// <param name="request">更新 Task 請求 DTO</param>
    /// <returns>更新後 Task DTO</returns>
    [HttpPatch("api/v1/tasks/{taskId:guid}")]
    public async Task<ActionResult<CardTaskDto>> UpdateTask(Guid taskId, UpdateTaskRequest request)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (IsStale(request.UpdatedAt, task.UpdatedAt))
        {
            return Conflict("Task has changed since it was loaded.");
        }

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title cannot be empty.");
            }

            task.Title = request.Title.Trim();
        }

        task.SequenceOrder = request.SequenceOrder ?? task.SequenceOrder;
        task.DueDate = request.DueDate ?? task.DueDate;
        task.DevOpsUrl = request.DevOpsUrl ?? task.DevOpsUrl;
        task.UpdatedAt = DateTimeProvider.TaiwanNow;
        card.UpdatedAt = task.UpdatedAt;

        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return Ok(task.ToDto());
    }

    /// <summary>
    /// 切換/勾選 Task 完成狀態 IsCompleted (Card Owner 或被指派之 Task Assignee 均可操作)。
    /// </summary>
    /// <param name="taskId">任務細項 GUID</param>
    /// <returns>更新狀態後之 Task DTO</returns>
    [HttpPatch("api/v1/tasks/{taskId:guid}/toggle")]
    public async Task<ActionResult<CardTaskDto>> ToggleTask(Guid taskId)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanToggleTask(currentUser.UserId, task, card))
        {
            return Forbid();
        }

        task.IsCompleted = !task.IsCompleted;
        task.UpdatedAt = DateTimeProvider.TaiwanNow;
        card.UpdatedAt = task.UpdatedAt;

        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return Ok(task.ToDto());
    }

    /// <summary>
    /// 變更 Task 的被指派人員 (僅限 Card Owner)。
    /// 若卡片範疇為 Personal，則拒絕指派給他人並返回 400 Bad Request。
    /// </summary>
    /// <param name="taskId">任務細項 GUID</param>
    /// <param name="request">指派人員請求 DTO</param>
    /// <returns>更新後 Task DTO</returns>
    [HttpPut("api/v1/tasks/{taskId:guid}/assign")]
    public async Task<ActionResult<CardTaskDto>> AssignTask(Guid taskId, AssignTaskRequest request)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (card.Scope == CardScope.Personal)
        {
            return BadRequest("個人卡片的任務無法指派給他人。");
        }

        if (IsStale(request.UpdatedAt, task.UpdatedAt))
        {
            return Conflict("Task has changed since it was loaded.");
        }

        task.AssigneeId = request.AssigneeId;
        task.UpdatedAt = DateTimeProvider.TaiwanNow;
        card.UpdatedAt = task.UpdatedAt;

        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return Ok(task.ToDto());
    }

    /// <summary>
    /// 刪除指定之 Task (僅限 Card Owner)。
    /// </summary>
    /// <param name="taskId">任務細項 GUID</param>
    /// <returns>無內容成功回應 (204 No Content)</returns>
    [HttpDelete("api/v1/tasks/{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        db.CardTasks.Remove(task);
        card.UpdatedAt = DateTimeProvider.TaiwanNow;
        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return NoContent();
    }

    /// <summary>
    /// 載入卡片及其所有 Tasks 與被指派者資料。
    /// </summary>
    private async Task<Card?> GetCardWithTasks(Guid cardId)
    {
        return await db.Cards
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .FirstOrDefaultAsync(card => card.Id == cardId);
    }

    /// <summary>
    /// 載入 Task 及其父卡片資料。
    /// </summary>
    private async Task<CardTask?> GetTaskWithCard(Guid taskId)
    {
        return await db.CardTasks
            .IgnoreQueryFilters()
            .Include(task => task.Assignee)
            .Include(task => task.Card)
            .ThenInclude(card => card!.Tasks)
            .FirstOrDefaultAsync(task => task.Id == taskId);
    }

    /// <summary>
    /// 檢查當前登入者是否具備檢視卡片及其 Task 的權限。
    /// </summary>
    private bool CanView(Card card)
    {
        return card.OwnerId == currentUser.UserId
            || (card.Scope == CardScope.Organization
                && (card.DepartmentId == currentUser.DepartmentId
                    || card.Tasks.Any(task => task.AssigneeId == currentUser.UserId)));
    }

    /// <summary>
    /// 檢查樂觀鎖 UpdatedAt 是否過期。
    /// </summary>
    private static bool IsStale(DateTime? requestUpdatedAt, DateTime entityUpdatedAt)
    {
        return requestUpdatedAt is not null && requestUpdatedAt.Value != entityUpdatedAt;
    }

    /// <summary>
    /// 透過 SignalR 廣播 Task 狀態更新至 Owner 與部門群組。
    /// </summary>
    private async Task PublishAsync(string eventName, Guid cardId)
    {
        var card = await db.Cards
            .Include(item => item.Owner)
            .Include(item => item.Department)
            .Include(item => item.Tasks)
            .ThenInclude(task => task.Assignee)
            .FirstAsync(item => item.Id == cardId);

        await hub.Clients.User(card.OwnerId.ToString()).SendAsync(eventName, card.ToDto());

        if (card.DepartmentId is int departmentId)
        {
            await hub.Clients.Group(KanbanHub.GetDepartmentGroup(departmentId)).SendAsync(eventName, card.ToDto());
        }
    }
}
