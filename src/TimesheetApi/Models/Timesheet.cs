using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimesheetApi.Models;

public class Timesheet
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [Required]
    public DateTime WeekStartDate { get; set; }

    [Required]
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ManagerDecisionAt { get; set; }

    public Guid? ManagerId { get; set; }

    [MaxLength(1000)]
    public string? ManagerDecisionReason { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<TimesheetEntry> Entries { get; set; } = new List<TimesheetEntry>();
}
