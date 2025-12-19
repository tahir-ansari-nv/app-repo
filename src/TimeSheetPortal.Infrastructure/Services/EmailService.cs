using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeSheetPortal.Core.Interfaces;

namespace TimeSheetPortal.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordRecoveryEmailAsync(string email, string token)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var resetUrl = $"{emailSettings["ResetPasswordBaseUrl"]}/reset-password?token={token}";

        _logger.LogInformation("Sending password recovery email to {Email} with token {Token}", email, token);
        _logger.LogInformation("Reset URL: {ResetUrl}", resetUrl);

        await Task.CompletedTask;
    }

    public async Task SendMFACodeEmailAsync(string email, string code)
    {
        _logger.LogInformation("Sending MFA code email to {Email} with code {Code}", email, code);

        await Task.CompletedTask;
    }
}
