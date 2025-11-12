namespace Tempus.Core.Models;

/// <summary>
/// Response availability for a specific time slot in a poll
/// </summary>
public enum PollResponseType
{
    Yes,
    No,
    Maybe
}

/// <summary>
/// Represents an attendee's response to a poll time slot
/// </summary>
public class PollResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SchedulingPollId { get; set; }
    public SchedulingPoll? SchedulingPoll { get; set; }

    public Guid PollTimeSlotId { get; set; }
    public PollTimeSlot? PollTimeSlot { get; set; }

    public string RespondentEmail { get; set; } = string.Empty;
    public string RespondentName { get; set; } = string.Empty;

    public PollResponseType Response { get; set; }
    public string? Comment { get; set; }

    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
