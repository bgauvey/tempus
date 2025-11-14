# Event System Architecture Analysis - Tempus Application

## Overview
The Tempus application is a .NET 9 Blazor calendar application with a comprehensive event management system. This document outlines how events are created, stored, displayed, and how timestamps are handled across the application.

---

## 1. EVENT MODEL/SCHEMA DEFINITION

### Primary File: `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Models/Event.cs`

**Key Properties:**
- **Id** (Guid): Unique identifier for the event
- **Title** (string): Event name, required, max 200 characters
- **Description** (string, nullable): Event details, max 10,000 characters
- **StartTime** (DateTime): Event start timestamp
- **EndTime** (DateTime): Event end timestamp
- **TimeZoneId** (string, nullable): IANA timezone ID (e.g., "America/New_York")
- **Location** (string, nullable): Event location, max 500 characters
- **EventType** (EventType enum): Meeting, Appointment, Task, TimeBlock, Reminder, Deadline
- **Priority** (Priority enum): Low, Medium, High, Urgent (default: Medium)
- **IsAllDay** (bool): Flag for all-day events
- **Color** (string, nullable): Event color for UI display
- **CreatedAt** (DateTime): Set to `DateTime.UtcNow` on creation (Line 39)
- **UpdatedAt** (DateTime, nullable): Set to `DateTime.UtcNow` on update (Line 40)
- **IsCompleted** (bool): Event completion status

**Recurrence Properties:**
- **IsRecurring** (bool): Whether event repeats
- **RecurrencePattern** (RecurrencePattern): Daily, Weekly, Monthly, Yearly, None
- **RecurrenceInterval** (int): Repeat every X periods
- **RecurrenceDaysOfWeek** (string, nullable): Comma-separated day numbers (e.g., "0,1,2")
- **RecurrenceEndType** (RecurrenceEndType): Never, AfterOccurrences, OnDate
- **RecurrenceCount** (int, nullable): Number of occurrences
- **RecurrenceEndDate** (DateTime, nullable): When recurrence ends
- **RecurrenceParentId** (Guid, nullable): Parent event ID for instances
- **IsRecurrenceException** (bool): True if this is a modified instance
- **RecurrenceExceptionDate** (DateTime, nullable): Original date for exception

**Reminder/Notification Properties:**
- **ReminderMinutes** (string, nullable): Comma-separated reminder times in minutes (e.g., "15,60,1440")
  - Supported values: 15, 30, 60, 120, 1440, 2880, 10080
  - Defined in EventFormDialog.razor lines 464-473
- **MeetingCost** (decimal, nullable): Calculated cost for meetings

**Meeting Properties:**
- **Attendees** (List<Attendee>): Event participants
- **HourlyCostPerAttendee** (decimal): Default $75.00 for cost calculations

**User Properties:**
- **UserId** (string): Owner/creator of the event
- **User** (ApplicationUser, nullable): Navigation property

**External Calendar:**
- **ExternalCalendarId** (string, nullable): ID for synced events
- **ExternalCalendarProvider** (string, nullable): Google, Apple, Outlook, etc.

**Tags:**
- **Tags** (List<string>): Event categorization/labeling

---

## 2. EVENT CREATION AND STORAGE

### Creation Flow

#### A. From EventFormDialog (UI)
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Dialogs/EventFormDialog.razor`

**Key Entry Points:**
- **Line 616-675:** `Save()` method - Primary save logic
- **Line 653-657:** New event creation
  ```csharp
  _event.Id = Guid.NewGuid();
  _event.CreatedAt = DateTime.UtcNow;  // Always UTC
  await EventRepository.CreateAsync(_event);
  ```

**Initialization:**
- **Line 511-608:** `OnInitializedAsync()` - Load existing event or setup new event
- **Line 595-599:** Prefilled date handling sets start time to 9 AM on specified date
- **Line 356-362:** Default new event initialization (DateTime.Now - uses local time)

**Reminder Setup:**
- **Line 757-767:** `SaveReminders()` - Converts selected reminder values to comma-separated string
- **Line 292-299:** Reminder UI for selection (15min, 30min, 1hr, 2hrs, 1day, 2days, 1week)

**Timezone Handling:**
- **Line 50-72:** Timezone dropdown with user's default timezone
- **Line 622-625:** Sets `_event.TimeZoneId` to user's timezone if not specified

#### B. Event Repository (Persistence)
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Repositories/EventRepository.cs`

**CreateAsync Method (Lines 108-148):**
```csharp
public async Task<Event> CreateAsync(Event @event)
{
    // Lines 110-115: Debug logging
    // Lines 117-137: Process attendees, assign IDs and EventId
    // Lines 139-140: Add event to DbContext
    // Lines 143-144: Save to database
    return @event;  // Line 147
}
```

