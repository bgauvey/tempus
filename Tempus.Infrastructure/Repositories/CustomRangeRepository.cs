using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class CustomRangeRepository : ICustomRangeRepository
{
    private readonly TempusDbContext _context;

    public CustomRangeRepository(TempusDbContext context)
    {
        _context = context;
    }

    public async Task<CustomCalendarRange?> GetByIdAsync(Guid id)
    {
        return await _context.CustomCalendarRanges.FindAsync(id);
    }

    public async Task<List<CustomCalendarRange>> GetAllAsync()
    {
        return await _context.CustomCalendarRanges
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<CustomCalendarRange> CreateAsync(CustomCalendarRange range)
    {
        _context.CustomCalendarRanges.Add(range);
        await _context.SaveChangesAsync();
        return range;
    }

    public async Task<CustomCalendarRange> UpdateAsync(CustomCalendarRange range)
    {
        _context.CustomCalendarRanges.Update(range);
        await _context.SaveChangesAsync();
        return range;
    }

    public async Task DeleteAsync(Guid id)
    {
        var range = await GetByIdAsync(id);
        if (range != null)
        {
            _context.CustomCalendarRanges.Remove(range);
            await _context.SaveChangesAsync();
        }
    }
}
