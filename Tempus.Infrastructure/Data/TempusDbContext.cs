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
    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<Attendee> Attendees { get; set; }
    public DbSet<EventAttachment> EventAttachments { get; set; }
    public DbSet<ProposedTime> ProposedTimes { get; set; }
    public DbSet<CalendarIntegration> CalendarIntegrations { get; set; }
    public DbSet<CustomCalendarRange> CustomCalendarRanges { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<CalendarSettings> CalendarSettings { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SchedulingPoll> SchedulingPolls { get; set; }
    public DbSet<PollTimeSlot> PollTimeSlots { get; set; }
    public DbSet<PollResponse> PollResponses { get; set; }
    public DbSet<VideoConference> VideoConferences { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<TeamInvitation> TeamInvitations { get; set; }
    public DbSet<CalendarShare> CalendarShares { get; set; }
    public DbSet<PublicCalendar> PublicCalendars { get; set; }

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

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Events)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Calendar)
                  .WithMany(c => c.Events)
                  .HasForeignKey(e => e.CalendarId)
                  .OnDelete(DeleteBehavior.NoAction); // NoAction to avoid cascade path conflicts

            entity.HasIndex(e => new { e.UserId, e.CalendarId });
        });

        modelBuilder.Entity<EventAttachment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FileName).HasMaxLength(255);
            entity.Property(a => a.FilePath).HasMaxLength(500);
            entity.Property(a => a.ContentType).HasMaxLength(100);
            entity.Property(a => a.ExternalUrl).HasMaxLength(2000);
            entity.Property(a => a.LinkTitle).HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(500);
            entity.Property(a => a.UploadedBy).IsRequired().HasMaxLength(200);

            entity.HasOne(a => a.Event)
                  .WithMany(e => e.Attachments)
                  .HasForeignKey(a => a.EventId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => a.EventId);
            entity.HasIndex(a => new { a.EventId, a.Type });
        });

        modelBuilder.Entity<Calendar>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.Color).IsRequired().HasMaxLength(50);
            entity.Property(c => c.UserId).IsRequired();
            entity.Property(c => c.DefaultEventColor).HasMaxLength(50);
            entity.Property(c => c.DefaultLocation).HasMaxLength(500);
            entity.Property(c => c.DefaultReminderTimes).HasMaxLength(100);

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Calendars)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.UserId, c.SortOrder });
            entity.HasIndex(c => new { c.UserId, c.IsDefault });
        });

        modelBuilder.Entity<Attendee>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Email).IsRequired().HasMaxLength(200);
            entity.Property(a => a.ResponseNotes).HasMaxLength(1000);

            entity.HasOne(a => a.Event)
                  .WithMany(e => e.Attendees)
                  .HasForeignKey(a => a.EventId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.ProposedTimes)
                  .WithOne(p => p.Attendee)
                  .HasForeignKey(p => p.AttendeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.EventId, a.Email });
        });

        modelBuilder.Entity<ProposedTime>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Reason).HasMaxLength(500);

            entity.HasOne(p => p.Attendee)
                  .WithMany(a => a.ProposedTimes)
                  .HasForeignKey(p => p.AttendeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.AttendeeId);
        });

        modelBuilder.Entity<CalendarIntegration>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.UserId).IsRequired();
            entity.Property(c => c.Provider).IsRequired().HasMaxLength(50);
            entity.Property(c => c.CalendarName).HasMaxLength(200);
            entity.Property(c => c.CalendarId).HasMaxLength(200);
            entity.Property(c => c.AccessToken).HasMaxLength(2000);
            entity.Property(c => c.RefreshToken).HasMaxLength(2000);
            entity.Property(c => c.SyncToken).HasMaxLength(2000);

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.UserId, c.Provider });
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

        modelBuilder.Entity<SchedulingPoll>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.OrganizerEmail).IsRequired().HasMaxLength(200);
            entity.Property(p => p.OrganizerName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Location).HasMaxLength(500);

            entity.HasMany(p => p.TimeSlots)
                  .WithOne(ts => ts.SchedulingPoll)
                  .HasForeignKey(ts => ts.SchedulingPollId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Responses)
                  .WithOne(r => r.SchedulingPoll)
                  .HasForeignKey(r => r.SchedulingPollId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.OrganizerEmail);
            entity.HasIndex(p => p.CreatedAt);
        });

        modelBuilder.Entity<PollTimeSlot>(entity =>
        {
            entity.HasKey(ts => ts.Id);

            entity.HasOne(ts => ts.SchedulingPoll)
                  .WithMany(p => p.TimeSlots)
                  .HasForeignKey(ts => ts.SchedulingPollId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(ts => ts.Responses)
                  .WithOne(r => r.PollTimeSlot)
                  .HasForeignKey(r => r.PollTimeSlotId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ts => ts.SchedulingPollId);
        });

        modelBuilder.Entity<PollResponse>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RespondentEmail).IsRequired().HasMaxLength(200);
            entity.Property(r => r.RespondentName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Comment).HasMaxLength(500);

            entity.HasOne(r => r.SchedulingPoll)
                  .WithMany(p => p.Responses)
                  .HasForeignKey(r => r.SchedulingPollId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(r => r.PollTimeSlot)
                  .WithMany(ts => ts.Responses)
                  .HasForeignKey(r => r.PollTimeSlotId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => new { r.SchedulingPollId, r.RespondentEmail });
            entity.HasIndex(r => r.PollTimeSlotId);
        });

        modelBuilder.Entity<VideoConference>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.MeetingUrl).IsRequired().HasMaxLength(2000);
            entity.Property(v => v.MeetingId).HasMaxLength(200);
            entity.Property(v => v.Passcode).HasMaxLength(100);
            entity.Property(v => v.DialInNumbers).HasMaxLength(5000); // JSON storage
            entity.Property(v => v.DialInPasscode).HasMaxLength(100);
            entity.Property(v => v.HostKey).HasMaxLength(100);
            entity.Property(v => v.ExternalMeetingId).HasMaxLength(200);
            entity.Property(v => v.CreatedBy).IsRequired().HasMaxLength(200);

            entity.HasOne(v => v.Event)
                  .WithOne(e => e.VideoConference)
                  .HasForeignKey<VideoConference>(v => v.EventId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(v => v.EventId).IsUnique();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.Property(t => t.CreatedBy).IsRequired().HasMaxLength(450);

            entity.HasOne(t => t.Creator)
                  .WithMany()
                  .HasForeignKey(t => t.CreatedBy)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(t => t.Members)
                  .WithOne(m => m.Team)
                  .HasForeignKey(m => m.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Invitations)
                  .WithOne(i => i.Team)
                  .HasForeignKey(i => i.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => t.CreatedBy);
            entity.HasIndex(t => new { t.IsActive, t.CreatedAt });
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.UserId).IsRequired().HasMaxLength(450);
            entity.Property(m => m.InvitedBy).HasMaxLength(450);

            entity.HasOne(m => m.Team)
                  .WithMany(t => t.Members)
                  .HasForeignKey(m => m.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.User)
                  .WithMany()
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(m => m.Inviter)
                  .WithMany()
                  .HasForeignKey(m => m.InvitedBy)
                  .OnDelete(DeleteBehavior.NoAction);

            // Ensure a user can only be a member of a team once
            entity.HasIndex(m => new { m.TeamId, m.UserId }).IsUnique();
            entity.HasIndex(m => m.UserId);
        });

        modelBuilder.Entity<TeamInvitation>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Email).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Token).IsRequired().HasMaxLength(100);
            entity.Property(i => i.InvitedBy).IsRequired().HasMaxLength(450);

            entity.HasOne(i => i.Team)
                  .WithMany(t => t.Invitations)
                  .HasForeignKey(i => i.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Inviter)
                  .WithMany()
                  .HasForeignKey(i => i.InvitedBy)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(i => i.Token).IsUnique();
            entity.HasIndex(i => new { i.TeamId, i.Email });
            entity.HasIndex(i => new { i.Status, i.ExpiresAt });
        });

        modelBuilder.Entity<CalendarShare>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SharedWithUserId).IsRequired().HasMaxLength(450);
            entity.Property(s => s.SharedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(s => s.Note).HasMaxLength(500);
            entity.Property(s => s.Color).HasMaxLength(50);

            entity.HasOne(s => s.Calendar)
                  .WithMany()
                  .HasForeignKey(s => s.CalendarId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.SharedWithUser)
                  .WithMany()
                  .HasForeignKey(s => s.SharedWithUserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(s => s.SharedByUser)
                  .WithMany()
                  .HasForeignKey(s => s.SharedByUserId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Ensure a calendar can only be shared once with a specific user
            entity.HasIndex(s => new { s.CalendarId, s.SharedWithUserId }).IsUnique();
            entity.HasIndex(s => s.SharedWithUserId);
            entity.HasIndex(s => new { s.SharedWithUserId, s.IsAccepted });
        });

        modelBuilder.Entity<PublicCalendar>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.UserId).IsRequired().HasMaxLength(450);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.IcsUrl).IsRequired().HasMaxLength(2000);
            entity.Property(p => p.Color).IsRequired().HasMaxLength(50);

            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => new { p.UserId, p.Category });
            entity.HasIndex(p => new { p.UserId, p.IsActive });
        });
    }
}
