namespace Dashboard.Api.Domain;

public sealed class Card
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public CardStatus Status { get; set; }
    public CardScope Scope { get; set; }
    public int OwnerId { get; set; }
    public int? DepartmentId { get; set; }
    public DateOnly? DueDate { get; set; }
    public int SequenceOrder { get; set; }
    public string? DevOpsUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User? Owner { get; set; }
    public Department? Department { get; set; }
    public ICollection<CardTask> Tasks { get; set; } = [];
}
