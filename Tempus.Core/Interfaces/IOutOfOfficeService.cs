using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IOutOfOfficeService
{
    /// <summary>
    /// Create a new out-of-office status for a user
    /// </summary>
    Task<OutOfOfficeStatus> CreateAsync(OutOfOfficeStatus status);

    /// <summary>
    /// Update an existing out-of-office status
    /// </summary>
    Task<OutOfOfficeStatus> UpdateAsync(OutOfOfficeStatus status);

    /// <summary>
    /// Delete an out-of-office status
    /// </summary>
    Task<bool> DeleteAsync(Guid id, string userId);

    /// <summary>
    /// Get a specific out-of-office status by ID
    /// </summary>
    Task<OutOfOfficeStatus?> GetByIdAsync(Guid id, string userId);

    /// <summary>
    /// Get all out-of-office statuses for a user
    /// </summary>
    Task<List<OutOfOfficeStatus>> GetAllForUserAsync(string userId, bool activeOnly = false);

    /// <summary>
    /// Get the active out-of-office status for a user at a specific time
    /// </summary>
    Task<OutOfOfficeStatus?> GetActiveStatusAtTimeAsync(string userId, DateTime dateTime);

    /// <summary>
    /// Check if a user is out of office at a specific time
    /// </summary>
    Task<bool> IsUserOutOfOfficeAsync(string userId, DateTime dateTime);

    /// <summary>
    /// Check if a meeting should be auto-declined for a user
    /// </summary>
    Task<(bool ShouldDecline, string? Reason)> ShouldAutoDeclineMeetingAsync(
        string userId,
        DateTime meetingStart,
        DateTime meetingEnd,
        string organizerEmail,
        bool isOptional);

    /// <summary>
    /// Process auto-decline for a meeting invitation
    /// </summary>
    Task ProcessAutoDeclineAsync(Event meetingEvent, Attendee attendee);

    /// <summary>
    /// Send auto-responder email if applicable
    /// </summary>
    Task SendAutoResponderIfNeededAsync(string userId, string senderEmail, Event? relatedEvent = null);

    /// <summary>
    /// Get upcoming out-of-office periods for a user
    /// </summary>
    Task<List<OutOfOfficeStatus>> GetUpcomingStatusesAsync(string userId, int days = 30);

    /// <summary>
    /// Check if user has focus time enabled and should decline meetings
    /// </summary>
    Task<bool> IsFocusTimeActiveAsync(string userId, DateTime dateTime);

    /// <summary>
    /// Validate out-of-office dates don't conflict with existing statuses
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateDateRangeAsync(
        string userId,
        DateTime startDate,
        DateTime endDate,
        Guid? excludeId = null);
}
