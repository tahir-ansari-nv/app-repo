namespace TimesheetApi.DTOs;

public class TimesheetEntryDto
{
    public Guid? EntryId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string? TaskDescription { get; set; }
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
}
