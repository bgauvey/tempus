namespace Tempus.Core.Models;

/// <summary>
/// Represents a public booking page (like Calendly) for scheduling appointments
/// </summary>
public class BookingPage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Ownership
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Public URL slug (e.g., "john-smith" for /book/john-smith)
    public string Slug { get; set; } = string.Empty;

    // Basic Information
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WelcomeMessage { get; set; }

    // Appointment Settings
    public int DurationMinutes { get; set; } = 30; // Default duration
    public string? AvailableDurations { get; set; } // Comma-separated: "15,30,60" (allows guest to choose)

    // Buffer Time (prevents back-to-back bookings)
    public int BufferBeforeMinutes { get; set; } = 0; // Buffer before appointment
    public int BufferAfterMinutes { get; set; } = 0; // Buffer after appointment

    // Booking Limits & Rules
    public int? MaxBookingsPerDay { get; set; } // Null = unlimited
    public int MinimumNoticeMinutes { get; set; } = 60; // How far in advance must they book (default 1 hour)
    public int MaxAdvanceBookingDays { get; set; } = 60; // How far in the future can they book (default 60 days)

    // Availability Configuration
    // These define the general availability window
    // Actual free/busy will be calculated from calendar events
    public string? AvailableDaysOfWeek { get; set; } // Comma-separated: "1,2,3,4,5" (Mon-Fri)
    public TimeSpan? DailyStartTime { get; set; } // e.g., 09:00:00
    public TimeSpan? DailyEndTime { get; set; } // e.g., 17:00:00
    public string? TimeZoneId { get; set; } // IANA timezone (e.g., "America/New_York")

    // Calendar Integration
    public Guid? CalendarId { get; set; } // Which calendar to add booked appointments to
    public Calendar? Calendar { get; set; }

    // Booking Form Configuration
    public bool RequireGuestName { get; set; } = true;
    public bool RequireGuestEmail { get; set; } = true;
    public bool RequireGuestPhone { get; set; } = false;
    public bool AllowGuestNotes { get; set; } = true;
    public string? CustomFields { get; set; } // JSON array of custom form fields

    // Confirmation Settings
    public string? ConfirmationMessage { get; set; }
    public string? RedirectUrl { get; set; } // Redirect after booking
    public bool SendConfirmationEmail { get; set; } = true;
    public bool SendReminderEmail { get; set; } = true;
    public int ReminderMinutesBeforeAppointment { get; set; } = 1440; // Default 24 hours

    // Display & Behavior
    public bool IsActive { get; set; } = true;
    public bool ShowOnPublicProfile { get; set; } = false; // Show in user's public profile/directory
    public string? Color { get; set; } // Theme color for the booking page
    public string? LogoUrl { get; set; } // Custom branding

    // Meeting Details
    public string? Location { get; set; } // Physical location or "Virtual"
    public bool IncludeVideoConference { get; set; } = false;
    public string? VideoConferenceProvider { get; set; } // "Zoom", "Teams", "Meet"

    // Analytics
    public int TotalBookings { get; set; } = 0;
    public DateTime? LastBookingAt { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the list of available duration options
    /// </summary>
    public List<int> GetAvailableDurations()
    {
        if (string.IsNullOrWhiteSpace(AvailableDurations))
        {
            return new List<int> { DurationMinutes };
        }

        return AvailableDurations
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => int.TryParse(d.Trim(), out var minutes) ? minutes : 0)
            .Where(d => d > 0)
            .OrderBy(d => d)
            .ToList();
    }

    /// <summary>
    /// Gets the list of available days of week (0=Sunday, 6=Saturday)
    /// </summary>
    public List<int> GetAvailableDaysOfWeek()
    {
        if (string.IsNullOrWhiteSpace(AvailableDaysOfWeek))
        {
            return new List<int> { 1, 2, 3, 4, 5 }; // Default Mon-Fri
        }

        return AvailableDaysOfWeek
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => int.TryParse(d.Trim(), out var day) ? day : -1)
            .Where(d => d >= 0 && d <= 6)
            .ToList();
    }

    /// <summary>
    /// Checks if the booking page is currently accepting bookings
    /// </summary>
    public bool IsAcceptingBookings()
    {
        return IsActive;
    }

    /// <summary>
    /// Generates the public booking URL
    /// </summary>
    public string GetPublicUrl(string baseUrl)
    {
        return $"{baseUrl}/book/{Slug}";
    }
}