**UpdateAsync Method (Lines 150-182):**
```csharp
public async Task<Event> UpdateAsync(Event @event)
{
    @event.UpdatedAt = DateTime.UtcNow;  // Line 154 - Always UTC
    // Attach to context and save
}
```

**Key Points:**
- Events are saved with `StartTime` and `EndTime` as-is (stored in database as datetime2)
- No automatic timezone conversion on save
- TimeZoneId is stored separately for reference
- UpdatedAt is always set to DateTime.UtcNow

#### C. Attendee Handling
- **Lines 118-137:** Auto-generates IDs for new attendees
- **Lines 160-178:** Distinguishes between new and existing attendees during updates
- **Lines 677-709 (EventFormDialog):** Auto-creates Contact entries for new attendees

#### D. Template/Quick Creation
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Helpers/CalendarEventManager.cs`

**CreateFromTemplateAsync (Lines 139-181):**
- Creates events from predefined templates (Meeting, Break)
- Uses DateTime.Now for current day, respects work hours for other dates
- Start time is rounded to 15-minute intervals
- CreatedAt is set to DateTime.UtcNow (Line 174)

---

## 3. EVENT DISPLAY ON UI

### A. Calendar View (Main)
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Pages/Calendar.razor`

**Data Binding:**
- Uses RadzenScheduler component
- Events are converted to Appointment objects for display
- **Key Lines 430-431 & 970-971:**
  ```csharp
  Start = displayEvent.StartTime,
  End = displayEvent.EndTime,
  ```

**Time Display:**
- Events are displayed using their StartTime and EndTime properties directly
- No timezone conversion applied before display (times shown as stored)

**Event Fetching:**
- **Line 333-334:** Loads events for date range using:
  ```csharp
  var startDate = DateTime.Today.AddMonths(-1);
  var endDate = DateTime.Today.AddMonths(2);
  ```

### B. Events Grid View
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Shared/EventsGrid.razor`

**Display Formatting:**
- **Line 74-75:** Start time display
  ```razor
  <div class="event-date">@evt.StartTime.ToString("MMM dd, yyyy")</div>
  <div class="event-time">@evt.StartTime.ToString("hh:mm tt")</div>
  ```
- **Line 82-83:** End time display
  ```razor
  <div class="event-date">@evt.EndTime.ToString("MMM dd, yyyy")</div>
  <div class="event-time">@evt.EndTime.ToString("hh:mm tt")</div>
  ```

**Formatting Options:**
- Date format: "MMM dd, yyyy" (e.g., "Nov 07, 2025")
- Time format: "hh:mm tt" (12-hour with AM/PM)
- Configurable via CalendarFormatter based on user settings

### C. Calendar Formatter Helper
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Helpers/CalendarFormatter.cs`

**FormatTime Method (Lines 18-24):**
```csharp
public string FormatTime(DateTime time)
{
    var format = _settings?.TimeFormat ?? TimeFormat.TwelveHour;
    return format == TimeFormat.TwelveHour
        ? time.ToString("h:mm tt")      // 12-hour
        : time.ToString("HH:mm");        // 24-hour
}
```

**FormatDateTime Method (Lines 26-29):**
```csharp
return dt.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt");
// Example: "Thursday, November 07, 2025 at 2:30 PM"
```

**FormatHourLabel Method (Lines 31-45):**
- Converts hour to display format based on user preference
- 12-hour: "12 AM", "1 AM", ..., "12 PM", "1 PM", etc.
- 24-hour: "00:00", "01:00", ..., "23:00"

**Event Colors:**
- Default colors assigned by EventType:
  - Meeting: #1E88E5 (Blue)
  - Appointment: #43A047 (Green)
  - Task: #FB8C00 (Orange)
  - TimeBlock: #8E24AA (Purple)
  - Reminder: #FDD835 (Yellow)
  - Deadline: #E53935 (Red)

---

## 4. TIMESTAMP HANDLING (UTC vs LOCAL TIME)

### A. Database Storage
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Data/TempusDbContext.cs`

**Database Column Type:** `datetime2` (SQL Server)
- Stores DateTime without timezone information
- No automatic conversion applied by Entity Framework

**Storage Strategy:**
- **CreatedAt:** Always `DateTime.UtcNow` (UTC timezone - Line 39 in Event.cs)
- **UpdatedAt:** Always `DateTime.UtcNow` on updates (UTC timezone - Line 154 in EventRepository.cs)
- **StartTime/EndTime:** Stored as-is without conversion
- **ScheduledFor (Notifications):** DateTime format for scheduled reminder times

### B. Event Creation Timestamps
**Always UTC:**
- EventRepository.CreateAsync: No conversion, saved as-is
- EventFormDialog.Save: Line 656 - `_event.CreatedAt = DateTime.UtcNow;`
- CalendarEventManager.CreateFromTemplateAsync: Line 174 - `CreatedAt = DateTime.UtcNow,`
- CalendarEventManager.DeleteSingleOccurrenceAsync: Line 111 - `CreatedAt = DateTime.UtcNow,`
- EventFormDialog (single occurrence edit): Line 565 - `CreatedAt = DateTime.UtcNow,`

### C. Timezone Conversion Service
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Services/TimeZoneConversionService.cs`

