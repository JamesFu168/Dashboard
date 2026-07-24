namespace Dashboard.Api.Domain;

/// <summary>
/// 代表系統使用者實體 (User Entity)。
/// </summary>
public sealed class User
{
    /// <summary>
    /// 使用者唯一識別碼 (整數主鍵)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者姓名
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 使用者電子郵件 (登入帳號)
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// BCrypt 雜湊處理後的密碼 (不儲存明碼)
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// 所屬部門識別碼
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// 使用者角色
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// 所屬部門導覽屬性
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// 該使用者擁有的卡片集合
    /// </summary>
    public ICollection<Card> OwnedCards { get; set; } = [];

    /// <summary>
    /// 指派給該使用者的細項任務集合
    /// </summary>
    public ICollection<CardTask> AssignedTasks { get; set; } = [];
}
