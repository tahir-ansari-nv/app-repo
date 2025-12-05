using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesheetManagement.Models.DTOs;
using TimesheetManagement.Services;

namespace TimesheetManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;
    
    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets notifications for the current employee
    /// </summary>
    [HttpGet("{employeeId}")]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications(Guid employeeId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Verify employee is requesting their own notifications
            if (employeeId != currentUserId)
            {
                return Forbid();
            }
            
            var notifications = await _notificationService.GetNotificationsByEmployeeAsync(employeeId);
            
            var result = notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                RecipientId = n.RecipientId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving notifications.");
        }
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("employeeId")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
        
        return userId;
    }
}
