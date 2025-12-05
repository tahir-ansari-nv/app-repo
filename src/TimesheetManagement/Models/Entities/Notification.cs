using System.ComponentModel.DataAnnotations;

namespace TimesheetManagement.Models.Entities;

public class Notification
{
    [Key]
    public Guid NotificationId { get; set; }
    
    [Required]
    public Guid RecipientId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    public bool IsRead { get; set; } = false;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
