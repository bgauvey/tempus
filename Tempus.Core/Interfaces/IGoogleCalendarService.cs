using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IGoogleCalendarService
{
    /// <summary>
    /// Gets the OAuth authorization URL for Google Calendar
    /// </summary>
    string GetAuthorizationUrl(string userId, string redirectUri);

    /// <summary>
    /// Exchanges authorization code for access and refresh tokens
    /// </summary>
    Task<CalendarIntegration> ExchangeCodeForTokensAsync(string code, string userId, string redirectUri);

    /// <summary>
    /// Refreshes the access token using the refresh token
    /// </summary>
    Task<bool> RefreshAccessTokenAsync(CalendarIntegration integration);

    /// <summary>
    /// Syncs events from Google Calendar to Tempus
    /// </summary>
    Task<int> SyncFromGoogleAsync(string userId);

    /// <summary>
    /// Syncs events from Tempus to Google Calendar
    /// </summary>
    Task<int> SyncToGoogleAsync(string userId);

    /// <summary>
    /// Two-way sync between Google Calendar and Tempus
    /// </summary>
    Task<(int imported, int exported)> SyncBothWaysAsync(string userId);

    /// <summary>
    /// Gets list of available calendars from Google
    /// </summary>
    Task<List<(string id, string name)>> GetCalendarListAsync(string userId);
}
