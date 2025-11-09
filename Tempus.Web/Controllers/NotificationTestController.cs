using Microsoft.AspNetCore.Mvc;
using Tempus.Core.Interfaces;

namespace Tempus.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationTestController : ControllerBase
{
    private readonly INotificationSchedulerService _schedulerService;
    private readonly ILogger<NotificationTestController> _logger;

    public NotificationTestController(
        INotificationSchedulerService schedulerService,
        ILogger<NotificationTestController> logger)
    {
        _schedulerService = schedulerService;
        _logger = logger;
    }

    [HttpGet("check-pending")]
    public async Task<IActionResult> CheckPending()
    {
        var now = DateTime.UtcNow;
        var pending = await _schedulerService.GetPendingNotificationsAsync(now);

        _logger.LogInformation("Manual check: Found {Count} pending notifications", pending.Count);

        return Ok(new
        {
            checkTime = now,
            pendingCount = pending.Count,
            notifications = pending.Select(p => new
            {
                eventTitle = p.Event.Title,
                eventId = p.Event.Id,
                reminderMinutes = p.ReminderMinutes,
                scheduledFor = p.ScheduledTimeUtc,
                userId = p.UserId
            })
        });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            message = "Notification system is running",
            timestamp = DateTime.UtcNow,
            schedulerServiceType = _schedulerService.GetType().Name
        });
    }
}
