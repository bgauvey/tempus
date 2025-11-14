using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface ICalendarSharingService
{
    #region Calendar Sharing

    /// <summary>
    /// Share a calendar with another user
    /// </summary>
    Task<CalendarShare> ShareCalendarAsync(Guid calendarId, string sharedWithUserId,
        CalendarSharePermission permission, string sharedByUserId, string? note = null);

    /// <summary>
    /// Update share permission for an existing share
    /// </summary>
    Task<CalendarShare> UpdateSharePermissionAsync(Guid shareId, CalendarSharePermission newPermission, string userId);

    /// <summary>
    /// Remove a calendar share
    /// </summary>
    Task<bool> RemoveShareAsync(Guid shareId, string userId);

    /// <summary>
    /// Get all calendars shared with a specific user
    /// </summary>
    Task<List<CalendarShare>> GetCalendarsSharedWithUserAsync(string userId, bool includeUnaccepted = false);

    /// <summary>
    /// Get all shares for a specific calendar
    /// </summary>
    Task<List<CalendarShare>> GetCalendarSharesAsync(Guid calendarId);

    /// <summary>
    /// Get a specific calendar share by ID
    /// </summary>
    Task<CalendarShare?> GetShareByIdAsync(Guid shareId);

    /// <summary>
    /// Accept a calendar share
    /// </summary>
    Task<bool> AcceptShareAsync(Guid shareId, string userId, string? color = null);

    /// <summary>
    /// Decline/hide a calendar share
    /// </summary>
    Task<bool> DeclineShareAsync(Guid shareId, string userId);

    /// <summary>
    /// Check if a user has specific permission on a calendar
    /// </summary>
    Task<bool> HasPermissionAsync(Guid calendarId, string userId, CalendarSharePermission requiredPermission);

    /// <summary>
    /// Get the permission level a user has on a calendar
    /// </summary>
    Task<CalendarSharePermission?> GetUserPermissionAsync(Guid calendarId, string userId);

    /// <summary>
    /// Update the visibility of a shared calendar
    /// </summary>
    Task<bool> UpdateShareVisibilityAsync(Guid shareId, bool isVisible, string userId);

    #endregion

    #region Public Calendars

    /// <summary>
    /// Subscribe to a public calendar
    /// </summary>
    Task<PublicCalendar> SubscribeToPublicCalendarAsync(string userId, string name, string icsUrl,
        PublicCalendarCategory category, string? description = null, string? color = null);

    /// <summary>
    /// Unsubscribe from a public calendar
    /// </summary>
    Task<bool> UnsubscribeFromPublicCalendarAsync(Guid publicCalendarId, string userId);

    /// <summary>
    /// Get all public calendar subscriptions for a user
    /// </summary>
    Task<List<PublicCalendar>> GetUserPublicCalendarsAsync(string userId, bool activeOnly = true);

    /// <summary>
    /// Update public calendar settings
    /// </summary>
    Task<PublicCalendar> UpdatePublicCalendarAsync(Guid publicCalendarId, string userId,
        string? name = null, string? color = null, bool? isActive = null);

    /// <summary>
    /// Sync a public calendar (refresh events from ICS feed)
    /// </summary>
    Task<int> SyncPublicCalendarAsync(Guid publicCalendarId, string userId);

    /// <summary>
    /// Get a public calendar by ID
    /// </summary>
    Task<PublicCalendar?> GetPublicCalendarByIdAsync(Guid publicCalendarId);

    #endregion
}
