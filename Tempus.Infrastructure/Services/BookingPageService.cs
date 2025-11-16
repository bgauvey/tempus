using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class BookingPageService : IBookingPageService
{
    private readonly IBookingPageRepository _bookingPageRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<BookingPageService> _logger;

    public BookingPageService(
        IBookingPageRepository bookingPageRepository,
        IEventRepository eventRepository,
        IDbContextFactory<TempusDbContext> contextFactory,
        IEmailNotificationService emailService,
        ILogger<BookingPageService> logger)
    {
        _bookingPageRepository = bookingPageRepository;
        _eventRepository = eventRepository;
        _contextFactory = contextFactory;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<DateTime>> GetAvailableTimeSlotsAsync(
        Guid bookingPageId,
        DateTime startDate,
        DateTime endDate,
        int durationMinutes)
    {
        var bookingPage = await _bookingPageRepository.GetByIdAsync(bookingPageId, string.Empty);
        if (bookingPage == null)
        {
            throw new InvalidOperationException("Booking page not found");
        }

        return await GetAvailableSlotsForBookingPageAsync(bookingPage, startDate, endDate, durationMinutes);
    }

    public async Task<List<DateTime>> GetAvailableTimeSlotsBySlugAsync(
        string slug,
        DateTime startDate,
        DateTime endDate,
        int durationMinutes)
    {
        var bookingPage = await _bookingPageRepository.GetBySlugAsync(slug);
        if (bookingPage == null || !bookingPage.IsActive)
        {
            throw new InvalidOperationException("Booking page not found or inactive");
        }

        return await GetAvailableSlotsForBookingPageAsync(bookingPage, startDate, endDate, durationMinutes);
    }

    public async Task<Event> CreateBookingAsync(
        Guid bookingPageId,
        DateTime startTime,
        int durationMinutes,
        string guestName,
        string guestEmail,
        string? guestPhone = null,
        string? guestNotes = null)
    {
        var bookingPage = await _bookingPageRepository.GetByIdAsync(bookingPageId, string.Empty);
        if (bookingPage == null)
        {
            throw new InvalidOperationException("Booking page not found");
        }

        return await CreateBookingForPageAsync(
            bookingPage, startTime, durationMinutes, guestName, guestEmail, guestPhone, guestNotes);
    }

    public async Task<Event> CreateBookingBySlugAsync(
        string slug,
        DateTime startTime,
        int durationMinutes,
        string guestName,
        string guestEmail,
        string? guestPhone = null,
        string? guestNotes = null)
    {
        var bookingPage = await _bookingPageRepository.GetBySlugAsync(slug);
        if (bookingPage == null || !bookingPage.IsActive)
        {
            throw new InvalidOperationException("Booking page not found or inactive");
        }

        return await CreateBookingForPageAsync(
            bookingPage, startTime, durationMinutes, guestName, guestEmail, guestPhone, guestNotes);
    }

    public async Task<bool> IsTimeSlotAvailableAsync(
        Guid bookingPageId,
        DateTime startTime,
        int durationMinutes)
    {
        var bookingPage = await _bookingPageRepository.GetByIdAsync(bookingPageId, string.Empty);
        if (bookingPage == null || !bookingPage.IsActive)
        {
            return false;
        }

        var endTime = startTime.AddMinutes(durationMinutes);

        // Check if time is within configured availability
        if (!IsWithinAvailabilityWindow(bookingPage, startTime))
        {
            return false;
        }

        // Check minimum notice requirement
        var now = DateTime.UtcNow;
        if (startTime < now.AddMinutes(bookingPage.MinimumNoticeMinutes))
        {
            return false;
        }

        // Check maximum advance booking limit
        if (startTime > now.AddDays(bookingPage.MaxAdvanceBookingDays))
        {
            return false;
        }

        // Check daily booking limit
        if (await HasReachedDailyLimitAsync(bookingPageId, startTime.Date))
        {
            return false;
        }

        // Check for conflicts with existing events (including buffer times)
        var bufferStart = startTime.AddMinutes(-bookingPage.BufferBeforeMinutes);
        var bufferEnd = endTime.AddMinutes(bookingPage.BufferAfterMinutes);

        var conflicts = await _eventRepository.GetEventsByDateRangeAsync(
            bufferStart,
            bufferEnd,
            bookingPage.UserId);

        return conflicts.Count == 0;
    }

    public async Task<bool> HasReachedDailyLimitAsync(Guid bookingPageId, DateTime date)
    {
        var bookingPage = await _bookingPageRepository.GetByIdAsync(bookingPageId, string.Empty);
        if (bookingPage == null || !bookingPage.MaxBookingsPerDay.HasValue)
        {
            return false;
        }

        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var bookingsToday = await _eventRepository.GetEventsByDateRangeAsync(
            dayStart,
            dayEnd,
            bookingPage.UserId);

        // Count bookings created via this booking page (check tags or description)
        var bookingPageBookings = bookingsToday
            .Count(e => e.Tags.Contains($"booking-page-{bookingPageId}"));

        return bookingPageBookings >= bookingPage.MaxBookingsPerDay.Value;
    }

    public async Task<string> GenerateUniqueSlugAsync(string baseName)
    {
        // Convert to lowercase, remove special characters, replace spaces with hyphens
        var slug = Regex.Replace(baseName.ToLower(), @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        // Ensure it's not empty
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "booking";
        }

        // Check if slug exists, if so, append a number
        var originalSlug = slug;
        var counter = 1;

        while (await _bookingPageRepository.SlugExistsAsync(slug))
        {
            slug = $"{originalSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    public async Task<TimeZoneInfo> GetBookingPageTimeZoneAsync(Guid bookingPageId)
    {
        var bookingPage = await _bookingPageRepository.GetByIdAsync(bookingPageId, string.Empty);
        if (bookingPage == null)
        {
            throw new InvalidOperationException("Booking page not found");
        }

        if (string.IsNullOrEmpty(bookingPage.TimeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(bookingPage.TimeZoneId);
        }
        catch
        {
            _logger.LogWarning("Invalid timezone {TimeZoneId} for booking page {BookingPageId}, falling back to UTC",
                bookingPage.TimeZoneId, bookingPageId);
            return TimeZoneInfo.Utc;
        }
    }

    // Private helper methods

    private async Task<List<DateTime>> GetAvailableSlotsForBookingPageAsync(
        BookingPage bookingPage,
        DateTime startDate,
        DateTime endDate,
        int durationMinutes)
    {
        var availableSlots = new List<DateTime>();
        var now = DateTime.UtcNow;

        // Get timezone for the booking page
        var timeZone = string.IsNullOrEmpty(bookingPage.TimeZoneId)
            ? TimeZoneInfo.Utc
            : TimeZoneInfo.FindSystemTimeZoneById(bookingPage.TimeZoneId);

        var dailyStart = bookingPage.DailyStartTime ?? TimeSpan.FromHours(9); // Default 9 AM
        var dailyEnd = bookingPage.DailyEndTime ?? TimeSpan.FromHours(17); // Default 5 PM
        var availableDays = bookingPage.GetAvailableDaysOfWeek();

        // Get all events in the date range to check conflicts
        var events = await _eventRepository.GetEventsByDateRangeAsync(
            startDate,
            endDate.AddDays(1),
            bookingPage.UserId);

        // Iterate through each day in the range
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            // Check if this day of week is available
            if (!availableDays.Contains((int)date.DayOfWeek))
            {
                continue;
            }

            // Check daily booking limit
            if (await HasReachedDailyLimitAsync(bookingPage.Id, date))
            {
                continue;
            }

            // Generate time slots for this day
            var slotStart = date + dailyStart;
            var dayEndTime = date + dailyEnd;

            while (slotStart.AddMinutes(durationMinutes) <= dayEndTime)
            {
                var slotEnd = slotStart.AddMinutes(durationMinutes);

                // Check minimum notice
                if (slotStart < now.AddMinutes(bookingPage.MinimumNoticeMinutes))
                {
                    slotStart = slotStart.AddMinutes(15); // Move to next 15-min slot
                    continue;
                }

                // Check maximum advance booking
                if (slotStart > now.AddDays(bookingPage.MaxAdvanceBookingDays))
                {
                    break;
                }

                // Check for conflicts (including buffer times)
                var bufferStart = slotStart.AddMinutes(-bookingPage.BufferBeforeMinutes);
                var bufferEnd = slotEnd.AddMinutes(bookingPage.BufferAfterMinutes);

                var hasConflict = events.Any(e =>
                    (e.StartTime < bufferEnd && e.EndTime > bufferStart));

                if (!hasConflict)
                {
                    availableSlots.Add(slotStart);
                }

                // Move to next time slot (15-minute intervals)
                slotStart = slotStart.AddMinutes(15);
            }
        }

        return availableSlots;
    }

    private async Task<Event> CreateBookingForPageAsync(
        BookingPage bookingPage,
        DateTime startTime,
        int durationMinutes,
        string guestName,
        string guestEmail,
        string? guestPhone,
        string? guestNotes)
    {
        // Validate the booking
        if (!await IsTimeSlotAvailableAsync(bookingPage.Id, startTime, durationMinutes))
        {
            throw new InvalidOperationException("The selected time slot is no longer available");
        }

        // Create the event
        var endTime = startTime.AddMinutes(durationMinutes);
        var bookingEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = $"{bookingPage.Title} - {guestName}",
            Description = BuildBookingDescription(guestName, guestEmail, guestPhone, guestNotes),
            StartTime = startTime,
            EndTime = endTime,
            UserId = bookingPage.UserId,
            CalendarId = bookingPage.CalendarId,
            Location = bookingPage.Location,
            EventType = EventType.Meeting,
            TimeZoneId = bookingPage.TimeZoneId,
            CreatedAt = DateTime.UtcNow,
            Tags = new List<string> { $"booking-page-{bookingPage.Id}", "public-booking" },
            Attendees = new List<Attendee>
            {
                new Attendee
                {
                    Id = Guid.NewGuid(),
                    Name = guestName,
                    Email = guestEmail,
                    IsOrganizer = false,
                    Status = AttendeeStatus.Accepted
                }
            }
        };

        // Add video conference if configured
        if (bookingPage.IncludeVideoConference && !string.IsNullOrEmpty(bookingPage.VideoConferenceProvider))
        {
            // Parse the provider string to enum
            var provider = Enum.TryParse<VideoConferenceProvider>(bookingPage.VideoConferenceProvider, true, out var parsedProvider)
                ? parsedProvider
                : VideoConferenceProvider.Custom;

            // This would typically call a video conference service
            // For now, we'll just set a placeholder
            bookingEvent.VideoConference = new VideoConference
            {
                Id = Guid.NewGuid(),
                EventId = bookingEvent.Id,
                Provider = provider,
                MeetingUrl = $"https://meet.example.com/{Guid.NewGuid()}",
                CreatedBy = bookingPage.UserId,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Save the event
        var createdEvent = await _eventRepository.CreateAsync(bookingEvent);

        // Increment booking count
        await _bookingPageRepository.IncrementBookingCountAsync(bookingPage.Id);

        // Send confirmation email
        if (bookingPage.SendConfirmationEmail)
        {
            try
            {
                await SendBookingConfirmationEmailAsync(bookingPage, createdEvent, guestName, guestEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation email for event {EventId}", createdEvent.Id);
                // Don't throw - the booking was successful even if email failed
            }
        }

        _logger.LogInformation(
            "Created booking {EventId} on booking page {BookingPageId} for {GuestName} at {StartTime}",
            createdEvent.Id, bookingPage.Id, guestName, startTime);

        return createdEvent;
    }

    private bool IsWithinAvailabilityWindow(BookingPage bookingPage, DateTime dateTime)
    {
        // Check day of week
        var availableDays = bookingPage.GetAvailableDaysOfWeek();
        if (!availableDays.Contains((int)dateTime.DayOfWeek))
        {
            return false;
        }

        // Check time of day
        var timeOfDay = dateTime.TimeOfDay;
        var dailyStart = bookingPage.DailyStartTime ?? TimeSpan.FromHours(9);
        var dailyEnd = bookingPage.DailyEndTime ?? TimeSpan.FromHours(17);

        return timeOfDay >= dailyStart && timeOfDay < dailyEnd;
    }

    private string BuildBookingDescription(
        string guestName,
        string guestEmail,
        string? guestPhone,
        string? guestNotes)
    {
        var description = $"Booked by: {guestName}\n";
        description += $"Email: {guestEmail}\n";

        if (!string.IsNullOrWhiteSpace(guestPhone))
        {
            description += $"Phone: {guestPhone}\n";
        }

        if (!string.IsNullOrWhiteSpace(guestNotes))
        {
            description += $"\nNotes:\n{guestNotes}";
        }

        return description;
    }

    private async Task SendBookingConfirmationEmailAsync(
        BookingPage bookingPage,
        Event bookingEvent,
        string guestName,
        string guestEmail)
    {
        // Use SendMeetingInvitationAsync to send the confirmation
        // We'll modify the event description to include the confirmation message if needed
        var originalDescription = bookingEvent.Description;

        if (!string.IsNullOrEmpty(bookingPage.ConfirmationMessage))
        {
            bookingEvent.Description = bookingPage.ConfirmationMessage + "\n\n---\n\n" + originalDescription;
        }

        await _emailService.SendMeetingInvitationAsync(bookingEvent, bookingPage.User?.Email ?? "Organizer");

        // Restore original description
        bookingEvent.Description = originalDescription;

        _logger.LogDebug("Sent booking confirmation email to {GuestEmail}", guestEmail);
    }

    private string BuildDefaultConfirmationMessage(BookingPage bookingPage, Event bookingEvent)
    {
        return $@"Hello,

Your appointment has been confirmed!

Event: {bookingPage.Title}
Date & Time: {bookingEvent.StartTime:dddd, MMMM dd, yyyy} at {bookingEvent.StartTime:h:mm tt}
Duration: {(bookingEvent.EndTime - bookingEvent.StartTime).TotalMinutes} minutes
{(string.IsNullOrEmpty(bookingEvent.Location) ? "" : $"Location: {bookingEvent.Location}\n")}
{(bookingEvent.VideoConference != null ? $"Join URL: {bookingEvent.VideoConference.MeetingUrl}\n" : "")}
We look forward to meeting with you!

Best regards";
    }
}
