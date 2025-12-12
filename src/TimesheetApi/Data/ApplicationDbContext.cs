using Microsoft.EntityFrameworkCore;
using TimesheetApi.Models;

namespace TimesheetApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Timesheet> Timesheets { get; set; } = null!;
    public DbSet<TimesheetEntry> TimesheetEntries { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Timesheet Configuration
        modelBuilder.Entity<Timesheet>(entity =>
        {
            entity.HasIndex(t => new { t.EmployeeId, t.WeekStartDate }).IsUnique();
            entity.HasIndex(t => t.EmployeeId);
            entity.HasIndex(t => t.WeekStartDate);

            entity.HasOne(t => t.Employee)
                .WithMany(e => e.Timesheets)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(t => t.RowVersion)
                .IsRowVersion();
        });

        // TimesheetEntry Configuration
        modelBuilder.Entity<TimesheetEntry>(entity =>
        {
            entity.HasIndex(te => te.Date);
            entity.HasIndex(te => te.ProjectCode);
            entity.HasIndex(te => te.TimesheetId);

            entity.HasOne(te => te.Timesheet)
                .WithMany(t => t.Entries)
                .HasForeignKey(te => te.TimesheetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
            entity.HasIndex(a => a.Timestamp);
        });
    }
}
