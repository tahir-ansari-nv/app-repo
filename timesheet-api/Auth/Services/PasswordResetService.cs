using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Extensions;
using System.Security.Cryptography;

namespace timesheet_api.Auth.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly ITimeLimitedDataProtector _protector;

    public PasswordResetService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("timesheet_api:auth:password-reset")
            .ToTimeLimitedDataProtector();
    }

    public Task RequestResetAsync(string email, string ip, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }

        // Generate a time-limited token bound to the email. Expiry: 60 minutes.
        var token = _protector.Protect(email.Trim().ToLowerInvariant(), TimeSpan.FromMinutes(60));

        // TODO: Enqueue email with reset link containing the token. Avoid revealing account existence.
        // Example reset link (not returned to caller): https://example.com/reset?token={token}

        return Task.CompletedTask;
    }

    public Task ConfirmResetAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required.");
        }
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("New password is required.");
        }
        if (newPassword.Length < 12)
        {
            throw new ArgumentException("Password must be at least 12 characters long.");
        }

        string email;
        try
        {
            email = _protector.Unprotect(token, out var expiration);
            if (DateTimeOffset.UtcNow > expiration)
            {
                throw new UnauthorizedAccessException("Token expired.");
            }
        }
        catch (CryptographicException)
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }

        // TODO: Look up user by email, update password securely, invalidate sessions.
        // Note: Single-use enforcement requires persistence (DB) to mark tokens as consumed.

        return Task.CompletedTask;
    }
}
