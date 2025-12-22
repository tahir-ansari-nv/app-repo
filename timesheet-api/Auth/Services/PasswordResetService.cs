namespace timesheet_api.Auth.Services;

public class PasswordResetService : IPasswordResetService
{
    public Task RequestResetAsync(string email, string ip, string userAgent)
    {
        // TODO: Generate token and enqueue email per issue #41
        return Task.CompletedTask;
    }

    public Task ConfirmResetAsync(string token, string newPassword)
    {
        // TODO: Validate token and update password per issue #41
        return Task.CompletedTask;
    }
}
