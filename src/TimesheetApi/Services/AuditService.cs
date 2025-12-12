using TimesheetApi.Models;
using TimesheetApi.Repositories;

namespace TimesheetApi.Services;

public interface IAuditService
{
    Task LogAsync(string entityType, Guid entityId, AuditAction action, Guid performedBy, string? details = null);
}

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task LogAsync(string entityType, Guid entityId, AuditAction action, Guid performedBy, string? details = null)
    {
        var auditLog = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow,
            Details = details
        };

        await _auditRepository.AddAsync(auditLog);
    }
}
