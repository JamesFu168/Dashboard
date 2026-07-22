namespace Dashboard.Api.Domain;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }

    public User? User { get; set; }
}
