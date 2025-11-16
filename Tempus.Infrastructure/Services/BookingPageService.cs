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
            _logger.LogWarning("VALIDATION FAILED: Booking page {BookingPageId} not found or inactive", bookingPageId);
            return false;
        }

        return await IsTimeSlotAvailableForPageAsync(bookingPage, startTime, durationMinutes);
    }

    private async Task<bool> IsTimeSlotAvailableForPageAsync(
        BookingPage bookingPage,
        DateTime startTime,
        int durationMinutes)
    {
        if (!bookingPage.IsActive)
        {
            _logger.LogWarning("VALIDATION FAILED: Booking page {BookingPageId} is inactive", bookingPage.Id);
            return false;
        }

        var endTime = startTime.AddMinutes(durationMinutes);

        _logger.LogInformation("Starting validation for slot {StartTime} UTC (duration: {Duration} min) on booking page {BookingPageId}",
            startTime, durationMinutes, bookingPage.Id);

        // Check if time is within configured availability
        if (!IsWithinAvailabilityWindow(bookingPage, startTime))
        {
            _logger.LogWarning("VALIDATION FAILED: Time {StartTime} UTC not within availability window (Working hours: {Start}-{End}, Days: {Days}, Timezone: {TZ})",
                startTime, bookingPage.DailyStartTime, bookingPage.DailyEndTime, bookingPage.AvailableDaysOfWeek, bookingPage.TimeZoneId ?? "UTC");
            return false;
        }

        // Check minimum notice requirement
        var now = DateTime.UtcNow;
        if (startTime < now.AddMinutes(bookingPage.MinimumNoticeMinutes))
        {
            _logger.LogWarning("VALIDATION FAILED: Time {StartTime} UTC does not meet minimum notice requirement of {MinNotice} minutes (now: {Now} UTC, required by: {RequiredBy} UTC)",
                startTime, bookingPage.MinimumNoticeMinutes, now, now.AddMinutes(bookingPage.MinimumNoticeMinutes));
            return false;
        }

        // Check maximum advance booking limit
        if (startTime > now.AddDays(bookingPage.MaxAdvanceBookingDays))
        {
            _logger.LogWarning("VALIDATION FAILED: Time {StartTime} UTC exceeds maximum advance booking limit of {MaxDays} days",
                startTime, bookingPage.MaxAdvanceBookingDays);
            return false;
        }

        // Check daily booking limit
        if (await HasReachedDailyLimitAsync(bookingPage.Id, startTime.Date))
        {
            _logger.LogWarning("VALIDATION FAILED: Daily booking limit reached for {Date}", startTime.Date);
            return false;
        }

        // Check for conflicts with existing events (including buffer times)
        var isFree = await IsSlotFreeAsync(bookingPage, startTime, durationMinutes);
        if (!isFree)
        {
            _logger.LogWarning("VALIDATION FAILED: Time slot {StartTime} UTC has conflicts with existing events", startTime);
        }
        else
        {
            _logger.LogInformation("VALIDATION PASSED: Slot {StartTime} UTC is available", startTime);
        }

        return isFree;
    }

    private async Task<bool> IsSlotFreeAsync(BookingPage bookingPage, DateTime slotStartUtc, int durationMinutes)
    {
        var slotEndUtc = slotStartUtc.AddMinutes(durationMinutes);
        var bufferStartUtc = slotStartUtc.AddMinutes(-bookingPage.BufferBeforeMinutes);
        var bufferEndUtc = slotEndUtc.AddMinutes(bookingPage.BufferAfterMinutes);

        // Get events that might conflict - expand range to be safe
        var searchStart = bufferStartUtc.AddHours(-1);
        var searchEnd = bufferEndUtc.AddHours(1);

        var events = await _eventRepository.GetEventsByDateRangeAsync(
            searchStart,
            searchEnd,
            bookingPage.UserId);

        _logger.LogInformation("Conflict check: Found {EventCount} events to check against slot {SlotStart}-{SlotEnd} UTC (with buffers: {BufferStart}-{BufferEnd} UTC)",
            events.Count, slotStartUtc, slotEndUtc, bufferStartUtc, bufferEndUtc);

        // Check if any event conflicts with this slot (including buffers)
        // Events are stored in UTC, ensure we're comparing UTC to UTC
        foreach (var e in events)
        {
            // Ensure event times are treated as UTC for comparison
            var eventStart = DateTime.SpecifyKind(e.StartTime, DateTimeKind.Utc);
            var eventEnd = DateTime.SpecifyKind(e.EndTime, DateTimeKind.Utc);

            _logger.LogInformation("  Checking event '{Title}': {EventStart} UTC to {EventEnd} UTC",
                e.Title, eventStart, eventEnd);

            // Event conflicts if it overlaps with the buffer window
            // Overlap occurs if: eventStart < bufferEnd AND eventEnd > bufferStart
            if (eventStart < bufferEndUtc && eventEnd > bufferStartUtc)
            {
                _logger.LogWarning("  *** CONFLICT: Event '{Title}' ({EventStart}-{EventEnd} UTC) overlaps with requested slot ({SlotStart}-{SlotEnd} UTC + buffers)",
                    e.Title, eventStart, eventEnd, slotStartUtc, slotEndUtc);
                return false;
            }
            else
            {
                _logger.LogInformation("  OK: No overlap");
            }
        }

        _logger.LogInformation("Conflict check complete: No conflicts found, slot is available");
        return true;
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

        // Pre-load all events in the date range once for efficiency
        var localStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Unspecified);
        var localEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Unspecified);
        var utcSearchStart = TimeZoneInfo.ConvertTimeToUtc(localStartDate, timeZone).AddHours(-24);
        var utcSearchEnd = TimeZoneInfo.ConvertTimeToUtc(localEndDate, timeZone).AddHours(24);

        var allEvents = await _eventRepository.GetEventsByDateRangeAsync(
            utcSearchStart,
            utcSearchEnd,
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

            // Generate time slots for this day in the booking page's timezone, then convert to UTC
            // Create local datetime (unspecified kind) for this date at the daily start time
            var localDateTime = DateTime.SpecifyKind(date + dailyStart, DateTimeKind.Unspecified);
            var localEndTime = DateTime.SpecifyKind(date + dailyEnd, DateTimeKind.Unspecified);

            // Convert to UTC and ensure DateTimeKind is set correctly
            var slotStart = DateTime.SpecifyKind(
                TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone),
                DateTimeKind.Utc);
            var dayEndTime = DateTime.SpecifyKind(
                TimeZoneInfo.ConvertTimeToUtc(localEndTime, timeZone),
                DateTimeKind.Utc);

            while (slotStart.AddMinutes(durationMinutes) <= dayEndTime)
            {
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

                // Check for conflicts using pre-loaded events
                if (IsSlotFreeWithEvents(bookingPage, slotStart, durationMinutes, allEvents))
                {
                    // Ensure the slot is UTC before adding
                    availableSlots.Add(DateTime.SpecifyKind(slotStart, DateTimeKind.Utc));
                }

                // Move to next time slot (15-minute intervals)
                slotStart = DateTime.SpecifyKind(slotStart.AddMinutes(15), DateTimeKind.Utc);
            }
        }

        return availableSlots;
    }

    private bool IsSlotFreeWithEvents(BookingPage bookingPage, DateTime slotStartUtc, int durationMinutes, List<Event> events)
    {
        var slotEndUtc = slotStartUtc.AddMinutes(durationMinutes);
        var bufferStartUtc = slotStartUtc.AddMinutes(-bookingPage.BufferBeforeMinutes);
        var bufferEndUtc = slotEndUtc.AddMinutes(bookingPage.BufferAfterMinutes);

        // Check if any event conflicts with this slot (including buffers)
        // Events are stored in UTC, ensure we're comparing UTC to UTC
        var hasConflict = events.Any(e =>
        {
            // Ensure event times are treated as UTC for comparison
            var eventStart = DateTime.SpecifyKind(e.StartTime, DateTimeKind.Utc);
            var eventEnd = DateTime.SpecifyKind(e.EndTime, DateTimeKind.Utc);

            // Event conflicts if it overlaps with the buffer window
            // Overlap occurs if: eventStart < bufferEnd AND eventEnd > bufferStart
            return eventStart < bufferEndUtc && eventEnd > bufferStartUtc;
        });

        return !hasConflict;
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
        // Ensure we're working with UTC time
        var utcStartTime = startTime.Kind == DateTimeKind.Utc
            ? startTime
            : DateTime.SpecifyKind(startTime, DateTimeKind.Utc);

        _logger.LogInformation("=== CREATE BOOKING: Guest {GuestName} ({GuestEmail}) requesting slot {StartTime} UTC for {Duration} min on booking page {BookingPageSlug}",
            guestName, guestEmail, utcStartTime, durationMinutes, bookingPage.Slug);

        // Validate the booking using the booking page object we already have
        if (!await IsTimeSlotAvailableForPageAsync(bookingPage, utcStartTime, durationMinutes))
        {
            _logger.LogError("=== BOOKING FAILED: Validation failed for slot {StartTime} UTC - see validation logs above for specific reason",
                utcStartTime);
            throw new InvalidOperationException("The selected time slot is no longer available");
        }

        startTime = utcStartTime;
        _logger.LogInformation("=== VALIDATION PASSED: Creating event for {StartTime} UTC", utcStartTime);

        // Create the event
        var endTime = startTime.AddMinutes(durationMinutes);

        _logger.LogInformation("Creating event with CalendarId: {CalendarId} (null = {IsNull})",
            bookingPage.CalendarId, bookingPage.CalendarId == null);

        var bookingEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = $"Appointment booked with {guestName}",
            Description = BuildBookingDescription(bookingPage.Title, guestName, guestEmail, guestPhone, guestNotes),
            StartTime = startTime,
            EndTime = endTime,
            UserId = bookingPage.UserId,
            CalendarId = bookingPage.CalendarId,
            Location = bookingPage.Location,
            Color = bookingPage.Color,
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

    private bool IsWithinAvailabilityWindow(BookingPage bookingPage, DateTime utcDateTime)
    {
        // Convert UTC time to booking page's timezone for comparison with working hours
        // Working hours are defined in the booking page's timezone (e.g., 9 AM - 5 PM EST)
        var timeZone = string.IsNullOrEmpty(bookingPage.TimeZoneId)
            ? TimeZoneInfo.Local
            : TimeZoneInfo.FindSystemTimeZoneById(bookingPage.TimeZoneId);

        // Ensure the datetime is treated as UTC before conversion
        var utcTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);

        // Check day of week in local timezone
        var availableDays = bookingPage.GetAvailableDaysOfWeek();
        if (!availableDays.Contains((int)localDateTime.DayOfWeek))
        {
            return false;
        }

        // Check time of day in local timezone
        var timeOfDay = localDateTime.TimeOfDay;
        var dailyStart = bookingPage.DailyStartTime ?? TimeSpan.FromHours(9);
        var dailyEnd = bookingPage.DailyEndTime ?? TimeSpan.FromHours(17);

        return timeOfDay >= dailyStart && timeOfDay < dailyEnd;
    }

    private string BuildBookingDescription(
        string bookingPageTitle,
        string guestName,
        string guestEmail,
        string? guestPhone,
        string? guestNotes)
    {
        var description = $"Booking Type: {bookingPageTitle}\n\n";
        description += $"Booked by: {guestName}\n";
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
        // Convert event times from UTC to booking page's timezone for email display
        var timeZone = string.IsNullOrEmpty(bookingPage.TimeZoneId)
            ? TimeZoneInfo.Local
            : TimeZoneInfo.FindSystemTimeZoneById(bookingPage.TimeZoneId);

        var originalStartTime = bookingEvent.StartTime;
        var originalEndTime = bookingEvent.EndTime;
        var originalDescription = bookingEvent.Description;

        try
        {
            // Temporarily convert times to local timezone for email display
            // Ensure times are treated as UTC before conversion
            var utcStart = DateTime.SpecifyKind(bookingEvent.StartTime, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(bookingEvent.EndTime, DateTimeKind.Utc);

            bookingEvent.StartTime = TimeZoneInfo.ConvertTimeFromUtc(utcStart, timeZone);
            bookingEvent.EndTime = TimeZoneInfo.ConvertTimeFromUtc(utcEnd, timeZone);

            if (!string.IsNullOrEmpty(bookingPage.ConfirmationMessage))
            {
                bookingEvent.Description = bookingPage.ConfirmationMessage + "\n\n---\n\n" + originalDescription;
            }

            await _emailService.SendMeetingInvitationAsync(bookingEvent, bookingPage.User?.Email ?? "Organizer");

            _logger.LogInformation("Sent booking confirmation email to {GuestEmail} with times in {TimeZone} timezone",
                guestEmail, timeZone.Id);
        }
        finally
        {
            // Restore original UTC times
            bookingEvent.StartTime = originalStartTime;
            bookingEvent.EndTime = originalEndTime;
            bookingEvent.Description = originalDescription;
        }
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
