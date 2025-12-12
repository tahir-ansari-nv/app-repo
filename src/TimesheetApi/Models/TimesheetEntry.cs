using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimesheetApi.Models;

public class TimesheetEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TimesheetId { get; set; }

    [ForeignKey(nameof(TimesheetId))]
    public Timesheet Timesheet { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string ProjectCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TaskDescription { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Column(TypeName = "decimal(4,2)")]
    public decimal Hours { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
