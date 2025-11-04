using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class AdvancedSearchFilter
{
    // Text search
    public string? SearchTerm { get; set; }
    public bool SearchInTitle { get; set; } = true;
    public bool SearchInDescription { get; set; } = true;
    public bool SearchInLocation { get; set; } = true;
    public bool SearchInAttendees { get; set; } = false;

    // Date range filtering
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Event type filtering
    public List<EventType>? EventTypes { get; set; }

    // Priority filtering
    public List<Priority>? Priorities { get; set; }

    // Status filtering
    public bool? IsCompleted { get; set; }
    public bool IncludeRecurring { get; set; } = true;

    // Attendee filtering
    public string? AttendeeEmail { get; set; }
    public bool? HasAttendees { get; set; }

    // Time of day filtering
    public TimeSpan? EarliestStartTime { get; set; }
    public TimeSpan? LatestEndTime { get; set; }

    // Sorting
    public SearchSortBy SortBy { get; set; } = SearchSortBy.StartTime;
    public bool SortDescending { get; set; } = false;

    // Result limiting
    public int? MaxResults { get; set; }

    // Helper method to check if any filters are active
    public bool HasActiveFilters()
    {
        return !string.IsNullOrWhiteSpace(SearchTerm) ||
               StartDate.HasValue ||
               EndDate.HasValue ||
               EventTypes?.Any() == true ||
               Priorities?.Any() == true ||
               IsCompleted.HasValue ||
               !string.IsNullOrWhiteSpace(AttendeeEmail) ||
               HasAttendees.HasValue ||
               EarliestStartTime.HasValue ||
               LatestEndTime.HasValue;
    }
}

public enum SearchSortBy
{
    StartTime,
    EndTime,
    Title,
    Priority,
    CreatedDate,
    UpdatedDate
}
