using TimeSheetPortal.Infrastructure.Services;
using Xunit;

namespace TimeSheetPortal.Tests.Services;

public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _service;

    public PasswordHashingServiceTests()
    {
        _service = new PasswordHashingService();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashAndSalt()
    {
        var password = "Test@123";

        var (hash, salt) = _service.HashPassword(password);

        Assert.NotNull(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(hash);
        Assert.NotEmpty(salt);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "Test@123";
        var (hash, salt) = _service.HashPassword(password);

        var result = _service.VerifyPassword(password, hash, salt);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "Test@123";
        var wrongPassword = "Wrong@123";
        var (hash, salt) = _service.HashPassword(password);

        var result = _service.VerifyPassword(wrongPassword, hash, salt);

        Assert.False(result);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashesForSamePassword()
    {
        var password = "Test@123";

        var (hash1, salt1) = _service.HashPassword(password);
        var (hash2, salt2) = _service.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }
}
