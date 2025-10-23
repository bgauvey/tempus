using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Helpers;

public static class RecurrenceHelper
{
    /// <summary>
    /// Expands a recurring event into individual instances within a date range
    /// </summary>
    public static List<Event> ExpandRecurringEvent(Event recurringEvent, DateTime rangeStart, DateTime rangeEnd, int maxInstances = 100)
    {
        if (!recurringEvent.IsRecurring || recurringEvent.RecurrencePattern == RecurrencePattern.None)
        {
            return new List<Event> { recurringEvent };
        }

        var instances = new List<Event>();
        var currentDate = recurringEvent.StartTime.Date;
        var eventDuration = recurringEvent.EndTime - recurringEvent.StartTime;
        int occurrenceCount = 0;

        while (occurrenceCount < maxInstances)
        {
            // Check if we've reached the end condition
            if (recurringEvent.RecurrenceEndType == RecurrenceEndType.AfterOccurrences &&
                recurringEvent.RecurrenceCount.HasValue &&
                occurrenceCount >= recurringEvent.RecurrenceCount.Value)
            {
                break;
            }

            if (recurringEvent.RecurrenceEndType == RecurrenceEndType.OnDate &&
                recurringEvent.RecurrenceEndDate.HasValue &&
                currentDate > recurringEvent.RecurrenceEndDate.Value.Date)
            {
                break;
            }

            // Stop if we're beyond the requested range
            if (currentDate > rangeEnd.Date)
            {
                break;
            }

            // Create instance if it falls within the range
            if (currentDate >= rangeStart.Date && currentDate <= rangeEnd.Date)
            {
                if (ShouldIncludeOccurrence(recurringEvent, currentDate))
                {
                    var instance = CreateEventInstance(recurringEvent, currentDate, eventDuration);
                    instances.Add(instance);
                }
            }

            occurrenceCount++;

            // Move to next occurrence date
            currentDate = GetNextOccurrenceDate(recurringEvent, currentDate);

            // Safety check for infinite loops
            if (currentDate > DateTime.Now.AddYears(10))
            {
                break;
            }
        }

        return instances;
    }

    private static bool ShouldIncludeOccurrence(Event recurringEvent, DateTime date)
    {
        // For weekly recurrence, check if the day of week is included
        if (recurringEvent.RecurrencePattern == RecurrencePattern.Weekly)
        {
            if (string.IsNullOrEmpty(recurringEvent.RecurrenceDaysOfWeek))
            {
                return true; // Include all days if none specified
            }

            var dayOfWeek = (int)date.DayOfWeek;
            var selectedDays = recurringEvent.RecurrenceDaysOfWeek.Split(',')
                .Select(d => int.TryParse(d.Trim(), out var day) ? day : -1)
                .Where(d => d >= 0)
                .ToList();

            return selectedDays.Contains(dayOfWeek);
        }

        return true;
    }

    private static DateTime GetNextOccurrenceDate(Event recurringEvent, DateTime currentDate)
    {
        return recurringEvent.RecurrencePattern switch
        {
            RecurrencePattern.Daily => currentDate.AddDays(recurringEvent.RecurrenceInterval),
            RecurrencePattern.Weekly => currentDate.AddDays(7 * recurringEvent.RecurrenceInterval),
            RecurrencePattern.Monthly => currentDate.AddMonths(recurringEvent.RecurrenceInterval),
            RecurrencePattern.Yearly => currentDate.AddYears(recurringEvent.RecurrenceInterval),
            _ => currentDate.AddDays(1)
        };
    }

    private static Event CreateEventInstance(Event template, DateTime date, TimeSpan duration)
    {
        var timeOfDay = template.StartTime.TimeOfDay;
        var startTime = date.Date.Add(timeOfDay);
        var endTime = startTime.Add(duration);

        return new Event
        {
            Id = Guid.NewGuid(), // Each instance gets a unique ID for display
            Title = template.Title,
            Description = template.Description,
            StartTime = startTime,
            EndTime = endTime,
            Location = template.Location,
            EventType = template.EventType,
            Priority = template.Priority,
            IsAllDay = template.IsAllDay,
            IsRecurring = false, // Instances are not themselves recurring
            RecurrenceParentId = template.Id, // Link back to the parent
            Color = template.Color,
            UserId = template.UserId,
            ExternalCalendarId = template.ExternalCalendarId,
            ExternalCalendarProvider = template.ExternalCalendarProvider,
            Tags = template.Tags.ToList(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            IsCompleted = template.IsCompleted
        };
    }

    /// <summary>
    /// Gets a human-readable description of the recurrence pattern
    /// </summary>
    public static string GetRecurrenceDescription(Event evt)
    {
        if (!evt.IsRecurring || evt.RecurrencePattern == RecurrencePattern.None)
        {
            return "Does not repeat";
        }

        var interval = evt.RecurrenceInterval > 1 ? $"every {evt.RecurrenceInterval} " : "every ";
        var pattern = evt.RecurrencePattern switch
        {
            RecurrencePattern.Daily => $"Repeats {interval}day{(evt.RecurrenceInterval > 1 ? "s" : "")}",
            RecurrencePattern.Weekly => GetWeeklyDescription(evt),
            RecurrencePattern.Monthly => $"Repeats {interval}month{(evt.RecurrenceInterval > 1 ? "s" : "")}",
            RecurrencePattern.Yearly => $"Repeats {interval}year{(evt.RecurrenceInterval > 1 ? "s" : "")}",
            _ => "Does not repeat"
        };

        var endDescription = evt.RecurrenceEndType switch
        {
            RecurrenceEndType.Never => "",
            RecurrenceEndType.AfterOccurrences => $", {evt.RecurrenceCount} times",
            RecurrenceEndType.OnDate => $", until {evt.RecurrenceEndDate:MMM d, yyyy}",
            _ => ""
        };

        return pattern + endDescription;
    }

    private static string GetWeeklyDescription(Event evt)
    {
        var interval = evt.RecurrenceInterval > 1 ? $"every {evt.RecurrenceInterval} weeks" : "weekly";

        if (string.IsNullOrEmpty(evt.RecurrenceDaysOfWeek))
        {
            return $"Repeats {interval}";
        }

        var dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        var selectedDays = evt.RecurrenceDaysOfWeek.Split(',')
            .Select(d => int.TryParse(d.Trim(), out var day) ? day : -1)
            .Where(d => d >= 0 && d < 7)
            .OrderBy(d => d)
            .Select(d => dayNames[d])
            .ToList();

        if (selectedDays.Count == 0)
        {
            return $"Repeats {interval}";
        }

        return $"Repeats {interval} on {string.Join(", ", selectedDays)}";
    }
}
