using Tempus.Core.Enums;

namespace Tempus.Core.Models;

/// <summary>
/// Represents a user's working location status for a specific time period
/// </summary>
public class WorkingLocationStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User this location status belongs to
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Start of the location period (in UTC)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End of the location period (in UTC)
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Type of working location
    /// </summary>
    public WorkingLocationType LocationType { get; set; } = WorkingLocationType.NotSet;

    /// <summary>
    /// Custom location description (e.g., "Client Office - Downtown", "WeWork Brooklyn")
    /// </summary>
    public string? LocationDescription { get; set; }

    /// <summary>
    /// Physical address or location details
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Additional notes about the working location
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether to show this location in calendar view
    /// </summary>
    public bool ShowInCalendar { get; set; } = true;

    /// <summary>
    /// Whether to show location details to team members
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// Whether this location status is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Color to display in calendar (hex code)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Whether to send notifications for this location change
    /// </summary>
    public bool SendNotifications { get; set; } = true;

    /// <summary>
    /// List of user IDs who should be notified about this location change
    /// </summary>
    public List<string> NotifyUserIds { get; set; } = new();

    /// <summary>
    /// Whether this is a recurring location pattern
    /// </summary>
    public bool IsRecurring { get; set; } = false;

    /// <summary>
    /// Recurrence pattern (e.g., "Every Monday and Friday")
    /// </summary>
    public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;

    /// <summary>
    /// Days of week for recurring locations (comma-separated: "1,5" for Mon,Fri)
    /// </summary>
    public string? RecurrenceDaysOfWeek { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Check if this location status is active at a given time
    /// </summary>
    public bool IsActiveAt(DateTime dateTime)
    {
        return IsActive &&
               dateTime >= StartDate &&
               dateTime <= EndDate;
    }

    /// <summary>
    /// Get display name for the location
    /// </summary>
    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(LocationDescription))
            return LocationDescription;

        return LocationType switch
        {
            WorkingLocationType.Office => "Office",
            WorkingLocationType.Home => "Working from Home",
            WorkingLocationType.Remote => "Remote",
            WorkingLocationType.Traveling => "Traveling",
            WorkingLocationType.Hybrid => "Hybrid",
            _ => "Location Not Set"
        };
    }

    /// <summary>
    /// Get icon name for the location type
    /// </summary>
    public string GetIcon()
    {
        return LocationType switch
        {
            WorkingLocationType.Office => "business",
            WorkingLocationType.Home => "home",
            WorkingLocationType.Remote => "laptop",
            WorkingLocationType.Traveling => "flight",
            WorkingLocationType.Hybrid => "swap_horiz",
            _ => "location_on"
        };
    }
}
