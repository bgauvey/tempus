using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class CalendarSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // User ownership
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Time & Date Display
    public DayOfWeek StartOfWeek { get; set; } = DayOfWeek.Sunday;
    public TimeFormat TimeFormat { get; set; } = TimeFormat.TwelveHour;
    public DateFormat DateFormat { get; set; } = DateFormat.MonthDayYear;
    public bool ShowWeekNumbers { get; set; } = false;
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    // Calendar View Preferences
    public CalendarView DefaultCalendarView { get; set; } = CalendarView.Week;
    public bool ShowWeekendInWeekView { get; set; } = true;
    public TimeSlotDuration TimeSlotDuration { get; set; } = TimeSlotDuration.ThirtyMinutes;
    public TimeSpan ScrollToTime { get; set; } = new TimeSpan(8, 0, 0); // 8:00 AM

    // Working Hours & Availability
    public TimeSpan WorkHoursStart { get; set; } = new TimeSpan(8, 0, 0); // 08:00
    public TimeSpan WorkHoursEnd { get; set; } = new TimeSpan(17, 0, 0); // 17:00
    public string WeekendDays { get; set; } = "0,6"; // Sunday=0, Saturday=6
    public string WorkingDays { get; set; } = "1,2,3,4,5"; // Monday-Friday
    public TimeSpan? LunchBreakStart { get; set; }
    public TimeSpan? LunchBreakEnd { get; set; }
    public int BufferTimeBetweenEvents { get; set; } = 0; // minutes

    // Event Defaults
    public int DefaultMeetingDuration { get; set; } = 30; // minutes
    public string? DefaultEventColor { get; set; }
    public EventVisibility DefaultEventVisibility { get; set; } = EventVisibility.Public;
    public string? DefaultLocation { get; set; }

    // Reminders/Notifications
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool DesktopNotificationsEnabled { get; set; } = true;
    public string DefaultReminderTimes { get; set; } = "15,60"; // 15 min, 1 hour (in minutes)

    // Other
    public Guid? DefaultCalendarId { get; set; } // For future multi-calendar support

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
