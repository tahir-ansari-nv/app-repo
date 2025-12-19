using Microsoft.Extensions.Configuration;
using Moq;
using TimeSheetPortal.Infrastructure.Services;
using Xunit;

namespace TimeSheetPortal.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _service;
    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "ThisIsATestSecretKeyForJWTTokenGeneration123456789"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpiryMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _service = new TokenService(_configuration);
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var token = _service.GenerateJwtToken(userId, username, email);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var token = _service.GenerateJwtToken(userId, username, email);

        var principal = _service.ValidateToken(token);

        Assert.NotNull(principal);
        Assert.NotNull(principal.Identity);
        Assert.True(principal.Identity.IsAuthenticated);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        var invalidToken = "invalid.token.here";

        var principal = _service.ValidateToken(invalidToken);

        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "ThisIsATestSecretKeyForJWTTokenGeneration123456789"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpiryMinutes", "-1"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var service = new TokenService(configuration);

        var userId = Guid.NewGuid();
        var token = service.GenerateJwtToken(userId, "testuser", "test@example.com");

        var principal = service.ValidateToken(token);

        Assert.Null(principal);
    }
}
