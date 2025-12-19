using Microsoft.EntityFrameworkCore;
using TimeSheetPortal.Core.Entities;
using TimeSheetPortal.Core.Interfaces;
using TimeSheetPortal.Infrastructure.Data;

namespace TimeSheetPortal.Infrastructure.Repositories;

public class PasswordRecoveryTokenRepository : IPasswordRecoveryTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordRecoveryTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordRecoveryToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordRecoveryTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<PasswordRecoveryToken> CreateAsync(PasswordRecoveryToken recoveryToken)
    {
        _context.PasswordRecoveryTokens.Add(recoveryToken);
        await _context.SaveChangesAsync();
        return recoveryToken;
    }

    public async Task UpdateAsync(PasswordRecoveryToken recoveryToken)
    {
        _context.PasswordRecoveryTokens.Update(recoveryToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.PasswordRecoveryTokens
            .Where(t => t.Expiry < DateTimeOffset.UtcNow)
            .ToListAsync();

        _context.PasswordRecoveryTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
