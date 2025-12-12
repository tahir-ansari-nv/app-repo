namespace TimesheetApi.DTOs;

public class CreateOrUpdateTimesheetRequest
{
    public DateTime WeekStartDate { get; set; }
    public List<TimesheetEntryDto> Entries { get; set; } = new();
}
