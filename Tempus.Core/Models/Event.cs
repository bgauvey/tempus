using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public EventType EventType { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public bool IsAllDay { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public string? ExternalCalendarId { get; set; }
    public string? ExternalCalendarProvider { get; set; }
    public string? Color { get; set; }
    public List<Attendee> Attendees { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsCompleted { get; set; }
}
