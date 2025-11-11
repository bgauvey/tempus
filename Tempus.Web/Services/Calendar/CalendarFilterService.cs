using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Web.Services.Calendar;

/// <summary>
/// Handles calendar event filtering and search operations
/// </summary>
public class CalendarFilterService
{
    private readonly ILogger<CalendarFilterService> _logger;

    public CalendarFilterService(ILogger<CalendarFilterService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Applies advanced search filter to events
    /// </summary>
    public List<Event> ApplySearchFilter(List<Event> events, AdvancedSearchFilter filter)
    {
        var filteredEvents = events.AsEnumerable();

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            filteredEvents = filteredEvents.Where(e =>
                (filter.SearchInTitle && (e.Title?.ToLower().Contains(searchLower) ?? false)) ||
                (filter.SearchInDescription && (e.Description?.ToLower().Contains(searchLower) ?? false)) ||
                (filter.SearchInLocation && (e.Location?.ToLower().Contains(searchLower) ?? false)) ||
                (filter.SearchInAttendees && e.Attendees != null && e.Attendees.Any(a =>
                    (a.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (a.Email?.ToLower().Contains(searchLower) ?? false)))
            );
        }

        // Filter by event type
        if (filter.EventTypes != null && filter.EventTypes.Any())
        {
            filteredEvents = filteredEvents.Where(e => filter.EventTypes.Contains(e.EventType));
        }

        // Filter by priority
        if (filter.Priorities != null && filter.Priorities.Any())
        {
            filteredEvents = filteredEvents.Where(e => filter.Priorities.Contains(e.Priority));
        }

        // Filter by date range
        if (filter.StartDate.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.StartTime.Date >= filter.StartDate.Value.Date);
        }

        if (filter.EndDate.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.StartTime.Date <= filter.EndDate.Value.Date);
        }

        // Filter by attendee email
        if (!string.IsNullOrWhiteSpace(filter.AttendeeEmail))
        {
            var attendeeLower = filter.AttendeeEmail.ToLower();
            filteredEvents = filteredEvents.Where(e =>
                e.Attendees != null && e.Attendees.Any(a =>
                    (a.Email?.ToLower().Contains(attendeeLower) ?? false)
                )
            );
        }

        // Filter by has attendees
        if (filter.HasAttendees.HasValue)
        {
            if (filter.HasAttendees.Value)
                filteredEvents = filteredEvents.Where(e => e.Attendees != null && e.Attendees.Any());
            else
                filteredEvents = filteredEvents.Where(e => e.Attendees == null || !e.Attendees.Any());
        }

        // Filter by completion status
        if (filter.IsCompleted.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.IsCompleted == filter.IsCompleted.Value);
        }

        // Filter by recurring
        if (!filter.IncludeRecurring)
        {
            filteredEvents = filteredEvents.Where(e => !e.IsRecurring);
        }

        // Filter by time of day
        if (filter.EarliestStartTime.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.StartTime.TimeOfDay >= filter.EarliestStartTime.Value);
        }

        if (filter.LatestEndTime.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.EndTime.TimeOfDay <= filter.LatestEndTime.Value);
        }

        var result = filteredEvents.ToList();

        // Apply sorting
        result = filter.SortBy switch
        {
            SearchSortBy.StartTime => filter.SortDescending
                ? result.OrderByDescending(e => e.StartTime).ToList()
                : result.OrderBy(e => e.StartTime).ToList(),
            SearchSortBy.EndTime => filter.SortDescending
                ? result.OrderByDescending(e => e.EndTime).ToList()
                : result.OrderBy(e => e.EndTime).ToList(),
            SearchSortBy.Title => filter.SortDescending
                ? result.OrderByDescending(e => e.Title).ToList()
                : result.OrderBy(e => e.Title).ToList(),
            SearchSortBy.Priority => filter.SortDescending
                ? result.OrderByDescending(e => e.Priority).ToList()
                : result.OrderBy(e => e.Priority).ToList(),
            SearchSortBy.CreatedDate => filter.SortDescending
                ? result.OrderByDescending(e => e.CreatedAt).ToList()
                : result.OrderBy(e => e.CreatedAt).ToList(),
            SearchSortBy.UpdatedDate => filter.SortDescending
                ? result.OrderByDescending(e => e.UpdatedAt).ToList()
                : result.OrderBy(e => e.UpdatedAt).ToList(),
            _ => result
        };

        // Apply max results limit
        if (filter.MaxResults.HasValue && filter.MaxResults.Value > 0)
        {
            result = result.Take(filter.MaxResults.Value).ToList();
        }

        _logger.LogDebug("Applied search filter. Result: {Count} events", result.Count);

        return result;
    }

    /// <summary>
    /// Filters events by calendar visibility
    /// </summary>
    public List<Event> FilterByVisibleCalendars(List<Event> events, HashSet<Guid> visibleCalendarIds)
    {
        return events.Where(e =>
            !e.CalendarId.HasValue || visibleCalendarIds.Contains(e.CalendarId.Value)
        ).ToList();
    }

    /// <summary>
    /// Filters events by completion status based on settings
    /// </summary>
    public List<Event> FilterByCompletionStatus(List<Event> events, CalendarSettings? settings)
    {
        if (settings == null || settings.ShowCompletedTasks)
            return events;

        return events.Where(e => !e.IsCompleted).ToList();
    }

    /// <summary>
    /// Filters events by cancelled status based on settings
    /// </summary>
    public List<Event> FilterByCancelledStatus(List<Event> events, CalendarSettings? settings)
    {
        if (settings == null || settings.ShowCancelledEvents)
            return events;

        // Assuming there's an IsCancelled property or Status property
        // For now, returning all events as there's no explicit cancelled status in the model
        return events;
    }

    /// <summary>
    /// Filters events by hidden event types based on settings
    /// </summary>
    public List<Event> FilterByHiddenEventTypes(List<Event> events, CalendarSettings? settings)
    {
        if (settings == null || string.IsNullOrWhiteSpace(settings.HiddenEventTypes))
            return events;

        var hiddenTypes = settings.HiddenEventTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => Enum.TryParse<EventType>(t.Trim(), out var eventType) ? eventType : (EventType?)null)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .ToHashSet();

        if (!hiddenTypes.Any())
            return events;

        return events.Where(e => !hiddenTypes.Contains(e.EventType)).ToList();
    }

    /// <summary>
    /// Applies all active filters
    /// </summary>
    public List<Event> ApplyAllFilters(
        List<Event> events,
        CalendarSettings? settings,
        HashSet<Guid> visibleCalendarIds,
        AdvancedSearchFilter? searchFilter = null)
    {
        var filtered = events;

        // Apply calendar visibility filter
        filtered = FilterByVisibleCalendars(filtered, visibleCalendarIds);

        // Apply settings-based filters
        filtered = FilterByCompletionStatus(filtered, settings);
        filtered = FilterByCancelledStatus(filtered, settings);
        filtered = FilterByHiddenEventTypes(filtered, settings);

        // Apply search filter if active
        if (searchFilter != null)
        {
            filtered = ApplySearchFilter(filtered, searchFilter);
        }

        _logger.LogDebug("Applied all filters. Input: {InputCount}, Output: {OutputCount}",
            events.Count, filtered.Count);

        return filtered;
    }
}
