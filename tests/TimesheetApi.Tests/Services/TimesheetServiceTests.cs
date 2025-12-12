using Moq;
using TimesheetApi.DTOs;
using TimesheetApi.Models;
using TimesheetApi.Repositories;
using TimesheetApi.Services;
using Xunit;

namespace TimesheetApi.Tests.Services;

public class TimesheetServiceTests
{
    private readonly Mock<ITimesheetRepository> _mockTimesheetRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly TimesheetService _timesheetService;

    public TimesheetServiceTests()
    {
        _mockTimesheetRepository = new Mock<ITimesheetRepository>();
        _mockAuditService = new Mock<IAuditService>();
        _mockNotificationService = new Mock<INotificationService>();
        _timesheetService = new TimesheetService(
            _mockTimesheetRepository.Object,
            _mockAuditService.Object,
            _mockNotificationService.Object);
    }

    [Fact]
    public async Task GetTimesheetAsync_ReturnsNull_WhenTimesheetDoesNotExist()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var weekStartDate = new DateTime(2024, 6, 3); // Monday
        _mockTimesheetRepository.Setup(r => r.GetByEmployeeAndWeekAsync(employeeId, weekStartDate, It.IsAny<bool>()))
            .ReturnsAsync((Timesheet?)null);

        // Act
        var result = await _timesheetService.GetTimesheetAsync(employeeId, weekStartDate);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrUpdateTimesheetAsync_CreatesNewTimesheet_WhenNoneExists()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var weekStartDate = new DateTime(2024, 6, 3);
        var request = new CreateOrUpdateTimesheetRequest
        {
            WeekStartDate = weekStartDate,
            Entries = new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto
                {
                    ProjectCode = "PRJ001",
                    TaskDescription = "Development",
                    Date = weekStartDate,
                    Hours = 8.0m
                }
            }
        };

        _mockTimesheetRepository.Setup(r => r.GetByEmployeeAndWeekAsync(employeeId, weekStartDate, It.IsAny<bool>()))
            .ReturnsAsync((Timesheet?)null);
        _mockTimesheetRepository.Setup(r => r.CreateAsync(It.IsAny<Timesheet>()))
            .ReturnsAsync((Timesheet t) => t);

        // Act
        var result = await _timesheetService.CreateOrUpdateTimesheetAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(weekStartDate, result.WeekStartDate);
        Assert.Single(result.Entries);
        _mockTimesheetRepository.Verify(r => r.CreateAsync(It.IsAny<Timesheet>()), Times.Once);
        _mockAuditService.Verify(a => a.LogAsync("Timesheet", It.IsAny<Guid>(), AuditAction.Created, employeeId, null), Times.Once);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsException_WhenTimesheetNotFound()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var timesheetId = Guid.NewGuid();
        _mockTimesheetRepository.Setup(r => r.GetByIdAsync(timesheetId, It.IsAny<bool>()))
            .ReturnsAsync((Timesheet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _timesheetService.SubmitTimesheetAsync(employeeId, timesheetId));
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsException_WhenTimesheetIsEmpty()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var timesheetId = Guid.NewGuid();
        var timesheet = new Timesheet
        {
            Id = timesheetId,
            EmployeeId = employeeId,
            WeekStartDate = new DateTime(2024, 6, 3),
            Status = TimesheetStatus.Draft,
            Entries = new List<TimesheetEntry>()
        };

        _mockTimesheetRepository.Setup(r => r.GetByIdAsync(timesheetId, It.IsAny<bool>()))
            .ReturnsAsync(timesheet);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _timesheetService.SubmitTimesheetAsync(employeeId, timesheetId));
    }

    [Fact]
    public async Task SubmitTimesheetAsync_Success_WhenValidTimesheet()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var timesheetId = Guid.NewGuid();
        var timesheet = new Timesheet
        {
            Id = timesheetId,
            EmployeeId = employeeId,
            WeekStartDate = new DateTime(2024, 6, 3),
            Status = TimesheetStatus.Draft,
            Entries = new List<TimesheetEntry>
            {
                new TimesheetEntry
                {
                    ProjectCode = "PRJ001",
                    Date = new DateTime(2024, 6, 3),
                    Hours = 8.0m
                }
            }
        };

        _mockTimesheetRepository.Setup(r => r.GetByIdAsync(timesheetId, It.IsAny<bool>()))
            .ReturnsAsync(timesheet);
        _mockTimesheetRepository.Setup(r => r.UpdateAsync(It.IsAny<Timesheet>()))
            .ReturnsAsync((Timesheet t) => t);

        // Act
        var result = await _timesheetService.SubmitTimesheetAsync(employeeId, timesheetId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Submitted", result.Status);
        _mockTimesheetRepository.Verify(r => r.UpdateAsync(It.IsAny<Timesheet>()), Times.Once);
        _mockAuditService.Verify(a => a.LogAsync("Timesheet", timesheetId, AuditAction.Submitted, employeeId, null), Times.Once);
    }

    [Fact]
    public async Task ApproveTimesheetAsync_Success_WhenTimesheetIsSubmitted()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var timesheetId = Guid.NewGuid();
        var timesheet = new Timesheet
        {
            Id = timesheetId,
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = new DateTime(2024, 6, 3),
            Status = TimesheetStatus.Submitted,
            Employee = new Employee { Name = "Test User", Email = "test@test.com" },
            Entries = new List<TimesheetEntry>
            {
                new TimesheetEntry
                {
                    ProjectCode = "PRJ001",
                    Date = new DateTime(2024, 6, 3),
                    Hours = 8.0m
                }
            }
        };

        _mockTimesheetRepository.Setup(r => r.GetByIdAsync(timesheetId, It.IsAny<bool>()))
            .ReturnsAsync(timesheet);
        _mockTimesheetRepository.Setup(r => r.UpdateAsync(It.IsAny<Timesheet>()))
            .ReturnsAsync((Timesheet t) => t);

        // Act
        var result = await _timesheetService.ApproveTimesheetAsync(timesheetId, managerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
        _mockTimesheetRepository.Verify(r => r.UpdateAsync(It.IsAny<Timesheet>()), Times.Once);
        _mockAuditService.Verify(a => a.LogAsync("Timesheet", timesheetId, AuditAction.Approved, managerId, null), Times.Once);
    }

    [Fact]
    public async Task RejectTimesheetAsync_SendsNotification_WhenSuccessful()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var timesheetId = Guid.NewGuid();
        var reason = "Missing task descriptions";
        var timesheet = new Timesheet
        {
            Id = timesheetId,
            EmployeeId = employeeId,
            WeekStartDate = new DateTime(2024, 6, 3),
            Status = TimesheetStatus.Submitted,
            Employee = new Employee { Name = "Test User", Email = "test@test.com" },
            Entries = new List<TimesheetEntry>
            {
                new TimesheetEntry
                {
                    ProjectCode = "PRJ001",
                    Date = new DateTime(2024, 6, 3),
                    Hours = 8.0m
                }
            }
        };

        _mockTimesheetRepository.Setup(r => r.GetByIdAsync(timesheetId, It.IsAny<bool>()))
            .ReturnsAsync(timesheet);
        _mockTimesheetRepository.Setup(r => r.UpdateAsync(It.IsAny<Timesheet>()))
            .ReturnsAsync((Timesheet t) => t);

        // Act
        var result = await _timesheetService.RejectTimesheetAsync(timesheetId, managerId, reason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rejected", result.Status);
        Assert.Equal(reason, result.ManagerDecisionReason);
        _mockTimesheetRepository.Verify(r => r.UpdateAsync(It.IsAny<Timesheet>()), Times.Once);
        _mockAuditService.Verify(a => a.LogAsync("Timesheet", timesheetId, AuditAction.Rejected, managerId, $"Reason: {reason}"), Times.Once);
        _mockNotificationService.Verify(n => n.SendTimesheetRejectionNotificationAsync(employeeId, timesheetId, reason), Times.Once);
    }
}
