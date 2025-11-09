# Quick Reference: Event System File Locations & Line Numbers

## Event Creation Flow

### 1. UI Entry Point
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Dialogs/EventFormDialog.razor`
- **Line 616-675:** `Save()` method - primary save logic
- **Line 653-657:** New event creation
- **Line 511-608:** `OnInitializedAsync()` - initialization
- **Line 757-767:** `SaveReminders()` - reminder persistence
- **Line 50-72:** Timezone selector
- **Line 292-299:** Reminder UI checkboxes

### 2. Database Persistence
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Repositories/EventRepository.cs`
- **Line 108-148:** `CreateAsync()` method
- **Line 150-182:** `UpdateAsync()` method
- **Line 37-106:** `GetEventsByDateRangeAsync()` - fetches events for calendar view
- **Line 154:** Updates set `UpdatedAt = DateTime.UtcNow`

### 3. Event Model
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Models/Event.cs`
- **Line 1-53:** Complete Event class definition
- **Line 39:** `CreatedAt` property defaults to `DateTime.UtcNow`
- **Line 44:** `ReminderMinutes` property (comma-separated string)
- **Line 15:** `TimeZoneId` property (nullable IANA timezone)

---

## Event Display Flow

### 1. Calendar View (Main)
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Pages/Calendar.razor`
- **Line 430-431:** Event to Appointment mapping (Start/End times)
- **Line 333-334:** Date range for event fetch
- **Line 970-971:** Display event mapping
- **Line 24:** Injects `ITimeZoneConversionService`

### 2. Grid View
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Shared/EventsGrid.razor`
- **Line 71-86:** Time display columns
- **Line 74-75:** `StartTime.ToString("MMM dd, yyyy")` and `"hh:mm tt"`
- **Line 82-83:** `EndTime.ToString()` same formats
- **Line 359-410:** Color and icon helper methods

### 3. Formatting Helper
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Helpers/CalendarFormatter.cs`
- **Line 18-24:** `FormatTime()` - handles 12h/24h format
- **Line 26-29:** `FormatDateTime()` - full datetime format
- **Line 31-45:** `FormatHourLabel()` - hour display
- **Line 47-78:** Color and icon methods

---

## Timezone Handling

### 1. Timezone Conversion Service
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Services/TimeZoneConversionService.cs`
- **Line 83-125:** `ConvertTime()` - converts between timezones
- **Line 28-81:** `ConvertEventToTimeZone()` - converts entire event
- **Line 127-133:** `GetAvailableTimeZones()` - all system timezones
- **Line 135-154:** `GetCommonTimeZones()` - predefined common zones
- **Line 9-26:** Common timezone list

### 2. User Timezone
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Models/ApplicationUser.cs`
- Property: `TimeZone` string field
- Default: `TimeZoneInfo.Local.Id`

### 3. Event Timezone Storage
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Dialogs/EventFormDialog.razor`
- **Line 505-509:** `SelectedTimeZoneId` property binding
- **Line 622-625:** Sets timezone if not provided
- **Line 914-961:** `LoadTimeZones()` populates dropdown

---

## Reminder/Notification System

### 1. Event Reminders
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Models/Event.cs`
- **Line 44:** `ReminderMinutes` property (string, comma-separated)

**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Components/Dialogs/EventFormDialog.razor`
- **Line 464-473:** Reminder options definition
- **Line 741-755:** `LoadReminders()` method
- **Line 757-767:** `SaveReminders()` method
- **Line 286-329:** Reminders tab UI

### 2. Notification Model
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Core/Models/Notification.cs`
- **Line 1-28:** Complete Notification class
- **Line 20:** `ReminderMinutes` (int, which reminder triggered)
- **Line 21:** `ScheduledFor` (DateTime when to trigger)
- **Line 22:** `IsSent` (bool)
- **Line 23:** `SentAt` (DateTime)
- **Line 26:** `CreatedAt` (always `DateTime.UtcNow`)

### 3. Reminder Tracking Migration
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Migrations/20251107015648_AddReminderTracking.cs`
- **Line 1-70:** Migration adding reminder tracking fields to Notifications table
- **Line 20-31:** ScheduledFor, IsSent, SentAt, ReminderMinutes columns
- **Line 40-43:** ReminderMinutes column on Events table

---

## Database Schema

### Event Table Configuration
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Data/TempusDbContext.cs`
- **Line 25-46:** Event entity configuration
- **Line 27-30:** Column constraints
- **Line 34-35:** Decimal precision (18,2) for costs
- **Line 37-40:** Attendee relationship

### Notification Table Configuration
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Data/TempusDbContext.cs`
- **Line 127-146:** Notification entity configuration
- **Line 140-143:** Event foreign key (SetNull on delete)

---

## Key Timestamp Points

### Creation - Always UTC
1. **Line 39 (Event.cs):** Default `CreatedAt = DateTime.UtcNow`
2. **Line 656 (EventFormDialog.razor):** `_event.CreatedAt = DateTime.UtcNow;`
3. **Line 174 (CalendarEventManager.cs):** Template creation
4. **Line 111, 232 (CalendarEventManager.cs):** Exception events
5. **Line 26 (Notification.cs):** Notification creation

### Updates - Always UTC
1. **Line 154 (EventRepository.cs):** `@event.UpdatedAt = DateTime.UtcNow;`
2. **Line 367, 384, 401, 418, 436 (EventRepository.cs):** Bulk operations

### Display - As Stored (No Conversion)
1. **Line 74-75, 82-83 (EventsGrid.razor):** Raw ToString() calls
2. **Line 430-431, 970-971 (Calendar.razor):** Direct property access
3. **Line 18-24 (CalendarFormatter.cs):** Formatting without conversion

---

## Event Type Colors

**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Web/Helpers/CalendarFormatter.cs` Lines 47-64

| Type | Color | Hex |
|------|-------|-----|
| Meeting | Blue | #1E88E5 |
| Appointment | Green | #43A047 |
| Task | Orange | #FB8C00 |
| TimeBlock | Purple | #8E24AA |
| Reminder | Yellow | #FDD835 |
| Deadline | Red | #E53935 |

---

## Testing & Debugging

### Verbose Logging
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Infrastructure/Repositories/EventRepository.cs`
- **Line 110-147:** Console.WriteLine debug logging in CreateAsync()

### Test File
**File:** `/Users/billgauvey/source/repos/tempus/src/Tempus.Tests/EventRepositoryTests.cs`
- Tests for event repository operations

---

## Critical Issues/Gotchas

1. **Timezone Conversion Not Applied in Display**
   - TimeZoneConversionService exists but isn't used in Calendar/EventsGrid
   - Events display as stored, not converted to user timezone

2. **Mixed DateTime Usage**
   - CreatedAt/UpdatedAt: Always UTC
   - StartTime/EndTime: Stored as-is, no conversion
   - DateTime.Now used in some places instead of DateTime.UtcNow

3. **ReminderMinutes Storage**
   - Stored as string in Event table: "15,60,1440"
   - Must be parsed for use
   - Separate from Notification.ReminderMinutes (int)

4. **Recurrence Expansion**
   - Uses RecurrenceHelper to expand recurring events into instances
   - Line 84 (EventRepository.cs): `RecurrenceHelper.ExpandRecurringEvent()`

