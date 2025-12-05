using TimesheetManagement.Models.Entities;
using TimesheetManagement.Data.Repositories;

namespace TimesheetManagement.Services;

public interface INotificationService
{
    Task SendRejectionNotificationAsync(Guid employeeId, string timesheetWeek, string reason);
    Task<List<Notification>> GetNotificationsByEmployeeAsync(Guid employeeId);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    
    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }
    
    public async Task SendRejectionNotificationAsync(Guid employeeId, string timesheetWeek, string reason)
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            RecipientId = employeeId,
            Title = "Timesheet Rejected",
            Message = $"Your timesheet for week {timesheetWeek} has been rejected. Reason: {reason}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        
        await _notificationRepository.CreateNotificationAsync(notification);
        await _notificationRepository.SaveChangesAsync();
    }
    
    public async Task<List<Notification>> GetNotificationsByEmployeeAsync(Guid employeeId)
    {
        return await _notificationRepository.GetByRecipientAsync(employeeId);
    }
}
