using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class VideoConferenceService : IVideoConferenceService
{
    private readonly TempusDbContext _context;
    private readonly ILogger<VideoConferenceService> _logger;

    public VideoConferenceService(TempusDbContext context, ILogger<VideoConferenceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VideoConference> CreateCustomMeetingAsync(Guid eventId, string meetingUrl, string? meetingId, string? passcode, string userId)
    {
        // Check if event exists and belongs to user
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId && e.UserId == userId);

        if (!eventExists)
        {
            _logger.LogWarning("Event {EventId} not found or doesn't belong to user {UserId}", eventId, userId);
            throw new UnauthorizedAccessException("Event not found or access denied");
        }

        // Check if event already has a video conference
        var existing = await _context.VideoConferences
            .FirstOrDefaultAsync(v => v.EventId == eventId);

        if (existing != null)
        {
            _logger.LogWarning("Event {EventId} already has a video conference", eventId);
            throw new InvalidOperationException("Event already has a video conference. Delete the existing one first.");
        }

        // Validate URL
        if (string.IsNullOrWhiteSpace(meetingUrl))
        {
            throw new ArgumentException("Meeting URL is required", nameof(meetingUrl));
        }

        if (!Uri.TryCreate(meetingUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid meeting URL format", nameof(meetingUrl));
        }

        var videoConference = new VideoConference
        {
            EventId = eventId,
            Provider = DetectProvider(meetingUrl),
            MeetingUrl = meetingUrl,
            MeetingId = meetingId,
            Passcode = passcode,
            CreatedBy = userId
        };

        _context.VideoConferences.Add(videoConference);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created video conference {VideoConferenceId} for event {EventId}",
            videoConference.Id, eventId);

        return videoConference;
    }

    public async Task<VideoConference?> GetVideoConferenceAsync(Guid eventId)
    {
        return await _context.VideoConferences
            .Include(v => v.Event)
            .FirstOrDefaultAsync(v => v.EventId == eventId);
    }

    public async Task<VideoConference> UpdateVideoConferenceAsync(Guid videoConferenceId, string meetingUrl, string? meetingId, string? passcode)
    {
        var videoConference = await _context.VideoConferences.FindAsync(videoConferenceId);

        if (videoConference == null)
        {
            _logger.LogWarning("Video conference {VideoConferenceId} not found", videoConferenceId);
            throw new KeyNotFoundException("Video conference not found");
        }

        // Validate URL
        if (string.IsNullOrWhiteSpace(meetingUrl))
        {
            throw new ArgumentException("Meeting URL is required", nameof(meetingUrl));
        }

        if (!Uri.TryCreate(meetingUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid meeting URL format", nameof(meetingUrl));
        }

        videoConference.MeetingUrl = meetingUrl;
        videoConference.MeetingId = meetingId;
        videoConference.Passcode = passcode;
        videoConference.Provider = DetectProvider(meetingUrl);
        videoConference.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated video conference {VideoConferenceId}", videoConferenceId);

        return videoConference;
    }

    public async Task<bool> DeleteVideoConferenceAsync(Guid videoConferenceId, string userId)
    {
        var videoConference = await _context.VideoConferences
            .Include(v => v.Event)
            .FirstOrDefaultAsync(v => v.Id == videoConferenceId);

        if (videoConference == null)
        {
            _logger.LogWarning("Video conference {VideoConferenceId} not found", videoConferenceId);
            return false;
        }

        // Verify user owns the event
        if (videoConference.Event?.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete video conference {VideoConferenceId} they don't own",
                userId, videoConferenceId);
            throw new UnauthorizedAccessException("You don't have permission to delete this video conference");
        }

        _context.VideoConferences.Remove(videoConference);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted video conference {VideoConferenceId}", videoConferenceId);

        return true;
    }

    public async Task<VideoConference> AddDialInNumbersAsync(Guid videoConferenceId, List<DialInNumber> dialInNumbers, string? dialInPasscode = null)
    {
        var videoConference = await _context.VideoConferences.FindAsync(videoConferenceId);

        if (videoConference == null)
        {
            _logger.LogWarning("Video conference {VideoConferenceId} not found", videoConferenceId);
            throw new KeyNotFoundException("Video conference not found");
        }

        videoConference.SetDialInNumbers(dialInNumbers);
        videoConference.DialInPasscode = dialInPasscode;
        videoConference.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Added dial-in numbers to video conference {VideoConferenceId}", videoConferenceId);

        return videoConference;
    }

    public bool IsProviderAvailable(VideoConferenceProvider provider)
    {
        // For now, only custom links are supported
        // In the future, check for API credentials and configuration for each provider
        return provider == VideoConferenceProvider.Custom;
    }

    private VideoConferenceProvider DetectProvider(string meetingUrl)
    {
        var url = meetingUrl.ToLowerInvariant();

        if (url.Contains("zoom.us"))
            return VideoConferenceProvider.Zoom;

        if (url.Contains("teams.microsoft.com") || url.Contains("teams.live.com"))
            return VideoConferenceProvider.MicrosoftTeams;

        if (url.Contains("meet.google.com"))
            return VideoConferenceProvider.GoogleMeet;

        if (url.Contains("webex.com"))
            return VideoConferenceProvider.WebexMeetings;

        return VideoConferenceProvider.Custom;
    }
}
