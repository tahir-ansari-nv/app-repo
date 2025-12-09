using LoginApi.Models;

namespace LoginApi.Services;

public interface ITokenService
{
    string GenerateToken(User user, out DateTimeOffset expiresAt);
}
