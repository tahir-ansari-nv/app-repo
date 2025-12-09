using LoginApi.DTOs;
using LoginApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoginApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.AuthenticateAsync(request.Email, request.Password);
        
        if (!result.Success)
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse
        {
            Token = result.Token!,
            Expiration = result.Expiration
        });
    }
}
