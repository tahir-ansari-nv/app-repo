using timesheet_api.Auth.Models;
using timesheet_api.Auth.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService auth, HttpContext ctx) =>
{
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
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
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
    var userAgent = ctx.Request.Headers["User-Agent"].ToString();
    await svc.RequestResetAsync(request.Email, ip, userAgent);
    return Results.Ok(new { message = "If the account exists, a reset link will be sent." });
})
.WithName("PasswordResetRequest");

app.MapPost("/api/auth/password-reset/confirm", async (PasswordResetConfirmRequest request, IPasswordResetService svc) =>
{
    await svc.ConfirmResetAsync(request.Token, request.NewPassword);
    return Results.Ok(new { message = "Password reset confirmation scaffolded" });
})
.WithName("PasswordResetConfirm");

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
