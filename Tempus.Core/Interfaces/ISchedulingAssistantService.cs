using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for intelligent meeting scheduling and availability analysis
/// </summary>
public interface ISchedulingAssistantService
{
    /// <summary>
    /// Find optimal meeting times for a list of attendees
    /// </summary>
    /// <param name="attendeeEmails">List of attendee email addresses</param>
    /// <param name="durationMinutes">Duration of the meeting in minutes</param>
    /// <param name="searchStartDate">Start date for search window</param>
    /// <param name="searchEndDate">End date for search window</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return</param>
    /// <returns>List of scheduling suggestions ranked by score</returns>
    Task<List<SchedulingSuggestion>> FindOptimalTimesAsync(
        List<string> attendeeEmails,
        int durationMinutes,
        DateTime searchStartDate,
        DateTime searchEndDate,
        int maxSuggestions = 5);

    /// <summary>
    /// Analyze availability for a specific time slot
    /// </summary>
    /// <param name="attendeeEmails">List of attendee email addresses</param>
    /// <param name="startTime">Proposed start time</param>
    /// <param name="endTime">Proposed end time</param>
    /// <returns>Availability information for the time slot</returns>
    Task<AvailabilitySlot> AnalyzeAvailabilityAsync(
        List<string> attendeeEmails,
        DateTime startTime,
        DateTime endTime);

    /// <summary>
    /// Get availability grid for multiple time slots
    /// </summary>
    /// <param name="attendeeEmails">List of attendee email addresses</param>
    /// <param name="startDate">Start date for grid</param>
    /// <param name="endDate">End date for grid</param>
    /// <param name="slotDurationMinutes">Duration of each slot in minutes</param>
    /// <returns>List of availability slots</returns>
    Task<List<AvailabilitySlot>> GetAvailabilityGridAsync(
        List<string> attendeeEmails,
        DateTime startDate,
        DateTime endDate,
        int slotDurationMinutes = 30);

    /// <summary>
    /// Find next available time slot for all attendees
    /// </summary>
    /// <param name="attendeeEmails">List of attendee email addresses</param>
    /// <param name="durationMinutes">Duration of the meeting in minutes</param>
    /// <param name="startSearchFrom">When to start searching (defaults to now)</param>
    /// <returns>First available time slot or null if none found</returns>
    Task<SchedulingSuggestion?> FindNextAvailableSlotAsync(
        List<string> attendeeEmails,
        int durationMinutes,
        DateTime? startSearchFrom = null);

    /// <summary>
    /// Detect conflicts for a specific time slot
    /// </summary>
    /// <param name="attendeeEmails">List of attendee email addresses</param>
    /// <param name="startTime">Proposed start time</param>
    /// <param name="endTime">Proposed end time</param>
    /// <param name="excludeEventId">Event ID to exclude from conflict check</param>
    /// <returns>List of conflicting events</returns>
    Task<List<Event>> DetectConflictsAsync(
        List<string> attendeeEmails,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeEventId = null);

    /// <summary>
    /// Find alternative times to resolve conflicts
    /// </summary>
    /// <param name="existingEventId">ID of the event with conflicts</param>
    /// <param name="maxSuggestions">Maximum number of alternatives to suggest</param>
    /// <returns>List of alternative time suggestions</returns>
    Task<List<SchedulingSuggestion>> SuggestAlternativeTimesAsync(
        Guid existingEventId,
        int maxSuggestions = 3);

    /// <summary>
    /// Check if a specific time works for all attendees
    /// </summary>
    /// <param name="attendeeEmails">List of attendee email addresses</param>
    /// <param name="startTime">Proposed start time</param>
    /// <param name="endTime">Proposed end time</param>
    /// <returns>True if all attendees are available</returns>
    Task<bool> IsTimeAvailableForAllAsync(
        List<string> attendeeEmails,
        DateTime startTime,
        DateTime endTime);
}
