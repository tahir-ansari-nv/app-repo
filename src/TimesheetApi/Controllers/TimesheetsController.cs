using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApi.DTOs;
using TimesheetApi.Services;

namespace TimesheetApi.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Employee")]
public class TimesheetsController : BaseController
{
    private readonly ITimesheetService _timesheetService;
    private readonly ILogger<TimesheetsController> _logger;

    public TimesheetsController(
        ITimesheetService timesheetService,
        ILogger<TimesheetsController> logger)
    {
        _timesheetService = timesheetService;
        _logger = logger;
    }

    /// <summary>
    /// Get timesheet for a specific week
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TimesheetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimesheetResponse>> GetTimesheet([FromQuery] DateTime weekStartDate)
    {
        try
        {
            // Validate that the date is a Monday
            if (weekStartDate.DayOfWeek != DayOfWeek.Monday)
            {
                return BadRequest(new { error = "Week start date must be a Monday" });
            }

            var userId = GetCurrentUserId();
            var timesheet = await _timesheetService.GetTimesheetAsync(userId, weekStartDate);

            if (timesheet == null)
            {
                return NotFound(new { error = "No timesheet found for the specified week" });
            }

            return Ok(timesheet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving timesheet for week {WeekStartDate}", weekStartDate);
            return StatusCode(500, new { error = "An error occurred while retrieving the timesheet" });
        }
    }

    /// <summary>
    /// Create or update a draft timesheet
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TimesheetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TimesheetResponse>> CreateOrUpdateTimesheet([FromBody] CreateOrUpdateTimesheetRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var timesheet = await _timesheetService.CreateOrUpdateTimesheetAsync(userId, request);
            return Ok(timesheet);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating timesheet");
            return StatusCode(500, new { error = "An error occurred while saving the timesheet" });
        }
    }

    /// <summary>
    /// Submit a timesheet for review
    /// </summary>
    [HttpPost("{timesheetId}/submit")]
    [ProducesResponseType(typeof(TimesheetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimesheetResponse>> SubmitTimesheet(Guid timesheetId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var timesheet = await _timesheetService.SubmitTimesheetAsync(userId, timesheetId);
            return Ok(timesheet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting timesheet {TimesheetId}", timesheetId);
            return StatusCode(500, new { error = "An error occurred while submitting the timesheet" });
        }
    }
}
