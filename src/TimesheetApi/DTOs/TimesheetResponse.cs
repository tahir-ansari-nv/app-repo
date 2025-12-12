using TimesheetApi.Models;

namespace TimesheetApi.DTOs;

public class TimesheetResponse
{
    public Guid TimesheetId { get; set; }
    public DateTime WeekStartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TimesheetEntryDto> Entries { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? ManagerDecisionReason { get; set; }
}
