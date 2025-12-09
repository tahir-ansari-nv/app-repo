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

    public async Task AddAsync(LoginAttempt loginAttempt)
    {
        await _context.LoginAttempts.AddAsync(loginAttempt);
        await _context.SaveChangesAsync();
    }
}
