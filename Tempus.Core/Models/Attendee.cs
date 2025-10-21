namespace Tempus.Core.Models;

public class Attendee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsOrganizer { get; set; }
    public AttendeeStatus Status { get; set; } = AttendeeStatus.Pending;
    public Guid EventId { get; set; }
}

public enum AttendeeStatus
{
    Pending,
    Accepted,
    Declined,
    Tentative
}
