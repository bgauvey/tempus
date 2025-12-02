using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service interface for working location management
/// </summary>
public interface IWorkingLocationService
{
    /// <summary>
    /// Get a working location status by ID
    /// </summary>
    Task<WorkingLocationStatus?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all working location statuses for a specific user
    /// </summary>
    Task<List<WorkingLocationStatus>> GetUserLocationsAsync(string userId);

    /// <summary>
    /// Get current working location for a user
    /// </summary>
    Task<WorkingLocationStatus?> GetCurrentLocationAsync(string userId);

    /// <summary>
    /// Get working location status for a user at a specific date/time
    /// </summary>
    Task<WorkingLocationStatus?> GetLocationAtDateAsync(string userId, DateTime date);

    /// <summary>
    /// Get working location statuses within a date range
    /// </summary>
    Task<List<WorkingLocationStatus>> GetLocationsInRangeAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get team members' working locations for a date range
    /// </summary>
    Task<Dictionary<string, List<WorkingLocationStatus>>> GetTeamLocationsAsync(Guid teamId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Set working location status
    /// </summary>
    Task<WorkingLocationStatus> SetLocationAsync(string userId, WorkingLocationType locationType, DateTime startDate, DateTime endDate, string? locationDescription = null, string? address = null, string? notes = null);

    /// <summary>
    /// Update working location status
    /// </summary>
    Task<WorkingLocationStatus> UpdateLocationAsync(string userId, Guid id, WorkingLocationType locationType, DateTime startDate, DateTime endDate, string? locationDescription = null, string? address = null, string? notes = null);

    /// <summary>
    /// Delete working location status
    /// </summary>
    Task DeleteLocationAsync(string userId, Guid id);

    /// <summary>
    /// Set recurring working location (e.g., "Office on Mon/Wed/Fri, Home on Tue/Thu")
    /// </summary>
    Task<List<WorkingLocationStatus>> SetRecurringLocationAsync(string userId, WorkingLocationType locationType, List<DayOfWeek> daysOfWeek, DateTime startDate, DateTime endDate, string? locationDescription = null);

    /// <summary>
    /// Send notifications to specified users about location change
    /// </summary>
    Task SendLocationNotificationsAsync(WorkingLocationStatus location, List<string> recipientUserIds);

    /// <summary>
    /// Get location statistics for analytics
    /// </summary>
    Task<Dictionary<WorkingLocationType, int>> GetLocationStatisticsAsync(string userId, DateTime startDate, DateTime endDate);
}
