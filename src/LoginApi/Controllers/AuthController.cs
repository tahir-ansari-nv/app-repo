using Microsoft.AspNetCore.Mvc;
using LoginApi.Models;
using LoginApi.Services;

namespace LoginApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string userAgent = Request.Headers.UserAgent.ToString();

        var (Success, Token, ExpiresAt) = await _authService.LoginAsync(request.Email, request.Password, ipAddress, userAgent);

        if (!Success)
        {
            return Unauthorized(new { Message = "Invalid credentials or account locked." });
        }

        return Ok(new LoginResponse
        {
            Token = Token!,
            ExpiresAt = ExpiresAt!.Value
        });
    }
}
