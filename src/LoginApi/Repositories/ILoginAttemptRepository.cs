using LoginApi.Models;

namespace LoginApi.Repositories;

public interface ILoginAttemptRepository
{
    Task AddAsync(LoginAttempt attempt);
    Task<int> GetFailedAttemptCountAsync(string email, string ipAddress, DateTimeOffset since);
}
