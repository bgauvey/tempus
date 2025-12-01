using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class WorkingLocationRepository : IWorkingLocationRepository
{
    private readonly TempusDbContext _context;

    public WorkingLocationRepository(TempusDbContext context)
    {
        _context = context;
    }

    public async Task<WorkingLocationStatus?> GetByIdAsync(Guid id)
    {
        return await _context.WorkingLocationStatuses
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<List<WorkingLocationStatus>> GetByUserIdAsync(string userId)
    {
        return await _context.WorkingLocationStatuses
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.StartDate)
            .ToListAsync();
    }

    public async Task<List<WorkingLocationStatus>> GetActiveByUserIdAsync(string userId)
    {
        return await _context.WorkingLocationStatuses
            .Where(w => w.UserId == userId && w.IsActive)
            .OrderBy(w => w.StartDate)
            .ToListAsync();
    }

    public async Task<WorkingLocationStatus?> GetByUserIdAndDateAsync(string userId, DateTime date)
    {
        return await _context.WorkingLocationStatuses
            .Where(w => w.UserId == userId &&
                       w.IsActive &&
                       w.StartDate <= date &&
                       w.EndDate >= date)
            .OrderByDescending(w => w.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WorkingLocationStatus>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _context.WorkingLocationStatuses
            .Where(w => w.UserId == userId &&
                       w.IsActive &&
                       w.StartDate <= endDate &&
                       w.EndDate >= startDate)
            .OrderBy(w => w.StartDate)
            .ToListAsync();
    }

    public async Task<List<WorkingLocationStatus>> GetByUserIdsAndDateRangeAsync(List<string> userIds, DateTime startDate, DateTime endDate)
    {
        return await _context.WorkingLocationStatuses
            .Include(w => w.User)
            .Where(w => userIds.Contains(w.UserId) &&
                       w.IsActive &&
                       w.IsPublic &&
                       w.StartDate <= endDate &&
                       w.EndDate >= startDate)
            .OrderBy(w => w.UserId)
            .ThenBy(w => w.StartDate)
            .ToListAsync();
    }

    public async Task<WorkingLocationStatus> AddAsync(WorkingLocationStatus workingLocationStatus)
    {
        _context.WorkingLocationStatuses.Add(workingLocationStatus);
        await _context.SaveChangesAsync();
        return workingLocationStatus;
    }

    public async Task<WorkingLocationStatus> UpdateAsync(WorkingLocationStatus workingLocationStatus)
    {
        workingLocationStatus.UpdatedAt = DateTime.UtcNow;
        _context.WorkingLocationStatuses.Update(workingLocationStatus);
        await _context.SaveChangesAsync();
        return workingLocationStatus;
    }

    public async Task DeleteAsync(Guid id)
    {
        var workingLocationStatus = await _context.WorkingLocationStatuses.FindAsync(id);
        if (workingLocationStatus != null)
        {
            _context.WorkingLocationStatuses.Remove(workingLocationStatus);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        var workingLocationStatuses = await _context.WorkingLocationStatuses
            .Where(w => w.UserId == userId)
            .ToListAsync();

        _context.WorkingLocationStatuses.RemoveRange(workingLocationStatuses);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasConflictAsync(string userId, DateTime startDate, DateTime endDate, Guid? excludeId = null)
    {
        var query = _context.WorkingLocationStatuses
            .Where(w => w.UserId == userId &&
                       w.IsActive &&
                       w.StartDate <= endDate &&
                       w.EndDate >= startDate);

        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
