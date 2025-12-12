namespace TimesheetApi.DTOs;

public class TimesheetSummaryDto
{
    public Guid TimesheetId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public decimal TotalHours { get; set; }
}
