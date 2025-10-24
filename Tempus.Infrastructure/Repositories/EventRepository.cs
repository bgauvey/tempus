using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Helpers;
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
        // Get all non-recurring events in the date range (excluding exceptions)
        var nonRecurringEvents = await _context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId &&
                       !e.IsRecurring &&
                       !e.IsRecurrenceException &&
                       e.StartTime >= startDate &&
                       e.StartTime <= endDate)
            .ToListAsync();

        // Get all recurring events that could have instances in this range
        var recurringEvents = await _context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId &&
                       e.IsRecurring &&
                       e.RecurrenceParentId == null && // Only get parent events, not instances
                       e.StartTime <= endDate) // Started before or during the range
            .ToListAsync();

        // Get all exception events (modified or deleted occurrences)
        var exceptionEvents = await _context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId &&
                       e.IsRecurrenceException &&
                       e.RecurrenceExceptionDate.HasValue &&
                       e.RecurrenceExceptionDate.Value >= startDate.Date &&
                       e.RecurrenceExceptionDate.Value <= endDate.Date)
            .ToListAsync();

        // Build a set of exception dates for each recurring event
        var exceptionDatesByParent = exceptionEvents
            .Where(e => e.RecurrenceParentId.HasValue && e.RecurrenceExceptionDate.HasValue)
            .Select(e => new { ParentId = e.RecurrenceParentId!.Value, Date = e.RecurrenceExceptionDate!.Value.Date })
            .GroupBy(x => x.ParentId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Date).ToHashSet()
            );

        // Expand recurring events into instances, excluding exception dates
        var recurringInstances = new List<Event>();
        foreach (var recurringEvent in recurringEvents)
        {
            var instances = RecurrenceHelper.ExpandRecurringEvent(recurringEvent, startDate, endDate);

            // Filter out instances that have exceptions
            if (exceptionDatesByParent.TryGetValue(recurringEvent.Id, out var exceptionDates))
            {
                instances = instances.Where(i => !exceptionDates.Contains(i.StartTime.Date)).ToList();
            }

            recurringInstances.AddRange(instances.Where(i => i.Id != recurringEvent.Id)); // Exclude the parent
        }

        // Add exception events that aren't deleted (have meaningful title)
        var visibleExceptions = exceptionEvents.Where(e => e.Title != "(Deleted)").ToList();

        // Combine and sort all events
        var allEvents = nonRecurringEvents
            .Concat(recurringInstances)
            .Concat(visibleExceptions)
            .OrderBy(e => e.StartTime)
            .ToList();

        return allEvents;
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
