using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface INotificationSchedulerService
{
    /// <summary>
    /// Gets all pending notifications that should be sent now
    /// </summary>
    /// <param name="checkTimeUtc">The current time in UTC to check against</param>
    /// <param name="toleranceSeconds">Tolerance window in seconds (default 30)</param>
    /// <returns>List of notifications to send</returns>
    Task<List<PendingNotification>> GetPendingNotificationsAsync(DateTime checkTimeUtc, int toleranceSeconds = 30);

    /// <summary>
    /// Checks if a notification has already been sent for a specific event/reminder combination
    /// </summary>
    Task<bool> HasNotificationBeenSentAsync(Guid eventId, int reminderMinutes, DateTime eventStartTimeUtc, string userId);

    /// <summary>
    /// Creates a notification record for a reminder
    /// </summary>
    Task CreateNotificationRecordAsync(Guid eventId, int reminderMinutes, DateTime eventStartTimeUtc, string userId);

    /// <summary>
    /// Sends a browser notification for an event reminder
    /// </summary>
    Task SendBrowserNotificationAsync(Event evt, int reminderMinutes, string userId);
}

/// <summary>
/// Represents a notification that is pending to be sent
/// </summary>
public class PendingNotification
{
    public Event Event { get; set; } = null!;
    public int ReminderMinutes { get; set; }
    public DateTime ScheduledTimeUtc { get; set; }
    public string UserId { get; set; } = string.Empty;
}
