using TimeSheetPortal.Core.Entities;

namespace TimeSheetPortal.Core.Interfaces;

public interface IPasswordRecoveryTokenRepository
{
    Task<PasswordRecoveryToken?> GetByTokenAsync(string token);
    Task<PasswordRecoveryToken> CreateAsync(PasswordRecoveryToken recoveryToken);
    Task UpdateAsync(PasswordRecoveryToken recoveryToken);
    Task DeleteExpiredTokensAsync();
}
