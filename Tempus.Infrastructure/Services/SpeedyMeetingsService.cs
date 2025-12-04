using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class SpeedyMeetingsService : ISpeedyMeetingsService
{
    private readonly ISettingsService _settingsService;

    public SpeedyMeetingsService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<bool> IsEnabledForUserAsync(string userId)
    {
        var settings = await _settingsService.GetOrCreateDefaultSettingsAsync(userId);
        return settings.EnableSpeedyMeetings;
    }

    public async Task<DateTime> ApplySpeedyMeetingsAsync(string userId, DateTime startTime, DateTime originalEndTime)
    {
        var settings = await _settingsService.GetOrCreateDefaultSettingsAsync(userId);

        // Check if speedy meetings is enabled
        if (!settings.EnableSpeedyMeetings)
        {
            return originalEndTime;
        }

        // Calculate original duration
        var duration = (int)(originalEndTime - startTime).TotalMinutes;

        // Check if event duration meets threshold
        if (!ShouldApplyToEvent(settings, duration))
        {
            return originalEndTime;
        }

        // Apply speedy meetings reduction
        var adjustedEndTime = originalEndTime.AddMinutes(-settings.SpeedyMeetingsMinutes);

        // Make sure adjusted time doesn't go before start time
        if (adjustedEndTime <= startTime)
        {
            return originalEndTime;
        }

        return adjustedEndTime;
    }

    public async Task<int> GetAdjustedDurationAsync(string userId, int originalDurationMinutes)
    {
        var settings = await _settingsService.GetOrCreateDefaultSettingsAsync(userId);

        // Check if speedy meetings is enabled
        if (!settings.EnableSpeedyMeetings)
        {
            return originalDurationMinutes;
        }

        // Check if event duration meets threshold
        if (!ShouldApplyToEvent(settings, originalDurationMinutes))
        {
            return originalDurationMinutes;
        }

        // Apply speedy meetings reduction
        var adjustedDuration = originalDurationMinutes - settings.SpeedyMeetingsMinutes;

        // Make sure we don't go negative or too short
        if (adjustedDuration <= 5)
        {
            return originalDurationMinutes;
        }

        return adjustedDuration;
    }

    public async Task<bool> ShouldApplyToEventAsync(string userId, int durationMinutes)
    {
        var settings = await _settingsService.GetOrCreateDefaultSettingsAsync(userId);
        return settings.EnableSpeedyMeetings && ShouldApplyToEvent(settings, durationMinutes);
    }

    public async Task<int> GetSpeedyMeetingsMinutesAsync(string userId)
    {
        var settings = await _settingsService.GetOrCreateDefaultSettingsAsync(userId);
        return settings.SpeedyMeetingsMinutes;
    }

    private bool ShouldApplyToEvent(CalendarSettings settings, int durationMinutes)
    {
        // If applying to short events is disabled, check threshold
        if (!settings.ApplySpeedyMeetingsToShortEvents)
        {
            // Only apply to meetings longer than threshold
            if (durationMinutes <= settings.SpeedyMeetingsThresholdMinutes)
            {
                return false;
            }
        }

        // Make sure the meeting is long enough to be shortened
        // Don't apply if the meeting would become less than 10 minutes
        if (durationMinutes - settings.SpeedyMeetingsMinutes < 10)
        {
            return false;
        }

        return true;
    }
}
