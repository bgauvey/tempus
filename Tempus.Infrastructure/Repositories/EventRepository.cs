using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Enums;
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
        Console.WriteLine($"[EventRepository.CreateAsync] Starting for event: {@event.Title}");
        Console.WriteLine($"[EventRepository.CreateAsync] Event ID: {@event.Id}");
        Console.WriteLine($"[EventRepository.CreateAsync] User ID: {@event.UserId}");

        await using var context = await _contextFactory.CreateDbContextAsync();
        Console.WriteLine($"[EventRepository.CreateAsync] DbContext created");

        // Handle attendees - ensure they have proper IDs and EventId set
        if (@event.Attendees != null && @event.Attendees.Any())
        {
            Console.WriteLine($"[EventRepository.CreateAsync] Processing {@event.Attendees.Count} attendees");
            foreach (var attendee in @event.Attendees)
            {
                // Ensure attendee has an ID
                if (attendee.Id == Guid.Empty)
                {
                    attendee.Id = Guid.NewGuid();
                    Console.WriteLine($"[EventRepository.CreateAsync]   Generated new attendee ID: {attendee.Id}");
                }
                // Set the EventId to link the attendee to this event
                attendee.EventId = @event.Id;
                Console.WriteLine($"[EventRepository.CreateAsync]   Attendee: {attendee.Name} ({attendee.Email}), EventId: {attendee.EventId}");
            }
        }
        else
        {
            Console.WriteLine($"[EventRepository.CreateAsync] No attendees to process");
        }

        Console.WriteLine($"[EventRepository.CreateAsync] Adding event to context...");
        context.Events.Add(@event);

        Console.WriteLine($"[EventRepository.CreateAsync] Calling SaveChangesAsync...");
        var changeCount = await context.SaveChangesAsync();
        Console.WriteLine($"[EventRepository.CreateAsync] SaveChangesAsync completed. Changes saved: {changeCount}");

        Console.WriteLine($"[EventRepository.CreateAsync] Returning event with ID: {@event.Id}");
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

    public async Task<List<Event>> AdvancedSearchAsync(AdvancedSearchFilter filter, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Events
            .Include(e => e.Attendees)
            .Where(e => e.UserId == userId);

        // Text search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(e =>
                (filter.SearchInTitle && e.Title.ToLower().Contains(searchLower)) ||
                (filter.SearchInDescription && e.Description != null && e.Description.ToLower().Contains(searchLower)) ||
                (filter.SearchInLocation && e.Location != null && e.Location.ToLower().Contains(searchLower)) ||
                (filter.SearchInAttendees && e.Attendees.Any(a =>
                    a.Name.ToLower().Contains(searchLower) ||
                    a.Email.ToLower().Contains(searchLower))));
        }

        // Date range filtering
        if (filter.StartDate.HasValue)
        {
            query = query.Where(e => e.StartTime >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(e => e.EndTime <= filter.EndDate.Value);
        }

        // Event type filtering
        if (filter.EventTypes?.Any() == true)
        {
            query = query.Where(e => filter.EventTypes.Contains(e.EventType));
        }

        // Priority filtering
        if (filter.Priorities?.Any() == true)
        {
            query = query.Where(e => filter.Priorities.Contains(e.Priority));
        }

        // Status filtering
        if (filter.IsCompleted.HasValue)
        {
            query = query.Where(e => e.IsCompleted == filter.IsCompleted.Value);
        }

        if (!filter.IncludeRecurring)
        {
            query = query.Where(e => !e.IsRecurring);
        }

        // Attendee filtering
        if (!string.IsNullOrWhiteSpace(filter.AttendeeEmail))
        {
            var emailLower = filter.AttendeeEmail.ToLower();
            query = query.Where(e => e.Attendees.Any(a => a.Email.ToLower() == emailLower));
        }

        if (filter.HasAttendees.HasValue)
        {
            if (filter.HasAttendees.Value)
            {
                query = query.Where(e => e.Attendees.Any());
            }
            else
            {
                query = query.Where(e => !e.Attendees.Any());
            }
        }

        // Time of day filtering
        if (filter.EarliestStartTime.HasValue)
        {
            query = query.Where(e => e.StartTime.TimeOfDay >= filter.EarliestStartTime.Value);
        }

        if (filter.LatestEndTime.HasValue)
        {
            query = query.Where(e => e.EndTime.TimeOfDay <= filter.LatestEndTime.Value);
        }

        // Sorting
        query = filter.SortBy switch
        {
            SearchSortBy.StartTime => filter.SortDescending
                ? query.OrderByDescending(e => e.StartTime)
                : query.OrderBy(e => e.StartTime),
            SearchSortBy.EndTime => filter.SortDescending
                ? query.OrderByDescending(e => e.EndTime)
                : query.OrderBy(e => e.EndTime),
            SearchSortBy.Title => filter.SortDescending
                ? query.OrderByDescending(e => e.Title)
                : query.OrderBy(e => e.Title),
            SearchSortBy.Priority => filter.SortDescending
                ? query.OrderByDescending(e => e.Priority)
                : query.OrderBy(e => e.Priority),
            SearchSortBy.CreatedDate => filter.SortDescending
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt),
            SearchSortBy.UpdatedDate => filter.SortDescending
                ? query.OrderByDescending(e => e.UpdatedAt)
                : query.OrderBy(e => e.UpdatedAt),
            _ => query.OrderBy(e => e.StartTime)
        };

        // Result limiting
        if (filter.MaxResults.HasValue && filter.MaxResults.Value > 0)
        {
            query = query.Take(filter.MaxResults.Value);
        }

        return await query.ToListAsync();
    }

    // Bulk operations
    public async Task<List<Event>> GetByIdsAsync(List<Guid> ids, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events
            .Include(e => e.Attendees)
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .ToListAsync();
    }

    public async Task<int> BulkDeleteAsync(List<Guid> ids, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.Events
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .ToListAsync();

        if (events.Any())
        {
            context.Events.RemoveRange(events);
            return await context.SaveChangesAsync();
        }

        return 0;
    }

    public async Task<int> BulkUpdatePriorityAsync(List<Guid> ids, Priority priority, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.Events
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .ToListAsync();

        foreach (var evt in events)
        {
            evt.Priority = priority;
            evt.UpdatedAt = DateTime.UtcNow;
        }

        return await context.SaveChangesAsync();
    }

    public async Task<int> BulkUpdateEventTypeAsync(List<Guid> ids, EventType eventType, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.Events
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .ToListAsync();

        foreach (var evt in events)
        {
            evt.EventType = eventType;
            evt.UpdatedAt = DateTime.UtcNow;
        }

        return await context.SaveChangesAsync();
    }

    public async Task<int> BulkUpdateColorAsync(List<Guid> ids, string color, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.Events
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .ToListAsync();

        foreach (var evt in events)
        {
            evt.Color = color;
            evt.UpdatedAt = DateTime.UtcNow;
        }

        return await context.SaveChangesAsync();
    }

    public async Task<int> BulkCompleteAsync(List<Guid> ids, bool isCompleted, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.Events
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .ToListAsync();

        foreach (var evt in events)
        {
            evt.IsCompleted = isCompleted;
            evt.UpdatedAt = DateTime.UtcNow;
        }

        return await context.SaveChangesAsync();
    }

    public async Task<int> BulkMoveAsync(List<Guid> ids, TimeSpan offset, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.Events
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .ToListAsync();

        foreach (var evt in events)
        {
            evt.StartTime = evt.StartTime.Add(offset);
            evt.EndTime = evt.EndTime.Add(offset);
            evt.UpdatedAt = DateTime.UtcNow;
        }

        return await context.SaveChangesAsync();
    }
}
