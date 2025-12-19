using Microsoft.EntityFrameworkCore;
using TimeSheetPortal.Core.Entities;
using TimeSheetPortal.Core.Interfaces;
using TimeSheetPortal.Infrastructure.Data;

namespace TimeSheetPortal.Infrastructure.Repositories;

public class MFACodeRepository : IMFACodeRepository
{
    private readonly ApplicationDbContext _context;

    public MFACodeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MFACode?> GetByUserIdAndCodeAsync(Guid userId, string code)
    {
        return await _context.MFACodes
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Code == code);
    }

    public async Task<MFACode> CreateAsync(MFACode mfaCode)
    {
        _context.MFACodes.Add(mfaCode);
        await _context.SaveChangesAsync();
        return mfaCode;
    }

    public async Task UpdateAsync(MFACode mfaCode)
    {
        _context.MFACodes.Update(mfaCode);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredCodesAsync()
    {
        var expiredCodes = await _context.MFACodes
            .Where(m => m.Expiry < DateTimeOffset.UtcNow)
            .ToListAsync();

        _context.MFACodes.RemoveRange(expiredCodes);
        await _context.SaveChangesAsync();
    }
}
