using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class TimeZoneConversionService : ITimeZoneConversionService
{
    // Common timezones that are frequently used
    private static readonly string[] CommonTimeZoneIds = new[]
    {
        "Pacific/Honolulu",      // Hawaii
        "America/Anchorage",     // Alaska
        "America/Los_Angeles",   // Pacific
        "America/Denver",        // Mountain
        "America/Chicago",       // Central
        "America/New_York",      // Eastern
        "UTC",                   // UTC
        "Europe/London",         // GMT/BST
        "Europe/Paris",          // CET/CEST
        "Europe/Berlin",         // CET/CEST
        "Asia/Dubai",            // Gulf
        "Asia/Kolkata",          // India
        "Asia/Shanghai",         // China
        "Asia/Tokyo",            // Japan
        "Australia/Sydney",      // Australian Eastern
    };

    public Event ConvertEventToTimeZone(Event @event, string targetTimeZoneId)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        if (string.IsNullOrEmpty(targetTimeZoneId))
            throw new ArgumentException("Target timezone ID cannot be null or empty", nameof(targetTimeZoneId));

        // If event has no timezone, assume UTC
        var sourceTimeZoneId = @event.TimeZoneId ?? "UTC";

        // If timezones are the same, return as-is
        if (sourceTimeZoneId == targetTimeZoneId)
            return @event;

        // Create a copy of the event with converted times
        var convertedEvent = new Event
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartTime = ConvertTime(@event.StartTime, sourceTimeZoneId, targetTimeZoneId),
            EndTime = ConvertTime(@event.EndTime, sourceTimeZoneId, targetTimeZoneId),
            TimeZoneId = targetTimeZoneId,
            Location = @event.Location,
            EventType = @event.EventType,
            Priority = @event.Priority,
            IsAllDay = @event.IsAllDay,
            IsRecurring = @event.IsRecurring,
            RecurrencePattern = @event.RecurrencePattern,
            RecurrenceInterval = @event.RecurrenceInterval,
            RecurrenceDaysOfWeek = @event.RecurrenceDaysOfWeek,
            RecurrenceEndType = @event.RecurrenceEndType,
            RecurrenceCount = @event.RecurrenceCount,
            RecurrenceEndDate = @event.RecurrenceEndDate,
            RecurrenceParentId = @event.RecurrenceParentId,
            IsRecurrenceException = @event.IsRecurrenceException,
            RecurrenceExceptionDate = @event.RecurrenceExceptionDate,
            ExternalCalendarId = @event.ExternalCalendarId,
            ExternalCalendarProvider = @event.ExternalCalendarProvider,
            Color = @event.Color,
            Attendees = @event.Attendees,
            Tags = @event.Tags,
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.UpdatedAt,
            IsCompleted = @event.IsCompleted,
            HourlyCostPerAttendee = @event.HourlyCostPerAttendee,
            MeetingCost = @event.MeetingCost,
            UserId = @event.UserId,
            User = @event.User
        };

        return convertedEvent;
    }

    public DateTime ConvertTime(DateTime dateTime, string fromTimeZoneId, string toTimeZoneId)
    {
        if (string.IsNullOrEmpty(fromTimeZoneId))
            throw new ArgumentException("Source timezone ID cannot be null or empty", nameof(fromTimeZoneId));

        if (string.IsNullOrEmpty(toTimeZoneId))
            throw new ArgumentException("Target timezone ID cannot be null or empty", nameof(toTimeZoneId));

        try
        {
            // Get the timezone info objects
            var sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(fromTimeZoneId);
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(toTimeZoneId);

            // Convert to UTC first, then to target timezone
            DateTime utcTime;

            // If the datetime has DateTimeKind.Utc, use it directly
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                utcTime = dateTime;
            }
            // If DateTimeKind.Local, convert to UTC
            else if (dateTime.Kind == DateTimeKind.Local)
            {
                utcTime = dateTime.ToUniversalTime();
            }
            // Otherwise, treat as unspecified and convert from source timezone
            else
            {
                utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);
            }

            // Convert from UTC to target timezone
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, targetTimeZone);

            return convertedTime;
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new ArgumentException($"Invalid timezone ID: {ex.Message}", ex);
        }
    }

    public List<TimeZoneInfo> GetAvailableTimeZones()
    {
        return TimeZoneInfo.GetSystemTimeZones()
            .OrderBy(tz => tz.BaseUtcOffset)
            .ThenBy(tz => tz.DisplayName)
            .ToList();
    }

    public List<TimeZoneInfo> GetCommonTimeZones()
    {
        var commonTimeZones = new List<TimeZoneInfo>();

        foreach (var tzId in CommonTimeZoneIds)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                commonTimeZones.Add(tz);
            }
            catch (TimeZoneNotFoundException)
            {
                // Skip if timezone not found on this system
                continue;
            }
        }

        return commonTimeZones;
    }

    public string GetTimeZoneDisplayName(string timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId))
            return "Unknown";

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return tz.DisplayName;
        }
        catch (TimeZoneNotFoundException)
        {
            return timeZoneId;
        }
    }

    public Event ConvertEventToUserTimeZone(Event @event, string userTimeZoneId)
    {
        if (string.IsNullOrEmpty(userTimeZoneId))
            userTimeZoneId = TimeZoneInfo.Local.Id;

        return ConvertEventToTimeZone(@event, userTimeZoneId);
    }
}
