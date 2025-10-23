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
        var calendar = Calendar.Load(icsContent);
        var events = new List<Event>();

        foreach (var calendarEvent in calendar.Events)
        {
            var tempusEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = calendarEvent.Summary ?? "Untitled Event",
                Description = calendarEvent.Description,
                StartTime = calendarEvent.Start.AsDateTimeOffset.DateTime,
                EndTime = calendarEvent.End.AsDateTimeOffset.DateTime,
                Location = calendarEvent.Location,
                IsAllDay = calendarEvent.IsAllDay,
                EventType = EventType.Appointment,
                ExternalCalendarId = calendarEvent.Uid,
                CreatedAt = DateTime.UtcNow
            };

            // Import attendees if present
            if (calendarEvent.Attendees != null)
            {
                foreach (var attendee in calendarEvent.Attendees)
                {
                    tempusEvent.Attendees.Add(new Tempus.Core.Models.Attendee
                    {
                        Id = Guid.NewGuid(),
                        Name = attendee.CommonName ?? attendee.Value.ToString(),
                        Email = attendee.Value.ToString().Replace("mailto:", ""),
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
