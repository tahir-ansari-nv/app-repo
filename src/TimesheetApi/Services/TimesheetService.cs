using Microsoft.EntityFrameworkCore;
using TimesheetApi.DTOs;
using TimesheetApi.Models;
using TimesheetApi.Repositories;

namespace TimesheetApi.Services;

public interface ITimesheetService
{
    Task<TimesheetResponse?> GetTimesheetAsync(Guid employeeId, DateTime weekStartDate);
    Task<TimesheetResponse> CreateOrUpdateTimesheetAsync(Guid employeeId, CreateOrUpdateTimesheetRequest request);
    Task<TimesheetResponse> SubmitTimesheetAsync(Guid employeeId, Guid timesheetId);
    Task<List<TimesheetSummaryDto>> GetSubmittedTimesheetsAsync();
    Task<TimesheetResponse> ApproveTimesheetAsync(Guid timesheetId, Guid managerId);
    Task<TimesheetResponse> RejectTimesheetAsync(Guid timesheetId, Guid managerId, string reason);
}

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public TimesheetService(
        ITimesheetRepository timesheetRepository,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _timesheetRepository = timesheetRepository;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<TimesheetResponse?> GetTimesheetAsync(Guid employeeId, DateTime weekStartDate)
    {
        var timesheet = await _timesheetRepository.GetByEmployeeAndWeekAsync(employeeId, weekStartDate, includeEntries: true);
        
        if (timesheet == null)
        {
            return null;
        }

        return MapToResponse(timesheet);
    }

    public async Task<TimesheetResponse> CreateOrUpdateTimesheetAsync(Guid employeeId, CreateOrUpdateTimesheetRequest request)
    {
        var existingTimesheet = await _timesheetRepository.GetByEmployeeAndWeekAsync(employeeId, request.WeekStartDate, includeEntries: true);

        if (existingTimesheet != null)
        {
            // Validate that timesheet can be edited
            if (existingTimesheet.Status == TimesheetStatus.Approved || existingTimesheet.Status == TimesheetStatus.Submitted)
            {
                throw new InvalidOperationException("Cannot edit a timesheet that has been submitted or approved");
            }

            // Update existing timesheet
            existingTimesheet.UpdatedAt = DateTime.UtcNow;
            
            // Remove old entries and add new ones
            existingTimesheet.Entries.Clear();
            
            foreach (var entryDto in request.Entries)
            {
                var entry = new TimesheetEntry
                {
                    Id = entryDto.EntryId ?? Guid.NewGuid(),
                    TimesheetId = existingTimesheet.Id,
                    ProjectCode = entryDto.ProjectCode,
                    TaskDescription = entryDto.TaskDescription,
                    Date = entryDto.Date,
                    Hours = entryDto.Hours,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                existingTimesheet.Entries.Add(entry);
            }

            await _timesheetRepository.UpdateAsync(existingTimesheet);
            await _auditService.LogAsync("Timesheet", existingTimesheet.Id, AuditAction.Updated, employeeId);

            return MapToResponse(existingTimesheet);
        }
        else
        {
            // Create new timesheet
            var timesheet = new Timesheet
            {
                EmployeeId = employeeId,
                WeekStartDate = request.WeekStartDate,
                Status = TimesheetStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var entryDto in request.Entries)
            {
                var entry = new TimesheetEntry
                {
                    ProjectCode = entryDto.ProjectCode,
                    TaskDescription = entryDto.TaskDescription,
                    Date = entryDto.Date,
                    Hours = entryDto.Hours,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                timesheet.Entries.Add(entry);
            }

            await _timesheetRepository.CreateAsync(timesheet);
            await _auditService.LogAsync("Timesheet", timesheet.Id, AuditAction.Created, employeeId);

            return MapToResponse(timesheet);
        }
    }

    public async Task<TimesheetResponse> SubmitTimesheetAsync(Guid employeeId, Guid timesheetId)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId, includeEntries: true);

        if (timesheet == null)
        {
            throw new KeyNotFoundException($"Timesheet with ID {timesheetId} not found");
        }

        if (timesheet.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only submit your own timesheets");
        }

        if (timesheet.Status != TimesheetStatus.Draft && timesheet.Status != TimesheetStatus.Rejected)
        {
            throw new InvalidOperationException("Only draft or rejected timesheets can be submitted");
        }

        // Validate entries
        if (!timesheet.Entries.Any())
        {
            throw new InvalidOperationException("Cannot submit an empty timesheet");
        }

        var totalHours = timesheet.Entries.Sum(e => e.Hours);
        if (totalHours <= 0)
        {
            throw new InvalidOperationException("Cannot submit a timesheet with zero hours");
        }

        if (totalHours > 100)
        {
            throw new InvalidOperationException("Total weekly hours cannot exceed 100");
        }

        timesheet.Status = TimesheetStatus.Submitted;
        timesheet.SubmittedAt = DateTime.UtcNow;
        timesheet.UpdatedAt = DateTime.UtcNow;

        await _timesheetRepository.UpdateAsync(timesheet);
        await _auditService.LogAsync("Timesheet", timesheet.Id, AuditAction.Submitted, employeeId);

        return MapToResponse(timesheet);
    }

    public async Task<List<TimesheetSummaryDto>> GetSubmittedTimesheetsAsync()
    {
        var timesheets = await _timesheetRepository.GetByStatusAsync(TimesheetStatus.Submitted);

        return timesheets.Select(t => new TimesheetSummaryDto
        {
            TimesheetId = t.Id,
            EmployeeId = t.EmployeeId,
            EmployeeName = t.Employee.Name,
            EmployeeEmail = t.Employee.Email,
            WeekStartDate = t.WeekStartDate,
            Status = t.Status.ToString(),
            SubmittedAt = t.SubmittedAt,
            TotalHours = t.Entries.Sum(e => e.Hours)
        }).ToList();
    }

    public async Task<TimesheetResponse> ApproveTimesheetAsync(Guid timesheetId, Guid managerId)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId, includeEntries: true);

        if (timesheet == null)
        {
            throw new KeyNotFoundException($"Timesheet with ID {timesheetId} not found");
        }

        if (timesheet.Status != TimesheetStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted timesheets can be approved");
        }

        timesheet.Status = TimesheetStatus.Approved;
        timesheet.ManagerId = managerId;
        timesheet.ManagerDecisionAt = DateTime.UtcNow;
        timesheet.UpdatedAt = DateTime.UtcNow;

        await _timesheetRepository.UpdateAsync(timesheet);
        await _auditService.LogAsync("Timesheet", timesheet.Id, AuditAction.Approved, managerId);

        return MapToResponse(timesheet);
    }

    public async Task<TimesheetResponse> RejectTimesheetAsync(Guid timesheetId, Guid managerId, string reason)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId, includeEntries: true);

        if (timesheet == null)
        {
            throw new KeyNotFoundException($"Timesheet with ID {timesheetId} not found");
        }

        if (timesheet.Status != TimesheetStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted timesheets can be rejected");
        }

        timesheet.Status = TimesheetStatus.Rejected;
        timesheet.ManagerId = managerId;
        timesheet.ManagerDecisionAt = DateTime.UtcNow;
        timesheet.ManagerDecisionReason = reason;
        timesheet.UpdatedAt = DateTime.UtcNow;

        await _timesheetRepository.UpdateAsync(timesheet);
        await _auditService.LogAsync("Timesheet", timesheet.Id, AuditAction.Rejected, managerId, $"Reason: {reason}");
        await _notificationService.SendTimesheetRejectionNotificationAsync(timesheet.EmployeeId, timesheetId, reason);

        return MapToResponse(timesheet);
    }

    private TimesheetResponse MapToResponse(Timesheet timesheet)
    {
        return new TimesheetResponse
        {
            TimesheetId = timesheet.Id,
            WeekStartDate = timesheet.WeekStartDate,
            Status = timesheet.Status.ToString(),
            Entries = timesheet.Entries.Select(e => new TimesheetEntryDto
            {
                EntryId = e.Id,
                ProjectCode = e.ProjectCode,
                TaskDescription = e.TaskDescription,
                Date = e.Date,
                Hours = e.Hours
            }).ToList(),
            CreatedAt = timesheet.CreatedAt,
            UpdatedAt = timesheet.UpdatedAt,
            SubmittedAt = timesheet.SubmittedAt,
            ManagerDecisionReason = timesheet.ManagerDecisionReason
        };
    }
}
