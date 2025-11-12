namespace Tempus.Core.Models;

/// <summary>
/// Represents a proposed time slot in a scheduling poll
/// </summary>
public class PollTimeSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SchedulingPollId { get; set; }
    public SchedulingPoll? SchedulingPoll { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Statistics
    public int ResponseCount { get; set; } = 0;
    public int YesCount { get; set; } = 0;
    public int NoCount { get; set; } = 0;
    public int MaybeCount { get; set; } = 0;

    // Navigation Properties
    public List<PollResponse> Responses { get; set; } = new();

    /// <summary>
    /// Gets the response rate percentage
    /// </summary>
    public double GetResponseRate(int totalInvited)
    {
        return totalInvited > 0 ? (double)ResponseCount / totalInvited * 100 : 0;
    }

    /// <summary>
    /// Gets the percentage of Yes responses
    /// </summary>
    public double GetYesPercentage()
    {
        return ResponseCount > 0 ? (double)YesCount / ResponseCount * 100 : 0;
    }

    /// <summary>
    /// Calculates a popularity score (Yes = 1, Maybe = 0.5, No = 0)
    /// </summary>
    public double GetPopularityScore()
    {
        return YesCount + (MaybeCount * 0.5);
    }
}
