using System.ComponentModel.DataAnnotations;

namespace TimesheetManagement.Models.Entities;

public class Employee
{
    [Key]
    public Guid EmployeeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public Guid? ManagerId { get; set; }
    
    public virtual ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
}
