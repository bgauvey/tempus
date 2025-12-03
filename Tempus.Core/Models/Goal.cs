using Tempus.Core.Enums;

namespace Tempus.Core.Models;

/// <summary>
/// Represents a goal or habit that a user wants to track and achieve
/// </summary>
public class Goal
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User this goal belongs to
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Title/name of the goal or habit
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the goal
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category of this goal
    /// </summary>
    public GoalCategory Category { get; set; } = GoalCategory.Personal;

    /// <summary>
    /// Current status of the goal
    /// </summary>
    public GoalStatus Status { get; set; } = GoalStatus.Active;

    /// <summary>
    /// Frequency pattern for this goal
    /// </summary>
    public GoalFrequency Frequency { get; set; } = GoalFrequency.Weekly;

    /// <summary>
    /// Target number of times to complete within the frequency period
    /// (e.g., 3 for "3 times per week")
    /// </summary>
    public int TargetCount { get; set; } = 1;

    /// <summary>
    /// Days of week for weekly goals (comma-separated: "1,3,5" for Mon,Wed,Fri)
    /// </summary>
    public string? TargetDaysOfWeek { get; set; }

    /// <summary>
    /// Target duration in minutes for each session (optional)
    /// </summary>
    public int? TargetDurationMinutes { get; set; }

    /// <summary>
    /// Preferred time of day for scheduling (HH:mm format)
    /// </summary>
    public string? PreferredTimeOfDay { get; set; }

    /// <summary>
    /// Priority level of this goal
    /// </summary>
    public Priority Priority { get; set; } = Priority.Medium;

    /// <summary>
    /// When this goal starts (in UTC)
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this goal ends (optional, null for ongoing habits)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Color to display in calendar (hex code)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Icon name for display
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Whether to automatically schedule time blocks for this goal
    /// </summary>
    public bool EnableSmartScheduling { get; set; } = false;

    /// <summary>
    /// Whether to send reminders for this goal
    /// </summary>
    public bool SendReminders { get; set; } = true;

    /// <summary>
    /// Minutes before scheduled time to send reminder
    /// </summary>
    public int ReminderMinutesBefore { get; set; } = 15;

    /// <summary>
    /// Custom notes or instructions
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Progress entries for this goal
    /// </summary>
    public List<GoalProgress> ProgressEntries { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Get display icon based on category
    /// </summary>
    public string GetIcon()
    {
        if (!string.IsNullOrEmpty(Icon))
            return Icon;

        return Category switch
        {
            GoalCategory.Health => "health_and_safety",
            GoalCategory.Fitness => "fitness_center",
            GoalCategory.Productivity => "productivity",
            GoalCategory.Learning => "school",
            GoalCategory.Finance => "attach_money",
            GoalCategory.Social => "groups",
            GoalCategory.Career => "work",
            GoalCategory.Personal => "person",
            _ => "flag"
        };
    }

    /// <summary>
    /// Get default color based on category
    /// </summary>
    public string GetDefaultColor()
    {
        if (!string.IsNullOrEmpty(Color))
            return Color;

        return Category switch
        {
            GoalCategory.Health => "#10b981",
            GoalCategory.Fitness => "#ef4444",
            GoalCategory.Productivity => "#8b5cf6",
            GoalCategory.Learning => "#3b82f6",
            GoalCategory.Finance => "#f59e0b",
            GoalCategory.Social => "#ec4899",
            GoalCategory.Career => "#06b6d4",
            GoalCategory.Personal => "#6366f1",
            _ => "#6b7280"
        };
    }

    /// <summary>
    /// Calculate completion percentage for current period
    /// </summary>
    public double GetCurrentPeriodCompletionPercentage()
    {
        var periodStart = GetCurrentPeriodStart();
        var completedCount = ProgressEntries.Count(p =>
            p.CompletedAt >= periodStart &&
            p.CompletedAt < DateTime.UtcNow);

        return TargetCount > 0
            ? Math.Min(100, (completedCount * 100.0) / TargetCount)
            : 0;
    }

    /// <summary>
    /// Get the start of the current frequency period
    /// </summary>
    public DateTime GetCurrentPeriodStart()
    {
        var now = DateTime.UtcNow;

        return Frequency switch
        {
            GoalFrequency.Daily => now.Date,
            GoalFrequency.Weekly => now.Date.AddDays(-(int)now.DayOfWeek),
            GoalFrequency.Monthly => new DateTime(now.Year, now.Month, 1),
            _ => StartDate
        };
    }

    /// <summary>
    /// Check if goal is active and within date range
    /// </summary>
    public bool IsActiveNow()
    {
        var now = DateTime.UtcNow;
        return Status == GoalStatus.Active &&
               now >= StartDate &&
               (EndDate == null || now <= EndDate);
    }

    /// <summary>
    /// Get total completion count
    /// </summary>
    public int GetTotalCompletionCount()
    {
        return ProgressEntries.Count;
    }

    /// <summary>
    /// Get current streak (consecutive days/weeks/months with completions)
    /// </summary>
    public int GetCurrentStreak()
    {
        if (!ProgressEntries.Any()) return 0;

        var orderedEntries = ProgressEntries
            .OrderByDescending(p => p.CompletedAt)
            .ToList();

        int streak = 0;
        var checkDate = DateTime.UtcNow.Date;

        foreach (var entry in orderedEntries)
        {
            var entryDate = entry.CompletedAt.Date;

            if (Frequency == GoalFrequency.Daily)
            {
                if (entryDate == checkDate || entryDate == checkDate.AddDays(-1))
                {
                    streak++;
                    checkDate = entryDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }
            else if (Frequency == GoalFrequency.Weekly)
            {
                var entryWeekStart = entryDate.AddDays(-(int)entryDate.DayOfWeek);
                var checkWeekStart = checkDate.AddDays(-(int)checkDate.DayOfWeek);

                if (entryWeekStart == checkWeekStart || entryWeekStart == checkWeekStart.AddDays(-7))
                {
                    streak++;
                    checkDate = entryWeekStart.AddDays(-7);
                }
                else
                {
                    break;
                }
            }
        }

        return streak;
    }
}
