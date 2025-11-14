using Tempus.Core.Enums;

namespace Tempus.Core.Models;

/// <summary>
/// Represents an out-of-office or focus time period for a user
/// </summary>
public class OutOfOfficeStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User this OOO status belongs to
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Start of the OOO period (in UTC)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End of the OOO period (in UTC)
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Type of availability status
    /// </summary>
    public AvailabilityStatusType StatusType { get; set; } = AvailabilityStatusType.OutOfOffice;

    /// <summary>
    /// Custom title/reason for the OOO period
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Auto-responder message to send
    /// </summary>
    public string? AutoResponderMessage { get; set; }

    /// <summary>
    /// Whether to send auto-responder emails
    /// </summary>
    public bool SendAutoResponder { get; set; } = true;

    /// <summary>
    /// Automatically decline all meeting invitations during this period
    /// </summary>
    public bool AutoDeclineMeetings { get; set; } = true;

    /// <summary>
    /// Allow declining optional meetings but keep required ones
    /// </summary>
    public bool DeclineOptionalOnly { get; set; } = false;

    /// <summary>
    /// List of email addresses that can still book meetings (e.g., manager, direct reports)
    /// </summary>
    public List<string> ExemptOrganizerEmails { get; set; } = new();

    /// <summary>
    /// Show as "Busy" on calendar instead of specific details
    /// </summary>
    public bool ShowAsBusy { get; set; } = true;

    /// <summary>
    /// Whether this OOO status is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Custom decline message to include when auto-declining
    /// </summary>
    public string? DeclineMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Check if this OOO status is active at a given time
    /// </summary>
    public bool IsActiveAt(DateTime dateTime)
    {
        return IsActive &&
               dateTime >= StartDate &&
               dateTime <= EndDate;
    }

    /// <summary>
    /// Check if an organizer is exempt from auto-decline
    /// </summary>
    public bool IsOrganizerExempt(string email)
    {
        return ExemptOrganizerEmails.Any(e =>
            e.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
