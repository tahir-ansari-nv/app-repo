using Microsoft.EntityFrameworkCore;
using LoginApi.Data;
using LoginApi.Models;

namespace LoginApi.Repositories;

public class LoginAttemptRepository : ILoginAttemptRepository
{
    private readonly ApplicationDbContext _context;

    public LoginAttemptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoginAttempt attempt)
    {
        _context.LoginAttempts.Add(attempt);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetFailedAttemptCountAsync(string email, string ipAddress, DateTimeOffset since)
    {
        return await _context.LoginAttempts
            .Where(la => la.EmailAttempted == email && 
                         la.IPAddress == ipAddress && 
                         !la.Success && 
                         la.Timestamp >= since)
            .CountAsync();
    }
}
