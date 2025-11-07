using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }

    // Link to related event
    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    // Reminder tracking (for EventReminder notifications)
    public int? ReminderMinutes { get; set; } // Which reminder triggered this (15, 60, 1440, etc.)
    public DateTime? ScheduledFor { get; set; } // When this notification was scheduled to trigger
    public bool IsSent { get; set; } = false; // Whether notification was actually delivered
    public DateTime? SentAt { get; set; } // When notification was sent

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
