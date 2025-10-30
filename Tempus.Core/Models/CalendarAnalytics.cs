namespace Tempus.Core.Models;

/// <summary>
/// Comprehensive analytics data for a user's calendar
/// </summary>
public class CalendarAnalytics
{
    public string UserId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Time Usage Metrics
    public TimeUsageMetrics TimeUsage { get; set; } = new();

    // Meeting Analytics
    public MeetingAnalytics MeetingStats { get; set; } = new();

    // Productivity Insights
    public ProductivityMetrics Productivity { get; set; } = new();

    // Quick Stats
    public int TotalEvents { get; set; }
    public int TotalHours { get; set; }
    public int MeetingCount { get; set; }
    public decimal TotalMeetingCost { get; set; }
}

/// <summary>
/// Time usage breakdown by event types and categories
/// </summary>
public class TimeUsageMetrics
{
    public Dictionary<string, int> EventTypeHours { get; set; } = new();
    public Dictionary<string, int> EventTypeCounts { get; set; } = new();
    public Dictionary<DayOfWeek, int> DayOfWeekHours { get; set; } = new();
    public Dictionary<int, int> HourOfDayDistribution { get; set; } = new();

    public int TotalScheduledHours { get; set; }
    public int TotalFreeHours { get; set; }
    public double ScheduledPercentage { get; set; }
}

/// <summary>
/// Meeting-specific analytics and cost tracking
/// </summary>
public class MeetingAnalytics
{
    public int TotalMeetings { get; set; }
    public int TotalMeetingHours { get; set; }
    public double AverageMeetingDuration { get; set; }
    public decimal TotalMeetingCost { get; set; }
    public decimal AverageMeetingCost { get; set; }

    public Dictionary<string, int> MeetingsByDay { get; set; } = new();
    public Dictionary<int, int> MeetingsByHour { get; set; } = new();

    public List<TopAttendee> TopAttendees { get; set; } = new();
    public List<CostlyMeeting> MostCostlyMeetings { get; set; } = new();

    public int BackToBackMeetings { get; set; }
    public int MeetingsWithBreaks { get; set; }
}

/// <summary>
/// Productivity and calendar health metrics
/// </summary>
public class ProductivityMetrics
{
    public double CalendarHealthScore { get; set; }
    public int FocusTimeBlocks { get; set; }
    public int TotalFocusHours { get; set; }
    public int FragmentedHours { get; set; }

    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double TaskCompletionRate { get; set; }

    public List<string> Recommendations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Top meeting attendee information
/// </summary>
public class TopAttendee
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int MeetingCount { get; set; }
    public int TotalHours { get; set; }
}

/// <summary>
/// Costly meeting details for analytics
/// </summary>
public class CostlyMeeting
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
    public int AttendeeCount { get; set; }
    public int DurationMinutes { get; set; }
}
