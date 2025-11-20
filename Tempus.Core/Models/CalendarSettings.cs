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

    // Saved Layout Preferences
    public bool RememberLastView { get; set; } = true; // Remember and restore last used view
    public CalendarView? LastUsedView { get; set; } // Last view the user was using
    public DateTime? LastViewChangeDate { get; set; }

    // Event Display Filters
    public string? HiddenEventTypes { get; set; } // Comma-separated list of EventType enums to hide
    public bool ShowCompletedTasks { get; set; } = true;
    public bool ShowCancelledEvents { get; set; } = false;

    // Calendar Display Customization
    public bool ShowEventIcons { get; set; } = true;
    public bool ShowEventColors { get; set; } = true;
    public bool CompactView { get; set; } = false; // More events in same space
    public int CalendarStartHour { get; set; } = 0; // Start hour for day/week views (0-23)
    public int CalendarEndHour { get; set; } = 24; // End hour for day/week views (1-24)

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

    // Focus Time & Availability
    public bool EnableFocusTimeProtection { get; set; } = false;
    public int MaxMeetingsPerDay { get; set; } = 0; // 0 = unlimited
    public int MinimumNoticePeriodHours { get; set; } = 24; // Require X hours notice for meetings
    public bool AutoDeclineMeetingsOutsideWorkingHours { get; set; } = false;
    public bool AllowMeetingsDuringLunchBreak { get; set; } = true;

    // Free/Busy Information Sharing
    public bool PublishFreeBusyInformation { get; set; } = true; // Allow others to see when you're free or busy
    public FreeBusySharingLevel FreeBusySharingLevel { get; set; } = FreeBusySharingLevel.TeamMembers; // Who can see your free/busy times
    public int FreeBusyLookAheadDays { get; set; } = 60; // How many days ahead to share free/busy information
    public bool ShowPrivateEventsAsBusy { get; set; } = true; // Show private events as "Busy" in free/busy view

    // Other
    public Guid? DefaultCalendarId { get; set; } // For future multi-calendar support

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
