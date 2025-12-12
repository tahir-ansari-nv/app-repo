using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApi.DTOs;
using TimesheetApi.Models;
using TimesheetApi.Services;

namespace TimesheetApi.Controllers;

[Route("api/manager/timesheets")]
[Authorize(Roles = "Manager")]
public class ManagerReviewController : BaseController
{
    private readonly ITimesheetService _timesheetService;
    private readonly ILogger<ManagerReviewController> _logger;

    public ManagerReviewController(
        ITimesheetService timesheetService,
        ILogger<ManagerReviewController> logger)
    {
        _timesheetService = timesheetService;
        _logger = logger;
    }

    /// <summary>
    /// Get all submitted timesheets for review
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TimesheetSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TimesheetSummaryDto>>> GetSubmittedTimesheets([FromQuery] string? status = "Submitted")
    {
        try
        {
            // For now, only support "Submitted" status
            var timesheets = await _timesheetService.GetSubmittedTimesheetsAsync();
            return Ok(timesheets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving submitted timesheets");
            return StatusCode(500, new { error = "An error occurred while retrieving timesheets" });
        }
    }

    /// <summary>
    /// Approve a timesheet
    /// </summary>
    [HttpPost("{timesheetId}/approve")]
    [ProducesResponseType(typeof(TimesheetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimesheetResponse>> ApproveTimesheet(Guid timesheetId)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var timesheet = await _timesheetService.ApproveTimesheetAsync(timesheetId, managerId);
            return Ok(timesheet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving timesheet {TimesheetId}", timesheetId);
            return StatusCode(500, new { error = "An error occurred while approving the timesheet" });
        }
    }

    /// <summary>
    /// Reject a timesheet with a reason
    /// </summary>
    [HttpPost("{timesheetId}/reject")]
    [ProducesResponseType(typeof(TimesheetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimesheetResponse>> RejectTimesheet(
        Guid timesheetId,
        [FromBody] RejectTimesheetRequest request)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var timesheet = await _timesheetService.RejectTimesheetAsync(timesheetId, managerId, request.Reason);
            return Ok(timesheet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting timesheet {TimesheetId}", timesheetId);
            return StatusCode(500, new { error = "An error occurred while rejecting the timesheet" });
        }
    }
}
