using Tempus.Core.Interfaces;

namespace Tempus.Web.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("NOTIFICATION BACKGROUND SERVICE STARTED");
        _logger.LogInformation("========================================");
        Console.WriteLine("========================================");
        Console.WriteLine("NOTIFICATION BACKGROUND SERVICE STARTED");
        Console.WriteLine("========================================");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendNotifications();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking and sending notifications");
            }

            // Wait for the check interval before running again
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Notification Background Service stopped");
    }

    private async Task CheckAndSendNotifications()
    {
        using var scope = _serviceProvider.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<INotificationSchedulerService>();

        var now = DateTime.UtcNow;
        _logger.LogInformation("Checking for pending notifications at {Time}", now);
        Console.WriteLine($"[{now:HH:mm:ss}] Checking for pending notifications...");

        // Get all notifications that should be sent now
        var pendingNotifications = await schedulerService.GetPendingNotificationsAsync(now);

        if (pendingNotifications.Any())
        {
            _logger.LogInformation("Found {Count} pending notifications to send", pendingNotifications.Count);
            Console.WriteLine($"[{now:HH:mm:ss}] Found {pendingNotifications.Count} pending notifications!");
        }

        foreach (var pending in pendingNotifications)
        {
            try
            {
                _logger.LogInformation(
                    "Sending notification for event '{EventTitle}' (ID: {EventId}) to user {UserId} - {Minutes} minute reminder",
                    pending.Event.Title,
                    pending.Event.Id,
                    pending.UserId,
                    pending.ReminderMinutes
                );

                // Create the notification record in the database
                await schedulerService.CreateNotificationRecordAsync(
                    pending.Event.Id,
                    pending.ReminderMinutes,
                    pending.Event.StartTime.ToUniversalTime(),
                    pending.UserId
                );

                // Note: Browser notifications need to be sent from the client side
                // The notification record is created here, and the client will poll
                // for new notifications and display them using the browser's notification API

                _logger.LogInformation(
                    "Successfully created notification record for event '{EventTitle}'",
                    pending.Event.Title
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send notification for event '{EventTitle}' (ID: {EventId})",
                    pending.Event.Title,
                    pending.Event.Id
                );
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
