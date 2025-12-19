using TimeSheetPortal.Core.Entities;
using TimeSheetPortal.Core.Interfaces;

namespace TimeSheetPortal.Infrastructure.Services;

public class MFAService : IMFAService
{
    private readonly IMFACodeRepository _mfaCodeRepository;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;

    public MFAService(
        IMFACodeRepository mfaCodeRepository,
        IEmailService emailService,
        IUserRepository userRepository)
    {
        _mfaCodeRepository = mfaCodeRepository;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task<string> GenerateMFACodeAsync(Guid userId)
    {
        var code = GenerateRandomCode();
        var mfaCode = new MFACode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code,
            Expiry = DateTimeOffset.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _mfaCodeRepository.CreateAsync(mfaCode);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            await _emailService.SendMFACodeEmailAsync(user.Email, code);
        }

        return code;
    }

    public async Task<bool> ValidateMFACodeAsync(Guid userId, string code)
    {
        var mfaCode = await _mfaCodeRepository.GetByUserIdAndCodeAsync(userId, code);
        
        if (mfaCode == null || mfaCode.IsUsed || mfaCode.Expiry < DateTimeOffset.UtcNow)
        {
            return false;
        }

        mfaCode.IsUsed = true;
        await _mfaCodeRepository.UpdateAsync(mfaCode);

        return true;
    }

    private static string GenerateRandomCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
