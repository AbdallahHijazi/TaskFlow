namespace TaskFlow.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    public User User { get; set; } = null!;
}
