namespace Tempus.Core.Models;

/// <summary>
/// Represents availability information for a time slot
/// </summary>
public class AvailabilitySlot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int TotalAttendees { get; set; }
    public int AvailableAttendees { get; set; }
    public int BusyAttendees { get; set; }
    public int UnknownAttendees { get; set; }

    public List<string> AvailableEmails { get; set; } = new();
    public List<string> BusyEmails { get; set; } = new();
    public List<string> UnknownEmails { get; set; } = new();

    public List<Event> ConflictingEvents { get; set; } = new();

    /// <summary>
    /// Gets the availability percentage (0-100)
    /// </summary>
    public double AvailabilityPercentage =>
        TotalAttendees > 0 ? (double)AvailableAttendees / TotalAttendees * 100 : 0;

    /// <summary>
    /// Checks if all required attendees are available
    /// </summary>
    public bool AllAvailable => AvailableAttendees == TotalAttendees;

    /// <summary>
    /// Checks if this slot has no conflicts
    /// </summary>
    public bool HasNoConflicts => BusyAttendees == 0;

    /// <summary>
    /// Gets a quality score for this time slot (0-100)
    /// Higher is better
    /// </summary>
    public double QualityScore
    {
        get
        {
            if (TotalAttendees == 0) return 0;

            // Base score on availability percentage
            double score = AvailabilityPercentage;

            // Penalize for unknown availability
            double unknownPenalty = (double)UnknownAttendees / TotalAttendees * 20;
            score -= unknownPenalty;

            return Math.Max(0, Math.Min(100, score));
        }
    }
}
