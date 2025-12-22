namespace timesheet_api.Auth.Models;

public record LoginRequest(string Identifier, string Password, bool? RememberMe, string? CorrelationId);

public record LogoutRequest(Guid UserId, string? SessionId);

public record PasswordResetRequest(string Email);

public record PasswordResetConfirmRequest(string Token, string NewPassword, string ConfirmPassword);
