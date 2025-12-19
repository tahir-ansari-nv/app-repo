using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeSheetPortal.Core.DTOs;
using TimeSheetPortal.Infrastructure.Data;
using Xunit;

namespace TimeSheetPortal.Tests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                db.Database.EnsureCreated();
                DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin@123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.False(result.RequiresMFA);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "WrongPassword"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMFAEnabledUser_RequiresMFA()
    {
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "Test@123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        Assert.Null(result.Token);
        Assert.True(result.RequiresMFA);
    }

    [Fact]
    public async Task Login_WithInvalidModel_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequest
        {
            Username = "",
            Password = ""
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PasswordRecoveryRequest_WithValidEmail_ReturnsSuccess()
    {
        var request = new PasswordRecoveryRequest
        {
            Email = "admin@timesheetportal.com"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/password-recovery/request", request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PasswordRecoveryResponse>();
        Assert.NotNull(result);
        Assert.Contains("password recovery has been initiated", result.Message);
    }

    [Fact]
    public async Task PasswordRecoveryRequest_WithNonExistentEmail_ReturnsSuccess()
    {
        var request = new PasswordRecoveryRequest
        {
            Email = "nonexistent@example.com"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/password-recovery/request", request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
