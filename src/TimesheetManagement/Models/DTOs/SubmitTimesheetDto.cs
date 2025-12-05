using System.ComponentModel.DataAnnotations;

namespace TimesheetManagement.Models.DTOs;

public class SubmitTimesheetDto
{
    [Required]
    public Guid TimesheetId { get; set; }
}
