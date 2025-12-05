using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesheetManagement.Models.DTOs;
using TimesheetManagement.Services;

namespace TimesheetManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager")]
public class ManagerController : ControllerBase
{
    private readonly IManagerService _managerService;
    private readonly ILogger<ManagerController> _logger;
    
    public ManagerController(IManagerService managerService, ILogger<ManagerController> logger)
    {
        _managerService = managerService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets timesheets with status Submitted for manager's employees
    /// </summary>
    [HttpGet("timesheets/pending")]
    public async Task<ActionResult<List<TimesheetSummaryDto>>> GetPendingTimesheets()
    {
        try
        {
            var managerId = GetCurrentUserId();
            var timesheets = await _managerService.GetPendingTimesheetsAsync(managerId);
            return Ok(timesheets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending timesheets");
            return StatusCode(500, "An error occurred while retrieving pending timesheets.");
        }
    }
    
    /// <summary>
    /// Approves a timesheet
    /// </summary>
    [HttpPost("timesheets/{id}/approve")]
    public async Task<ActionResult> ApproveTimesheet(Guid id)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var success = await _managerService.ApproveTimesheetAsync(id, managerId);
            
            if (success)
                return Ok(new { message = "Timesheet approved successfully." });
            
            return BadRequest("Failed to approve timesheet.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving timesheet {TimesheetId}", id);
            return StatusCode(500, "An error occurred while approving the timesheet.");
        }
    }
    
    /// <summary>
    /// Rejects a timesheet with a reason
    /// </summary>
    [HttpPost("timesheets/{id}/reject")]
    public async Task<ActionResult> RejectTimesheet(Guid id, [FromBody] RejectTimesheetDto dto)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var success = await _managerService.RejectTimesheetAsync(id, managerId, dto.RejectionReason);
            
            if (success)
                return Ok(new { message = "Timesheet rejected successfully." });
            
            return BadRequest("Failed to reject timesheet.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting timesheet {TimesheetId}", id);
            return StatusCode(500, "An error occurred while rejecting the timesheet.");
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
