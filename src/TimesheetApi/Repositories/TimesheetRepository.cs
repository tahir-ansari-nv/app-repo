using Microsoft.EntityFrameworkCore;
using TimesheetApi.Data;
using TimesheetApi.Models;

namespace TimesheetApi.Repositories;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly ApplicationDbContext _context;

    public TimesheetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Timesheet?> GetByIdAsync(Guid id, bool includeEntries = true)
    {
        var query = _context.Timesheets.AsQueryable();

        if (includeEntries)
        {
            query = query.Include(t => t.Entries);
        }

        query = query.Include(t => t.Employee);

        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Timesheet?> GetByEmployeeAndWeekAsync(Guid employeeId, DateTime weekStartDate, bool includeEntries = true)
    {
        var query = _context.Timesheets.AsQueryable();

        if (includeEntries)
        {
            query = query.Include(t => t.Entries);
        }

        query = query.Include(t => t.Employee);

        return await query.FirstOrDefaultAsync(t => 
            t.EmployeeId == employeeId && t.WeekStartDate.Date == weekStartDate.Date);
    }

    public async Task<List<Timesheet>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
            .Include(t => t.Employee)
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.WeekStartDate)
            .ToListAsync();
    }

    public async Task<List<Timesheet>> GetByStatusAsync(TimesheetStatus status)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
            .Include(t => t.Employee)
            .Where(t => t.Status == status)
            .OrderBy(t => t.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Timesheet> CreateAsync(Timesheet timesheet)
    {
        _context.Timesheets.Add(timesheet);
        await _context.SaveChangesAsync();
        return timesheet;
    }

    public async Task<Timesheet> UpdateAsync(Timesheet timesheet)
    {
        timesheet.UpdatedAt = DateTime.UtcNow;
        _context.Timesheets.Update(timesheet);
        await _context.SaveChangesAsync();
        return timesheet;
    }

    public async Task DeleteAsync(Guid id)
    {
        var timesheet = await _context.Timesheets.FindAsync(id);
        if (timesheet != null)
        {
            _context.Timesheets.Remove(timesheet);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid employeeId, DateTime weekStartDate)
    {
        return await _context.Timesheets
            .AnyAsync(t => t.EmployeeId == employeeId && t.WeekStartDate.Date == weekStartDate.Date);
    }
}
