# Calendar.razor Refactoring Guide

## Overview

The Calendar.razor component has grown to 1,403 lines, making it difficult to maintain and test. This document outlines the refactoring strategy to break it into manageable, testable service classes.

## Problem

- **Large component**: 1,403 lines in a single Blazor component
- **Mixed concerns**: UI logic, business logic, and data access mixed together
- **Hard to test**: Difficult to unit test business logic
- **Hard to maintain**: Changes require understanding the entire component
- **Code duplication**: Similar logic repeated in multiple places

## Solution: Service-Based Architecture

We've created specialized service classes to separate concerns:

### 1. CalendarStateService
**Purpose**: Manages calendar component state

**Responsibilities**:
- User information (ID, email, timezone)
- Calendar state (selected date, settings, calendars)
- Event data (events list, filtered events)
- UI state (loading, search active, selection mode)
- Selection management (selected event IDs)

**Key Methods**:
- `GetVisibleCalendars()` - Returns calendars that are visible
- `GetVisibleCalendarIds()` - Returns IDs of visible calendars
- `IsCalendarVisible(Guid?)` - Checks if a calendar is visible
- `GetEventsForDay(DateTime)` - Returns events for a specific day
- `ClearSelection()` - Clears selected events
- `ToggleEventSelection(Guid)` - Toggles event selection
- `GetSelectedEvents()` - Returns currently selected events

### 2. CalendarEventService
**Purpose**: Handles all event CRUD operations

**Responsibilities**:
- Loading events with timezone conversion
- Creating events from templates
- Duplicating events
- Updating event times (drag & drop)
- Deleting events
- Creating recurrence exceptions
- Bulk operations (complete, delete, update fields)

**Key Methods**:
- `LoadEventsAsync()` - Loads events for date range
- `CreateEventFromTemplate()` - Creates event from template
- `DuplicateEvent()` - Duplicates an existing event
- `UpdateEventTimesAsync()` - Updates event start/end times
- `DeleteEventAsync()` - Deletes an event
- `CreateRecurrenceException()` - Creates modified occurrence
- `BulkUpdateCompletionAsync()` - Bulk mark complete/incomplete
- `BulkDeleteAsync()` - Bulk delete events
- `BulkUpdateFieldAsync()` - Bulk update event fields

### 3. CalendarViewService
**Purpose**: Manages calendar view and navigation

**Responsibilities**:
- View type management (Day, Week, Month, Year)
- View persistence (RememberLastView feature)
- Timezone abbreviation display
- Date range calculation per view

**Key Methods**:
- `GetSchedulerIndexFromCalendarView()` - Converts enum to index
- `GetCalendarViewFromSchedulerIndex()` - Converts index to enum
- `ChangeViewAsync()` - Changes view and persists if enabled
- `GetInitialViewIndex()` - Gets initial view from settings
- `GetTimeZoneAbbreviation()` - Gets TZ abbreviation for display
- `GetDateRangeForView()` - Calculates date range for current view

### 4. CalendarFilterService
**Purpose**: Handles event filtering and search

**Responsibilities**:
- Advanced search filtering
- Calendar visibility filtering
- Completion status filtering
- Event type filtering
- Applying all filters

**Key Methods**:
- `ApplySearchFilter()` - Applies advanced search with sorting
- `FilterByVisibleCalendars()` - Filters by calendar visibility
- `FilterByCompletionStatus()` - Filters by completion
- `FilterByCancelledStatus()` - Filters cancelled events
- `FilterByHiddenEventTypes()` - Filters by hidden types
- `ApplyAllFilters()` - Applies all active filters

### 5. CalendarHelperService
**Purpose**: UI helper methods and formatting

**Responsibilities**:
- Icon selection for event types
- Color management
- Duration formatting
- DateTime formatting
- CSS class generation
- Status badges
- Tooltips

