using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for handling meeting RSVP responses and invitations
/// </summary>
public interface IRSVPService
{
    /// <summary>
    /// Submit an RSVP response for an attendee
    /// </summary>
    Task<Attendee> SubmitResponseAsync(Guid eventId, string attendeeEmail, AttendeeStatus status, string? responseNotes = null);

    /// <summary>
    /// Get RSVP statistics for an event
    /// </summary>
    Task<RSVPStatistics> GetRSVPStatisticsAsync(Guid eventId);

    /// <summary>
    /// Propose an alternative time for a meeting
    /// </summary>
    Task<ProposedTime> ProposeAlternativeTimeAsync(Guid eventId, string attendeeEmail, DateTime proposedStartTime, DateTime proposedEndTime, string? reason = null);

    /// <summary>
    /// Vote on a proposed alternative time
    /// </summary>
    Task<ProposedTime> VoteOnProposedTimeAsync(Guid proposedTimeId, string voterEmail);

    /// <summary>
    /// Get all proposed times for an event
    /// </summary>
    Task<List<ProposedTime>> GetProposedTimesAsync(Guid eventId);

    /// <summary>
    /// Get attendees who haven't responded yet
    /// </summary>
    Task<List<Attendee>> GetNonRespondersAsync(Guid eventId);

    /// <summary>
    /// Send RSVP reminder to non-responders
    /// </summary>
    Task SendRSVPRemindersAsync(Guid eventId);

    /// <summary>
    /// Check if an attendee can see the guest list based on visibility settings
    /// </summary>
    Task<bool> CanViewGuestListAsync(Guid eventId, string attendeeEmail);

    /// <summary>
    /// Get visible attendees based on guest list visibility settings
    /// </summary>
    Task<List<Attendee>> GetVisibleAttendeesAsync(Guid eventId, string requestingAttendeeEmail);

    /// <summary>
    /// Update RSVP deadline for an event
    /// </summary>
    Task UpdateRSVPDeadlineAsync(Guid eventId, DateTime? deadline);
}

/// <summary>
/// Statistics about RSVP responses for an event
/// </summary>
public class RSVPStatistics
{
    public int TotalInvited { get; set; }
    public int Accepted { get; set; }
    public int Declined { get; set; }
    public int Tentative { get; set; }
    public int Pending { get; set; }
    public double ResponseRate => TotalInvited > 0 ? (double)(Accepted + Declined + Tentative) / TotalInvited * 100 : 0;
}
