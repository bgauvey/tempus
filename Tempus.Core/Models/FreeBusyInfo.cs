namespace Tempus.Core.Models;

/// <summary>
/// Represents free/busy information for a user within a date range
/// </summary>
public class FreeBusyInfo
{
    /// <summary>
    /// User ID this free/busy information belongs to
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Start of the free/busy time range (UTC)
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End of the free/busy time range (UTC)
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// List of busy time slots
    /// </summary>
    public List<FreeBusyTimeSlot> BusyTimes { get; set; } = new();

    /// <summary>
    /// Whether the user has granted permission to view details
    /// </summary>
    public bool CanViewDetails { get; set; } = false;

    /// <summary>
    /// User's timezone
    /// </summary>
    public string TimeZone { get; set; } = "UTC";
}
