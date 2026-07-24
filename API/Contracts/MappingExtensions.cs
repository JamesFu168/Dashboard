using Dashboard.Api.Domain;

namespace Dashboard.Api.Contracts;

/// <summary>
/// 提供 Domain Entity 轉換為 API DTO 之擴充方法 (Mapping Extensions)。
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// 將 Card Domain Entity 轉譯為 CardDto。
    /// </summary>
    /// <param name="card">Card 實體</param>
    /// <returns>CardDto 傳輸物件</returns>
    public static CardDto ToDto(this Card card)
    {
        return new CardDto(
            card.Id,
            card.Title,
            card.Description,
            card.Status,
            card.Scope,
            card.OwnerId,
            card.Owner?.Name,
            card.DepartmentId,
            card.Department?.Name,
            card.DueDate,
            card.SequenceOrder,
            card.DevOpsUrl,
            card.DeletedAt,
            card.CreatedAt,
            card.UpdatedAt,
            card.Tasks
                .OrderBy(task => task.SequenceOrder)
                .Select(task => task.ToDto())
                .ToArray());
    }

    /// <summary>
    /// 將 CardTask Domain Entity 轉譯為 CardTaskDto。
    /// </summary>
    /// <param name="task">CardTask 實體</param>
    /// <returns>CardTaskDto 傳輸物件</returns>
    public static CardTaskDto ToDto(this CardTask task)
    {
        return new CardTaskDto(
            task.Id,
            task.CardId,
            task.Title,
            task.IsCompleted,
            task.AssigneeId,
            task.Assignee?.Name,
            task.SequenceOrder,
            task.DueDate,
            task.DevOpsUrl,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
