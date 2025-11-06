using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service interface for Microsoft Outlook/Office 365 calendar integration using Microsoft Graph API
/// </summary>
public interface IOutlookCalendarService
{
    /// <summary>
    /// Gets the authorization URL to initiate OAuth2 flow for Microsoft account
    /// </summary>
    /// <param name="userId">The user ID for state tracking</param>
    /// <param name="redirectUri">The callback URI after authorization</param>
    /// <returns>The authorization URL to redirect the user to</returns>
    string GetAuthorizationUrl(string userId, string redirectUri);

    /// <summary>
    /// Exchanges the authorization code for access and refresh tokens
    /// </summary>
    /// <param name="code">The authorization code from OAuth callback</param>
    /// <param name="redirectUri">The redirect URI used in the authorization request</param>
    /// <param name="userId">The user ID to associate the integration with</param>
    /// <returns>The created calendar integration</returns>
    Task<CalendarIntegration> ExchangeCodeForTokensAsync(string code, string redirectUri, string userId);

    /// <summary>
    /// Tests the connection to Outlook calendar using stored credentials
    /// </summary>
    /// <param name="userId">The user ID to test connection for</param>
    /// <returns>True if connection is successful, false otherwise</returns>
    Task<bool> TestConnectionAsync(string userId);

    /// <summary>
    /// Synchronizes events from Outlook calendar to Tempus
    /// </summary>
    /// <param name="userId">The user ID to sync events for</param>
    /// <returns>The number of events imported</returns>
    Task<int> SyncFromOutlookAsync(string userId);

    /// <summary>
    /// Synchronizes events from Tempus to Outlook calendar
    /// </summary>
    /// <param name="userId">The user ID to sync events for</param>
    /// <returns>The number of events exported</returns>
    Task<int> SyncToOutlookAsync(string userId);

    /// <summary>
    /// Performs two-way synchronization between Outlook calendar and Tempus
    /// </summary>
    /// <param name="userId">The user ID to sync events for</param>
    /// <returns>A tuple containing (imported count, exported count)</returns>
    Task<(int imported, int exported)> SyncBothWaysAsync(string userId);

    /// <summary>
    /// Gets the list of available calendars for the user
    /// </summary>
    /// <param name="userId">The user ID to get calendars for</param>
    /// <returns>A list of calendar ID and name pairs</returns>
    Task<List<(string id, string name)>> GetCalendarListAsync(string userId);

    /// <summary>
    /// Updates the calendar selection for the integration
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="calendarId">The calendar ID to use</param>
    /// <param name="calendarName">The calendar name</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateCalendarSelectionAsync(string userId, string calendarId, string calendarName);
}
