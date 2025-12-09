using LoginApi.Data;
using LoginApi.Models;
using LoginApi.Repositories;
using LoginApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LoginApi.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<ILoginAttemptRepository> _mockLoginAttemptRepository;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _mockLoginAttemptRepository = new Mock<ILoginAttemptRepository>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenGenerator.Object,
            _mockLoginAttemptRepository.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword",
            IsActive = true
        };
        var expectedToken = "jwt-token";
        var expectedExpiration = DateTimeOffset.UtcNow.AddMinutes(60);

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email.ToLowerInvariant()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, password))
            .Returns(PasswordVerificationResult.Success);
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(user))
            .Returns((expectedToken, expectedExpiration));

        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(expectedExpiration, result.Expiration);
        _mockLoginAttemptRepository.Verify(r => r.AddAsync(It.Is<LoginAttempt>(
            a => a.Email == email && a.Success == true && a.FailureReason == null)), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email.ToLowerInvariant()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        _mockLoginAttemptRepository.Verify(r => r.AddAsync(It.Is<LoginAttempt>(
            a => a.Email == email && a.Success == false && a.FailureReason == "User not found or inactive")), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveUser_ReturnsFailure()
    {
        // Arrange
        var email = "inactive@example.com";
        var password = "password";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword",
            IsActive = false
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email.ToLowerInvariant()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        _mockLoginAttemptRepository.Verify(r => r.AddAsync(It.Is<LoginAttempt>(
            a => a.Email == email && a.Success == false && a.FailureReason == "User not found or inactive")), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email.ToLowerInvariant()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, password))
            .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        _mockLoginAttemptRepository.Verify(r => r.AddAsync(It.Is<LoginAttempt>(
            a => a.Email == email && a.Success == false && a.FailureReason == "Invalid password")), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenLoggingFails_StillReturnsSuccessForValidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword",
            IsActive = true
        };
        var expectedToken = "jwt-token";
        var expectedExpiration = DateTimeOffset.UtcNow.AddMinutes(60);

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email.ToLowerInvariant()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, password))
            .Returns(PasswordVerificationResult.Success);
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(user))
            .Returns((expectedToken, expectedExpiration));
        _mockLoginAttemptRepository.Setup(r => r.AddAsync(It.IsAny<LoginAttempt>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedToken, result.Token);
    }
}
