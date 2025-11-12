using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for managing Doodle-style meeting polls
/// </summary>
public interface IPollService
{
    /// <summary>
    /// Create a new scheduling poll
    /// </summary>
    Task<SchedulingPoll> CreatePollAsync(
        string title,
        string organizerEmail,
        string organizerName,
        List<DateTime> proposedStartTimes,
        int durationMinutes,
        string? description = null,
        string? location = null,
        DateTime? deadline = null);

    /// <summary>
    /// Get a poll by ID
    /// </summary>
    Task<SchedulingPoll?> GetPollByIdAsync(Guid pollId);

    /// <summary>
    /// Get all polls created by a user
    /// </summary>
    Task<List<SchedulingPoll>> GetPollsByOrganizerAsync(string organizerEmail);

    /// <summary>
    /// Get active polls (not finalized and not expired)
    /// </summary>
    Task<List<SchedulingPoll>> GetActivePollsAsync();

    /// <summary>
    /// Add time slots to an existing poll
    /// </summary>
    Task<SchedulingPoll> AddTimeSlotsAsync(Guid pollId, List<DateTime> startTimes);

    /// <summary>
    /// Remove a time slot from a poll
    /// </summary>
    Task<SchedulingPoll> RemoveTimeSlotAsync(Guid pollId, Guid timeSlotId);

    /// <summary>
    /// Submit a response to a poll
    /// </summary>
    Task<PollResponse> SubmitResponseAsync(
        Guid pollId,
        Guid timeSlotId,
        string respondentEmail,
        string respondentName,
        PollResponseType response,
        string? comment = null);

    /// <summary>
    /// Submit multiple responses at once
    /// </summary>
    Task<List<PollResponse>> SubmitMultipleResponsesAsync(
        Guid pollId,
        string respondentEmail,
        string respondentName,
        Dictionary<Guid, PollResponseType> responses);

    /// <summary>
    /// Update an existing response
    /// </summary>
    Task<PollResponse> UpdateResponseAsync(Guid responseId, PollResponseType newResponse, string? comment = null);

    /// <summary>
    /// Get all responses for a poll
    /// </summary>
    Task<List<PollResponse>> GetPollResponsesAsync(Guid pollId);

    /// <summary>
    /// Get responses for a specific time slot
    /// </summary>
    Task<List<PollResponse>> GetTimeSlotResponsesAsync(Guid timeSlotId);

    /// <summary>
    /// Get a specific user's responses to a poll
    /// </summary>
    Task<List<PollResponse>> GetUserPollResponsesAsync(Guid pollId, string userEmail);

    /// <summary>
    /// Finalize a poll by selecting a time slot
    /// </summary>
    Task<SchedulingPoll> FinalizePollAsync(Guid pollId, Guid selectedTimeSlotId);

    /// <summary>
    /// Cancel/deactivate a poll
    /// </summary>
    Task<SchedulingPoll> CancelPollAsync(Guid pollId);

    /// <summary>
    /// Get poll results with statistics
    /// </summary>
    Task<SchedulingPoll> GetPollResultsAsync(Guid pollId);

    /// <summary>
    /// Get the most popular time slot from a poll
    /// </summary>
    Task<PollTimeSlot?> GetMostPopularTimeSlotAsync(Guid pollId);

    /// <summary>
    /// Send poll invitation emails to attendees
    /// </summary>
    Task SendPollInvitationsAsync(Guid pollId, List<string> attendeeEmails);

    /// <summary>
    /// Send poll reminder emails
    /// </summary>
    Task SendPollRemindersAsync(Guid pollId, List<string> attendeeEmails);

    /// <summary>
    /// Create an event from a finalized poll
    /// </summary>
    Task<Event> CreateEventFromPollAsync(Guid pollId);
}
