using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Web.Services.Calendar;

/// <summary>
/// Manages the state of the calendar component
/// </summary>
public class CalendarStateService
{
    // User information
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserTimeZone { get; set; } = TimeZoneInfo.Local.Id;

    // Calendar state
    public DateTime SelectedDate { get; set; } = DateTime.Today;
    public CalendarSettings? Settings { get; set; }
    public List<Tempus.Core.Models.Calendar> Calendars { get; set; } = new();
    public int SelectedViewIndex { get; set; } = 1; // Default to Week view

    // Event data
    public List<Event> Events { get; set; } = new();
    public List<Event> AllEvents { get; set; } = new(); // For search functionality

    // UI state
    public bool IsLoading { get; set; } = true;
    public bool IsSearchActive { get; set; } = false;
    public bool IsSelectionMode { get; set; } = false;

    // Selection state
    public HashSet<Guid> SelectedEventIds { get; set; } = new();

    /// <summary>
    /// Gets visible calendars (not hidden)
    /// </summary>
    public List<Tempus.Core.Models.Calendar> GetVisibleCalendars()
    {
        return Calendars.Where(c => c.IsVisible).ToList();
    }

    /// <summary>
    /// Gets visible calendar IDs
    /// </summary>
    public HashSet<Guid> GetVisibleCalendarIds()
    {
        return GetVisibleCalendars().Select(c => c.Id).ToHashSet();
    }

    /// <summary>
    /// Checks if a calendar is visible
    /// </summary>
    public bool IsCalendarVisible(Guid? calendarId)
    {
        if (!calendarId.HasValue)
            return true; // Events without calendar are always visible

        var visibleIds = GetVisibleCalendarIds();
        return visibleIds.Contains(calendarId.Value);
    }

    /// <summary>
    /// Gets events for a specific day
    /// </summary>
    public List<Event> GetEventsForDay(DateTime date)
    {
        return Events.Where(e =>
            e.StartTime.Date == date.Date &&
            IsCalendarVisible(e.CalendarId)
        ).OrderBy(e => e.StartTime).ToList();
    }

    /// <summary>
    /// Clears all selected event IDs
    /// </summary>
    public void ClearSelection()
    {
        SelectedEventIds.Clear();
        IsSelectionMode = false;
    }

    /// <summary>
    /// Toggles selection of an event
    /// </summary>
    public void ToggleEventSelection(Guid eventId)
    {
        if (SelectedEventIds.Contains(eventId))
            SelectedEventIds.Remove(eventId);
        else
            SelectedEventIds.Add(eventId);
    }

    /// <summary>
    /// Gets the currently selected events
    /// </summary>
    public List<Event> GetSelectedEvents()
    {
        return Events.Where(e => SelectedEventIds.Contains(e.Id)).ToList();
    }
}
