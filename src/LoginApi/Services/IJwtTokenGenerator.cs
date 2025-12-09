using LoginApi.Models;

namespace LoginApi.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset Expiration) GenerateToken(User user);
}
