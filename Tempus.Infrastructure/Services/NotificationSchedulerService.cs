using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Helpers;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class NotificationSchedulerService : INotificationSchedulerService
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<NotificationSchedulerService> _logger;

    public NotificationSchedulerService(IDbContextFactory<TempusDbContext> contextFactory, ILogger<NotificationSchedulerService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<PendingNotification>> GetPendingNotificationsAsync(DateTime checkTimeUtc, int toleranceSeconds = 30)
    {
        var pendingNotifications = new List<PendingNotification>();

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get all users who have events with reminders
        var users = await context.Users.ToListAsync();

        // Define check window: next 24 hours
        var endTimeUtc = checkTimeUtc.AddHours(24);

        foreach (var user in users)
        {
            // Get all non-recurring events with reminders in the next 24 hours
            var upcomingEvents = await context.Events
                .Where(e => e.UserId == user.Id &&
                           !string.IsNullOrEmpty(e.ReminderMinutes) &&
                           !e.IsRecurring &&
                           !e.IsRecurrenceException &&
                           e.StartTime >= checkTimeUtc &&
                           e.StartTime <= endTimeUtc)
                .ToListAsync();

            // Get recurring events that could have instances in this window
            var recurringEvents = await context.Events
                .Where(e => e.UserId == user.Id &&
                           !string.IsNullOrEmpty(e.ReminderMinutes) &&
                           e.IsRecurring &&
                           e.RecurrenceParentId == null &&
                           e.StartTime <= endTimeUtc)
                .ToListAsync();

            // Expand recurring events
            foreach (var recurringEvent in recurringEvents)
            {
                var instances = RecurrenceHelper.ExpandRecurringEvent(
                    recurringEvent,
                    checkTimeUtc,
                    endTimeUtc
                );

                upcomingEvents.AddRange(instances.Where(i => i.Id != recurringEvent.Id));
            }

            // For each event with reminders, check if any reminder should trigger now
            foreach (var evt in upcomingEvents)
            {
                if (string.IsNullOrEmpty(evt.ReminderMinutes))
                    continue;

                _logger.LogDebug("Event '{Title}' (ID: {EventId}) has reminders: {ReminderMinutes}", evt.Title, evt.Id, evt.ReminderMinutes);
                _logger.LogDebug("  Event StartTime: {StartTime:yyyy-MM-dd HH:mm:ss} (Kind: {Kind})", evt.StartTime, evt.StartTime.Kind);

                var reminderMinutes = evt.ReminderMinutes
                    .Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var minutes) ? minutes : 0)
                    .Where(m => m > 0)
                    .ToList();

                foreach (var reminderMin in reminderMinutes)
                {
                    // Calculate when this reminder should trigger (in UTC)
                    // Events are now stored in UTC, so no conversion needed
                    var eventStartUtc = evt.StartTime.Kind == DateTimeKind.Utc
                        ? evt.StartTime
                        : DateTime.SpecifyKind(evt.StartTime, DateTimeKind.Utc);
                    var reminderTriggerTime = eventStartUtc.AddMinutes(-reminderMin);

                    _logger.LogDebug("  Reminder: {ReminderMinutes} min before", reminderMin);
                    _logger.LogDebug("  Event Start (UTC): {EventStart:yyyy-MM-dd HH:mm:ss}", eventStartUtc);
                    _logger.LogDebug("  Trigger Time: {TriggerTime:yyyy-MM-dd HH:mm:ss}", reminderTriggerTime);
                    _logger.LogDebug("  Check Time: {CheckTime:yyyy-MM-dd HH:mm:ss}", checkTimeUtc);

                    // Check if we're within the tolerance window
                    var timeDifference = Math.Abs((checkTimeUtc - reminderTriggerTime).TotalSeconds);
                    _logger.LogDebug("  Time Difference: {TimeDifference:F1} seconds (Tolerance: {Tolerance} seconds)", timeDifference, toleranceSeconds);

                    if (timeDifference <= toleranceSeconds)
                    {
                        _logger.LogDebug("  ✓ Within tolerance! Checking if already sent...");

                        // Check if this notification has already been sent
                        var alreadySent = await HasNotificationBeenSentAsync(
                            evt.Id,
                            reminderMin,
                            eventStartUtc,
                            user.Id
                        );

                        if (!alreadySent)
                        {
                            _logger.LogDebug("  ✓ Not sent yet! Adding to pending notifications.");
                            pendingNotifications.Add(new PendingNotification
                            {
                                Event = evt,
                                ReminderMinutes = reminderMin,
                                ScheduledTimeUtc = reminderTriggerTime,
                                UserId = user.Id
                            });
                        }
                        else
                        {
                            _logger.LogDebug("  ✗ Already sent. Skipping.");
                        }
                    }
                    else
                    {
                        _logger.LogDebug("  ✗ Outside tolerance window. Skipping.");
                    }
                }
            }
        }

        return pendingNotifications;
    }

    public async Task<bool> HasNotificationBeenSentAsync(Guid eventId, int reminderMinutes, DateTime eventStartTimeUtc, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Calculate scheduled time
        var scheduledTime = eventStartTimeUtc.AddMinutes(-reminderMinutes);

        // Check if notification exists with IsSent = true
        var exists = await context.Notifications
            .AnyAsync(n => n.EventId == eventId &&
                          n.UserId == userId &&
                          n.Type == NotificationType.EventReminder &&
                          n.ReminderMinutes == reminderMinutes &&
                          n.IsSent == true &&
                          n.ScheduledFor.HasValue &&
                          // Use date comparison to handle slight time variations
                          n.ScheduledFor.Value.Date == scheduledTime.Date &&
                          n.ScheduledFor.Value.Hour == scheduledTime.Hour &&
                          n.ScheduledFor.Value.Minute == scheduledTime.Minute);

        return exists;
    }

    public async Task CreateNotificationRecordAsync(Guid eventId, int reminderMinutes, DateTime eventStartTimeUtc, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get event to create descriptive notification
        var evt = await context.Events.FindAsync(eventId);
        if (evt == null) return;

        var scheduledTime = eventStartTimeUtc.AddMinutes(-reminderMinutes);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = eventId,
            Type = NotificationType.EventReminder,
            Title = $"Reminder: {evt.Title}",
            Message = GetReminderMessage(evt, reminderMinutes),
            ReminderMinutes = reminderMinutes,
            ScheduledFor = scheduledTime,
            IsSent = true,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task SendBrowserNotificationAsync(Event evt, int reminderMinutes, string userId)
    {
        // This will be called from the background service
        // The background service will inject IBrowserNotificationService and call it
        // This method is a placeholder for now
        await Task.CompletedTask;
    }

    private string GetReminderMessage(Event evt, int reminderMinutes)
    {
        var timeDescription = reminderMinutes switch
        {
            1 => "in 1 minute",
            5 => "in 5 minutes",
            15 => "in 15 minutes",
            30 => "in 30 minutes",
            60 => "in 1 hour",
            120 => "in 2 hours",
            1440 => "tomorrow",
            2880 => "in 2 days",
            10080 => "in 1 week",
            _ => $"in {reminderMinutes} minutes"
        };

        var message = $"Starts {timeDescription}";

        if (!string.IsNullOrEmpty(evt.Location))
        {
            message += $" • {evt.Location}";
        }

        if (evt.StartTime != default)
        {
            message += $" • {evt.StartTime:h:mm tt}";
        }

        return message;
    }
}
