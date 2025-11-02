using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class CustomRangeRepository : ICustomRangeRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;

    public CustomRangeRepository(IDbContextFactory<TempusDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<CustomCalendarRange?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CustomCalendarRanges
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<List<CustomCalendarRange>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CustomCalendarRanges
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<CustomCalendarRange> CreateAsync(CustomCalendarRange range)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.CustomCalendarRanges.Add(range);
        await context.SaveChangesAsync();
        return range;
    }

    public async Task<CustomCalendarRange> UpdateAsync(CustomCalendarRange range)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.CustomCalendarRanges.Update(range);
        await context.SaveChangesAsync();
        return range;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var range = await context.CustomCalendarRanges
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (range != null)
        {
            context.CustomCalendarRanges.Remove(range);
            await context.SaveChangesAsync();
        }
    }
}
