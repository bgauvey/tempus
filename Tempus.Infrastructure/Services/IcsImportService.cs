using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Enums;

namespace Tempus.Infrastructure.Services;

public class IcsImportService : IIcsImportService
{
    public async Task<List<Event>> ImportFromStreamAsync(Stream fileStream)
    {
        // Read stream asynchronously to avoid "Synchronous reads are not supported" error
        using var reader = new StreamReader(fileStream);
        var icsContent = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(icsContent))
        {
            throw new InvalidOperationException("ICS file is empty or could not be read.");
        }

        var calendar = Calendar.Load(icsContent);
        if (calendar == null)
        {
            throw new InvalidOperationException("Failed to parse ICS file.");
        }

        var events = new List<Event>();

        if (calendar.Events == null || calendar.Events.Count == 0)
        {
            return events; // Return empty list if no events
        }

        foreach (var calendarEvent in calendar.Events)
        {
            // Skip events without start/end times
            if (calendarEvent.Start == null || calendarEvent.End == null)
            {
                continue;
            }

            // Extract start and end times with null checks
            DateTime startTime;
            DateTime endTime;

            try
            {
                // Try to get the DateTime value, falling back to UTC if needed
                startTime = calendarEvent.Start.AsDateTimeOffset.DateTime;
                endTime = calendarEvent.End.AsDateTimeOffset.DateTime;
            }
            catch
            {
                try
                {
                    // Fallback to AsUtc if AsDateTimeOffset fails
                    startTime = calendarEvent.Start.AsUtc;
                    endTime = calendarEvent.End.AsUtc;
                }
                catch
                {
                    // Skip events with invalid date/time formats
                    continue;
                }
            }

            var tempusEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = calendarEvent.Summary ?? "Untitled Event",
                Description = calendarEvent.Description,
                StartTime = startTime,
                EndTime = endTime,
                Location = calendarEvent.Location,
                IsAllDay = calendarEvent.IsAllDay,
                EventType = EventType.Appointment,
                ExternalCalendarId = calendarEvent.Uid,
                CreatedAt = DateTime.UtcNow
            };

            // Import attendees if present
            if (calendarEvent.Attendees != null && calendarEvent.Attendees.Count > 0)
            {
                foreach (var attendee in calendarEvent.Attendees)
                {
                    if (attendee?.Value == null) continue;

                    var email = attendee.Value.ToString().Replace("mailto:", "");
                    if (string.IsNullOrWhiteSpace(email)) continue;

                    tempusEvent.Attendees.Add(new Tempus.Core.Models.Attendee
                    {
                        Id = Guid.NewGuid(),
                        Name = attendee.CommonName ?? email,
                        Email = email,
                        EventId = tempusEvent.Id
                    });
                }
            }

            // Check if event is recurring
            if (calendarEvent.RecurrenceRules != null && calendarEvent.RecurrenceRules.Count > 0)
            {
                tempusEvent.IsRecurring = true;
                tempusEvent.RecurrenceRule = calendarEvent.RecurrenceRules[0].ToString();
            }

            events.Add(tempusEvent);
        }

        return await Task.FromResult(events);
    }

    public async Task<string> ExportToIcsAsync(List<Event> events)
    {
        var calendar = new Calendar();
        calendar.ProductId = "-//Tempus//Tempus Calendar//EN";

        foreach (var tempusEvent in events)
        {
            var calendarEvent = new CalendarEvent
            {
                Summary = tempusEvent.Title,
                Description = tempusEvent.Description,
                Location = tempusEvent.Location,
                Start = new CalDateTime(tempusEvent.StartTime),
                End = new CalDateTime(tempusEvent.EndTime),
                Uid = tempusEvent.ExternalCalendarId ?? tempusEvent.Id.ToString(),
                IsAllDay = tempusEvent.IsAllDay,
                Created = new CalDateTime(tempusEvent.CreatedAt)
            };

            // Add attendees
            foreach (var attendee in tempusEvent.Attendees)
            {
                calendarEvent.Attendees.Add(new Ical.Net.DataTypes.Attendee
                {
                    CommonName = attendee.Name,
                    Value = new Uri($"mailto:{attendee.Email}")
                });
            }

            calendar.Events.Add(calendarEvent);
        }

        var serializer = new CalendarSerializer();
        var icsContent = serializer.SerializeToString(calendar);

        return await Task.FromResult(icsContent);
    }
}
