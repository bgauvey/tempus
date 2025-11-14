namespace Tempus.Core.Models;

/// <summary>
/// Represents a subscription to a public calendar (holidays, sports, etc.)
/// </summary>
public class PublicCalendar
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User who subscribed to this public calendar
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Name of the public calendar
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the public calendar
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL to the ICS feed for this calendar
    /// </summary>
    public string IcsUrl { get; set; } = string.Empty;

    /// <summary>
    /// Category of the public calendar
    /// </summary>
    public PublicCalendarCategory Category { get; set; } = PublicCalendarCategory.Other;

    /// <summary>
    /// Display color for events from this calendar
    /// </summary>
    public string Color { get; set; } = "#3498db";

    /// <summary>
    /// Whether this calendar is currently active/visible
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last time the calendar was synced
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Number of events imported from this calendar
    /// </summary>
    public int EventCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Get display name for category
    /// </summary>
    public string GetCategoryDisplayName() => Category switch
    {
        PublicCalendarCategory.Holidays => "Holidays",
        PublicCalendarCategory.Sports => "Sports",
        PublicCalendarCategory.School => "School",
        PublicCalendarCategory.Religious => "Religious",
        PublicCalendarCategory.Weather => "Weather",
        PublicCalendarCategory.Entertainment => "Entertainment",
        PublicCalendarCategory.Other => "Other",
        _ => "Unknown"
    };
}

/// <summary>
/// Categories for public calendars
/// </summary>
public enum PublicCalendarCategory
{
    Holidays,
    Sports,
    School,
    Religious,
    Weather,
    Entertainment,
    Other
}
