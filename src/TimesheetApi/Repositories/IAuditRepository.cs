using TimesheetApi.Models;

namespace TimesheetApi.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
}
