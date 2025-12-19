using TimeSheetPortal.Core.Entities;

namespace TimeSheetPortal.Core.Interfaces;

public interface IMFACodeRepository
{
    Task<MFACode?> GetByUserIdAndCodeAsync(Guid userId, string code);
    Task<MFACode> CreateAsync(MFACode mfaCode);
    Task UpdateAsync(MFACode mfaCode);
    Task DeleteExpiredCodesAsync();
}