**Key Methods**:
- `GetEventIcon()` - Gets icon CSS class for event type
- `GetEventTypeColor()` - Gets color for event type
- `GetPriorityColor()` - Gets color for priority
- `FormatDuration()` - Formats duration (e.g., "1 hr 30 min")
- `FormatDateTime()` - Formats datetime per user preferences
- `FormatTime()` - Formats time per user preferences
- `GetEventCssClass()` - Generates CSS classes for event
- `ShouldShowWarning()` - Checks if deadline warning needed
- `GetEventStatusBadge()` - Gets status badge text
- `GenerateEventTooltip()` - Generates tooltip HTML

## Service Registration

Services are registered in `Program.cs` as scoped services:

```csharp
// Register calendar services for refactored Calendar component
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarStateService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarEventService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarViewService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarFilterService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarHelperService>();
```

## Usage Pattern (Example)

Before refactoring:
```csharp
@code {
    private List<Event> _events = new();
    private bool _loading = true;

    private async Task LoadEvents()
    {
        // 50+ lines of complex logic here
        _loading = true;
        var events = await EventRepository.GetEventsByDateRangeAsync(...);
        // timezone conversion
        // filtering
        // sorting
        _events = events;
        _loading = false;
    }
}
```

After refactoring:
```csharp
@inject CalendarStateService State
@inject CalendarEventService EventService

@code {
    private async Task LoadEvents()
    {
        State.IsLoading = true;
        State.Events = await EventService.LoadEventsAsync(
            startDate, endDate, State.UserId, State.UserTimeZone);
        State.IsLoading = false;
    }
}
```

## Benefits

1. **Separation of Concerns**: Each service has a single, well-defined responsibility
2. **Testability**: Services can be unit tested independently
3. **Reusability**: Services can be reused in other components
4. **Maintainability**: Changes are localized to specific services
5. **Readability**: Calendar.razor focuses on UI, not business logic
6. **Type Safety**: Strongly-typed methods with clear parameters

## Migration Strategy

### Phase 1: Create Services (✅ Complete)
- Created all 5 service classes
- Registered services in DI container
- Fixed compilation issues

### Phase 2: Incremental Migration (Next Steps)
1. Start with simple methods (GetEventIcon, FormatDuration, etc.)
2. Move to data loading (LoadEvents, OnLoadData)
3. Migrate event operations (Create, Update, Delete)
4. Move filtering and search logic
5. Migrate bulk operations
6. Final cleanup and testing

### Phase 3: Testing
1. Add unit tests for each service
2. Integration tests for service interactions
3. UI tests for Calendar component

### Phase 4: Documentation
1. Update inline documentation
2. Add XML comments to public methods
3. Create usage examples

## Known Issues to Fix

The following compilation errors need to be resolved:

1. **EventType enum**: Remove references to non-existent types (Birthday, Holiday, Other)
2. **Calendar.IsHidden**: Change to `Calendar.IsVisible` (inverse logic)
3. **CalendarView nullable**: Handle nullable CalendarView properly
4. **Attendee.ResponseStatus**: Remove if property doesn't exist
5. **AttendeeResponseStatus enum**: Remove if enum doesn't exist

## Next Steps

1. Fix compilation errors in service classes
2. Begin migrating Calendar.razor to use services
3. Add unit tests for services
4. Document migration progress
5. Create PR for review

## File Locations

```
Tempus.Web/
├── Services/
│   └── Calendar/
│       ├── CalendarStateService.cs
│       ├── CalendarEventService.cs
│       ├── CalendarViewService.cs
│       ├── CalendarFilterService.cs
│       └── CalendarHelperService.cs
└── Components/
    └── Pages/
        └── Calendar.razor (to be refactored)
```

## Additional Notes

- Services use dependency injection for repositories and other services
- All services include structured logging
- Services follow async/await best practices
- Services include XML documentation comments
- Error handling uses try-catch with logging

---

**Status**: Services created and registered, compilation errors to be fixed
**Last Updated**: 2025-11-10
**Author**: Claude Code
