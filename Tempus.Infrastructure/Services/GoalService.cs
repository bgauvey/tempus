using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IEventRepository _eventRepository;
    private readonly INotificationRepository _notificationRepository;

    public GoalService(
        IGoalRepository goalRepository,
        IEventRepository eventRepository,
        INotificationRepository notificationRepository)
    {
        _goalRepository = goalRepository;
        _eventRepository = eventRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<Goal?> GetByIdAsync(Guid id)
    {
        return await _goalRepository.GetByIdAsync(id);
    }

    public async Task<Goal?> GetByIdWithProgressAsync(Guid id)
    {
        return await _goalRepository.GetByIdWithProgressAsync(id);
    }

    public async Task<List<Goal>> GetUserGoalsAsync(string userId)
    {
        return await _goalRepository.GetByUserIdAsync(userId);
    }

    public async Task<List<Goal>> GetActiveGoalsAsync(string userId)
    {
        return await _goalRepository.GetActiveByUserIdAsync(userId);
    }

    public async Task<List<Goal>> GetGoalsByCategoryAsync(string userId, GoalCategory category)
    {
        return await _goalRepository.GetByUserIdAndCategoryAsync(userId, category);
    }

    public async Task<List<Goal>> GetGoalsWithProgressAsync(string userId)
    {
        return await _goalRepository.GetByUserIdWithProgressAsync(userId);
    }

    public async Task<Goal> CreateGoalAsync(string userId, Goal goal)
    {
        goal.UserId = userId;
        goal.CreatedAt = DateTime.UtcNow;

        // Set default color if not provided
        if (string.IsNullOrEmpty(goal.Color))
        {
            goal.Color = goal.GetDefaultColor();
        }

        // Set default icon if not provided
        if (string.IsNullOrEmpty(goal.Icon))
        {
            goal.Icon = goal.GetIcon();
        }

        var createdGoal = await _goalRepository.AddAsync(goal);

        // If smart scheduling is enabled, schedule initial sessions
        if (goal.EnableSmartScheduling)
        {
            var endDate = goal.EndDate ?? DateTime.UtcNow.AddMonths(3);
            await ScheduleGoalSessionsAsync(userId, createdGoal.Id, goal.StartDate, endDate);
        }

        return createdGoal;
    }

    public async Task<Goal> UpdateGoalAsync(string userId, Guid id, Goal updatedGoal)
    {
        var existingGoal = await _goalRepository.GetByIdAsync(id);
        if (existingGoal == null)
        {
            throw new KeyNotFoundException($"Goal with ID {id} not found");
        }

        if (existingGoal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this goal");
        }

        // Update properties
        existingGoal.Title = updatedGoal.Title;
        existingGoal.Description = updatedGoal.Description;
        existingGoal.Category = updatedGoal.Category;
        existingGoal.Frequency = updatedGoal.Frequency;
        existingGoal.TargetCount = updatedGoal.TargetCount;
        existingGoal.TargetDaysOfWeek = updatedGoal.TargetDaysOfWeek;
        existingGoal.TargetDurationMinutes = updatedGoal.TargetDurationMinutes;
        existingGoal.PreferredTimeOfDay = updatedGoal.PreferredTimeOfDay;
        existingGoal.Priority = updatedGoal.Priority;
        existingGoal.StartDate = updatedGoal.StartDate;
        existingGoal.EndDate = updatedGoal.EndDate;
        existingGoal.Color = updatedGoal.Color;
        existingGoal.Icon = updatedGoal.Icon;
        existingGoal.EnableSmartScheduling = updatedGoal.EnableSmartScheduling;
        existingGoal.SendReminders = updatedGoal.SendReminders;
        existingGoal.ReminderMinutesBefore = updatedGoal.ReminderMinutesBefore;
        existingGoal.Notes = updatedGoal.Notes;

        return await _goalRepository.UpdateAsync(existingGoal);
    }

    public async Task DeleteGoalAsync(string userId, Guid id)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null)
        {
            throw new KeyNotFoundException($"Goal with ID {id} not found");
        }

        if (goal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this goal");
        }

        await _goalRepository.DeleteAsync(id);
    }

    public async Task<Goal> ChangeGoalStatusAsync(string userId, Guid id, GoalStatus newStatus)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null)
        {
            throw new KeyNotFoundException($"Goal with ID {id} not found");
        }

        if (goal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this goal");
        }

        goal.Status = newStatus;
        return await _goalRepository.UpdateAsync(goal);
    }

    public async Task<GoalProgress> LogProgressAsync(string userId, Guid goalId, GoalProgress progress)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId);
        if (goal == null)
        {
            throw new KeyNotFoundException($"Goal with ID {goalId} not found");
        }

        if (goal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to log progress for this goal");
        }

        progress.GoalId = goalId;
        progress.CreatedAt = DateTime.UtcNow;

        var createdProgress = await _goalRepository.AddProgressAsync(progress);

        // Check if goal should be marked as completed
        if (goal.Status == GoalStatus.Active && await ShouldMarkAsCompleted(goal))
        {
            goal.Status = GoalStatus.Completed;
            await _goalRepository.UpdateAsync(goal);

            // Send completion notification
            await SendGoalCompletionNotificationAsync(userId, goal);
        }

        return createdProgress;
    }

    public async Task<GoalProgress> UpdateProgressAsync(string userId, Guid progressId, GoalProgress updatedProgress)
    {
        var existingProgress = await _goalRepository.GetProgressByGoalIdAsync(updatedProgress.GoalId);
        var progress = existingProgress.FirstOrDefault(p => p.Id == progressId);

        if (progress == null)
        {
            throw new KeyNotFoundException($"Progress entry with ID {progressId} not found");
        }

        var goal = await _goalRepository.GetByIdAsync(progress.GoalId);
        if (goal == null || goal.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this progress entry");
        }

        progress.CompletedAt = updatedProgress.CompletedAt;
        progress.DurationMinutes = updatedProgress.DurationMinutes;
        progress.Notes = updatedProgress.Notes;
        progress.Value = updatedProgress.Value;
        progress.ValueUnit = updatedProgress.ValueUnit;
        progress.Rating = updatedProgress.Rating;

        return await _goalRepository.UpdateProgressAsync(progress);
    }

    public async Task DeleteProgressAsync(string userId, Guid progressId)
    {
        // Note: We need to find the progress entry to verify ownership
        // This is a simplification - in production, you'd want a more efficient way
        var allGoals = await _goalRepository.GetByUserIdWithProgressAsync(userId);
        var progressEntry = allGoals
            .SelectMany(g => g.ProgressEntries)
            .FirstOrDefault(p => p.Id == progressId);

        if (progressEntry == null)
        {
            throw new KeyNotFoundException($"Progress entry with ID {progressId} not found");
        }

        await _goalRepository.DeleteProgressAsync(progressId);
    }

    public async Task<List<GoalProgress>> GetGoalProgressAsync(Guid goalId)
    {
        return await _goalRepository.GetProgressByGoalIdAsync(goalId);
    }

    public async Task<List<GoalProgress>> GetGoalProgressInRangeAsync(Guid goalId, DateTime startDate, DateTime endDate)
    {
        return await _goalRepository.GetProgressByGoalIdAndDateRangeAsync(goalId, startDate, endDate);
    }

    public async Task<List<Event>> ScheduleGoalSessionsAsync(string userId, Guid goalId, DateTime startDate, DateTime endDate)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId);
        if (goal == null || goal.UserId != userId)
        {
            throw new KeyNotFoundException($"Goal with ID {goalId} not found or access denied");
        }

        if (!goal.EnableSmartScheduling)
        {
            return new List<Event>();
        }

        var scheduledEvents = new List<Event>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            bool shouldSchedule = goal.Frequency switch
            {
                GoalFrequency.Daily => true,
                GoalFrequency.Weekly => ShouldScheduleOnDate(goal, currentDate),
                GoalFrequency.Monthly => currentDate.Day <= goal.TargetCount,
                _ => false
            };

            if (shouldSchedule && currentDate >= goal.StartDate.Date)
            {
                var eventTime = ParsePreferredTime(goal.PreferredTimeOfDay) ?? new TimeSpan(9, 0, 0);
                var eventStart = currentDate.Add(eventTime);
                var eventEnd = eventStart.AddMinutes(goal.TargetDurationMinutes ?? 60);

                var goalEvent = new Event
                {
                    UserId = userId,
                    Title = goal.Title,
                    Description = goal.Description ?? "Scheduled goal session",
                    StartTime = eventStart,
                    EndTime = eventEnd,
                    Color = goal.Color,
                    Priority = goal.Priority,
                    CreatedAt = DateTime.UtcNow
                };

                var createdEvent = await _eventRepository.CreateAsync(goalEvent);
                scheduledEvents.Add(createdEvent);
            }

            currentDate = currentDate.AddDays(1);
        }

        return scheduledEvents;
    }

    public async Task<GoalAnalytics> GetGoalAnalyticsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var goals = await _goalRepository.GetByUserIdWithProgressAsync(userId);
        var activeGoals = goals.Where(g => g.Status == GoalStatus.Active).ToList();
        var completedGoals = goals.Where(g => g.Status == GoalStatus.Completed).ToList();

        var analytics = new GoalAnalytics
        {
            TotalGoals = goals.Count,
            ActiveGoals = activeGoals.Count,
            CompletedGoals = completedGoals.Count
        };

        // Calculate total completions in date range
        var allProgress = goals.SelectMany(g => g.ProgressEntries)
            .Where(p => p.CompletedAt >= startDate && p.CompletedAt <= endDate)
            .ToList();

        analytics.TotalCompletions = allProgress.Count;

        // Calculate average completion rate
        if (activeGoals.Any())
        {
            var completionRates = activeGoals.Select(g => g.GetCurrentPeriodCompletionPercentage()).ToList();
            analytics.AverageCompletionRate = completionRates.Any() ? completionRates.Average() : 0;
        }

        // Group completions by category
        analytics.CompletionsByCategory = allProgress
            .GroupBy(p => goals.First(g => g.Id == p.GoalId).Category)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group completions by day of week
        analytics.CompletionsByDay = allProgress
            .GroupBy(p => p.CompletedAt.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Calculate streaks
        var streaks = activeGoals.Select(g => g.GetCurrentStreak()).Where(s => s > 0).ToList();
        analytics.CurrentLongestStreak = streaks.Any() ? streaks.Max() : 0;
        analytics.TotalActiveStreaks = streaks.Count;

        return analytics;
    }

    public async Task<Dictionary<GoalCategory, int>> GetCategoryStatisticsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _goalRepository.GetCategoryStatisticsAsync(userId, startDate, endDate);
    }

    public async Task<int> CalculateStreakAsync(Guid goalId)
    {
        var goal = await _goalRepository.GetByIdWithProgressAsync(goalId);
        return goal?.GetCurrentStreak() ?? 0;
    }

    private bool ShouldScheduleOnDate(Goal goal, DateTime date)
    {
        if (string.IsNullOrEmpty(goal.TargetDaysOfWeek))
            return false;

        var targetDays = goal.TargetDaysOfWeek.Split(',')
            .Select(d => int.Parse(d.Trim()))
            .ToList();

        return targetDays.Contains((int)date.DayOfWeek);
    }

    private TimeSpan? ParsePreferredTime(string? timeString)
    {
        if (string.IsNullOrEmpty(timeString))
            return null;

        if (TimeSpan.TryParse(timeString, out var time))
            return time;

        return null;
    }

    private async Task<bool> ShouldMarkAsCompleted(Goal goal)
    {
        // For goals with an end date that has passed and target is met
        if (goal.EndDate.HasValue && goal.EndDate.Value < DateTime.UtcNow)
        {
            var completionRate = goal.GetCurrentPeriodCompletionPercentage();
            return completionRate >= 80; // Consider complete if 80% or more achieved
        }

        return false;
    }

    private async Task SendGoalCompletionNotificationAsync(string userId, Goal goal)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = "Goal Completed! 🎉",
            Message = $"Congratulations! You've completed your goal: {goal.Title}",
            Type = NotificationType.GoalAchievement,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _notificationRepository.CreateAsync(notification);
    }
}
