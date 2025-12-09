using LoginApi.Models;
using LoginApi.Repositories;

namespace LoginApi.Services;

public class AuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILoginAttemptRepository _loginAttemptRepository;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly int _maxFailedAttempts = 5;
    private readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(15);

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILoginAttemptRepository loginAttemptRepository,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _loginAttemptRepository = loginAttemptRepository;
        _logger = logger;
    }

    public async Task<(bool Success, string? Token, DateTimeOffset? ExpiresAt)> LoginAsync(string email, string password, string ipAddress, string userAgent)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        // Check if user exists and is active
        if (user == null || user.IsActive == false)
        {
            await LogLoginAttempt(null, email, ipAddress, userAgent, false);
            return (false, null, null);
        }

        // Check lockout by failed attempts count
        var failedCount = await _loginAttemptRepository.GetFailedAttemptCountAsync(normalizedEmail, ipAddress, DateTimeOffset.UtcNow.Subtract(_lockoutDuration));
        if (failedCount >= _maxFailedAttempts)
        {
            _logger.LogWarning("User {Email} or IP {IP} is locked out due to too many failed attempts.", email, ipAddress);
            await LogLoginAttempt(user.Id, normalizedEmail, ipAddress, userAgent, false);
            return (false, null, null);
        }

        // Validate password
        if (_passwordHasher.VerifyHashedPassword(user.PasswordHash, password))
        {
            var token = _tokenService.GenerateToken(user, out var expiresAt);

            await LogLoginAttempt(user.Id, normalizedEmail, ipAddress, userAgent, true);

            return (true, token, expiresAt);
        }
        else
        {
            await LogLoginAttempt(user.Id, normalizedEmail, ipAddress, userAgent, false);
            return (false, null, null);
        }
    }

    private async Task LogLoginAttempt(Guid? userId, string email, string ipAddress, string userAgent, bool success)
    {
        var attempt = new LoginAttempt
        {
            UserId = userId,
            EmailAttempted = email,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            Timestamp = DateTimeOffset.UtcNow
        };
        await _loginAttemptRepository.AddAsync(attempt);
    }
}
