namespace Dashboard.Api.Domain;

/// <summary>
/// 代表卡片內部的細項任務實體 (CardTask Entity)。
/// </summary>
public sealed class CardTask
{
    /// <summary>
    /// 細項任務唯一識別碼 (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 所屬卡片識別碼
    /// </summary>
    public Guid CardId { get; set; }

    /// <summary>
    /// 任務標題
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 是否已勾選完成
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// 被指派人的使用者識別碼 (若卡片範疇為 Personal 則限制僅能為 Owner 本人)
    /// </summary>
    public int? AssigneeId { get; set; }

    /// <summary>
    /// 於卡片內部的顯示順序
    /// </summary>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// 任務到期日期
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// 關聯的 Azure DevOps 工作項目網址
    /// </summary>
    public string? DevOpsUrl { get; set; }

    /// <summary>
    /// 建立時間 (台灣時間 UTC+8)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後更新時間 (台灣時間 UTC+8，用於樂觀鎖比對)
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 所屬卡片導覽屬性
    /// </summary>
    public Card? Card { get; set; }

    /// <summary>
    /// 被指派人導覽屬性
    /// </summary>
    public User? Assignee { get; set; }
}
