using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimesheetManagement.Models.Enums;

namespace TimesheetManagement.Models.Entities;

public class AuditLog
{
    [Key]
    public Guid AuditLogId { get; set; }
    
    [Required]
    public Guid TimesheetId { get; set; }
    
    [Required]
    public AuditActionType ActionType { get; set; }
    
    [Required]
    public Guid PerformedBy { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [ForeignKey(nameof(TimesheetId))]
    public virtual Timesheet Timesheet { get; set; } = null!;
}
