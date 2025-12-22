namespace timesheet_api.Auth.Services;

public interface IAuthService
{
    Task AuthenticateAsync(string identifier, string password, bool rememberMe, string ip, string userAgent);
    Task SignOutAsync(Guid userId, string sessionId);
}
