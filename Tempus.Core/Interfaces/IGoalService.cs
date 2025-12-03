using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service interface for goal and habit management
/// </summary>
public interface IGoalService
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
    Task<List<Goal>> GetUserGoalsAsync(string userId);

    /// <summary>
    /// Get active goals for a user
    /// </summary>
    Task<List<Goal>> GetActiveGoalsAsync(string userId);

    /// <summary>
    /// Get goals by category
    /// </summary>
    Task<List<Goal>> GetGoalsByCategoryAsync(string userId, GoalCategory category);

    /// <summary>
    /// Get goals with progress for dashboard
    /// </summary>
    Task<List<Goal>> GetGoalsWithProgressAsync(string userId);

    /// <summary>
    /// Create a new goal
    /// </summary>
    Task<Goal> CreateGoalAsync(string userId, Goal goal);

    /// <summary>
    /// Update an existing goal
    /// </summary>
    Task<Goal> UpdateGoalAsync(string userId, Guid id, Goal updatedGoal);

    /// <summary>
    /// Delete a goal
    /// </summary>
    Task DeleteGoalAsync(string userId, Guid id);

    /// <summary>
    /// Change goal status (Active, Paused, Completed, Archived)
    /// </summary>
    Task<Goal> ChangeGoalStatusAsync(string userId, Guid id, GoalStatus newStatus);

    /// <summary>
    /// Log progress for a goal
    /// </summary>
    Task<GoalProgress> LogProgressAsync(string userId, Guid goalId, GoalProgress progress);

    /// <summary>
    /// Update progress entry
    /// </summary>
    Task<GoalProgress> UpdateProgressAsync(string userId, Guid progressId, GoalProgress updatedProgress);

    /// <summary>
    /// Delete progress entry
    /// </summary>
    Task DeleteProgressAsync(string userId, Guid progressId);

    /// <summary>
    /// Get progress entries for a goal
    /// </summary>
    Task<List<GoalProgress>> GetGoalProgressAsync(Guid goalId);

    /// <summary>
    /// Get progress entries for a goal within a date range
    /// </summary>
    Task<List<GoalProgress>> GetGoalProgressInRangeAsync(Guid goalId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Schedule goal sessions automatically (smart scheduling)
    /// </summary>
    Task<List<Event>> ScheduleGoalSessionsAsync(string userId, Guid goalId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get analytics for goals
    /// </summary>
    Task<GoalAnalytics> GetGoalAnalyticsAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get completion statistics by category
    /// </summary>
    Task<Dictionary<GoalCategory, int>> GetCategoryStatisticsAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Calculate streak for a goal
    /// </summary>
    Task<int> CalculateStreakAsync(Guid goalId);
}

/// <summary>
/// Analytics data for goals
/// </summary>
public class GoalAnalytics
{
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedGoals { get; set; }
    public int TotalCompletions { get; set; }
    public double AverageCompletionRate { get; set; }
    public Dictionary<GoalCategory, int> CompletionsByCategory { get; set; } = new();
    public Dictionary<string, int> CompletionsByDay { get; set; } = new();
    public int CurrentLongestStreak { get; set; }
    public int TotalActiveStreaks { get; set; }
}
