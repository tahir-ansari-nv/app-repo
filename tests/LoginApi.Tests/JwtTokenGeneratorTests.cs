using LoginApi.Models;
using LoginApi.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LoginApi.Tests;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ReturnsValidTokenAndExpiration()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "ThisIsAVerySecretKeyThatIsAtLeast32CharactersLong123456"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationMinutes", "60"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var tokenGenerator = new JwtTokenGenerator(configuration);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        // Act
        var result = tokenGenerator.GenerateToken(user);

        // Assert
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.True(result.Expiration > DateTimeOffset.UtcNow);
        Assert.True(result.Expiration <= DateTimeOffset.UtcNow.AddMinutes(61));
    }

    [Fact]
    public void GenerateToken_WithoutSecret_ThrowsException()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationMinutes", "60"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var tokenGenerator = new JwtTokenGenerator(configuration);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => tokenGenerator.GenerateToken(user));
    }
}
