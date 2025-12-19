using System.ComponentModel.DataAnnotations;

namespace TimeSheetPortal.Core.DTOs;

public class LoginRequest
{
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string? Token { get; set; }
    public bool RequiresMFA { get; set; }
}

public class MFAVerifyRequest
{
    [Required]
    [StringLength(256)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string MFACode { get; set; } = string.Empty;
}

public class MFAVerifyResponse
{
    public string Token { get; set; } = string.Empty;
}

public class PasswordRecoveryRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}

public class PasswordRecoveryResponse
{
    public string Message { get; set; } = "If the email exists, a password recovery has been initiated";
}

public class PasswordResetRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    public string NewPassword { get; set; } = string.Empty;
}

public class PasswordResetResponse
{
    public string Message { get; set; } = "Password reset successfully";
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}
