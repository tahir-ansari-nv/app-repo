using TimeSheetPortal.Core.Entities;

namespace TimeSheetPortal.Core.Interfaces;

public interface IAuthenticationService
{
    Task<(bool Success, User? User, bool RequiresMFA)> AuthenticateAsync(string username, string password);
    Task<bool> ValidateMFACodeAsync(Guid userId, string code);
    Task ResetFailedLoginAttemptsAsync(Guid userId);
    Task IncrementFailedLoginAttemptsAsync(Guid userId);
}
