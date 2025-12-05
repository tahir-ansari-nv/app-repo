using System.ComponentModel.DataAnnotations;

namespace TimesheetManagement.Models.DTOs;

public class TimesheetUpdateDto
{
    [Required]
    public Guid EmployeeId { get; set; }
    
    [Required]
    public DateTime WeekStartDate { get; set; }
    
    [Required]
    public List<TimesheetEntryDto> Entries { get; set; } = new();
}