**Key Method: ConvertTime (Lines 83-125)**
```csharp
public DateTime ConvertTime(DateTime dateTime, string fromTimeZoneId, string toTimeZoneId)
{
    // Handles DateTimeKind conversion:
    // - Utc: Uses directly
    // - Local: Converts to UTC first
    // - Unspecified: Treats as source timezone
    
    // Line 113: For unspecified times
    utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);
    
    // Line 117: Convert to target timezone
    var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, targetTimeZone);
    return convertedTime;
}
```

**Event Conversion (Lines 28-81):**
```csharp
public Event ConvertEventToTimeZone(Event @event, string targetTimeZoneId)
{
    var sourceTimeZoneId = @event.TimeZoneId ?? "UTC";
    
    // Creates new Event with converted times:
    // StartTime = ConvertTime(@event.StartTime, sourceTimeZoneId, targetTimeZoneId)
    // EndTime = ConvertTime(@event.EndTime, sourceTimeZoneId, targetTimeZoneId)
}
```

**Available Timezones (Lines 9-26):**
Common timezones predefined:
- Pacific/Honolulu (Hawaii)
- America/Anchorage (Alaska)
- America/Los_Angeles (Pacific)
- America/Denver (Mountain)
- America/Chicago (Central)
- America/New_York (Eastern)
- UTC
- Europe/London, Europe/Paris, Europe/Berlin
- Asia/Dubai, Asia/Kolkata, Asia/Shanghai, Asia/Tokyo
- Australia/Sydney

### D. User Timezone Settings
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Models/ApplicationUser.cs`

- Property: **TimeZone** (string) - User's configured timezone
- Default: `TimeZoneInfo.Local.Id` (Machine/OS timezone)
- Used in EventFormDialog Line 522: `_userTimeZone = appUser.TimeZone;`

### E. Current Implementation Issues
**Critical Finding:**
- StartTime and EndTime are stored WITHOUT explicit timezone awareness
- They are saved to database as-is (assumed UTC based on code patterns, but not enforced)
- When retrieved, they are displayed as stored without conversion
- **No automatic conversion between user timezone and stored times** during display

**Scenario:**
1. Event created: StartTime = 2:00 PM (user's local time)
2. Stored as: 2025-11-07 14:00:00 (no UTC offset)
3. Retrieved and displayed: 2025-11-07 2:00 PM (same value)
4. User timezone change: Still displays as 2:00 PM (NOT adjusted)

---

## 5. DATE/TIME FORMATTING UTILITIES

### A. CalendarFormatter Class
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Helpers/CalendarFormatter.cs`

| Method | Purpose | Format |
|--------|---------|--------|
| FormatTime(DateTime) | Format time only | "h:mm tt" (12h) or "HH:mm" (24h) |
| FormatDateTime(DateTime) | Full datetime | "dddd, MMMM dd, yyyy 'at' h:mm tt" |
| FormatHourLabel(int) | Hour in scheduler | "12 AM", "1 AM", etc. or "00:00", "01:00" |
| GetEventBackgroundColor(Event) | Event display color | Hex color by type or stored color |
| GetEventIcon(EventType) | Event icon/emoji | Type-specific emoji |

### B. DateTime.ToString Formats Used
**Throughout codebase:**

| Format | Example | Location |
|--------|---------|----------|
| "MMM dd, yyyy" | "Nov 07, 2025" | EventsGrid.razor:74, 82 |
| "hh:mm tt" | "02:30 PM" | EventsGrid.razor:75, 83 |
| "dddd, MMMM dd, yyyy 'at' h:mm tt" | "Thursday, November 07, 2025 at 2:30 PM" | CalendarFormatter.cs:28 |
| "yyyy-MM-dd HH:mm:ss" | "2025-11-07 14:30:00" | Reports (AnalyticsReportService.cs) |

### C. DateTime.Now vs DateTime.UtcNow Usage

**DateTime.UtcNow (Correct - UTC):**
- Event creation (EventFormDialog:656)
- Event updates (EventRepository:154)
- Template creation (CalendarEventManager:174)
- Recurrence exceptions (CalendarEventManager:111, 232)
- Benchmark generation (BenchmarkService:61)
- Settings updates (SettingsService:73)
- Calendar integration syncing (AppleCalendarService:64, 82-83, 142)
- Token expiry (GoogleCalendarService:101, 160)

