using timesheet_api.Auth.Models;
using timesheet_api.Auth.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDataProtection();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService auth, HttpContext ctx) =>
{
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    var userAgent = ctx.Request.Headers["User-Agent"].ToString();
    await auth.AuthenticateAsync(request.Identifier, request.Password, request.RememberMe ?? false, ip, userAgent);
    return Results.Ok(new
    {
        message = "Login endpoint scaffolded",
        sessionExpiresAt = DateTimeOffset.UtcNow.AddHours(8)
    });
})
.WithName("AuthLogin");

app.MapPost("/api/auth/logout", async (LogoutRequest request, IAuthService auth) =>
{
    await auth.SignOutAsync(request.UserId, request.SessionId ?? string.Empty);
    return Results.Ok(new { message = "Logout endpoint scaffolded" });
})
.WithName("AuthLogout");

app.MapPost("/api/auth/password-reset/request", async (PasswordResetRequest request, IPasswordResetService svc, HttpContext ctx) =>
{
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    var userAgent = ctx.Request.Headers["User-Agent"].ToString();
    try
    {
        await svc.RequestResetAsync(request.Email, ip, userAgent);
        return Results.Ok(new { message = "If an account with this email exists, a reset link has been sent." });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { code = "ValidationError", message = ex.Message });
    }
})
.WithName("PasswordResetRequest");

app.MapPost("/api/auth/password-reset/confirm", async (PasswordResetConfirmRequest request, IPasswordResetService svc) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return Results.BadRequest(new { code = "ValidationError", message = "Password fields are required." });
        }
        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Results.BadRequest(new { code = "ValidationError", message = "Passwords do not match." });
        }

        await svc.ConfirmResetAsync(request.Token, request.NewPassword);
        return Results.Ok(new { message = "Password updated successfully." });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { code = "ValidationError", message = ex.Message });
    }
})
.WithName("PasswordResetConfirm");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
