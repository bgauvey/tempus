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

    // Recurrence properties
    public bool IsRecurring { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
    public int RecurrenceInterval { get; set; } = 1; // Every X days/weeks/months/years
    public string? RecurrenceDaysOfWeek { get; set; } // Comma-separated: "0,1,2" for Sun,Mon,Tue
    public RecurrenceEndType RecurrenceEndType { get; set; } = RecurrenceEndType.Never;
    public int? RecurrenceCount { get; set; } // Number of occurrences
    public DateTime? RecurrenceEndDate { get; set; }
    public Guid? RecurrenceParentId { get; set; } // For instances: ID of the parent event
    public bool IsRecurrenceException { get; set; } // True if this is a modified single instance
    public DateTime? RecurrenceExceptionDate { get; set; } // Original date for exception instances

    public string? ExternalCalendarId { get; set; }
    public string? ExternalCalendarProvider { get; set; }
    public string? Color { get; set; }
    public List<Attendee> Attendees { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsCompleted { get; set; }

    // Meeting cost tracking
    public decimal HourlyCostPerAttendee { get; set; } = 75.00m;
    public decimal? MeetingCost { get; set; }

    // User ownership
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
