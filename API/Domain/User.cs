namespace Dashboard.Api.Domain;

public sealed class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public int DepartmentId { get; set; }
    public required string Role { get; set; }

    public Department? Department { get; set; }
    public ICollection<Card> OwnedCards { get; set; } = [];
    public ICollection<CardTask> AssignedTasks { get; set; } = [];
}
