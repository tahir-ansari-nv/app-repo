using TimesheetManagement.Models.Enums;

namespace TimesheetManagement.Models.DTOs;

public class TimesheetSummaryDto
{
    public Guid TimesheetId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public TimesheetStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal TotalHours { get; set; }
}
