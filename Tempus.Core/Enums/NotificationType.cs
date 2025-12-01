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
    ProposedTimeSubmitted, // Alternative time was proposed
    OutOfOfficeAutoDecline, // Meeting auto-declined due to OOO
    OutOfOfficeAutoResponder, // Auto-responder sent
    FocusTimeProtection, // Meeting declined due to focus time
    LocationChange // Working location status changed
}
