using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TimesheetApi.DTOs;
using TimesheetApi.Models;
using TimesheetApi.Persistence;
using TimesheetApi.Services;
using Xunit;

public class TimesheetServiceTests
{
    [Fact]
    public async Task AddEntryAsync_ShouldReturnSuccess_WhenRequestIsValidAndNotDuplicate()
    {
        // Arrange
        var repoMock = new Mock<ITimesheetRepository>();
        repoMock.Setup(r => r.IsDuplicateEntryAsync(It.IsAny<TimesheetEntry>()))
            .ReturnsAsync(false);
        repoMock.Setup(r => r.AddAsync(It.IsAny<TimesheetEntry>()))
            .Returns(Task.CompletedTask);

        var service = new TimesheetService(repoMock.Object);
        var request = new TimesheetEntryRequest
        {
            Date = DateTime.UtcNow.Date,
            Project = "ProjectX",
            Description = "Worked on task",
            HoursWorked = 8
        };
        var userId = "user-1";

        // Act
        var result = await service.AddEntryAsync(request, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Timesheet entry logged successfully.", result.Message);
        Assert.NotNull(result.Entry);
        Assert.Equal(request.Date.Value, result.Entry.Date);
        Assert.Equal(request.Project, result.Entry.Project);
        Assert.Equal(request.Description, result.Entry.Description);
        Assert.Equal(request.HoursWorked.Value, result.Entry.HoursWorked);
    }

    [Fact]
    public async Task AddEntryAsync_ShouldReturnError_WhenRequestHasMissingFields()
    {
        // Arrange
        var repoMock = new Mock<ITimesheetRepository>();
        var service = new TimesheetService(repoMock.Object);
        var request = new TimesheetEntryRequest
        {
            Date = null,
            Project = null,
            Description = null,
            HoursWorked = null
        };
        var userId = "user-2";

        // Act
        var result = await service.AddEntryAsync(request, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Date is required.", result.Message);
        Assert.Contains("Project is required.", result.Message);
        Assert.Contains("Description is required.", result.Message);
        Assert.Contains("Hours Worked is required.", result.Message);
        Assert.Null(result.Entry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task AddEntryAsync_ShouldReturnError_WhenUserIdIsMissingOrWhitespace(string userId)
    {
        // Arrange
        var repoMock = new Mock<ITimesheetRepository>();
        var service = new TimesheetService(repoMock.Object);
        var request = new TimesheetEntryRequest
        {
            Date = DateTime.UtcNow.Date,
            Project = "ProjectZ",
            Description = "Test Desc",
            HoursWorked = 2
        };

        // Act
        var result = await service.AddEntryAsync(request, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid user identifier.", result.Message);
        Assert.Null(result.Entry);
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(24.1)]
    public async Task AddEntryAsync_ShouldReturnError_WhenHoursWorkedOutOfRange(double hours)
    {
        // Arrange
        var repoMock = new Mock<ITimesheetRepository>();
        var service = new TimesheetService(repoMock.Object);
        var request = new TimesheetEntryRequest
        {
            Date = DateTime.UtcNow.Date,
            Project = "ProjectY",
            Description = "Worked on stuff",
            HoursWorked = hours
        };

        // Act
        var result = await service.AddEntryAsync(request, "user-5");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Hours Worked must be between 0.5 and 24.", result.Message);
    }

    [Fact]
    public async Task AddEntryAsync_ShouldReturnError_WhenDuplicateDetected()
    {
        // Arrange
        var repoMock = new Mock<ITimesheetRepository>();
        repoMock.Setup(r => r.IsDuplicateEntryAsync(It.IsAny<TimesheetEntry>()))
            .ReturnsAsync(true);

        var service = new TimesheetService(repoMock.Object);
        var request = new TimesheetEntryRequest
        {
            Date = DateTime.UtcNow.Date,
            Project = "ProjDup",
            Description = "Dup Test",
            HoursWorked = 1
        };

        // Act
        var result = await service.AddEntryAsync(request, "uDup");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Duplicate entry detected", result.Message);
        Assert.Null(result.Entry);
    }

    [Fact]
    public async Task AddEntryAsync_ShouldReturnError_WhenRepositoryThrowsException()
    {
        // Arrange
        var repoMock = new Mock<ITimesheetRepository>();
        repoMock.Setup(r => r.IsDuplicateEntryAsync(It.IsAny<TimesheetEntry>()))
            .ReturnsAsync(false);
        repoMock.Setup(r => r.AddAsync(It.IsAny<TimesheetEntry>()))
            .ThrowsAsync(new Exception("DB Save error"));

        var service = new TimesheetService(repoMock.Object);
        var request = new TimesheetEntryRequest
        {
            Date = DateTime.UtcNow.Date,
            Project = "ProjectE",
            Description = "Fail Save",
            HoursWorked = 2
        };

        // Act
        var result = await service.AddEntryAsync(request, "user-err");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to save entry: DB Save error", result.Message);
        Assert.Null(result.Entry);
    }

    [Fact]
    public async Task GetEntriesForUserAsync_ShouldReturnMappedDtos()
    {
        // Arrange
        var entryList = new List<TimesheetEntry>
        {
            new TimesheetEntry
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date,
                Project = "P1",
                Description = "Desc1",
                HoursWorked = 2,
                UserId = "userX",
                Timestamp = DateTime.UtcNow
            },
            new TimesheetEntry
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date.AddDays(-1),
                Project = "P2",
                Description = "Desc2",
                HoursWorked = 5,
                UserId = "userX",
                Timestamp = DateTime.UtcNow.AddDays(-1)
            }
        };

        var repoMock = new Mock<ITimesheetRepository>();
        repoMock.Setup(r => r.GetByUserAsync("userX"))
            .ReturnsAsync(entryList);

        var service = new TimesheetService(repoMock.Object);

        // Act
        var result = await service.GetEntriesForUserAsync("userX");

        // Assert
        Assert.Equal(entryList.Count, result.Count);
        Assert.Equal(entryList[0].Id, result[0].Id);
        Assert.Equal(entryList[1].Project, result[1].Project);
        Assert.Equal(entryList[0].Timestamp, result[0].Timestamp);
    }
}