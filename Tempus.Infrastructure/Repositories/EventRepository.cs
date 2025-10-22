using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly TempusDbContext _context;

    public EventRepository(TempusDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id, string userId)
    {
        return await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
    }

    public async Task<List<Event>> GetAllAsync(string userId)
    {
        return await _context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<List<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, string userId)
    {
        return await _context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId && e.StartTime >= startDate && e.StartTime <= endDate)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<Event> CreateAsync(Event @event)
    {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();
        return @event;
    }

    public async Task<Event> UpdateAsync(Event @event)
    {
        @event.UpdatedAt = DateTime.UtcNow;
        _context.Events.Update(@event);
        await _context.SaveChangesAsync();
        return @event;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var @event = await GetByIdAsync(id, userId);
        if (@event != null)
        {
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Event>> SearchAsync(string searchTerm, string userId)
    {
        return await _context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId &&
                       (e.Title.Contains(searchTerm) ||
                       (e.Description != null && e.Description.Contains(searchTerm)) ||
                       (e.Location != null && e.Location.Contains(searchTerm))))
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }
}
