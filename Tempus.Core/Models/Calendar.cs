using Tempus.Core.Enums;

namespace Tempus.Core.Models;

/// <summary>
/// Represents a personal calendar within a user's account
/// Supports multiple calendars per user with individual settings
/// </summary>
public class Calendar
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Ownership
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Basic Info
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#1E88E5"; // Default blue

    // Display & Behavior
    public bool IsVisible { get; set; } = true; // Toggle visibility in UI
    public bool IsDefault { get; set; } = false; // Is this the default calendar for new events
    public int SortOrder { get; set; } = 0; // Display order in calendar list

    // Calendar-specific Settings (override global defaults)
    public string? DefaultEventColor { get; set; } // Override global default
    public EventVisibility? DefaultEventVisibility { get; set; }
    public string? DefaultLocation { get; set; }
    public string? DefaultReminderTimes { get; set; } // Comma-separated minutes
    public int? DefaultMeetingDuration { get; set; } // Minutes

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation - Events in this calendar
    public List<Event> Events { get; set; } = new();
}
