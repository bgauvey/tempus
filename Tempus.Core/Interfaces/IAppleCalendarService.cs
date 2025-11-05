using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IAppleCalendarService
{
    /// <summary>
    /// Connects to Apple Calendar using CalDAV credentials
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="serverUrl">CalDAV server URL (e.g., https://caldav.icloud.com)</param>
    /// <param name="username">Apple ID email</param>
    /// <param name="appSpecificPassword">App-specific password from Apple</param>
    /// <returns>CalendarIntegration entity</returns>
    Task<CalendarIntegration> ConnectAsync(string userId, string serverUrl, string username, string appSpecificPassword);

    /// <summary>
    /// Tests CalDAV connection credentials
    /// </summary>
    /// <param name="serverUrl">CalDAV server URL</param>
    /// <param name="username">Apple ID email</param>
    /// <param name="appSpecificPassword">App-specific password</param>
    /// <returns>True if connection is successful</returns>
    Task<bool> TestConnectionAsync(string serverUrl, string username, string appSpecificPassword);

    /// <summary>
    /// Syncs events from Apple Calendar to Tempus
    /// </summary>
    Task<int> SyncFromAppleAsync(string userId);

    /// <summary>
    /// Syncs events from Tempus to Apple Calendar
    /// </summary>
    Task<int> SyncToAppleAsync(string userId);

    /// <summary>
    /// Two-way sync between Apple Calendar and Tempus
    /// </summary>
    Task<(int imported, int exported)> SyncBothWaysAsync(string userId);

    /// <summary>
    /// Gets list of available calendars from Apple CalDAV server
    /// </summary>
    Task<List<(string id, string name)>> GetCalendarListAsync(string userId);

    /// <summary>
    /// Updates the CalDAV credentials for an existing integration
    /// </summary>
    Task<bool> UpdateCredentialsAsync(string userId, string serverUrl, string username, string appSpecificPassword);
}
