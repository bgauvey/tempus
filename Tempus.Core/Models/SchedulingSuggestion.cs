namespace Tempus.Core.Models;

/// <summary>
/// Represents a smart scheduling suggestion with ranking
/// </summary>
public class SchedulingSuggestion
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public double Score { get; set; } // 0-100, higher is better
    public int Rank { get; set; }

    public int AvailableCount { get; set; }
    public int TotalCount { get; set; }
    public int ConflictCount { get; set; }

    public List<string> AvailableAttendees { get; set; } = new();
    public List<string> ConflictingAttendees { get; set; } = new();
    public List<Event> ConflictingEvents { get; set; } = new();

    public string? ReasonForSuggestion { get; set; }

    /// <summary>
    /// Gets availability percentage
    /// </summary>
    public double AvailabilityPercentage =>
        TotalCount > 0 ? (double)AvailableCount / TotalCount * 100 : 0;

    /// <summary>
    /// Checks if all attendees are available
    /// </summary>
    public bool AllAvailable => AvailableCount == TotalCount && ConflictCount == 0;

    /// <summary>
    /// Gets a description of this suggestion
    /// </summary>
    public string GetDescription()
    {
        if (AllAvailable)
            return "Perfect! All attendees are available.";

        if (ConflictCount == 0)
            return $"Good! {AvailableCount} of {TotalCount} attendees available.";

        if (ConflictCount == 1)
            return $"One conflict detected. {AvailableCount} of {TotalCount} available.";

        return $"{ConflictCount} conflicts detected. {AvailableCount} of {TotalCount} available.";
    }

    /// <summary>
    /// Gets a quality indicator (Excellent, Good, Fair, Poor)
    /// </summary>
    public string GetQualityIndicator()
    {
        return Score switch
        {
            >= 90 => "Excellent",
            >= 70 => "Good",
            >= 50 => "Fair",
            _ => "Poor"
        };
    }
}
