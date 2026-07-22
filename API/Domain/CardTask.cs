namespace Dashboard.Api.Domain;

public sealed class CardTask
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public required string Title { get; set; }
    public bool IsCompleted { get; set; }
    public int? AssigneeId { get; set; }
    public int SequenceOrder { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? DevOpsUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Card? Card { get; set; }
    public User? Assignee { get; set; }
}
