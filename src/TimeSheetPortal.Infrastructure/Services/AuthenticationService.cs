using TimeSheetPortal.Core.Entities;
using TimeSheetPortal.Core.Interfaces;

namespace TimeSheetPortal.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<(bool Success, User? User, bool RequiresMFA)> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            return (false, null, false);
        }

        if (user.IsLocked && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return (false, null, false);
        }

        if (user.IsLocked && user.LockoutEnd.HasValue && user.LockoutEnd.Value <= DateTimeOffset.UtcNow)
        {
            user.IsLocked = false;
            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);
        }

        if (!_passwordHashingService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            await IncrementFailedLoginAttemptsAsync(user.Id);
            return (false, null, false);
        }

        if (user.MFAEnabled)
        {
            return (true, user, true);
        }

        await ResetFailedLoginAttemptsAsync(user.Id);
        return (true, user, false);
    }

    public async Task<bool> ValidateMFACodeAsync(Guid userId, string code)
    {
        return true;
    }

    public async Task ResetFailedLoginAttemptsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockoutEnd = null;
            await _userRepository.UpdateAsync(user);
        }
    }

    public async Task IncrementFailedLoginAttemptsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.FailedLoginAttempts++;
            
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.IsLocked = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(LockoutMinutes);
            }

            await _userRepository.UpdateAsync(user);
        }
    }
}
