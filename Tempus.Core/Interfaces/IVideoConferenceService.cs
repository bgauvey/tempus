using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for managing video conferencing integrations
/// </summary>
public interface IVideoConferenceService
{
    /// <summary>
    /// Creates a custom video conference link (manual entry)
    /// </summary>
    Task<VideoConference> CreateCustomMeetingAsync(Guid eventId, string meetingUrl, string? meetingId, string? passcode, string userId);

    /// <summary>
    /// Gets video conference information for an event
    /// </summary>
    Task<VideoConference?> GetVideoConferenceAsync(Guid eventId);

    /// <summary>
    /// Updates video conference information
    /// </summary>
    Task<VideoConference> UpdateVideoConferenceAsync(Guid videoConferenceId, string meetingUrl, string? meetingId, string? passcode);

    /// <summary>
    /// Deletes video conference information from an event
    /// </summary>
    Task<bool> DeleteVideoConferenceAsync(Guid videoConferenceId, string userId);

    /// <summary>
    /// Adds dial-in numbers to a video conference
    /// </summary>
    Task<VideoConference> AddDialInNumbersAsync(Guid videoConferenceId, List<DialInNumber> dialInNumbers, string? dialInPasscode = null);

    /// <summary>
    /// Checks if a provider is configured and available
    /// </summary>
    bool IsProviderAvailable(VideoConferenceProvider provider);
}
