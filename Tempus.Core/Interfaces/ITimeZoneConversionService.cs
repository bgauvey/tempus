using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for handling timezone conversions for events
/// </summary>
public interface ITimeZoneConversionService
{
    /// <summary>
    /// Convert event times to the target timezone
    /// </summary>
    /// <param name="event">The event to convert</param>
    /// <param name="targetTimeZoneId">Target timezone ID (IANA format)</param>
    /// <returns>New event with converted times</returns>
    Event ConvertEventToTimeZone(Event @event, string targetTimeZoneId);

    /// <summary>
    /// Convert a DateTime from one timezone to another
    /// </summary>
    DateTime ConvertTime(DateTime dateTime, string fromTimeZoneId, string toTimeZoneId);

    /// <summary>
    /// Get a list of all available timezones
    /// </summary>
    List<TimeZoneInfo> GetAvailableTimeZones();

    /// <summary>
    /// Get common/popular timezones for quick selection
    /// </summary>
    List<TimeZoneInfo> GetCommonTimeZones();

    /// <summary>
    /// Get user-friendly display name for timezone
    /// </summary>
    string GetTimeZoneDisplayName(string timeZoneId);

    /// <summary>
    /// Convert event to user's local timezone based on their settings
    /// </summary>
    Event ConvertEventToUserTimeZone(Event @event, string userTimeZoneId);
}
