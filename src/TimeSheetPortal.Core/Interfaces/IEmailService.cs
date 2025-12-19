namespace TimeSheetPortal.Core.Interfaces;

public interface IEmailService
{
    Task SendPasswordRecoveryEmailAsync(string email, string token);
    Task SendMFACodeEmailAsync(string email, string code);
}
