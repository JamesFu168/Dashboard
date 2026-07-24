using Dashboard.Api.Data;
using Dashboard.Api.Domain;

namespace Dashboard.Api.Services;

/// <summary>
/// 提供看板卡片與細項任務的領域授權驗證服務 (Domain Authorization Service)。
/// 包含 Owner 驗證、Task 編輯權限判斷以及卡片可見性 LINQ 查詢過濾。
/// </summary>
public sealed class CardAuthorizationService
{
    /// <summary>
    /// 檢查當前使用者是否具備編輯卡片內容的權限 (僅限 Card Owner)。
    /// </summary>
    /// <param name="currentUserId">當前登入者 ID</param>
    /// <param name="card">目標卡片</param>
    /// <returns>若具有權限回傳 true，否則回傳 false</returns>
    public bool CanEditCard(int currentUserId, Card card) => card.OwnerId == currentUserId;

    /// <summary>
    /// 檢查當前使用者是否具備移動卡片狀態/排序的權限 (僅限 Card Owner)。
    /// </summary>
    /// <param name="currentUserId">當前登入者 ID</param>
    /// <param name="card">目標卡片</param>
    /// <returns>若具有權限回傳 true，否則回傳 false</returns>
    public bool CanMoveCard(int currentUserId, Card card) => card.OwnerId == currentUserId;

    /// <summary>
    /// 檢查當前使用者是否具備管理 (新增/編輯/刪除/指派) 該卡片內部細項任務的權限 (僅限 Card Owner)。
    /// </summary>
    /// <param name="currentUserId">當前登入者 ID</param>
    /// <param name="card">目標卡片</param>
    /// <returns>若具有權限回傳 true，否則回傳 false</returns>
    public bool CanManageTasks(int currentUserId, Card card) => card.OwnerId == currentUserId;

    /// <summary>
    /// 檢查當前使用者是否具備勾選/切換特定 Task 完成狀態的權限 (Card Owner 或 Task Assignee)。
    /// </summary>
    /// <param name="currentUserId">當前登入者 ID</param>
    /// <param name="task">目標任務細項</param>
    /// <param name="card">所屬卡片</param>
    /// <returns>若具有權限回傳 true，否則回傳 false</returns>
    public bool CanToggleTask(int currentUserId, CardTask task, Card card)
    {
        return card.OwnerId == currentUserId || task.AssigneeId == currentUserId;
    }

    /// <summary>
    /// 根據當前使用者視角與權限，產生取得可存取卡片列表的 IQueryable 查詢語法。
    /// </summary>
    /// <param name="db">資料庫 Context</param>
    /// <param name="userId">當前登入者 ID</param>
    /// <param name="userDepartmentId">當前登入者部門 ID</param>
    /// <param name="viewMode">檢視模式 (organization: 組織視角, personal: 個人視角)</param>
    /// <returns>包含過濾條件的卡片 LINQ 查詢</returns>
    public IQueryable<Card> GetAccessibleCardsQuery(
        AppDbContext db,
        int userId,
        int userDepartmentId,
        string? viewMode)
    {
        if (string.Equals(viewMode, "organization", StringComparison.OrdinalIgnoreCase))
        {
            return db.Cards.Where(card =>
                card.Scope == CardScope.Organization
                && (card.DepartmentId == userDepartmentId || card.Tasks.Any(task => task.AssigneeId == userId)));
        }

        return db.Cards.Where(card => card.OwnerId == userId && card.Scope == CardScope.Personal);
    }
}
