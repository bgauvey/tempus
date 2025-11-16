using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for managing booking pages and public appointment scheduling
/// </summary>
public interface IBookingPageService
{
    /// <summary>
    /// Get available time slots for a booking page
    /// </summary>
    /// <param name="bookingPageId">The booking page ID</param>
    /// <param name="startDate">Start date to search for slots</param>
    /// <param name="endDate">End date to search for slots</param>
    /// <param name="durationMinutes">Duration of the appointment</param>
    /// <returns>List of available time slots</returns>
    Task<List<DateTime>> GetAvailableTimeSlotsAsync(
        Guid bookingPageId,
        DateTime startDate,
        DateTime endDate,
        int durationMinutes);

    /// <summary>
    /// Get available time slots for a booking page by slug
    /// </summary>
    /// <param name="slug">The booking page slug</param>
    /// <param name="startDate">Start date to search for slots</param>
    /// <param name="endDate">End date to search for slots</param>
    /// <param name="durationMinutes">Duration of the appointment</param>
    /// <returns>List of available time slots</returns>
    Task<List<DateTime>> GetAvailableTimeSlotsBySlugAsync(
        string slug,
        DateTime startDate,
        DateTime endDate,
        int durationMinutes);

    /// <summary>
    /// Create a booking (appointment) on a booking page
    /// </summary>
    /// <param name="bookingPageId">The booking page ID</param>
    /// <param name="startTime">Start time of the appointment</param>
    /// <param name="durationMinutes">Duration of the appointment</param>
    /// <param name="guestName">Guest name</param>
    /// <param name="guestEmail">Guest email</param>
    /// <param name="guestPhone">Guest phone (optional)</param>
    /// <param name="guestNotes">Guest notes (optional)</param>
    /// <returns>The created event</returns>
    Task<Event> CreateBookingAsync(
        Guid bookingPageId,
        DateTime startTime,
        int durationMinutes,
        string guestName,
        string guestEmail,
        string? guestPhone = null,
        string? guestNotes = null);

    /// <summary>
    /// Create a booking (appointment) on a booking page by slug
    /// </summary>
    /// <param name="slug">The booking page slug</param>
    /// <param name="startTime">Start time of the appointment</param>
    /// <param name="durationMinutes">Duration of the appointment</param>
    /// <param name="guestName">Guest name</param>
    /// <param name="guestEmail">Guest email</param>
    /// <param name="guestPhone">Guest phone (optional)</param>
    /// <param name="guestNotes">Guest notes (optional)</param>
    /// <returns>The created event</returns>
    Task<Event> CreateBookingBySlugAsync(
        string slug,
        DateTime startTime,
        int durationMinutes,
        string guestName,
        string guestEmail,
        string? guestPhone = null,
        string? guestNotes = null);

    /// <summary>
    /// Validate if a time slot is available for booking
    /// </summary>
    /// <param name="bookingPageId">The booking page ID</param>
    /// <param name="startTime">Proposed start time</param>
    /// <param name="durationMinutes">Duration of the appointment</param>
    /// <returns>True if the time slot is available</returns>
    Task<bool> IsTimeSlotAvailableAsync(
        Guid bookingPageId,
        DateTime startTime,
        int durationMinutes);

    /// <summary>
    /// Check if the booking page has reached its daily booking limit
    /// </summary>
    /// <param name="bookingPageId">The booking page ID</param>
    /// <param name="date">The date to check</param>
    /// <returns>True if the limit has been reached</returns>
    Task<bool> HasReachedDailyLimitAsync(Guid bookingPageId, DateTime date);

    /// <summary>
    /// Generate a unique slug for a booking page
    /// </summary>
    /// <param name="baseName">Base name to generate slug from</param>
    /// <returns>A unique slug</returns>
    Task<string> GenerateUniqueSlugAsync(string baseName);

    /// <summary>
    /// Get the booking page's timezone
    /// </summary>
    /// <param name="bookingPageId">The booking page ID</param>
    /// <returns>TimeZoneInfo for the booking page</returns>
    Task<TimeZoneInfo> GetBookingPageTimeZoneAsync(Guid bookingPageId);
}
