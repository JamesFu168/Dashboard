namespace Dashboard.Api.Domain;

/// <summary>
/// 代表看板中的卡片實體 (Card Entity)。
/// </summary>
public sealed class Card
{
    /// <summary>
    /// 卡片唯一識別碼 (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 卡片標題
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 卡片詳細描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 卡片目前狀態 (Plan, ToDo, Doing, Done)
    /// </summary>
    public CardStatus Status { get; set; }

    /// <summary>
    /// 卡片範疇 (Personal, Organization)
    /// </summary>
    public CardScope Scope { get; set; }

    /// <summary>
    /// 卡片擁有者 (Owner) 的使用者識別碼
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 所屬部門識別碼 (僅當 Scope 為 Organization 時有值)
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// 到期日期
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// 於目前狀態欄位中的顯示順序
    /// </summary>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// 關聯的 Azure DevOps 工作項目網址
    /// </summary>
    public string? DevOpsUrl { get; set; }

    /// <summary>
    /// 是否已被軟刪除 (Soft Delete 標記)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 軟刪除時間 (台灣時間 UTC+8)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 建立時間 (台灣時間 UTC+8)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後更新時間 (台灣時間 UTC+8，用於樂觀鎖比對)
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 卡片擁有者導覽屬性
    /// </summary>
    public User? Owner { get; set; }

    /// <summary>
    /// 所屬部門導覽屬性
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// 卡片包含的細項任務列表 (Tasks)
    /// </summary>
    public ICollection<CardTask> Tasks { get; set; } = [];
}
