using LoginApi.Data;
using LoginApi.Models;
using LoginApi.Services;

namespace LoginApi;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // Check if we already have users
        if (context.Users.Any())
        {
            return;
        }

        // Create sample users
        var users = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = passwordHasher.HashPassword("Admin123!"),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = passwordHasher.HashPassword("User123!"),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "inactive@example.com",
                PasswordHash = passwordHasher.HashPassword("Inactive123!"),
                IsActive = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
