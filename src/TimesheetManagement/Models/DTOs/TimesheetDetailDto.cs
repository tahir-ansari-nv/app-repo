using TimesheetManagement.Models.Enums;

namespace TimesheetManagement.Models.DTOs;

public class TimesheetDetailDto
{
    public Guid TimesheetId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public TimesheetStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ManagerDecisionAt { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerDecisionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TimesheetEntryDto> Entries { get; set; } = new();
}

public class TimesheetEntryDto
{
    public Guid? TimesheetEntryId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
}
