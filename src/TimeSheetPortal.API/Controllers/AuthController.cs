using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeSheetPortal.Core.DTOs;
using TimeSheetPortal.Core.Interfaces;

namespace TimeSheetPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMFAService _mfaService;
    private readonly IPasswordRecoveryService _passwordRecoveryService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        IMFAService mfaService,
        IPasswordRecoveryService passwordRecoveryService,
        ITokenService tokenService,
        IUserRepository userRepository,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _mfaService = mfaService;
        _passwordRecoveryService = passwordRecoveryService;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse 
            { 
                Message = "Invalid request", 
                Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
            });
        }

        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var (success, user, requiresMFA) = await _authenticationService.AuthenticateAsync(
                request.Username, 
                request.Password);

            if (!success || user == null)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return Unauthorized(new ErrorResponse { Message = "Invalid credentials" });
            }

            if (user.IsLocked && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("Account locked for user: {Username}", request.Username);
                return StatusCode(423, new ErrorResponse 
                { 
                    Message = $"Account is locked until {user.LockoutEnd.Value:u}" 
                });
            }

            if (requiresMFA && user.MFAEnabled)
            {
                await _mfaService.GenerateMFACodeAsync(user.Id);
                _logger.LogInformation("MFA code generated for user: {Username}", request.Username);
                
                return Ok(new LoginResponse 
                { 
                    Token = null, 
                    RequiresMFA = true 
                });
            }

            var token = _tokenService.GenerateJwtToken(user.Id, user.Username, user.Email);
            _logger.LogInformation("Login successful for user: {Username}", request.Username);

            return Ok(new LoginResponse 
            { 
                Token = token, 
                RequiresMFA = false 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, new ErrorResponse { Message = "An error occurred during login" });
        }
    }

    [HttpPost("mfa/verify")]
    [AllowAnonymous]
    public async Task<ActionResult<MFAVerifyResponse>> VerifyMFA([FromBody] MFAVerifyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse 
            { 
                Message = "Invalid request", 
                Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
            });
        }

        try
        {
            _logger.LogInformation("MFA verification attempt for user: {Username}", request.Username);

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("MFA verification failed - user not found: {Username}", request.Username);
                return Unauthorized(new ErrorResponse { Message = "Invalid credentials" });
            }

            var isValid = await _mfaService.ValidateMFACodeAsync(user.Id, request.MFACode);
            if (!isValid)
            {
                _logger.LogWarning("Invalid MFA code for user: {Username}", request.Username);
                return Unauthorized(new ErrorResponse { Message = "Invalid or expired MFA code" });
            }

            await _authenticationService.ResetFailedLoginAttemptsAsync(user.Id);

            var token = _tokenService.GenerateJwtToken(user.Id, user.Username, user.Email);
            _logger.LogInformation("MFA verification successful for user: {Username}", request.Username);

            return Ok(new MFAVerifyResponse { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA verification for user: {Username}", request.Username);
            return StatusCode(500, new ErrorResponse { Message = "An error occurred during MFA verification" });
        }
    }

    [HttpPost("password-recovery/request")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordRecoveryResponse>> RequestPasswordRecovery([FromBody] PasswordRecoveryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse 
            { 
                Message = "Invalid request", 
                Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
            });
        }

        try
        {
            _logger.LogInformation("Password recovery requested for email: {Email}", request.Email);

            await _passwordRecoveryService.GenerateRecoveryTokenAsync(request.Email);

            return Ok(new PasswordRecoveryResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password recovery request for email: {Email}", request.Email);
            return Ok(new PasswordRecoveryResponse());
        }
    }

    [HttpPost("password-recovery/reset")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordResetResponse>> ResetPassword([FromBody] PasswordResetRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse 
            { 
                Message = "Invalid request", 
                Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
            });
        }

        try
        {
            _logger.LogInformation("Password reset attempt with token: {Token}", request.Token);

            var success = await _passwordRecoveryService.ValidateAndResetPasswordAsync(
                request.Token, 
                request.NewPassword);

            if (!success)
            {
                _logger.LogWarning("Password reset failed - invalid or expired token: {Token}", request.Token);
                return BadRequest(new ErrorResponse { Message = "Invalid or expired token" });
            }

            _logger.LogInformation("Password reset successful for token: {Token}", request.Token);
            return Ok(new PasswordResetResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for token: {Token}", request.Token);
            return StatusCode(500, new ErrorResponse { Message = "An error occurred during password reset" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        _logger.LogInformation("User logged out");
        return Ok(new { Message = "Logged out successfully" });
    }
}
