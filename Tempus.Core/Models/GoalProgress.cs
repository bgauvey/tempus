namespace Tempus.Core.Models;

/// <summary>
/// Represents a single completion/progress entry for a goal
/// </summary>
public class GoalProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The goal this progress entry belongs to
    /// </summary>
    public Guid GoalId { get; set; }
    public Goal? Goal { get; set; }

    /// <summary>
    /// When this progress entry was completed (in UTC)
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the activity in minutes (optional)
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Optional notes about this completion
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional numeric value (e.g., weight lifted, distance run, pages read)
    /// </summary>
    public double? Value { get; set; }

    /// <summary>
    /// Unit of measurement for the value (e.g., "kg", "miles", "pages")
    /// </summary>
    public string? ValueUnit { get; set; }

    /// <summary>
    /// Whether this was completed as scheduled or manually logged
    /// </summary>
    public bool WasScheduled { get; set; } = false;

    /// <summary>
    /// If scheduled, the ID of the event that was associated
    /// </summary>
    public Guid? ScheduledEventId { get; set; }

    /// <summary>
    /// Rating or satisfaction level (1-5 scale, optional)
    /// </summary>
    public int? Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Get formatted duration string
    /// </summary>
    public string GetFormattedDuration()
    {
        if (!DurationMinutes.HasValue || DurationMinutes.Value == 0)
            return string.Empty;

        if (DurationMinutes.Value < 60)
            return $"{DurationMinutes}m";

        var hours = DurationMinutes.Value / 60;
        var minutes = DurationMinutes.Value % 60;

        return minutes > 0
            ? $"{hours}h {minutes}m"
            : $"{hours}h";
    }

    /// <summary>
    /// Get formatted value with unit
    /// </summary>
    public string GetFormattedValue()
    {
        if (!Value.HasValue)
            return string.Empty;

        return string.IsNullOrEmpty(ValueUnit)
            ? Value.Value.ToString("0.##")
            : $"{Value.Value:0.##} {ValueUnit}";
    }
}
