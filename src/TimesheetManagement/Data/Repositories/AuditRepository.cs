using TimesheetManagement.Models.Entities;

namespace TimesheetManagement.Data.Repositories;

public interface IAuditRepository
{
    Task CreateAuditLogAsync(AuditLog auditLog);
    Task SaveChangesAsync();
}

public class AuditRepository : IAuditRepository
{
    private readonly TimesheetDbContext _context;
    
    public AuditRepository(TimesheetDbContext context)
    {
        _context = context;
    }
    
    public async Task CreateAuditLogAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
