namespace Tempus.Core.Enums;

/// <summary>
/// Controls who can see the guest list for an event
/// </summary>
public enum GuestListVisibility
{
    /// <summary>
    /// All attendees can see the full guest list
    /// </summary>
    AllAttendees,

    /// <summary>
    /// Only the organizer can see the full guest list
    /// </summary>
    OrganizerOnly,

    /// <summary>
    /// Attendees can only see their own status
    /// </summary>
    Hidden
}
