using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesheetManagement.Models.DTOs;
using TimesheetManagement.Services;

namespace TimesheetManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimesheetsController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;
    private readonly ILogger<TimesheetsController> _logger;
    
    public TimesheetsController(ITimesheetService timesheetService, ILogger<TimesheetsController> logger)
    {
        _timesheetService = timesheetService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets timesheet for employee for specified week
    /// </summary>
    [HttpGet("week/{employeeId}/{weekStartDate}")]
    public async Task<ActionResult<TimesheetDetailDto>> GetTimesheetByWeek(Guid employeeId, DateTime weekStartDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Check authorization - employee can view their own or manager can view
            if (!IsAuthorizedToView(currentUserId, employeeId))
            {
                return Forbid();
            }
            
            var timesheet = await _timesheetService.GetTimesheetByWeekAsync(employeeId, weekStartDate);
            
            if (timesheet == null)
                return NotFound("Timesheet not found for the specified week.");
            
            return Ok(timesheet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timesheet for employee {EmployeeId} and week {WeekStartDate}", employeeId, weekStartDate);
            return StatusCode(500, "An error occurred while retrieving the timesheet.");
        }
    }
    
    /// <summary>
    /// Creates or updates a draft timesheet
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TimesheetDetailDto>> CreateOrUpdateTimesheet([FromBody] TimesheetUpdateDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Verify that the employee is creating/updating their own timesheet
            if (dto.EmployeeId != currentUserId)
            {
                return Forbid();
            }
            
            var result = await _timesheetService.CreateOrUpdateTimesheetAsync(dto);
            return Ok(result);
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
            _logger.LogError(ex, "Error creating/updating timesheet for employee {EmployeeId}", dto.EmployeeId);
            return StatusCode(500, "An error occurred while processing the timesheet.");
        }
    }
    
    /// <summary>
    /// Submits a draft timesheet for manager review
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult> SubmitTimesheet([FromBody] SubmitTimesheetDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            var success = await _timesheetService.SubmitTimesheetAsync(dto.TimesheetId, currentUserId);
            
            if (success)
                return Ok(new { message = "Timesheet submitted successfully." });
            
            return BadRequest("Failed to submit timesheet.");
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
            _logger.LogError(ex, "Error submitting timesheet {TimesheetId}", dto.TimesheetId);
            return StatusCode(500, "An error occurred while submitting the timesheet.");
        }
    }
    
    /// <summary>
    /// Gets timesheet history for employee
    /// </summary>
    [HttpGet("history/{employeeId}")]
    public async Task<ActionResult<List<TimesheetSummaryDto>>> GetTimesheetHistory(Guid employeeId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Check authorization
            if (!IsAuthorizedToView(currentUserId, employeeId))
            {
                return Forbid();
            }
            
            var history = await _timesheetService.GetTimesheetHistoryAsync(employeeId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timesheet history for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving timesheet history.");
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
    
    private bool IsAuthorizedToView(Guid currentUserId, Guid employeeId)
    {
        // Employee can view their own timesheet
        if (currentUserId == employeeId)
            return true;
        
        // Manager can view their employees' timesheets
        // This is simplified - in production, you'd check if current user is the employee's manager
        if (User.IsInRole("Manager"))
            return true;
        
        return false;
    }
}
