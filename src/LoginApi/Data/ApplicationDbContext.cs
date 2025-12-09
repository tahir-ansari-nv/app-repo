using Microsoft.EntityFrameworkCore;
using LoginApi.Models;

namespace LoginApi.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        builder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.EmailAttempted).IsRequired();
            
            // Add composite index for efficient query performance on failed login attempts
            entity.HasIndex(l => new { l.EmailAttempted, l.IPAddress, l.Timestamp, l.Success })
                .HasDatabaseName("IX_LoginAttempt_Email_IP_Timestamp_Success");
        });
    }
}
