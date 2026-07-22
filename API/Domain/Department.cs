namespace Dashboard.Api.Domain;

public sealed class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Card> Cards { get; set; } = [];
}
