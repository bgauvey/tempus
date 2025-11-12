using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IEmailNotificationService
{
    /// <summary>
    /// Sends meeting update notifications to all attendees
    /// </summary>
    /// <param name="originalEvent">The original event before changes</param>
    /// <param name="updatedEvent">The updated event</param>
    /// <param name="organizerName">Name of the meeting organizer</param>
    /// <param name="updateType">Type of update (Rescheduled, Cancelled, etc.)</param>
    /// <returns>Task representing the async operation</returns>
    Task SendMeetingUpdateAsync(Event originalEvent, Event updatedEvent, string organizerName, MeetingUpdateType updateType);

    /// <summary>
    /// Sends a meeting invitation to all attendees
    /// </summary>
    /// <param name="meetingEvent">The meeting event</param>
    /// <param name="organizerName">Name of the meeting organizer</param>
    /// <returns>Task representing the async operation</returns>
    Task SendMeetingInvitationAsync(Event meetingEvent, string organizerName);

    /// <summary>
    /// Sends a meeting cancellation to all attendees
    /// </summary>
    /// <param name="meetingEvent">The cancelled meeting event</param>
    /// <param name="organizerName">Name of the meeting organizer</param>
    /// <returns>Task representing the async operation</returns>
    Task SendMeetingCancellationAsync(Event meetingEvent, string organizerName);

    /// <summary>
    /// Sends an RSVP reminder to a non-responder
    /// </summary>
    /// <param name="meetingEvent">The meeting event</param>
    /// <param name="attendee">The attendee to remind</param>
    /// <returns>Task representing the async operation</returns>
    Task SendRSVPReminderAsync(Event meetingEvent, Attendee attendee);

    /// <summary>
    /// Notifies organizer when an attendee responds to RSVP
    /// </summary>
    /// <param name="meetingEvent">The meeting event</param>
    /// <param name="respondingAttendee">The attendee who responded</param>
    /// <param name="organizer">The event organizer</param>
    /// <returns>Task representing the async operation</returns>
    Task SendRSVPResponseNotificationAsync(Event meetingEvent, Attendee respondingAttendee, Attendee organizer);

    /// <summary>
    /// Notifies organizer when an attendee proposes an alternative time
    /// </summary>
    /// <param name="meetingEvent">The meeting event</param>
    /// <param name="attendee">The attendee proposing alternative time</param>
    /// <param name="proposedTime">The proposed alternative time</param>
    /// <param name="organizer">The event organizer</param>
    /// <returns>Task representing the async operation</returns>
    Task SendProposedTimeNotificationAsync(Event meetingEvent, Attendee attendee, ProposedTime proposedTime, Attendee organizer);

    /// <summary>
    /// Sends a scheduling poll invitation to an attendee
    /// </summary>
    /// <param name="poll">The scheduling poll</param>
    /// <param name="attendeeEmail">Email address of the attendee</param>
    /// <returns>Task representing the async operation</returns>
    Task SendPollInvitationAsync(SchedulingPoll poll, string attendeeEmail);

    /// <summary>
    /// Sends a reminder to respond to a scheduling poll
    /// </summary>
    /// <param name="poll">The scheduling poll</param>
    /// <param name="attendeeEmail">Email address of the attendee</param>
    /// <returns>Task representing the async operation</returns>
    Task SendPollReminderAsync(SchedulingPoll poll, string attendeeEmail);

    /// <summary>
    /// Notifies the poll organizer that a poll has been finalized
    /// </summary>
    /// <param name="poll">The finalized scheduling poll</param>
    /// <returns>Task representing the async operation</returns>
    Task SendPollFinalizedNotificationAsync(SchedulingPoll poll);
}

public enum MeetingUpdateType
{
    Rescheduled,
    TimeChanged,
    LocationChanged,
    DetailsChanged,
    Cancelled
}
