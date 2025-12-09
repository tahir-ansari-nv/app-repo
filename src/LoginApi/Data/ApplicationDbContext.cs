using LoginApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);
            entity.HasIndex(e => e.Email)
                .IsUnique();
            entity.Property(e => e.PasswordHash)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });

        // Configure LoginAttempt entity
        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.Timestamp)
                .IsRequired();
            entity.Property(e => e.Success)
                .IsRequired();
            entity.Property(e => e.FailureReason)
                .HasMaxLength(512);
            entity.HasIndex(e => new { e.Email, e.Timestamp, e.Success });
        });
    }
}
