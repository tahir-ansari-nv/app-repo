using TimesheetApi.Models;

namespace TimesheetApi.Repositories;

public interface ITimesheetRepository
{
    Task<Timesheet?> GetByIdAsync(Guid id, bool includeEntries = true);
    Task<Timesheet?> GetByEmployeeAndWeekAsync(Guid employeeId, DateTime weekStartDate, bool includeEntries = true);
    Task<List<Timesheet>> GetByEmployeeAsync(Guid employeeId);
    Task<List<Timesheet>> GetByStatusAsync(TimesheetStatus status);
    Task<Timesheet> CreateAsync(Timesheet timesheet);
    Task<Timesheet> UpdateAsync(Timesheet timesheet);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid employeeId, DateTime weekStartDate);
}
