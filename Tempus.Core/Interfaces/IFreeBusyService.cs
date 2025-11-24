using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for managing free/busy information sharing
/// </summary>
public interface IFreeBusyService
{
    /// <summary>
    /// Get free/busy information for a specific user within a date range
    /// </summary>
    /// <param name="targetUserId">User whose free/busy information is being requested</param>
    /// <param name="requestingUserId">User requesting the information</param>
    /// <param name="startTime">Start of the date range (UTC)</param>
    /// <param name="endTime">End of the date range (UTC)</param>
    /// <param name="includeDetails">Whether to include event details (requires appropriate permissions)</param>
    /// <returns>Free/busy information for the requested user</returns>
    Task<FreeBusyInfo?> GetFreeBusyInfoAsync(string targetUserId, string requestingUserId,
        DateTime startTime, DateTime endTime, bool includeDetails = false);

    /// <summary>
    /// Get free/busy information for multiple users
    /// </summary>
    /// <param name="targetUserIds">List of user IDs whose free/busy information is being requested</param>
    /// <param name="requestingUserId">User requesting the information</param>
    /// <param name="startTime">Start of the date range (UTC)</param>
    /// <param name="endTime">End of the date range (UTC)</param>
    /// <param name="includeDetails">Whether to include event details (requires appropriate permissions)</param>
    /// <returns>List of free/busy information for the requested users</returns>
    Task<List<FreeBusyInfo>> GetFreeBusyInfoForMultipleUsersAsync(List<string> targetUserIds,
        string requestingUserId, DateTime startTime, DateTime endTime, bool includeDetails = false);

    /// <summary>
    /// Check if a user can view another user's free/busy information
    /// </summary>
    /// <param name="targetUserId">User whose free/busy information is being accessed</param>
    /// <param name="requestingUserId">User requesting access</param>
    /// <returns>True if the requesting user can view free/busy information</returns>
    Task<bool> CanViewFreeBusyAsync(string targetUserId, string requestingUserId);

    /// <summary>
    /// Find available time slots for a meeting with multiple attendees
    /// </summary>
    /// <param name="attendeeUserIds">List of attendee user IDs</param>
    /// <param name="requestingUserId">User organizing the meeting</param>
    /// <param name="startTime">Start of the search range (UTC)</param>
    /// <param name="endTime">End of the search range (UTC)</param>
    /// <param name="durationMinutes">Required meeting duration in minutes</param>
    /// <param name="workingHoursOnly">Whether to only suggest times during working hours</param>
    /// <returns>List of available time slots</returns>
    Task<List<FreeBusyTimeSlot>> FindAvailableTimeSlotsAsync(List<string> attendeeUserIds,
        string requestingUserId, DateTime startTime, DateTime endTime,
        int durationMinutes, bool workingHoursOnly = true);
}
