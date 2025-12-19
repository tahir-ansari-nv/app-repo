namespace TimeSheetPortal.Core.Entities;

public class MFACode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTimeOffset Expiry { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
