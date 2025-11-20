namespace Tempus.Core.Models;

/// <summary>
/// Represents a free or busy time slot for free/busy information sharing
/// </summary>
public class FreeBusyTimeSlot
{
    /// <summary>
    /// Start time of the time slot (UTC)
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the time slot (UTC)
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Whether the user is busy during this time slot
    /// </summary>
    public bool IsBusy { get; set; }

    /// <summary>
    /// Optional subject/title (only shown if user has ViewAll permission or higher)
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Optional location (only shown if user has ViewAll permission or higher)
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Whether this is a private event (will always show as "Busy" with no details)
    /// </summary>
    public bool IsPrivate { get; set; }
}
