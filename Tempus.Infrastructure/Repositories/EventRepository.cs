using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Helpers;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;

    public EventRepository(IDbContextFactory<TempusDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Event?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
    }

    public async Task<List<Event>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<List<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get all non-recurring events in the date range (excluding exceptions)
        var nonRecurringEvents = await context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId &&
                       !e.IsRecurring &&
                       !e.IsRecurrenceException &&
                       e.StartTime >= startDate &&
                       e.StartTime <= endDate)
            .ToListAsync();

        // Get all recurring events that could have instances in this range
        var recurringEvents = await context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId &&
                       e.IsRecurring &&
                       e.RecurrenceParentId == null && // Only get parent events, not instances
                       e.StartTime <= endDate) // Started before or during the range
            .ToListAsync();

        // Get all exception events (modified or deleted occurrences)
        var exceptionEvents = await context.Events
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Events.Add(@event);
        await context.SaveChangesAsync();
        return @event;
    }

    public async Task<Event> UpdateAsync(Event @event)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        @event.UpdatedAt = DateTime.UtcNow;

        // Attach the event to this new context
        context.Events.Attach(@event);
        context.Entry(@event).State = EntityState.Modified;

        // Find new attendees (those with Guid.Empty)
        var newAttendees = @event.Attendees.Where(a => a.Id == Guid.Empty).ToList();

        // Handle new attendees - explicitly mark them as Added
        foreach (var newAttendee in newAttendees)
        {
            newAttendee.Id = Guid.NewGuid();
            newAttendee.EventId = @event.Id;

            // Explicitly tell EF this is a new entity to INSERT, not UPDATE
            context.Entry(newAttendee).State = EntityState.Added;
        }

        // Handle existing attendees
        var existingAttendees = @event.Attendees.Where(a => a.Id != Guid.Empty).ToList();
        foreach (var attendee in existingAttendees)
        {
            context.Entry(attendee).State = EntityState.Modified;
        }

        await context.SaveChangesAsync();
        return @event;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var @event = await context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (@event != null)
        {
            context.Events.Remove(@event);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Event>> SearchAsync(string searchTerm, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId &&
                       (e.Title.Contains(searchTerm) ||
                       (e.Description != null && e.Description.Contains(searchTerm)) ||
                       (e.Location != null && e.Location.Contains(searchTerm))))
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }
}
