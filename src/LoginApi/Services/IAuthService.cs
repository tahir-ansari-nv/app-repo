namespace LoginApi.Services;

public interface IAuthService
{
    Task<(bool Success, string? Token, DateTimeOffset Expiration)> AuthenticateAsync(string email, string password);
}
