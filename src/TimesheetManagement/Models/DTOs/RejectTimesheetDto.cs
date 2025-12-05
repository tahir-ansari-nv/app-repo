using System.ComponentModel.DataAnnotations;

namespace TimesheetManagement.Models.DTOs;

public class RejectTimesheetDto
{
    [Required]
    [MaxLength(500)]
    public string RejectionReason { get; set; } = string.Empty;
}