**DateTime.Now (Local - POTENTIAL ISSUE):**
- AnalyticsReportService: Line 36, 179, 374 (Report generation timestamps)
- AnalyticsService: Line 236-237 (Task completion detection)
- CalendarEventManager: Line 141-142 (Template creation - for determining current time)
- EventFormDialog: Line 356-357 (Default new event times)
- CalendarEventManager: Line 141 (CreateFromTemplateAsync - uses DateTime.Now for current day reference)

### D. Recurrence Helper
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Helpers/RecurrenceHelper.cs`

Provides:
- `GetRecurrenceDescription()` - Human-readable recurrence text
- `ExpandRecurringEvent()` - Generates instances for date range
- Used in EventFormDialog Line 713

---

## 6. NOTIFICATION/REMINDER SYSTEM

### A. Notification Model
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Models/Notification.cs`

**Reminder Tracking Properties:**
- **ReminderMinutes** (int, nullable): Which reminder triggered (15, 60, 1440, etc.)
- **ScheduledFor** (DateTime, nullable): When notification scheduled to trigger
- **IsSent** (bool): Whether delivered (default: false)
- **SentAt** (DateTime, nullable): When notification was sent

**Timing Metadata:**
- **CreatedAt** (DateTime): Set to `DateTime.UtcNow` - When notification record created
- **IsRead** (bool): User marked as read
- **ReadAt** (DateTime, nullable): When user read notification

### B. Reminder Configuration
**Event.ReminderMinutes property:**
- Stored as comma-separated string: "15,60,1440"
- Supported intervals:
  - 15 minutes
  - 30 minutes
  - 60 minutes (1 hour)
  - 120 minutes (2 hours)
  - 1440 minutes (1 day)
  - 2880 minutes (2 days)
  - 10080 minutes (1 week)

**Configured in EventFormDialog (Lines 464-473):**
```csharp
private readonly List<ReminderOption> _reminderOptions = new()
{
    new ReminderOption { Text = "15 minutes before", Value = 15 },
    new ReminderOption { Text = "30 minutes before", Value = 30 },
    new ReminderOption { Text = "1 hour before", Value = 60 },
    new ReminderOption { Text = "2 hours before", Value = 120 },
    new ReminderOption { Text = "1 day before", Value = 1440 },
    new ReminderOption { Text = "2 days before", Value = 2880 },
    new ReminderOption { Text = "1 week before", Value = 10080 }
};
```

### C. Notification Scheduling
**Files:**
- INotificationSchedulerService: `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Interfaces/INotificationSchedulerService.cs`
- NotificationSchedulerService: `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Services/NotificationSchedulerService.cs`

**Background Service:**
- NotificationBackgroundService: `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Services/NotificationBackgroundService.cs`
- Registered in Program.cs

---

## 7. KEY FILES SUMMARY

| File | Purpose | Key Lines |
|------|---------|-----------|
| Event.cs | Core event model | 1-53 |
| EventRepository.cs | Database persistence | 108-148 (create), 150-182 (update) |
| EventFormDialog.razor | Event creation/edit UI | 616-675 (save), 511-608 (init) |
| TimeZoneConversionService.cs | Timezone conversion logic | 83-125 (time conversion) |
| Calendar.razor | Main calendar view | 333-334 (date range fetch) |
| EventsGrid.razor | Events list display | 71-86 (time formatting) |
| CalendarFormatter.cs | Display formatting | 18-45 (format methods) |
| CalendarEventManager.cs | Event operations | 139-181 (template creation) |
| Notification.cs | Reminder/notification model | 1-28 |
| TempusDbContext.cs | EF Core configuration | 25-46 (Event entity) |

---

## 8. CURRENT TIMESTAMP STRATEGY

**Summary:**
- **Stored in Database:** All datetime fields as `datetime2` (no timezone awareness at DB level)
- **CreatedAt/UpdatedAt:** Always UTC (`DateTime.UtcNow`)
- **StartTime/EndTime:** Stored as-is without conversion (implicit assumption about timezone handling)
- **Timezone Info:** Stored separately in Event.TimeZoneId (IANA format)
- **Display:** Times shown as stored (no automatic conversion to user timezone on display)
- **Conversion Service:** Available but must be explicitly called (not automatically applied in calendar/grid)

---

## 9. RECOMMENDATIONS FOR IMPROVEMENT

1. **Standardize timestamp handling:** Document whether StartTime/EndTime should always be UTC or local
2. **Add DateTimeKind specification:** Use `DateTime.SpecifyKind()` to make kind explicit
3. **Automatic timezone display:** Apply TimeZoneConversionService conversion before rendering in Calendar/EventsGrid
4. **Consistent DateTime.UtcNow usage:** Replace remaining DateTime.Now instances in event operations
5. **Document timezone expectations:** Add comments to Event model about timezone assumptions

