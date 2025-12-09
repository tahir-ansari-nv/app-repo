using LoginApi.Models;

namespace LoginApi.Repositories;

public interface ILoginAttemptRepository
{
    Task AddAsync(LoginAttempt loginAttempt);
}
