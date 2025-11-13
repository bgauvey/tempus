using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Web.Helpers.Calendar;

/// <summary>
/// Provides formatting utilities for the Calendar component
/// </summary>
public class CalendarFormatter
{
    private readonly CalendarSettings? _settings;

    public CalendarFormatter(CalendarSettings? settings)
    {
        _settings = settings;
    }

    public string FormatTime(DateTime time)
    {
        var format = _settings?.TimeFormat ?? TimeFormat.TwelveHour;
        return format == TimeFormat.TwelveHour
            ? time.ToString("h:mm tt")
            : time.ToString("HH:mm");
    }

    public string FormatDateTime(DateTime dt)
    {
        return dt.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt");
    }

    public string FormatHourLabel(int hour)
    {
        var format = _settings?.TimeFormat ?? TimeFormat.TwelveHour;
        if (format == TimeFormat.TwelveHour)
        {
            return hour == 0 ? "12 AM" :
                   hour < 12 ? $"{hour} AM" :
                   hour == 12 ? "12 PM" :
                   $"{hour - 12} PM";
        }
        else
        {
            return $"{hour:00}:00";
        }
    }

    public string GetEventBackgroundColor(Event evt)
    {
        if (!string.IsNullOrEmpty(evt.Color))
        {
            return evt.Color;
        }

        return evt.EventType switch
        {
            EventType.Meeting => "#1E88E5",
            EventType.Appointment => "#43A047",
            EventType.Task => "#FB8C00",
            EventType.TimeBlock => "#8E24AA",
            EventType.Reminder => "#FDD835",
            EventType.Deadline => "#E53935",
            _ => "#757575"
        };
    }

    public string GetEventIcon(EventType eventType)
    {
        return eventType switch
        {
            EventType.Meeting => "👥",
            EventType.Appointment => "📅",
            EventType.Task => "✓",
            EventType.TimeBlock => "🎯",
            EventType.Reminder => "🔔",
            EventType.Deadline => "⚡",
            _ => "📌"
        };
    }
}
