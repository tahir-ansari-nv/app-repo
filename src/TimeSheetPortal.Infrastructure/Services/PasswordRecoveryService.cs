using TimeSheetPortal.Core.Entities;
using TimeSheetPortal.Core.Interfaces;

namespace TimeSheetPortal.Infrastructure.Services;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordRecoveryTokenRepository _tokenRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IEmailService _emailService;

    public PasswordRecoveryService(
        IUserRepository userRepository,
        IPasswordRecoveryTokenRepository tokenRepository,
        IPasswordHashingService passwordHashingService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _passwordHashingService = passwordHashingService;
        _emailService = emailService;
    }

    public async Task<string> GenerateRecoveryTokenAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return string.Empty;
        }

        var token = Guid.NewGuid().ToString();
        var recoveryToken = new PasswordRecoveryToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            Expiry = DateTimeOffset.UtcNow.AddHours(1),
            IsUsed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _tokenRepository.CreateAsync(recoveryToken);
        await _emailService.SendPasswordRecoveryEmailAsync(email, token);

        return token;
    }

    public async Task<bool> ValidateAndResetPasswordAsync(string token, string newPassword)
    {
        var recoveryToken = await _tokenRepository.GetByTokenAsync(token);
        
        if (recoveryToken == null || 
            recoveryToken.IsUsed || 
            recoveryToken.Expiry < DateTimeOffset.UtcNow)
        {
            return false;
        }

        var user = recoveryToken.User;
        var (hash, salt) = _passwordHashingService.HashPassword(newPassword);
        
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEnd = null;

        recoveryToken.IsUsed = true;

        await _userRepository.UpdateAsync(user);
        await _tokenRepository.UpdateAsync(recoveryToken);

        return true;
    }
}
