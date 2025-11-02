using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Data;

public class TempusDbContext : IdentityDbContext<ApplicationUser>
{
    public TempusDbContext(DbContextOptions<TempusDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Attendee> Attendees { get; set; }
    public DbSet<CalendarIntegration> CalendarIntegrations { get; set; }
    public DbSet<CustomCalendarRange> CustomCalendarRanges { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<CalendarSettings> CalendarSettings { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(10000);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.UserId).IsRequired();

            // Configure decimal properties with precision and scale
            entity.Property(e => e.HourlyCostPerAttendee).HasPrecision(18, 2);
            entity.Property(e => e.MeetingCost).HasPrecision(18, 2);

            entity.HasMany(e => e.Attendees)
                  .WithOne()
                  .HasForeignKey(a => a.EventId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Events)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attendee>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Email).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<CalendarIntegration>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Provider).IsRequired().HasMaxLength(50);
            entity.Property(c => c.CalendarName).HasMaxLength(200);
        });

        modelBuilder.Entity<CustomCalendarRange>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.DaysCount).IsRequired();
            entity.Property(r => r.ShowWeekends).IsRequired();
            entity.Property(r => r.UserId).IsRequired();

            entity.HasOne(r => r.User)
                  .WithMany(u => u.CustomCalendarRanges)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(50);
            entity.Property(c => c.Company).HasMaxLength(200);
            entity.Property(c => c.JobTitle).HasMaxLength(200);
            entity.Property(c => c.Notes).HasMaxLength(1000);
            entity.Property(c => c.UserId).IsRequired();

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.Email, c.UserId }).IsUnique();
        });

        modelBuilder.Entity<CalendarSettings>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.UserId).IsRequired();
            entity.Property(s => s.TimeZone).IsRequired().HasMaxLength(100);
            entity.Property(s => s.WeekendDays).IsRequired().HasMaxLength(50);
            entity.Property(s => s.WorkingDays).IsRequired().HasMaxLength(50);
            entity.Property(s => s.DefaultEventColor).HasMaxLength(50);
            entity.Property(s => s.DefaultLocation).HasMaxLength(500);
            entity.Property(s => s.DefaultReminderTimes).IsRequired().HasMaxLength(100);

            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One settings record per user
            entity.HasIndex(s => s.UserId).IsUnique();
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            entity.Property(n => n.UserId).IsRequired();
            entity.Property(n => n.Type).IsRequired();

            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(n => n.Event)
                  .WithMany()
                  .HasForeignKey(n => n.EventId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });
    }
}
