using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Repository interface for managing goals and habits
/// </summary>
public interface IGoalRepository
{
    /// <summary>
    /// Get a goal by ID
    /// </summary>
    Task<Goal?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get a goal by ID with progress entries
    /// </summary>
    Task<Goal?> GetByIdWithProgressAsync(Guid id);

    /// <summary>
    /// Get all goals for a user
    /// </summary>
    Task<List<Goal>> GetByUserIdAsync(string userId);

    /// <summary>
    /// Get active goals for a user
    /// </summary>
    Task<List<Goal>> GetActiveByUserIdAsync(string userId);

    /// <summary>
    /// Get goals by status for a user
    /// </summary>
    Task<List<Goal>> GetByUserIdAndStatusAsync(string userId, GoalStatus status);

    /// <summary>
    /// Get goals by category for a user
    /// </summary>
    Task<List<Goal>> GetByUserIdAndCategoryAsync(string userId, GoalCategory category);

    /// <summary>
    /// Get goals with progress entries for analytics
    /// </summary>
    Task<List<Goal>> GetByUserIdWithProgressAsync(string userId);

    /// <summary>
    /// Get goals that need scheduling (active with smart scheduling enabled)
    /// </summary>
    Task<List<Goal>> GetGoalsNeedingSchedulingAsync(string userId);

    /// <summary>
    /// Add a new goal
    /// </summary>
    Task<Goal> AddAsync(Goal goal);

    /// <summary>
    /// Update an existing goal
    /// </summary>
    Task<Goal> UpdateAsync(Goal goal);

    /// <summary>
    /// Delete a goal
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Delete all goals for a user
    /// </summary>
    Task DeleteByUserIdAsync(string userId);

    /// <summary>
    /// Add progress entry to a goal
    /// </summary>
    Task<GoalProgress> AddProgressAsync(GoalProgress progress);

    /// <summary>
    /// Get progress entries for a goal
    /// </summary>
    Task<List<GoalProgress>> GetProgressByGoalIdAsync(Guid goalId);

    /// <summary>
    /// Get progress entries for a goal within a date range
    /// </summary>
    Task<List<GoalProgress>> GetProgressByGoalIdAndDateRangeAsync(Guid goalId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Update progress entry
    /// </summary>
    Task<GoalProgress> UpdateProgressAsync(GoalProgress progress);

    /// <summary>
    /// Delete progress entry
    /// </summary>
    Task DeleteProgressAsync(Guid progressId);

    /// <summary>
    /// Get completion statistics for analytics
    /// </summary>
    Task<Dictionary<GoalCategory, int>> GetCategoryStatisticsAsync(string userId, DateTime startDate, DateTime endDate);
}
