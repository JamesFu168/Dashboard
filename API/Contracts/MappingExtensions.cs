using Dashboard.Api.Domain;

namespace Dashboard.Api.Contracts;

public static class MappingExtensions
{
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
            card.CreatedAt,
            card.UpdatedAt,
            card.Tasks
                .OrderBy(task => task.SequenceOrder)
                .Select(task => task.ToDto())
                .ToArray());
    }

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
