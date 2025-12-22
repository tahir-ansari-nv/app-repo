namespace timesheet_api.Auth.Services;

public class AuthService : IAuthService
{
    public Task AuthenticateAsync(string identifier, string password, bool rememberMe, string ip, string userAgent)
    {
        // TODO: Implement ASP.NET Core Identity integration per issue #41
        return Task.CompletedTask;
    }

    public Task SignOutAsync(Guid userId, string sessionId)
    {
        // TODO: Invalidate user session(s) per issue #41
        return Task.CompletedTask;
    }
}
