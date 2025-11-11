using Tempus.Core.Enums;

namespace Tempus.Web.Services.Calendar;

/// <summary>
/// Provides helper methods for calendar UI operations
/// </summary>
public class CalendarHelperService
{
    /// <summary>
    /// Gets the icon CSS class for an event type
    /// </summary>
    public string GetEventIcon(EventType eventType)
    {
        return eventType switch
        {
            EventType.Meeting => "fa-users",
            EventType.Task => "fa-check-square",
            EventType.Reminder => "fa-bell",
            EventType.Appointment => "fa-calendar-check",
            EventType.Deadline => "fa-exclamation-triangle",
            EventType.TimeBlock => "fa-clock",
            _ => "fa-calendar"
        };
    }

    /// <summary>
    /// Gets the color for an event type
    /// </summary>
    public string GetEventTypeColor(EventType eventType)
    {
        return eventType switch
        {
            EventType.Meeting => "#2196F3",      // Blue
            EventType.Task => "#4CAF50",         // Green
            EventType.Reminder => "#FF9800",     // Orange
            EventType.Appointment => "#9C27B0",  // Purple
            EventType.Deadline => "#F44336",     // Red
            EventType.TimeBlock => "#00BCD4",    // Cyan
            _ => "#757575"                       // Grey
        };
    }

    /// <summary>
    /// Gets the color for a priority level
    /// </summary>
    public string GetPriorityColor(Priority priority)
    {
        return priority switch
        {
            Priority.Low => "#4CAF50",       // Green
            Priority.Medium => "#FF9800",    // Orange
            Priority.High => "#F44336",      // Red
            Priority.Urgent => "#D32F2F",    // Dark Red
            _ => "#757575"                   // Grey
        };
    }

    /// <summary>
    /// Formats a duration in minutes to a human-readable string
    /// </summary>
    public string FormatDuration(int minutes)
    {
        if (minutes < 60)
            return $"{minutes} min";

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;

        if (remainingMinutes == 0)
            return $"{hours} hr";

        return $"{hours} hr {remainingMinutes} min";
    }

    /// <summary>
    /// Formats a date time for display based on user preferences
    /// </summary>
    public string FormatDateTime(DateTime dateTime, string timeFormat = "12-hour")
    {
        return timeFormat == "24-hour"
            ? dateTime.ToString("yyyy-MM-dd HH:mm")
            : dateTime.ToString("yyyy-MM-dd hh:mm tt");
    }

    /// <summary>
    /// Formats a time for display based on user preferences
    /// </summary>
    public string FormatTime(DateTime dateTime, string timeFormat = "12-hour")
    {
        return timeFormat == "24-hour"
            ? dateTime.ToString("HH:mm")
            : dateTime.ToString("hh:mm tt");
    }

    /// <summary>
    /// Gets a CSS class for event styling based on type and priority
    /// </summary>
    public string GetEventCssClass(EventType eventType, Priority priority, bool isCompleted)
    {
        var classes = new List<string> { "calendar-event" };

        classes.Add($"event-type-{eventType.ToString().ToLower()}");
        classes.Add($"event-priority-{priority.ToString().ToLower()}");

        if (isCompleted)
            classes.Add("event-completed");

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Determines if an event should show a warning indicator (e.g., deadline approaching)
    /// </summary>
    public bool ShouldShowWarning(Core.Models.Event evt, int hoursThreshold = 24)
    {
        if (evt.EventType != EventType.Deadline)
            return false;

        var hoursUntilEvent = (evt.StartTime - DateTime.Now).TotalHours;
        return hoursUntilEvent > 0 && hoursUntilEvent <= hoursThreshold;
    }

    /// <summary>
    /// Gets a status badge text for an event
    /// </summary>
    public string GetEventStatusBadge(Core.Models.Event evt)
    {
        if (evt.IsCompleted)
            return "Completed";

        if (evt.StartTime < DateTime.Now && evt.EndTime > DateTime.Now)
            return "In Progress";

        if (evt.EndTime < DateTime.Now)
            return "Past";

        if (ShouldShowWarning(evt))
            return "Urgent";

        return "Upcoming";
    }

    /// <summary>
    /// Calculates the visual height for an event in the calendar grid
    /// </summary>
    public int CalculateEventHeight(DateTime startTime, DateTime endTime, int minutesPerPixel = 2)
    {
        var durationMinutes = (endTime - startTime).TotalMinutes;
        var height = (int)(durationMinutes / minutesPerPixel);
        return Math.Max(height, 20); // Minimum height of 20px
    }

    /// <summary>
    /// Generates a tooltip text for an event
    /// </summary>
    public string GenerateEventTooltip(Core.Models.Event evt, string timeFormat = "12-hour")
    {
        var parts = new List<string>
        {
            $"<strong>{evt.Title}</strong>"
        };

        if (!string.IsNullOrWhiteSpace(evt.Location))
            parts.Add($"Location: {evt.Location}");

        parts.Add($"Time: {FormatTime(evt.StartTime, timeFormat)} - {FormatTime(evt.EndTime, timeFormat)}");
        parts.Add($"Type: {evt.EventType}");
        parts.Add($"Priority: {evt.Priority}");

        if (evt.Attendees != null && evt.Attendees.Any())
            parts.Add($"Attendees: {evt.Attendees.Count}");

        if (!string.IsNullOrWhiteSpace(evt.Description))
            parts.Add($"<br/>{evt.Description}");

        return string.Join("<br/>", parts);
    }
}
