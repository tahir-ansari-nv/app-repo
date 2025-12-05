using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Models.Entities;

namespace TimesheetManagement.Data.Repositories;

public interface ITimesheetRepository
{
    Task<Timesheet?> GetByIdAsync(Guid timesheetId);
    Task<Timesheet?> GetByEmployeeAndWeekAsync(Guid employeeId, DateTime weekStartDate);
    Task<List<Timesheet>> GetHistoryByEmployeeAsync(Guid employeeId);
    Task<List<Timesheet>> GetPendingTimesheetsByManagerAsync(Guid managerId);
    Task<Timesheet> CreateAsync(Timesheet timesheet);
    Task<Timesheet> UpdateAsync(Timesheet timesheet);
    Task SaveChangesAsync();
}

public class TimesheetRepository : ITimesheetRepository
{
    private readonly TimesheetDbContext _context;
    
    public TimesheetRepository(TimesheetDbContext context)
    {
        _context = context;
    }
    
    public async Task<Timesheet?> GetByIdAsync(Guid timesheetId)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.TimesheetId == timesheetId);
    }
    
    public async Task<Timesheet?> GetByEmployeeAndWeekAsync(Guid employeeId, DateTime weekStartDate)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.EmployeeId == employeeId && t.WeekStartDate == weekStartDate);
    }
    
    public async Task<List<Timesheet>> GetHistoryByEmployeeAsync(Guid employeeId)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.WeekStartDate)
            .ToListAsync();
    }
    
    public async Task<List<Timesheet>> GetPendingTimesheetsByManagerAsync(Guid managerId)
    {
        // Get all employees managed by this manager
        var managedEmployeeIds = await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .Select(e => e.EmployeeId)
            .ToListAsync();
        
        // Get all submitted timesheets for managed employees
        return await _context.Timesheets
            .Include(t => t.Entries)
            .Include(t => t.Employee)
            .Where(t => managedEmployeeIds.Contains(t.EmployeeId) && t.Status == Models.Enums.TimesheetStatus.Submitted)
            .OrderBy(t => t.SubmittedAt)
            .ToListAsync();
    }
    
    public async Task<Timesheet> CreateAsync(Timesheet timesheet)
    {
        await _context.Timesheets.AddAsync(timesheet);
        return timesheet;
    }
    
    public async Task<Timesheet> UpdateAsync(Timesheet timesheet)
    {
        timesheet.UpdatedAt = DateTime.UtcNow;
        _context.Timesheets.Update(timesheet);
        return timesheet;
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
