using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Web.Services.Calendar;

/// <summary>
/// Handles event CRUD operations for the calendar
/// </summary>
public class CalendarEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ITimeZoneConversionService _timeZoneService;
    private readonly ILogger<CalendarEventService> _logger;

    public CalendarEventService(
        IEventRepository eventRepository,
        ITimeZoneConversionService timeZoneService,
        ILogger<CalendarEventService> logger)
    {
        _eventRepository = eventRepository;
        _timeZoneService = timeZoneService;
        _logger = logger;
    }

    /// <summary>
    /// Loads events for a date range in the user's timezone
    /// </summary>
    public async Task<List<Event>> LoadEventsAsync(DateTime startDate, DateTime endDate, string userId, string userTimeZone)
    {
        try
        {
            _logger.LogDebug("Loading events from {StartDate} to {EndDate}", startDate, endDate);

            // Get events from repository (stored in UTC)
            var eventsUtc = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);
            _logger.LogDebug("Retrieved {EventCount} events from repository", eventsUtc.Count);

            // Convert all events to user's timezone for display
            var eventsInUserTimeZone = eventsUtc.Select(evt =>
                _timeZoneService.ConvertEventToTimeZone(evt, userTimeZone)
            ).ToList();

            _logger.LogDebug("Converted {EventCount} events to user timezone: {TimeZone}", eventsInUserTimeZone.Count, userTimeZone);

            return eventsInUserTimeZone;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events");
            throw;
        }
    }

    /// <summary>
    /// Creates an event from a template
    /// </summary>
    public Event CreateEventFromTemplate(string templateName, int durationMinutes, DateTime startTime, string userId, string userTimeZone)
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Title = templateName,
            StartTime = startTime,
            EndTime = startTime.AddMinutes(durationMinutes),
            UserId = userId,
            TimeZoneId = userTimeZone,
            EventType = templateName switch
            {
                "Meeting" => EventType.Meeting,
                "Task" => EventType.Task,
                "Reminder" => EventType.Reminder,
                "Appointment" => EventType.Appointment,
                "Deadline" => EventType.Deadline,
                "TimeBlock" => EventType.TimeBlock,
                _ => EventType.Task // Default to Task for unknown templates
            },
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return evt;
    }

    /// <summary>
    /// Duplicates an existing event
    /// </summary>
    public Event DuplicateEvent(Event originalEvent, string userId)
    {
        var duplicatedEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = $"{originalEvent.Title} (Copy)",
            Description = originalEvent.Description,
            Location = originalEvent.Location,
            StartTime = originalEvent.StartTime.AddDays(1), // Next day by default
            EndTime = originalEvent.EndTime.AddDays(1),
            TimeZoneId = originalEvent.TimeZoneId,
            IsAllDay = originalEvent.IsAllDay,
            EventType = originalEvent.EventType,
            Priority = originalEvent.Priority,
            Color = originalEvent.Color,
            UserId = userId,
            CalendarId = originalEvent.CalendarId,
            ReminderMinutes = originalEvent.ReminderMinutes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Copy attendees
        if (originalEvent.Attendees != null && originalEvent.Attendees.Any())
        {
            duplicatedEvent.Attendees = originalEvent.Attendees.Select(a => new Attendee
            {
                Id = Guid.NewGuid(),
                Name = a.Name,
                Email = a.Email,
                IsOrganizer = a.IsOrganizer,
                Status = AttendeeStatus.Pending,
                EventId = duplicatedEvent.Id
            }).ToList();
        }

        return duplicatedEvent;
    }

    /// <summary>
    /// Updates event times after drag and drop
    /// </summary>
    public async Task<Event> UpdateEventTimesAsync(Guid eventId, DateTime newStart, DateTime newEnd, string userId)
    {
        var evt = await _eventRepository.GetByIdAsync(eventId, userId);
        if (evt == null)
            throw new InvalidOperationException($"Event {eventId} not found");

        evt.StartTime = newStart;
        evt.EndTime = newEnd;
        evt.UpdatedAt = DateTime.UtcNow;

        return await _eventRepository.UpdateAsync(evt);
    }

    /// <summary>
    /// Deletes an event
    /// </summary>
    public async Task DeleteEventAsync(Guid eventId, string userId)
    {
        await _eventRepository.DeleteAsync(eventId, userId);
        _logger.LogInformation("Deleted event {EventId}", eventId);
    }

    /// <summary>
    /// Creates a recurrence exception (modified single occurrence)
    /// </summary>
    public Event CreateRecurrenceException(Event originalInstance, string userId)
    {
        var exception = new Event
        {
            Id = Guid.NewGuid(),
            Title = originalInstance.Title,
            Description = originalInstance.Description,
            Location = originalInstance.Location,
            StartTime = originalInstance.StartTime,
            EndTime = originalInstance.EndTime,
            TimeZoneId = originalInstance.TimeZoneId,
            IsAllDay = originalInstance.IsAllDay,
            EventType = originalInstance.EventType,
            Priority = originalInstance.Priority,
            Color = originalInstance.Color,
            UserId = userId,
            CalendarId = originalInstance.CalendarId,
            ReminderMinutes = originalInstance.ReminderMinutes,

            // Recurrence exception fields
            IsRecurrenceException = true,
            RecurrenceParentId = originalInstance.RecurrenceParentId ?? originalInstance.Id,
            RecurrenceExceptionDate = originalInstance.StartTime.Date,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return exception;
    }

    /// <summary>
    /// Bulk updates event completion status
    /// </summary>
    public async Task BulkUpdateCompletionAsync(List<Guid> eventIds, bool isCompleted, string userId)
    {
        foreach (var eventId in eventIds)
        {
            var evt = await _eventRepository.GetByIdAsync(eventId, userId);
            if (evt != null)
            {
                evt.IsCompleted = isCompleted;
                evt.UpdatedAt = DateTime.UtcNow;
                await _eventRepository.UpdateAsync(evt);
            }
        }

        _logger.LogInformation("Bulk updated {Count} events completion status to {IsCompleted}", eventIds.Count, isCompleted);
    }

    /// <summary>
    /// Bulk deletes events
    /// </summary>
    public async Task BulkDeleteAsync(List<Guid> eventIds, string userId)
    {
        foreach (var eventId in eventIds)
        {
            await _eventRepository.DeleteAsync(eventId, userId);
        }

        _logger.LogInformation("Bulk deleted {Count} events", eventIds.Count);
    }

    /// <summary>
    /// Bulk updates event field
    /// </summary>
    public async Task BulkUpdateFieldAsync(List<Guid> eventIds, string fieldName, object value, string userId)
    {
        foreach (var eventId in eventIds)
        {
            var evt = await _eventRepository.GetByIdAsync(eventId, userId);
            if (evt != null)
            {
                // Update based on field name
                switch (fieldName.ToLower())
                {
                    case "priority":
                        evt.Priority = (Priority)value;
                        break;
                    case "eventtype":
                        evt.EventType = (EventType)value;
                        break;
                    case "calendar":
                        evt.CalendarId = (Guid?)value;
                        break;
                    case "color":
                        evt.Color = (string?)value;
                        break;
                }

                evt.UpdatedAt = DateTime.UtcNow;
                await _eventRepository.UpdateAsync(evt);
            }
        }

        _logger.LogInformation("Bulk updated {FieldName} for {Count} events", fieldName, eventIds.Count);
    }
}
