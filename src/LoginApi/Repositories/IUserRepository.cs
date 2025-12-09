using LoginApi.Models;

namespace LoginApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail);
}
