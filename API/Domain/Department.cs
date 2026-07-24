namespace Dashboard.Api.Domain;

/// <summary>
/// 代表部門實體 (Department Entity)。
/// </summary>
public sealed class Department
{
    /// <summary>
    /// 部門唯一識別碼
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 部門名稱
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 隸屬於該部門的使用者集合
    /// </summary>
    public ICollection<User> Users { get; set; } = [];

    /// <summary>
    /// 隸屬於該部門的組織卡片集合
    /// </summary>
    public ICollection<Card> Cards { get; set; } = [];
}
