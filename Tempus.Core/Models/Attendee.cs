namespace Tempus.Core.Models;

/// <summary>
/// Represents an attendee for an event with RSVP tracking
/// </summary>
public class Attendee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsOrganizer { get; set; }

    // RSVP Status and Tracking
    public AttendeeStatus Status { get; set; } = AttendeeStatus.Pending;
    public DateTime? ResponseDate { get; set; } // When they responded
    public string? ResponseNotes { get; set; } // Optional message with response

    // Reminder tracking
    public DateTime? LastReminderSent { get; set; }
    public int ReminderCount { get; set; } = 0;

    // Guest visibility
    public bool IsOptional { get; set; } = false; // Optional vs required attendee

    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    // Proposed alternative times
    public List<ProposedTime> ProposedTimes { get; set; } = new();
}

/// <summary>
/// RSVP status for meeting invitations
/// </summary>
public enum AttendeeStatus
{
    Pending,      // No response yet
    Accepted,     // Accepted invitation
    Declined,     // Declined invitation
    Tentative     // Maybe attending
}

/// <summary>
/// Alternative time proposed by an attendee
/// </summary>
public class ProposedTime
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttendeeId { get; set; }
    public Attendee? Attendee { get; set; }

    public DateTime ProposedStartTime { get; set; }
    public DateTime ProposedEndTime { get; set; }
    public string? Reason { get; set; } // Why proposing alternative time

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Voting on proposed times
    public int VoteCount { get; set; } = 0;
    public List<string> VotedByEmails { get; set; } = new(); // Emails of attendees who voted
}
