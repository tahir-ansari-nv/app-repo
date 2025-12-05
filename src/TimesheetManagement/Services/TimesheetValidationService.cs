using TimesheetManagement.Models.Entities;
using TimesheetManagement.Models.DTOs;

namespace TimesheetManagement.Services;

public interface ITimesheetValidationService
{
    ValidationResult ValidateTimesheetForSubmission(Timesheet timesheet);
    ValidationResult ValidateTimesheetUpdate(TimesheetUpdateDto dto);
}

public class TimesheetValidationService : ITimesheetValidationService
{
    public ValidationResult ValidateTimesheetForSubmission(Timesheet timesheet)
    {
        var errors = new List<string>();
        
        // Check if timesheet has entries
        if (timesheet.Entries == null || !timesheet.Entries.Any())
        {
            errors.Add("Timesheet must have at least one entry.");
        }
        else
        {
            // Validate each entry
            foreach (var entry in timesheet.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.ProjectName))
                    errors.Add("ProjectName is required for all entries.");
                
                if (string.IsNullOrWhiteSpace(entry.TaskName))
                    errors.Add("TaskName is required for all entries.");
                
                if (entry.Hours < 0 || entry.Hours > 24)
                    errors.Add($"Hours must be between 0 and 24. Invalid value: {entry.Hours}");
                
                if (entry.Date < timesheet.WeekStartDate || entry.Date > timesheet.WeekEndDate)
                    errors.Add($"Entry date {entry.Date:yyyy-MM-dd} is not within the week range.");
            }
            
            // Check total hours
            var totalHours = timesheet.Entries.Sum(e => e.Hours);
            if (totalHours > 100)
            {
                errors.Add($"Total hours ({totalHours}) exceed the maximum allowed (100 hours per week).");
            }
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
    
    public ValidationResult ValidateTimesheetUpdate(TimesheetUpdateDto dto)
    {
        var errors = new List<string>();
        
        // Validate week start date is Monday
        if (dto.WeekStartDate.DayOfWeek != DayOfWeek.Monday)
        {
            errors.Add("WeekStartDate must be a Monday.");
        }
        
        // Validate entries
        if (dto.Entries != null && dto.Entries.Any())
        {
            foreach (var entry in dto.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.ProjectName))
                    errors.Add("ProjectName is required for all entries.");
                
                if (string.IsNullOrWhiteSpace(entry.TaskName))
                    errors.Add("TaskName is required for all entries.");
                
                if (entry.Hours < 0 || entry.Hours > 24)
                    errors.Add($"Hours must be between 0 and 24. Invalid value: {entry.Hours}");
            }
            
            // Check total hours
            var totalHours = dto.Entries.Sum(e => e.Hours);
            if (totalHours > 100)
            {
                errors.Add($"Total hours ({totalHours}) exceed the maximum allowed (100 hours per week).");
            }
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
