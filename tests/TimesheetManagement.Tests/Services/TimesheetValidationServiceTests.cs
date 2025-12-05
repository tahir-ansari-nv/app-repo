using Xunit;
using FluentAssertions;
using TimesheetManagement.Services;
using TimesheetManagement.Models.DTOs;
using TimesheetManagement.Models.Entities;
using TimesheetManagement.Models.Enums;

namespace TimesheetManagement.Tests.Services;

public class TimesheetValidationServiceTests
{
    private readonly TimesheetValidationService _validationService;

    public TimesheetValidationServiceTests()
    {
        _validationService = new TimesheetValidationService();
    }

    [Fact]
    public void ValidateTimesheetUpdate_WithValidData_ReturnsValid()
    {
        // Arrange
        var dto = new TimesheetUpdateDto
        {
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = GetMonday(DateTime.UtcNow),
            Entries = new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto
                {
                    ProjectName = "Project A",
                    TaskName = "Development",
                    Date = GetMonday(DateTime.UtcNow),
                    Hours = 8.0m
                }
            }
        };

        // Act
        var result = _validationService.ValidateTimesheetUpdate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateTimesheetUpdate_WithNonMondayStartDate_ReturnsInvalid()
    {
        // Arrange
        var dto = new TimesheetUpdateDto
        {
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = DateTime.UtcNow.Date, // Not necessarily a Monday
            Entries = new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto
                {
                    ProjectName = "Project A",
                    TaskName = "Development",
                    Date = DateTime.UtcNow.Date,
                    Hours = 8.0m
                }
            }
        };

        // Only test if the date is not a Monday
        if (dto.WeekStartDate.DayOfWeek != DayOfWeek.Monday)
        {
            // Act
            var result = _validationService.ValidateTimesheetUpdate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Monday"));
        }
    }

    [Fact]
    public void ValidateTimesheetUpdate_WithHoursExceeding24_ReturnsInvalid()
    {
        // Arrange
        var dto = new TimesheetUpdateDto
        {
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = GetMonday(DateTime.UtcNow),
            Entries = new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto
                {
                    ProjectName = "Project A",
                    TaskName = "Development",
                    Date = GetMonday(DateTime.UtcNow),
                    Hours = 25.0m // Invalid: exceeds 24 hours
                }
            }
        };

        // Act
        var result = _validationService.ValidateTimesheetUpdate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Hours must be between 0 and 24"));
    }

    [Fact]
    public void ValidateTimesheetUpdate_WithTotalHoursExceeding100_ReturnsInvalid()
    {
        // Arrange
        var monday = GetMonday(DateTime.UtcNow);
        var dto = new TimesheetUpdateDto
        {
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = monday,
            Entries = new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto { ProjectName = "Project A", TaskName = "Task 1", Date = monday, Hours = 24m },
                new TimesheetEntryDto { ProjectName = "Project A", TaskName = "Task 2", Date = monday.AddDays(1), Hours = 24m },
                new TimesheetEntryDto { ProjectName = "Project A", TaskName = "Task 3", Date = monday.AddDays(2), Hours = 24m },
                new TimesheetEntryDto { ProjectName = "Project A", TaskName = "Task 4", Date = monday.AddDays(3), Hours = 24m },
                new TimesheetEntryDto { ProjectName = "Project A", TaskName = "Task 5", Date = monday.AddDays(4), Hours = 5m } // Total: 101
            }
        };

        // Act
        var result = _validationService.ValidateTimesheetUpdate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("exceed the maximum allowed (100 hours per week)"));
    }

    [Fact]
    public void ValidateTimesheetUpdate_WithMissingProjectName_ReturnsInvalid()
    {
        // Arrange
        var dto = new TimesheetUpdateDto
        {
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = GetMonday(DateTime.UtcNow),
            Entries = new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto
                {
                    ProjectName = "", // Missing
                    TaskName = "Development",
                    Date = GetMonday(DateTime.UtcNow),
                    Hours = 8.0m
                }
            }
        };

        // Act
        var result = _validationService.ValidateTimesheetUpdate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ProjectName is required"));
    }

    [Fact]
    public void ValidateTimesheetForSubmission_WithValidTimesheet_ReturnsValid()
    {
        // Arrange
        var monday = GetMonday(DateTime.UtcNow);
        var timesheet = new Timesheet
        {
            TimesheetId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = monday,
            WeekEndDate = monday.AddDays(6),
            Status = TimesheetStatus.Draft,
            Entries = new List<TimesheetEntry>
            {
                new TimesheetEntry
                {
                    TimesheetEntryId = Guid.NewGuid(),
                    ProjectName = "Project A",
                    TaskName = "Development",
                    Date = monday,
                    Hours = 8.0m
                }
            }
        };

        // Act
        var result = _validationService.ValidateTimesheetForSubmission(timesheet);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateTimesheetForSubmission_WithNoEntries_ReturnsInvalid()
    {
        // Arrange
        var monday = GetMonday(DateTime.UtcNow);
        var timesheet = new Timesheet
        {
            TimesheetId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = monday,
            WeekEndDate = monday.AddDays(6),
            Status = TimesheetStatus.Draft,
            Entries = new List<TimesheetEntry>() // Empty
        };

        // Act
        var result = _validationService.ValidateTimesheetForSubmission(timesheet);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least one entry"));
    }

    [Fact]
    public void ValidateTimesheetForSubmission_WithExactly100Hours_ReturnsValid()
    {
        // Arrange
        var monday = GetMonday(DateTime.UtcNow);
        var timesheet = new Timesheet
        {
            TimesheetId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            WeekStartDate = monday,
            WeekEndDate = monday.AddDays(6),
            Status = TimesheetStatus.Draft,
            Entries = new List<TimesheetEntry>
            {
                new TimesheetEntry { ProjectName = "P1", TaskName = "T1", Date = monday, Hours = 20m },
                new TimesheetEntry { ProjectName = "P1", TaskName = "T2", Date = monday.AddDays(1), Hours = 20m },
                new TimesheetEntry { ProjectName = "P1", TaskName = "T3", Date = monday.AddDays(2), Hours = 20m },
                new TimesheetEntry { ProjectName = "P1", TaskName = "T4", Date = monday.AddDays(3), Hours = 20m },
                new TimesheetEntry { ProjectName = "P1", TaskName = "T5", Date = monday.AddDays(4), Hours = 20m }
            }
        };

        // Act
        var result = _validationService.ValidateTimesheetForSubmission(timesheet);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private DateTime GetMonday(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }
}
