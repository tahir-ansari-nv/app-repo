using LoginApi.Models;
using LoginApi.Repositories;
using Microsoft.AspNetCore.Identity;

namespace LoginApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILoginAttemptRepository _loginAttemptRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILoginAttemptRepository loginAttemptRepository,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _loginAttemptRepository = loginAttemptRepository;
        _logger = logger;
    }

    public async Task<(bool Success, string? Token, DateTimeOffset Expiration)> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant());
        
        if (user == null || !user.IsActive)
        {
            await LogAttemptAsync(email, false, "User not found or inactive");
            return (false, null, default);
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        
        if (verification == PasswordVerificationResult.Failed)
        {
            await LogAttemptAsync(email, false, "Invalid password");
            return (false, null, default);
        }

        var tokenResult = _jwtTokenGenerator.GenerateToken(user);
        await LogAttemptAsync(email, true, null);

        return (true, tokenResult.Token, tokenResult.Expiration);
    }

    private async Task LogAttemptAsync(string email, bool success, string? failureReason)
    {
        try
        {
            var attempt = new LoginAttempt
            {
                Id = Guid.NewGuid(),
                Email = email,
                Timestamp = DateTimeOffset.UtcNow,
                Success = success,
                FailureReason = failureReason
            };
            await _loginAttemptRepository.AddAsync(attempt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log login attempt for email {Email}", email);
            // Swallow exception to not block login flow
        }
    }
}
