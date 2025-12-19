namespace TimeSheetPortal.Core.Entities;

public class PasswordRecoveryToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset Expiry { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
