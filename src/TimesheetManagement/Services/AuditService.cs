using TimesheetManagement.Models.Entities;
using TimesheetManagement.Models.Enums;
using TimesheetManagement.Data.Repositories;

namespace TimesheetManagement.Services;

public interface IAuditService
{
    Task LogActionAsync(Guid timesheetId, AuditActionType actionType, Guid performedBy, string? notes = null);
}

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    
    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }
    
    public async Task LogActionAsync(Guid timesheetId, AuditActionType actionType, Guid performedBy, string? notes = null)
    {
        var auditLog = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            TimesheetId = timesheetId,
            ActionType = actionType,
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow,
            Notes = notes
        };
        
        await _auditRepository.CreateAuditLogAsync(auditLog);
        await _auditRepository.SaveChangesAsync();
    }
}
