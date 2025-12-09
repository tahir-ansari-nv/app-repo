using Microsoft.EntityFrameworkCore;
using LoginApi.Data;
using LoginApi.Models;

namespace LoginApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string normalizedEmail)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
    }
}
