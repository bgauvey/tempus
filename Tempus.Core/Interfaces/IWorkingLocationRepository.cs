using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Repository interface for managing working location statuses
/// </summary>
public interface IWorkingLocationRepository
{
    /// <summary>
    /// Get a working location status by ID
    /// </summary>
    Task<WorkingLocationStatus?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all working location statuses for a user
    /// </summary>
    Task<List<WorkingLocationStatus>> GetByUserIdAsync(string userId);

    /// <summary>
    /// Get all active working location statuses for a user
    /// </summary>
    Task<List<WorkingLocationStatus>> GetActiveByUserIdAsync(string userId);

    /// <summary>
    /// Get working location status for a user at a specific date/time
    /// </summary>
    Task<WorkingLocationStatus?> GetByUserIdAndDateAsync(string userId, DateTime date);

    /// <summary>
    /// Get working location statuses for a user within a date range
    /// </summary>
    Task<List<WorkingLocationStatus>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get working location statuses for multiple users within a date range (for team view)
    /// </summary>
    Task<List<WorkingLocationStatus>> GetByUserIdsAndDateRangeAsync(List<string> userIds, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Add a new working location status
    /// </summary>
    Task<WorkingLocationStatus> AddAsync(WorkingLocationStatus workingLocationStatus);

    /// <summary>
    /// Update an existing working location status
    /// </summary>
    Task<WorkingLocationStatus> UpdateAsync(WorkingLocationStatus workingLocationStatus);

    /// <summary>
    /// Delete a working location status
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Delete all working location statuses for a user
    /// </summary>
    Task DeleteByUserIdAsync(string userId);

    /// <summary>
    /// Check if there's a conflicting working location status
    /// </summary>
    Task<bool> HasConflictAsync(string userId, DateTime startDate, DateTime endDate, Guid? excludeId = null);
}
