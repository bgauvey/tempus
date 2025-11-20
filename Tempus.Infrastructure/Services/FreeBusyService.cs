using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class FreeBusyService : IFreeBusyService
{
    private readonly TempusDbContext _context;
    private readonly ILogger<FreeBusyService> _logger;
    private readonly ITeamService _teamService;
    private readonly ISettingsService _settingsService;

    public FreeBusyService(
        TempusDbContext context,
        ILogger<FreeBusyService> logger,
        ITeamService teamService,
        ISettingsService settingsService)
    {
        _context = context;
        _logger = logger;
        _teamService = teamService;
        _settingsService = settingsService;
    }

    public async Task<FreeBusyInfo?> GetFreeBusyInfoAsync(string targetUserId, string requestingUserId,
        DateTime startTime, DateTime endTime, bool includeDetails = false)
    {
        // Check if requesting user has permission to view free/busy information
        if (!await CanViewFreeBusyAsync(targetUserId, requestingUserId))
        {
            _logger.LogWarning("User {RequestingUserId} does not have permission to view free/busy for user {TargetUserId}",
                requestingUserId, targetUserId);
            return null;
        }

        // Get target user information
        var targetUser = await _context.Users.FindAsync(targetUserId);
        if (targetUser == null)
        {
            _logger.LogWarning("Target user {TargetUserId} not found", targetUserId);
            return null;
        }

        // Get target user's settings
        var settings = await _settingsService.GetUserSettingsAsync(targetUserId);

        // Get events for the user in the specified date range
        var events = await _context.Events
            .Where(e => e.UserId == targetUserId &&
                       e.StartTime < endTime &&
                       e.EndTime > startTime)
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        // Create free/busy time slots
        var busyTimes = new List<FreeBusyTimeSlot>();

        foreach (var evt in events)
        {
            var timeSlot = new FreeBusyTimeSlot
            {
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                IsBusy = true,
                IsPrivate = evt.IsPrivate
            };

            // Only include details if requested and user has permission
            if (includeDetails && !evt.IsPrivate)
            {
                timeSlot.Subject = evt.Title;
                timeSlot.Location = evt.Location;
            }
            // If private event, show generic busy message
            else if (evt.IsPrivate && settings.ShowPrivateEventsAsBusy)
            {
                timeSlot.Subject = "Busy";
            }
            else if (!includeDetails)
            {
                timeSlot.Subject = "Busy";
            }

            busyTimes.Add(timeSlot);
        }

        var freeBusyInfo = new FreeBusyInfo
        {
            UserId = targetUserId,
            DisplayName = $"{targetUser.FirstName} {targetUser.LastName}".Trim(),
            Email = targetUser.Email ?? string.Empty,
            StartTime = startTime,
            EndTime = endTime,
            BusyTimes = busyTimes,
            CanViewDetails = includeDetails,
            TimeZone = targetUser.TimeZone
        };

        _logger.LogInformation("Retrieved free/busy information for user {TargetUserId} from {StartTime} to {EndTime}. Found {BusyCount} busy times",
            targetUserId, startTime, endTime, busyTimes.Count);

        return freeBusyInfo;
    }

    public async Task<List<FreeBusyInfo>> GetFreeBusyInfoForMultipleUsersAsync(List<string> targetUserIds,
        string requestingUserId, DateTime startTime, DateTime endTime, bool includeDetails = false)
    {
        var result = new List<FreeBusyInfo>();

        foreach (var userId in targetUserIds)
        {
            var freeBusyInfo = await GetFreeBusyInfoAsync(userId, requestingUserId, startTime, endTime, includeDetails);
            if (freeBusyInfo != null)
            {
                result.Add(freeBusyInfo);
            }
        }

        return result;
    }

    public async Task<bool> CanViewFreeBusyAsync(string targetUserId, string requestingUserId)
    {
        // Users can always view their own free/busy information
        if (targetUserId == requestingUserId)
        {
            return true;
        }

        // Get target user's settings
        var settings = await _settingsService.GetUserSettingsAsync(targetUserId);

        // Check if user has disabled free/busy sharing
        if (!settings.PublishFreeBusyInformation)
        {
            return false;
        }

        // Check sharing level
        switch (settings.FreeBusySharingLevel)
        {
            case FreeBusySharingLevel.None:
                return false;

            case FreeBusySharingLevel.TeamMembers:
                // Check if users are on the same team
                return await AreUsersOnSameTeamAsync(targetUserId, requestingUserId);

            case FreeBusySharingLevel.Organization:
                // For now, all authenticated users are considered part of the organization
                return true;

            case FreeBusySharingLevel.Public:
                return true;

            default:
                return false;
        }
    }

    public async Task<List<FreeBusyTimeSlot>> FindAvailableTimeSlotsAsync(List<string> attendeeUserIds,
        string requestingUserId, DateTime startTime, DateTime endTime,
        int durationMinutes, bool workingHoursOnly = true)
    {
        // Get free/busy information for all attendees
        var freeBusyInfoList = await GetFreeBusyInfoForMultipleUsersAsync(
            attendeeUserIds, requestingUserId, startTime, endTime, false);

        // Get requesting user's settings for working hours
        var settings = await _settingsService.GetUserSettingsAsync(requestingUserId);

        var availableSlots = new List<FreeBusyTimeSlot>();

        // Generate potential time slots (e.g., every 30 minutes)
        var slotIncrement = TimeSpan.FromMinutes(30);
        var currentTime = startTime;

        while (currentTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= endTime)
        {
            var slotEnd = currentTime.Add(TimeSpan.FromMinutes(durationMinutes));

            // Check if this time slot is during working hours
            if (workingHoursOnly)
            {
                var timeOfDay = currentTime.TimeOfDay;
                if (timeOfDay < settings.WorkHoursStart || timeOfDay >= settings.WorkHoursEnd)
                {
                    currentTime = currentTime.Add(slotIncrement);
                    continue;
                }

                // Check if it's a weekend
                var workingDays = settings.WorkingDays.Split(',').Select(int.Parse).ToList();
                if (!workingDays.Contains((int)currentTime.DayOfWeek))
                {
                    currentTime = currentTime.Add(slotIncrement);
                    continue;
                }
            }

            // Check if all attendees are available during this slot
            bool allAvailable = true;
            foreach (var freeBusyInfo in freeBusyInfoList)
            {
                if (freeBusyInfo.BusyTimes.Any(bt =>
                    bt.StartTime < slotEnd && bt.EndTime > currentTime))
                {
                    allAvailable = false;
                    break;
                }
            }

            if (allAvailable)
            {
                availableSlots.Add(new FreeBusyTimeSlot
                {
                    StartTime = currentTime,
                    EndTime = slotEnd,
                    IsBusy = false
                });
            }

            currentTime = currentTime.Add(slotIncrement);
        }

        _logger.LogInformation("Found {AvailableCount} available time slots for {AttendeeCount} attendees between {StartTime} and {EndTime}",
            availableSlots.Count, attendeeUserIds.Count, startTime, endTime);

        return availableSlots;
    }

    private async Task<bool> AreUsersOnSameTeamAsync(string userId1, string userId2)
    {
        // Get teams for both users
        var user1Teams = await _teamService.GetUserTeamsAsync(userId1);
        var user2Teams = await _teamService.GetUserTeamsAsync(userId2);

        // Check if they share any teams
        var user1TeamIds = user1Teams.Select(t => t.Id).ToHashSet();
        return user2Teams.Any(t => user1TeamIds.Contains(t.Id));
    }
}
