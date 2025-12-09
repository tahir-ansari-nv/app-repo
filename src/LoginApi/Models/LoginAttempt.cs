namespace LoginApi.Models;

public class LoginAttempt
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}
