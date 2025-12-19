using TimeSheetPortal.Core.Interfaces;

namespace TimeSheetPortal.Infrastructure.Services;

public class PasswordHashingService : IPasswordHashingService
{
    public (string Hash, string Salt) HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
