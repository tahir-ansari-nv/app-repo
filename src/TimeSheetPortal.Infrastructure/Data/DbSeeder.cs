using TimeSheetPortal.Core.Entities;
using TimeSheetPortal.Infrastructure.Services;

namespace TimeSheetPortal.Infrastructure.Data;

public class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var passwordHashingService = new PasswordHashingService();
        var (hash, salt) = passwordHashingService.HashPassword("Admin@123");

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@timesheetportal.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsLocked = false,
            LockoutEnd = null,
            FailedLoginAttempts = 0,
            MFAEnabled = false,
            MFASecret = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(adminUser);

        var (testHash, testSalt) = passwordHashingService.HashPassword("Test@123");
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@timesheetportal.com",
            PasswordHash = testHash,
            PasswordSalt = testSalt,
            IsLocked = false,
            LockoutEnd = null,
            FailedLoginAttempts = 0,
            MFAEnabled = true,
            MFASecret = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(testUser);

        await context.SaveChangesAsync();
    }
}
