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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.UserId).IsRequired();

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
    }
}
