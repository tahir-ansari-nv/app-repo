using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimesheetManagement.Models.Enums;

namespace TimesheetManagement.Models.Entities;

public class Timesheet
{
    [Key]
    public Guid TimesheetId { get; set; }
    
    [Required]
    public Guid EmployeeId { get; set; }
    
    [Required]
    public DateTime WeekStartDate { get; set; }
    
    [Required]
    public DateTime WeekEndDate { get; set; }
    
    [Required]
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? ManagerDecisionAt { get; set; }
    
    public Guid? ManagerId { get; set; }
    
    [MaxLength(500)]
    public string? ManagerDecisionReason { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;
    
    public virtual ICollection<TimesheetEntry> Entries { get; set; } = new List<TimesheetEntry>();
    
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
