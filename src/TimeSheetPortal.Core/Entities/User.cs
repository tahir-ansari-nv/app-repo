namespace TimeSheetPortal.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int FailedLoginAttempts { get; set; }
    public bool MFAEnabled { get; set; }
    public string? MFASecret { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<PasswordRecoveryToken> PasswordRecoveryTokens { get; set; } = new List<PasswordRecoveryToken>();
    public ICollection<MFACode> MFACodes { get; set; } = new List<MFACode>();
}
