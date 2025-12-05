using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Models.Entities;

namespace TimesheetManagement.Data;

public class TimesheetDbContext : DbContext
{
    public TimesheetDbContext(DbContextOptions<TimesheetDbContext> options) : base(options)
    {
    }
    
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Timesheet> Timesheets { get; set; }
    public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Timesheet configuration
        modelBuilder.Entity<Timesheet>(entity =>
        {
            // Unique constraint on EmployeeId and WeekStartDate
            entity.HasIndex(e => new { e.EmployeeId, e.WeekStartDate })
                .IsUnique()
                .HasDatabaseName("IX_Timesheet_EmployeeId_WeekStartDate");
            
            // Configure relationship with Employee
            entity.HasOne(t => t.Employee)
                .WithMany(e => e.Timesheets)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure relationship with TimesheetEntries
            entity.HasMany(t => t.Entries)
                .WithOne(e => e.Timesheet)
                .HasForeignKey(e => e.TimesheetId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure relationship with AuditLogs
            entity.HasMany(t => t.AuditLogs)
                .WithOne(a => a.Timesheet)
                .HasForeignKey(a => a.TimesheetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // TimesheetEntry configuration
        modelBuilder.Entity<TimesheetEntry>(entity =>
        {
            // Index on Date for better query performance
            entity.HasIndex(e => e.Date);
        });
        
        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            // Index on TimesheetId and Timestamp for better query performance
            entity.HasIndex(e => new { e.TimesheetId, e.Timestamp });
        });
        
        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            // Index on RecipientId for better query performance
            entity.HasIndex(e => e.RecipientId);
        });
    }
}
