using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Models.Entities;

namespace TimesheetManagement.Data.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetByRecipientAsync(Guid recipientId);
    Task CreateNotificationAsync(Notification notification);
    Task SaveChangesAsync();
}

public class NotificationRepository : INotificationRepository
{
    private readonly TimesheetDbContext _context;
    
    public NotificationRepository(TimesheetDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Notification>> GetByRecipientAsync(Guid recipientId)
    {
        return await _context.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
    
    public async Task CreateNotificationAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
