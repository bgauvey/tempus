namespace Tempus.Core.Enums;

public enum NotificationType
{
    EventReminder,
    EventUpdate,
    EventCancelled,
    EventInvitation,
    MeetingStartingSoon,
    TaskDue,
    System,
    ReminderSent,        // RSVP reminder was sent to attendee
    RSVPResponse,        // Attendee responded to RSVP
    ProposedTimeSubmitted // Alternative time was proposed
}
