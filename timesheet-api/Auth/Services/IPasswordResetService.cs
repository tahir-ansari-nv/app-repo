namespace timesheet_api.Auth.Services;

public interface IPasswordResetService
{
    Task RequestResetAsync(string email, string ip, string userAgent);
    Task ConfirmResetAsync(string token, string newPassword);
}
