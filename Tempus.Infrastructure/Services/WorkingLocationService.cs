using System.Security.Claims;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class WorkingLocationService : IWorkingLocationService
{
    private readonly IWorkingLocationRepository _repository;
    private readonly ITeamService _teamService;
    private readonly INotificationRepository _notificationRepository;

    public WorkingLocationService(
        IWorkingLocationRepository repository,
        ITeamService teamService,
        INotificationRepository notificationRepository)
    {
        _repository = repository;
        _teamService = teamService;
        _notificationRepository = notificationRepository;
    }

    public async Task<WorkingLocationStatus?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<WorkingLocationStatus>> GetUserLocationsAsync(string userId)
    {
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<WorkingLocationStatus?> GetCurrentLocationAsync(string userId)
    {
        return await _repository.GetByUserIdAndDateAsync(userId, DateTime.UtcNow);
    }

    public async Task<WorkingLocationStatus?> GetLocationAtDateAsync(string userId, DateTime date)
    {
        return await _repository.GetByUserIdAndDateAsync(userId, date);
    }

    public async Task<List<WorkingLocationStatus>> GetLocationsInRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _repository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);
    }

    public async Task<Dictionary<string, List<WorkingLocationStatus>>> GetTeamLocationsAsync(Guid teamId, DateTime startDate, DateTime endDate)
    {
        // Get team members
        var team = await _teamService.GetTeamByIdAsync(teamId);
        if (team == null)
            return new Dictionary<string, List<WorkingLocationStatus>>();

        var memberUserIds = team.Members.Select(m => m.UserId).ToList();

        // Get all locations for team members
        var locations = await _repository.GetByUserIdsAndDateRangeAsync(memberUserIds, startDate, endDate);

        // Group by user ID
        return locations
            .GroupBy(l => l.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<WorkingLocationStatus> SetLocationAsync(
        string userId,
        WorkingLocationType locationType,
        DateTime startDate,
        DateTime endDate,
        string? locationDescription = null,
        string? address = null,
        string? notes = null)
    {

        // Check for conflicts
        var hasConflict = await _repository.HasConflictAsync(userId, startDate, endDate);
        if (hasConflict)
        {
            throw new InvalidOperationException("A working location already exists for this time period.");
        }

        var location = new WorkingLocationStatus
        {
            UserId = userId,
            LocationType = locationType,
            StartDate = startDate,
            EndDate = endDate,
            LocationDescription = locationDescription,
            Address = address,
            Notes = notes,
            Color = GetDefaultColorForLocationType(locationType)
        };

        var createdLocation = await _repository.AddAsync(location);

        // Send notifications if enabled
        if (location.SendNotifications && location.NotifyUserIds.Any())
        {
            await SendLocationNotificationsAsync(createdLocation, location.NotifyUserIds);
        }

        return createdLocation;
    }

    public async Task<WorkingLocationStatus> UpdateLocationAsync(
        string userId,
        Guid id,
        WorkingLocationType locationType,
        DateTime startDate,
        DateTime endDate,
        string? locationDescription = null,
        string? address = null,
        string? notes = null)
    {
        var location = await _repository.GetByIdAsync(id);
        if (location == null)
        {
            throw new KeyNotFoundException($"Working location with ID {id} not found");
        }

        if (location.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this location");
        }

        // Check for conflicts
        var hasConflict = await _repository.HasConflictAsync(userId, startDate, endDate, id);
        if (hasConflict)
        {
            throw new InvalidOperationException("A working location already exists for this time period.");
        }

        location.LocationType = locationType;
        location.StartDate = startDate;
        location.EndDate = endDate;
        location.LocationDescription = locationDescription;
        location.Address = address;
        location.Notes = notes;
        location.Color = GetDefaultColorForLocationType(locationType);

        return await _repository.UpdateAsync(location);
    }

    public async Task DeleteLocationAsync(string userId, Guid id)
    {
        var location = await _repository.GetByIdAsync(id);
        if (location == null)
        {
            throw new KeyNotFoundException($"Working location with ID {id} not found");
        }

        if (location.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this location");
        }

        await _repository.DeleteAsync(id);
    }

    public async Task<List<WorkingLocationStatus>> SetRecurringLocationAsync(
        string userId,
        WorkingLocationType locationType,
        List<DayOfWeek> daysOfWeek,
        DateTime startDate,
        DateTime endDate,
        string? locationDescription = null)
    {
        var locations = new List<WorkingLocationStatus>();

        // Generate location entries for each specified day of week within the date range
        var currentDate = startDate.Date;
        while (currentDate <= endDate.Date)
        {
            if (daysOfWeek.Contains(currentDate.DayOfWeek))
            {
                var location = new WorkingLocationStatus
                {
                    UserId = userId,
                    LocationType = locationType,
                    StartDate = currentDate.Date.AddHours(startDate.Hour).AddMinutes(startDate.Minute),
                    EndDate = currentDate.Date.AddHours(endDate.Hour).AddMinutes(endDate.Minute),
                    LocationDescription = locationDescription,
                    IsRecurring = true,
                    RecurrencePattern = RecurrencePattern.Weekly,
                    RecurrenceDaysOfWeek = string.Join(",", daysOfWeek.Select(d => (int)d)),
                    Color = GetDefaultColorForLocationType(locationType)
                };

                var createdLocation = await _repository.AddAsync(location);
                locations.Add(createdLocation);
            }

            currentDate = currentDate.AddDays(1);
        }

        return locations;
    }

    public async Task SendLocationNotificationsAsync(WorkingLocationStatus location, List<string> recipientUserIds)
    {
        foreach (var recipientUserId in recipientUserIds)
        {
            var notification = new Notification
            {
                UserId = recipientUserId,
                Title = "Working Location Update",
                Message = $"Working location changed to {location.GetDisplayName()} from {location.StartDate:MMM dd} to {location.EndDate:MMM dd}",
                Type = NotificationType.LocationChange,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.CreateAsync(notification);
        }
    }

    public async Task<Dictionary<WorkingLocationType, int>> GetLocationStatisticsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var locations = await _repository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);

        var stats = locations
            .GroupBy(l => l.LocationType)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

        // Ensure all location types are represented
        foreach (WorkingLocationType locationType in Enum.GetValues(typeof(WorkingLocationType)))
        {
            if (!stats.ContainsKey(locationType))
            {
                stats[locationType] = 0;
            }
        }

        return stats;
    }

    private string GetDefaultColorForLocationType(WorkingLocationType locationType)
    {
        return locationType switch
        {
            WorkingLocationType.Office => "#0EA5E9", // Blue
            WorkingLocationType.Home => "#10b981", // Green
            WorkingLocationType.Remote => "#f59e0b", // Orange
            WorkingLocationType.Traveling => "#ef4444", // Red
            WorkingLocationType.Hybrid => "#8b5cf6", // Purple
            _ => "#6b7280" // Gray
        };
    }
}
