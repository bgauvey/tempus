using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class OutOfOfficeService : IOutOfOfficeService
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<OutOfOfficeService> _logger;
    private readonly IEmailNotificationService _emailService;
    private readonly IRSVPService _rsvpService;

    public OutOfOfficeService(
        IDbContextFactory<TempusDbContext> contextFactory,
        ILogger<OutOfOfficeService> logger,
        IEmailNotificationService emailService,
        IRSVPService rsvpService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _emailService = emailService;
        _rsvpService = rsvpService;
    }

    public async Task<OutOfOfficeStatus> CreateAsync(OutOfOfficeStatus status)
    {
        _logger.LogInformation("Creating OOO status for user {UserId} from {StartDate} to {EndDate}",
            status.UserId, status.StartDate, status.EndDate);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Validate dates
        if (status.StartDate >= status.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        // Check for conflicts
        var (isValid, errorMessage) = await ValidateDateRangeAsync(
            status.UserId,
            status.StartDate,
            status.EndDate);

        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage);
        }

        context.OutOfOfficeStatuses.Add(status);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created OOO status {Id} for user {UserId}", status.Id, status.UserId);

        return status;
    }

    public async Task<OutOfOfficeStatus> UpdateAsync(OutOfOfficeStatus status)
    {
        _logger.LogInformation("Updating OOO status {Id}", status.Id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Validate dates
        if (status.StartDate >= status.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        // Check for conflicts (excluding this status)
        var (isValid, errorMessage) = await ValidateDateRangeAsync(
            status.UserId,
            status.StartDate,
            status.EndDate,
            status.Id);

        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage);
        }

        status.UpdatedAt = DateTime.UtcNow;
        context.OutOfOfficeStatuses.Update(status);
        await context.SaveChangesAsync();

        _logger.LogInformation("Updated OOO status {Id}", status.Id);

        return status;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        _logger.LogInformation("Deleting OOO status {Id} for user {UserId}", id, userId);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var status = await context.OutOfOfficeStatuses
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (status == null)
        {
            return false;
        }

        context.OutOfOfficeStatuses.Remove(status);
        await context.SaveChangesAsync();

        _logger.LogInformation("Deleted OOO status {Id}", id);

        return true;
    }

    public async Task<OutOfOfficeStatus?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.OutOfOfficeStatuses
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
    }

    public async Task<List<OutOfOfficeStatus>> GetAllForUserAsync(string userId, bool activeOnly = false)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.OutOfOfficeStatuses
            .Where(o => o.UserId == userId);

        if (activeOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(o => o.IsActive && o.StartDate <= now && o.EndDate >= now);
        }

        return await query
            .OrderBy(o => o.StartDate)
            .ToListAsync();
    }

    public async Task<OutOfOfficeStatus?> GetActiveStatusAtTimeAsync(string userId, DateTime dateTime)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.OutOfOfficeStatuses
            .Where(o => o.UserId == userId &&
                       o.IsActive &&
                       o.StartDate <= dateTime &&
                       o.EndDate >= dateTime)
            .OrderByDescending(o => o.CreatedAt) // Get most recent if multiple
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsUserOutOfOfficeAsync(string userId, DateTime dateTime)
    {
        var status = await GetActiveStatusAtTimeAsync(userId, dateTime);
        return status != null;
    }

    public async Task<(bool ShouldDecline, string? Reason)> ShouldAutoDeclineMeetingAsync(
        string userId,
        DateTime meetingStart,
        DateTime meetingEnd,
        string organizerEmail,
        bool isOptional)
    {
        _logger.LogDebug("Checking if meeting should be auto-declined for user {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get user settings
        var settings = await context.CalendarSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        // Check if meeting is outside working hours
        if (settings != null && settings.AutoDeclineMeetingsOutsideWorkingHours)
        {
            var isOutsideWorkingHours = CheckIfOutsideWorkingHours(
                meetingStart,
                meetingEnd,
                settings);

            if (isOutsideWorkingHours)
            {
                return (true, "Meeting is outside my working hours");
            }
        }

        // Check for active OOO status
        var oooStatus = await GetActiveStatusAtTimeAsync(userId, meetingStart);

        if (oooStatus == null)
        {
            return (false, null);
        }

        // Check if organizer is exempt
        if (oooStatus.IsOrganizerExempt(organizerEmail))
        {
            _logger.LogDebug("Organizer {Email} is exempt from auto-decline", organizerEmail);
            return (false, null);
        }

        // Check if auto-decline is enabled
        if (!oooStatus.AutoDeclineMeetings)
        {
            return (false, null);
        }

        // Check if only declining optional meetings
        if (oooStatus.DeclineOptionalOnly && !isOptional)
        {
            _logger.LogDebug("Keeping required meeting during OOO period");
            return (false, null);
        }

        // Build decline reason
        var reason = oooStatus.DeclineMessage ?? GetDefaultDeclineMessage(oooStatus);

        _logger.LogInformation("Auto-declining meeting for user {UserId} due to {StatusType}",
            userId, oooStatus.StatusType);

        return (true, reason);
    }

    public async Task ProcessAutoDeclineAsync(Event meetingEvent, Attendee attendee)
    {
        _logger.LogInformation("Processing auto-decline for attendee {Email} on event {EventId}",
            attendee.Email, meetingEvent.Id);

        // Update attendee status to declined
        attendee.Status = AttendeeStatus.Declined;
        attendee.ResponseDate = DateTime.UtcNow;

        // Get the OOO status for the decline message
        var oooStatus = await GetActiveStatusAtTimeAsync(attendee.Email, meetingEvent.StartTime);
        string reason;

        if (oooStatus != null && !string.IsNullOrEmpty(oooStatus.DeclineMessage))
        {
            attendee.ResponseNotes = oooStatus.DeclineMessage;
            reason = oooStatus.DeclineMessage;
        }
        else
        {
            attendee.ResponseNotes = "Automatically declined - out of office";
            reason = "Automatically declined - out of office";
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Attendees.Update(attendee);
        await context.SaveChangesAsync();

        // Send notification to organizer
        try
        {
            await _emailService.SendAutoDeclineNotificationAsync(meetingEvent, attendee, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send auto-decline notification");
        }

        _logger.LogInformation("Auto-declined meeting {EventId} for attendee {Email}",
            meetingEvent.Id, attendee.Email);
    }

    public async Task SendAutoResponderIfNeededAsync(string userId, string senderEmail, Event? relatedEvent = null)
    {
        var oooStatus = await GetActiveStatusAtTimeAsync(userId, DateTime.UtcNow);

        if (oooStatus == null || !oooStatus.SendAutoResponder || string.IsNullOrEmpty(oooStatus.AutoResponderMessage))
        {
            return;
        }

        _logger.LogInformation("Sending auto-responder from {UserId} to {SenderEmail}", userId, senderEmail);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Get user information
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for auto-responder", userId);
                return;
            }

            var fromUserName = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                ? $"{user.FirstName} {user.LastName}"
                : user.Email ?? "Unknown User";
            var fromUserEmail = user.Email ?? "";

            // Send the auto-responder email
            await _emailService.SendOutOfOfficeAutoResponderAsync(
                senderEmail,
                senderEmail, // Use email as name if we don't know sender's name
                fromUserName,
                fromUserEmail,
                oooStatus);

            _logger.LogInformation("Auto-responder sent from {FromUser} to {ToEmail}", fromUserName, senderEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send auto-responder");
        }
    }

    public async Task<List<OutOfOfficeStatus>> GetUpcomingStatusesAsync(string userId, int days = 30)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var now = DateTime.UtcNow;
        var endDate = now.AddDays(days);

        return await context.OutOfOfficeStatuses
            .Where(o => o.UserId == userId &&
                       o.IsActive &&
                       o.StartDate <= endDate &&
                       o.EndDate >= now)
            .OrderBy(o => o.StartDate)
            .ToListAsync();
    }

    public async Task<bool> IsFocusTimeActiveAsync(string userId, DateTime dateTime)
    {
        var status = await GetActiveStatusAtTimeAsync(userId, dateTime);
        return status != null && status.StatusType == AvailabilityStatusType.FocusTime;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateDateRangeAsync(
        string userId,
        DateTime startDate,
        DateTime endDate,
        Guid? excludeId = null)
    {
        if (startDate >= endDate)
        {
            return (false, "Start date must be before end date");
        }

        if (endDate < DateTime.UtcNow)
        {
            return (false, "End date cannot be in the past");
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check for overlapping statuses
        var query = context.OutOfOfficeStatuses
            .Where(o => o.UserId == userId &&
                       o.IsActive &&
                       ((o.StartDate <= startDate && o.EndDate >= startDate) || // Overlaps start
                        (o.StartDate <= endDate && o.EndDate >= endDate) ||     // Overlaps end
                        (o.StartDate >= startDate && o.EndDate <= endDate)));   // Contained within

        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }

        var conflictingStatus = await query.FirstOrDefaultAsync();

        if (conflictingStatus != null)
        {
            return (false, $"Date range conflicts with existing out-of-office period from {conflictingStatus.StartDate:MMM dd} to {conflictingStatus.EndDate:MMM dd}");
        }

        return (true, null);
    }

    // Helper methods

    private bool CheckIfOutsideWorkingHours(DateTime meetingStart, DateTime meetingEnd, CalendarSettings settings)
    {
        // Convert meeting time to user's timezone
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
        var meetingStartLocal = TimeZoneInfo.ConvertTimeFromUtc(meetingStart, userTimeZone);
        var meetingEndLocal = TimeZoneInfo.ConvertTimeFromUtc(meetingEnd, userTimeZone);

        var meetingDay = (int)meetingStartLocal.DayOfWeek;

        // Check if it's a weekend day
        var weekendDays = settings.WeekendDays.Split(',').Select(int.Parse).ToList();
        if (weekendDays.Contains(meetingDay))
        {
            return true;
        }

        // Check if it's during work hours
        var meetingStartTime = meetingStartLocal.TimeOfDay;
        var meetingEndTime = meetingEndLocal.TimeOfDay;

        if (meetingStartTime < settings.WorkHoursStart || meetingEndTime > settings.WorkHoursEnd)
        {
            return true;
        }

        // Check if it's during lunch break
        if (!settings.AllowMeetingsDuringLunchBreak &&
            settings.LunchBreakStart.HasValue &&
            settings.LunchBreakEnd.HasValue)
        {
            if (meetingStartTime < settings.LunchBreakEnd.Value &&
                meetingEndTime > settings.LunchBreakStart.Value)
            {
                return true; // Overlaps with lunch
            }
        }

        return false;
    }

    private string GetDefaultDeclineMessage(OutOfOfficeStatus status)
    {
        return status.StatusType switch
        {
            AvailabilityStatusType.OutOfOffice =>
                $"I'm currently out of office until {status.EndDate:MMM dd, yyyy}. I'll respond when I return.",
            AvailabilityStatusType.FocusTime =>
                "I'm in focus time and unavailable for meetings. Please schedule for another time.",
            AvailabilityStatusType.DoNotDisturb =>
                "I'm currently unavailable. Please reach out later.",
            _ =>
                "I'm currently unavailable and cannot attend this meeting."
        };
    }
}
