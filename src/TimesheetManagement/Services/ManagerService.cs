using TimesheetManagement.Models.Entities;
using TimesheetManagement.Models.DTOs;
using TimesheetManagement.Models.Enums;
using TimesheetManagement.Data.Repositories;

namespace TimesheetManagement.Services;

public interface IManagerService
{
    Task<List<TimesheetSummaryDto>> GetPendingTimesheetsAsync(Guid managerId);
    Task<bool> ApproveTimesheetAsync(Guid timesheetId, Guid managerId);
    Task<bool> RejectTimesheetAsync(Guid timesheetId, Guid managerId, string reason);
}

public class ManagerService : IManagerService
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    
    public ManagerService(
        ITimesheetRepository timesheetRepository,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _timesheetRepository = timesheetRepository;
        _auditService = auditService;
        _notificationService = notificationService;
    }
    
    public async Task<List<TimesheetSummaryDto>> GetPendingTimesheetsAsync(Guid managerId)
    {
        var timesheets = await _timesheetRepository.GetPendingTimesheetsByManagerAsync(managerId);
        
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
    
    public async Task<bool> ApproveTimesheetAsync(Guid timesheetId, Guid managerId)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId);
        
        if (timesheet == null)
            throw new NotFoundException("Timesheet not found.");
        
        if (timesheet.Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException("Only submitted timesheets can be approved.");
        
        // Verify manager has authority (employee's manager)
        if (timesheet.Employee?.ManagerId != managerId)
            throw new UnauthorizedAccessException("You can only approve timesheets for your direct reports.");
        
        // Update timesheet status
        timesheet.Status = TimesheetStatus.Approved;
        timesheet.ManagerId = managerId;
        timesheet.ManagerDecisionAt = DateTime.UtcNow;
        timesheet.UpdatedAt = DateTime.UtcNow;
        
        await _timesheetRepository.UpdateAsync(timesheet);
        await _timesheetRepository.SaveChangesAsync();
        
        await _auditService.LogActionAsync(timesheetId, AuditActionType.Approved, managerId);
        
        return true;
    }
    
    public async Task<bool> RejectTimesheetAsync(Guid timesheetId, Guid managerId, string reason)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId);
        
        if (timesheet == null)
            throw new NotFoundException("Timesheet not found.");
        
        if (timesheet.Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException("Only submitted timesheets can be rejected.");
        
        // Verify manager has authority
        if (timesheet.Employee?.ManagerId != managerId)
            throw new UnauthorizedAccessException("You can only reject timesheets for your direct reports.");
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException("Rejection reason is required.");
        
        // Update timesheet status
        timesheet.Status = TimesheetStatus.Rejected;
        timesheet.ManagerId = managerId;
        timesheet.ManagerDecisionAt = DateTime.UtcNow;
        timesheet.ManagerDecisionReason = reason;
        timesheet.UpdatedAt = DateTime.UtcNow;
        
        await _timesheetRepository.UpdateAsync(timesheet);
        await _timesheetRepository.SaveChangesAsync();
        
        await _auditService.LogActionAsync(timesheetId, AuditActionType.Rejected, managerId, reason);
        
        // Send notification to employee
        var weekRange = $"{timesheet.WeekStartDate:yyyy-MM-dd} to {timesheet.WeekEndDate:yyyy-MM-dd}";
        await _notificationService.SendRejectionNotificationAsync(timesheet.EmployeeId, weekRange, reason);
        
        return true;
    }
}
