using Microsoft.Extensions.Logging;

namespace TimesheetApi.Services;

public interface INotificationService
{
    Task SendTimesheetRejectionNotificationAsync(Guid employeeId, Guid timesheetId, string reason);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendTimesheetRejectionNotificationAsync(Guid employeeId, Guid timesheetId, string reason)
    {
        // Stub implementation for notification service
        // In production, this would integrate with Azure Service Bus or RabbitMQ
        _logger.LogInformation(
            "Sending rejection notification for timesheet {TimesheetId} to employee {EmployeeId}. Reason: {Reason}",
            timesheetId, employeeId, reason);

        await Task.CompletedTask;
    }
}
