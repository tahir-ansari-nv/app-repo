using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimesheetManagement.Models.Entities;

public class TimesheetEntry
{
    [Key]
    public Guid TimesheetEntryId { get; set; }
    
    [Required]
    public Guid TimesheetId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProjectName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string TaskName { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Hours { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey(nameof(TimesheetId))]
    public virtual Timesheet Timesheet { get; set; } = null!;
}
