using Microsoft.EntityFrameworkCore;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly TempusDbContext _context;

    public GoalRepository(TempusDbContext context)
    {
        _context = context;
    }

    public async Task<Goal?> GetByIdAsync(Guid id)
    {
        return await _context.Goals
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Goal?> GetByIdWithProgressAsync(Guid id)
    {
        return await _context.Goals
            .Include(g => g.User)
            .Include(g => g.ProgressEntries)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Goal>> GetByUserIdAsync(string userId)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Goal>> GetActiveByUserIdAsync(string userId)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId && g.Status == GoalStatus.Active)
            .OrderBy(g => g.Priority)
            .ThenBy(g => g.StartDate)
            .ToListAsync();
    }

    public async Task<List<Goal>> GetByUserIdAndStatusAsync(string userId, GoalStatus status)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId && g.Status == status)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Goal>> GetByUserIdAndCategoryAsync(string userId, GoalCategory category)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId && g.Category == category)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Goal>> GetByUserIdWithProgressAsync(string userId)
    {
        return await _context.Goals
            .Include(g => g.ProgressEntries)
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Goal>> GetGoalsNeedingSchedulingAsync(string userId)
    {
        var now = DateTime.UtcNow;
        return await _context.Goals
            .Where(g => g.UserId == userId &&
                       g.Status == GoalStatus.Active &&
                       g.EnableSmartScheduling &&
                       g.StartDate <= now &&
                       (g.EndDate == null || g.EndDate >= now))
            .ToListAsync();
    }

    public async Task<Goal> AddAsync(Goal goal)
    {
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<Goal> UpdateAsync(Goal goal)
    {
        goal.UpdatedAt = DateTime.UtcNow;
        _context.Goals.Update(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task DeleteAsync(Guid id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal != null)
        {
            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        var goals = await _context.Goals
            .Where(g => g.UserId == userId)
            .ToListAsync();

        _context.Goals.RemoveRange(goals);
        await _context.SaveChangesAsync();
    }

    public async Task<GoalProgress> AddProgressAsync(GoalProgress progress)
    {
        _context.GoalProgress.Add(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<List<GoalProgress>> GetProgressByGoalIdAsync(Guid goalId)
    {
        return await _context.GoalProgress
            .Where(p => p.GoalId == goalId)
            .OrderByDescending(p => p.CompletedAt)
            .ToListAsync();
    }

    public async Task<List<GoalProgress>> GetProgressByGoalIdAndDateRangeAsync(Guid goalId, DateTime startDate, DateTime endDate)
    {
        return await _context.GoalProgress
            .Where(p => p.GoalId == goalId &&
                       p.CompletedAt >= startDate &&
                       p.CompletedAt <= endDate)
            .OrderByDescending(p => p.CompletedAt)
            .ToListAsync();
    }

    public async Task<GoalProgress> UpdateProgressAsync(GoalProgress progress)
    {
        _context.GoalProgress.Update(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task DeleteProgressAsync(Guid progressId)
    {
        var progress = await _context.GoalProgress.FindAsync(progressId);
        if (progress != null)
        {
            _context.GoalProgress.Remove(progress);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<GoalCategory, int>> GetCategoryStatisticsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var progressEntries = await _context.GoalProgress
            .Include(p => p.Goal)
            .Where(p => p.Goal!.UserId == userId &&
                       p.CompletedAt >= startDate &&
                       p.CompletedAt <= endDate)
            .ToListAsync();

        return progressEntries
            .GroupBy(p => p.Goal!.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
