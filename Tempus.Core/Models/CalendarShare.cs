using Tempus.Core.Enums;

namespace Tempus.Core.Models;

/// <summary>
/// Represents a calendar shared with a user
/// </summary>
public class CalendarShare
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The calendar being shared
    /// </summary>
    public Guid CalendarId { get; set; }
    public Calendar? Calendar { get; set; }

    /// <summary>
    /// User the calendar is shared with
    /// </summary>
    public string SharedWithUserId { get; set; } = string.Empty;
    public ApplicationUser? SharedWithUser { get; set; }

    /// <summary>
    /// User who shared the calendar
    /// </summary>
    public string SharedByUserId { get; set; } = string.Empty;
    public ApplicationUser? SharedByUser { get; set; }

    /// <summary>
    /// Permission level for the shared calendar
    /// </summary>
    public CalendarSharePermission Permission { get; set; } = CalendarSharePermission.ViewAll;

    /// <summary>
    /// Optional note/message to the person the calendar is shared with
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Whether the shared calendar is currently accepted/visible
    /// </summary>
    public bool IsAccepted { get; set; } = false;

    /// <summary>
    /// Whether the shared calendar is currently visible in the calendar view
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Display color for the shared calendar (can be customized by recipient)
    /// </summary>
    public string? Color { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Get display name for permission level
    /// </summary>
    public string GetPermissionDisplayName() => Permission switch
    {
        CalendarSharePermission.FreeBusyOnly => "See only free/busy (hide details)",
        CalendarSharePermission.ViewAll => "See all event details",
        CalendarSharePermission.Edit => "Make changes to events",
        CalendarSharePermission.ManageSharing => "Make changes and manage sharing",
        _ => "Unknown"
    };

    /// <summary>
    /// Check if user can view event details
    /// </summary>
    public bool CanViewDetails() => Permission >= CalendarSharePermission.ViewAll;

    /// <summary>
    /// Check if user can edit events
    /// </summary>
    public bool CanEdit() => Permission >= CalendarSharePermission.Edit;

    /// <summary>
    /// Check if user can manage sharing
    /// </summary>
    public bool CanManageSharing() => Permission >= CalendarSharePermission.ManageSharing;
}
