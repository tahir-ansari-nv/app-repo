using TimesheetManagement.Models.Entities;
using TimesheetManagement.Models.DTOs;
using TimesheetManagement.Models.Enums;
using TimesheetManagement.Data.Repositories;

namespace TimesheetManagement.Services;

public interface ITimesheetService
{
    Task<TimesheetDetailDto?> GetTimesheetByWeekAsync(Guid employeeId, DateTime weekStartDate);
    Task<TimesheetDetailDto> CreateOrUpdateTimesheetAsync(TimesheetUpdateDto dto);
    Task<bool> SubmitTimesheetAsync(Guid timesheetId, Guid employeeId);
    Task<List<TimesheetSummaryDto>> GetTimesheetHistoryAsync(Guid employeeId);
}

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly ITimesheetValidationService _validationService;
    private readonly IAuditService _auditService;
    
    public TimesheetService(
        ITimesheetRepository timesheetRepository,
        ITimesheetValidationService validationService,
        IAuditService auditService)
    {
        _timesheetRepository = timesheetRepository;
        _validationService = validationService;
        _auditService = auditService;
    }
    
    public async Task<TimesheetDetailDto?> GetTimesheetByWeekAsync(Guid employeeId, DateTime weekStartDate)
    {
        var timesheet = await _timesheetRepository.GetByEmployeeAndWeekAsync(employeeId, weekStartDate);
        
        if (timesheet == null)
            return null;
        
        return MapToDetailDto(timesheet);
    }
    
    public async Task<TimesheetDetailDto> CreateOrUpdateTimesheetAsync(TimesheetUpdateDto dto)
    {
        // Validate the update
        var validationResult = _validationService.ValidateTimesheetUpdate(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(string.Join("; ", validationResult.Errors));
        }
        
        // Calculate week end date (Sunday)
        var weekEndDate = dto.WeekStartDate.AddDays(6);
        
        // Check if timesheet exists
        var existingTimesheet = await _timesheetRepository.GetByEmployeeAndWeekAsync(dto.EmployeeId, dto.WeekStartDate);
        
        if (existingTimesheet != null)
        {
            // Cannot update if not in Draft status
            if (existingTimesheet.Status != TimesheetStatus.Draft && existingTimesheet.Status != TimesheetStatus.Rejected)
            {
                throw new InvalidOperationException("Cannot update timesheet that is not in Draft or Rejected status.");
            }
            
            // Update existing timesheet
            existingTimesheet.UpdatedAt = DateTime.UtcNow;
            
            // Clear existing entries and add new ones
            existingTimesheet.Entries.Clear();
            foreach (var entryDto in dto.Entries)
            {
                existingTimesheet.Entries.Add(new TimesheetEntry
                {
                    TimesheetEntryId = Guid.NewGuid(),
                    TimesheetId = existingTimesheet.TimesheetId,
                    ProjectName = entryDto.ProjectName,
                    TaskName = entryDto.TaskName,
                    Date = entryDto.Date,
                    Hours = entryDto.Hours,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            
            // If it was rejected, reset to draft
            if (existingTimesheet.Status == TimesheetStatus.Rejected)
            {
                existingTimesheet.Status = TimesheetStatus.Draft;
                existingTimesheet.ManagerDecisionReason = null;
                existingTimesheet.ManagerDecisionAt = null;
                existingTimesheet.ManagerId = null;
            }
            
            await _timesheetRepository.UpdateAsync(existingTimesheet);
            await _timesheetRepository.SaveChangesAsync();
            
            await _auditService.LogActionAsync(existingTimesheet.TimesheetId, AuditActionType.Updated, dto.EmployeeId);
            
            return MapToDetailDto(existingTimesheet);
        }
        else
        {
            // Create new timesheet
            var newTimesheet = new Timesheet
            {
                TimesheetId = Guid.NewGuid(),
                EmployeeId = dto.EmployeeId,
                WeekStartDate = dto.WeekStartDate,
                WeekEndDate = weekEndDate,
                Status = TimesheetStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Entries = dto.Entries.Select(e => new TimesheetEntry
                {
                    TimesheetEntryId = Guid.NewGuid(),
                    ProjectName = e.ProjectName,
                    TaskName = e.TaskName,
                    Date = e.Date,
                    Hours = e.Hours,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            };
            
            await _timesheetRepository.CreateAsync(newTimesheet);
            await _timesheetRepository.SaveChangesAsync();
            
            await _auditService.LogActionAsync(newTimesheet.TimesheetId, AuditActionType.Created, dto.EmployeeId);
            
            return MapToDetailDto(newTimesheet);
        }
    }
    
    public async Task<bool> SubmitTimesheetAsync(Guid timesheetId, Guid employeeId)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId);
        
        if (timesheet == null)
            throw new NotFoundException("Timesheet not found.");
        
        if (timesheet.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only submit your own timesheets.");
        
        if (timesheet.Status != TimesheetStatus.Draft)
            throw new InvalidOperationException("Only draft timesheets can be submitted.");
        
        // Validate timesheet for submission
        var validationResult = _validationService.ValidateTimesheetForSubmission(timesheet);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(string.Join("; ", validationResult.Errors));
        }
        
        // Update status to Submitted
        timesheet.Status = TimesheetStatus.Submitted;
        timesheet.SubmittedAt = DateTime.UtcNow;
        timesheet.UpdatedAt = DateTime.UtcNow;
        
        await _timesheetRepository.UpdateAsync(timesheet);
        await _timesheetRepository.SaveChangesAsync();
        
        await _auditService.LogActionAsync(timesheetId, AuditActionType.Submitted, employeeId);
        
        return true;
    }
    
    public async Task<List<TimesheetSummaryDto>> GetTimesheetHistoryAsync(Guid employeeId)
    {
        var timesheets = await _timesheetRepository.GetHistoryByEmployeeAsync(employeeId);
        
        return timesheets.Select(t => new TimesheetSummaryDto
        {
            TimesheetId = t.TimesheetId,
            EmployeeId = t.EmployeeId,
            EmployeeName = t.Employee?.Name ?? "",
            WeekStartDate = t.WeekStartDate,
            WeekEndDate = t.WeekEndDate,
            Status = t.Status,
            SubmittedAt = t.SubmittedAt,
            TotalHours = t.Entries.Sum(e => e.Hours)
        }).ToList();
    }
    
    private TimesheetDetailDto MapToDetailDto(Timesheet timesheet)
    {
        return new TimesheetDetailDto
        {
            TimesheetId = timesheet.TimesheetId,
            EmployeeId = timesheet.EmployeeId,
            WeekStartDate = timesheet.WeekStartDate,
            WeekEndDate = timesheet.WeekEndDate,
            Status = timesheet.Status,
            SubmittedAt = timesheet.SubmittedAt,
            ManagerDecisionAt = timesheet.ManagerDecisionAt,
            ManagerId = timesheet.ManagerId,
            ManagerDecisionReason = timesheet.ManagerDecisionReason,
            CreatedAt = timesheet.CreatedAt,
            UpdatedAt = timesheet.UpdatedAt,
            Entries = timesheet.Entries.Select(e => new TimesheetEntryDto
            {
                TimesheetEntryId = e.TimesheetEntryId,
                ProjectName = e.ProjectName,
                TaskName = e.TaskName,
                Date = e.Date,
                Hours = e.Hours
            }).ToList()
        };
    }
}

// Custom exception classes
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
