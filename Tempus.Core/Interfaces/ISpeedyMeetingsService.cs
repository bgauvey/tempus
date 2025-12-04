using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for applying speedy meetings logic to automatically shorten meetings
/// </summary>
public interface ISpeedyMeetingsService
{
    /// <summary>
    /// Check if speedy meetings should be applied for a user
    /// </summary>
    Task<bool> IsEnabledForUserAsync(string userId);

    /// <summary>
    /// Apply speedy meetings logic to an event's end time
    /// Returns adjusted end time or original if speedy meetings shouldn't apply
    /// </summary>
    Task<DateTime> ApplySpeedyMeetingsAsync(string userId, DateTime startTime, DateTime originalEndTime);

    /// <summary>
    /// Calculate the adjusted duration after applying speedy meetings
    /// </summary>
    Task<int> GetAdjustedDurationAsync(string userId, int originalDurationMinutes);

    /// <summary>
    /// Check if speedy meetings should apply to an event based on its duration
    /// </summary>
    Task<bool> ShouldApplyToEventAsync(string userId, int durationMinutes);

    /// <summary>
    /// Get the number of minutes to shorten meetings for a user
    /// </summary>
    Task<int> GetSpeedyMeetingsMinutesAsync(string userId);
}
