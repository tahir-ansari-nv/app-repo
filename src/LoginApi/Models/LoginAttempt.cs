using System.ComponentModel.DataAnnotations;

namespace LoginApi.Models;

public class LoginAttempt
{
    public Guid Id { get; set; }
    
    public Guid? UserId { get; set; }
    
    [Required]
    public string EmailAttempted { get; set; } = string.Empty;
    
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    public string IPAddress { get; set; } = string.Empty;
    
    public string UserAgent { get; set; } = string.Empty;
    
    public bool Success { get; set; }
}
