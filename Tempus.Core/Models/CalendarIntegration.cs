namespace Tempus.Core.Models;

public class CalendarIntegration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public string Provider { get; set; } = string.Empty; // Google, Outlook, Apple
    public string CalendarId { get; set; } = string.Empty;
    public string CalendarName { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiry { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool SyncEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncedAt { get; set; }
    public string? SyncToken { get; set; } // For incremental sync
}
