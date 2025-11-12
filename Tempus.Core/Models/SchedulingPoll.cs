namespace Tempus.Core.Models;

/// <summary>
/// Represents a Doodle-style meeting poll for scheduling
/// </summary>
public class SchedulingPoll
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string OrganizerEmail { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }

    public string? Location { get; set; }
    public int Duration { get; set; } // Duration in minutes

    // Poll Settings
    public bool AllowMultipleResponses { get; set; } = true;
    public bool ShowParticipantNames { get; set; } = true;
    public bool RequireLogin { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Results
    public Guid? SelectedTimeSlotId { get; set; }
    public DateTime? FinalizedAt { get; set; }

    // Navigation Properties
    public List<PollTimeSlot> TimeSlots { get; set; } = new();
    public List<PollResponse> Responses { get; set; } = new();

    /// <summary>
    /// Gets the time slot with the most responses
    /// </summary>
    public PollTimeSlot? GetMostPopularTimeSlot()
    {
        return TimeSlots.OrderByDescending(ts => ts.ResponseCount).FirstOrDefault();
    }

    /// <summary>
    /// Checks if poll has expired
    /// </summary>
    public bool IsExpired()
    {
        return Deadline.HasValue && Deadline.Value < DateTime.UtcNow;
    }
}
