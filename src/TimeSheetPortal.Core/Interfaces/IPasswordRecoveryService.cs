namespace TimeSheetPortal.Core.Interfaces;

public interface IPasswordRecoveryService
{
    Task<string> GenerateRecoveryTokenAsync(string email);
    Task<bool> ValidateAndResetPasswordAsync(string token, string newPassword);
}
