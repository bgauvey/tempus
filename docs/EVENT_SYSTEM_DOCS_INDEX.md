# Event System Documentation Index

This directory contains comprehensive documentation of the Tempus event management system. Use this index to find the information you need.

## Main Documentation Files

### 1. **EVENT_SYSTEM_ARCHITECTURE.md** (18 KB)
Comprehensive deep-dive into the entire event system architecture.

**Contains:**
- Event model/schema definition with all properties explained
- Event creation and storage workflow
- Event display on UI (Calendar and Grid views)
- Timestamp handling (UTC vs local time) - CRITICAL section
- Date/time formatting utilities
- Notification/reminder system details
- Key files summary table
- Recommendations for improvement

**Best For:** Understanding the overall system, debugging complex issues, long-term reference

### 2. **EVENT_SYSTEM_QUICK_REFERENCE.md** (7.2 KB)
Quick lookup guide with file locations and specific line numbers.

**Contains:**
- Event creation flow quick links
- Event display flow quick links
- Timezone handling quick reference
- Reminder/notification system quick reference
- Database schema quick reference
- Key timestamp points (Creation, Updates, Display)
- Event type colors table
- Critical issues/gotchas list

**Best For:** Quick lookups, finding specific code sections, quick problem diagnosis

### 3. **EVENT_FLOW_DIAGRAM.md** (6 KB)
Visual flow diagrams and code paths.

**Contains:**
- Event creation flow diagram
- Event display flow diagram
- Timezone handling (current vs ideal)
- Reminder/notification flow diagram
- Database schema (table structure)
- Code paths for common operations
- Key decision points
- Common issues and solutions

**Best For:** Visual learners, understanding data flow, finding where to make changes

---

## Quick Navigation by Task

### I need to understand how events are created:
1. Read: **EVENT_SYSTEM_QUICK_REFERENCE.md** - "Event Creation Flow" section
2. Read: **EVENT_FLOW_DIAGRAM.md** - "Event Creation Flow" diagram
3. Read code: `/Tempus.Web/Components/Dialogs/EventFormDialog.razor` (Lines 616-675)

### I need to fix event display times:
1. Check: **EVENT_FLOW_DIAGRAM.md** - "Timezone Handling" section
2. Read: **EVENT_SYSTEM_ARCHITECTURE.md** - Section 4 "Timestamp Handling"
3. Read code: `/Tempus.Infrastructure/Services/TimeZoneConversionService.cs`

### I need to fix reminders not working:
1. Read: **EVENT_SYSTEM_QUICK_REFERENCE.md** - "Reminder/Notification System" section
2. Read: **EVENT_FLOW_DIAGRAM.md** - "Reminder/Notification Flow" diagram
3. Check services in: `Program.cs`

### I need to understand the database structure:
1. Look at: **EVENT_FLOW_DIAGRAM.md** - "Database Schema" section
2. Read code: `/Tempus.Infrastructure/Data/TempusDbContext.cs` (Lines 25-146)

### I need to add a new feature to events:
1. Start with: **EVENT_SYSTEM_ARCHITECTURE.md** - Section 7 "Key Files Summary"
2. Look at: **EVENT_SYSTEM_QUICK_REFERENCE.md** - "Critical Issues/Gotchas" section
3. Review the relevant file in the architecture doc

### I need to debug a timestamp issue:
1. Check: **EVENT_FLOW_DIAGRAM.md** - "Common Issues" section #1 or #4
2. Read: **EVENT_SYSTEM_ARCHITECTURE.md** - Section 4.E "Current Implementation Issues"
3. Read code: `/Tempus.Infrastructure/Repositories/EventRepository.cs` (Lines 154)

---

## File Organization

All documentation is in the source directory (`/Users/billgauvey/source/repos/tempus/src/`):

```
src/
├── EVENT_SYSTEM_ARCHITECTURE.md      (Main reference)
├── EVENT_SYSTEM_QUICK_REFERENCE.md   (Quick lookups)
├── EVENT_FLOW_DIAGRAM.md             (Visual diagrams)
├── EVENT_SYSTEM_DOCS_INDEX.md        (This file)
│
├── Tempus.Core/
│   ├── Models/
│   │   ├── Event.cs                  (Event model - line 1-53)
│   │   ├── Notification.cs           (Notification model - line 1-28)
│   │   └── ApplicationUser.cs        (User model with TimeZone)
│   └── Helpers/
│       └── RecurrenceHelper.cs       (Recurrence expansion)
│
├── Tempus.Infrastructure/
│   ├── Repositories/
│   │   ├── EventRepository.cs        (Core CRUD operations)
│   │   └── NotificationRepository.cs (Notification persistence)
│   ├── Services/
│   │   ├── TimeZoneConversionService.cs (Timezone conversions)
│   │   └── NotificationSchedulerService.cs (Reminder scheduling)
│   ├── Data/
│   │   └── TempusDbContext.cs        (EF Core configuration)
│   └── Migrations/
│       └── 20251107015648_AddReminderTracking.cs (Latest migration)
│
└── Tempus.Web/
    ├── Components/
    │   ├── Dialogs/
    │   │   └── EventFormDialog.razor  (Event creation/edit UI)
    │   ├── Pages/
    │   │   └── Calendar.razor         (Main calendar view)
    │   └── Shared/
    │       └── EventsGrid.razor       (Events list view)
    ├── Helpers/
    │   ├── CalendarFormatter.cs       (Display formatting)
    │   └── CalendarEventManager.cs    (Event operations)
    └── Services/
        └── NotificationBackgroundService.cs (Background notifications)
```

---

## Key Code Sections by Topic

### Event Model Properties
- **File:** `/Tempus.Core/Models/Event.cs`
- **Lines:** 1-53
- **Key Properties:**
  - Line 7: `Id` (Guid)
  - Line 10-11: `StartTime`, `EndTime` (DateTime)
  - Line 15: `TimeZoneId` (string, nullable)
  - Line 39: `CreatedAt` (DateTime.UtcNow)
  - Line 44: `ReminderMinutes` (string, comma-separated)

### Event Creation
- **File:** `/Tempus.Web/Components/Dialogs/EventFormDialog.razor`
- **Line 616:** `Save()` method entry
- **Line 653-657:** New event initialization
- **Line 511-608:** Component initialization

### Event Retrieval & Display
- **File:** `/Tempus.Infrastructure/Repositories/EventRepository.cs`
- **Line 37-106:** `GetEventsByDateRangeAsync()` - main fetch method
- **Line 84:** Recurrence expansion call

### Timezone Handling
- **File:** `/Tempus.Infrastructure/Services/TimeZoneConversionService.cs`
- **Line 83-125:** `ConvertTime()` method
- **Line 28-81:** `ConvertEventToTimeZone()` method

### Display Formatting
- **File:** `/Tempus.Web/Helpers/CalendarFormatter.cs`
- **Line 18-24:** `FormatTime()` - 12h/24h formatting
- **Line 26-29:** `FormatDateTime()` - full datetime format

### Reminders Setup
- **File:** `/Tempus.Web/Components/Dialogs/EventFormDialog.razor`
- **Line 464-473:** Reminder options definition
- **Line 741-755:** `LoadReminders()` method
- **Line 757-767:** `SaveReminders()` method
- **Line 292-299:** Reminder UI element

### Database Configuration
- **File:** `/Tempus.Infrastructure/Data/TempusDbContext.cs`
- **Line 25-46:** Event entity configuration
- **Line 127-146:** Notification entity configuration

---

## Understanding Timestamp Strategy

**CRITICAL UNDERSTANDING:** The application has a subtle but important timestamp design:

### How Timestamps Are Used:
1. **CreatedAt/UpdatedAt:** Always `DateTime.UtcNow` (UTC timezone - consistent)
2. **StartTime/EndTime:** Stored as-is, no conversion (assumed to match timezone)
3. **TimeZoneId:** Stored separately (IANA format like "America/New_York")
4. **Display:** Times shown as stored, no automatic conversion to user timezone

### The Implication:
- Events created at 2:00 PM stay 2:00 PM regardless of timezone context
- TimeZoneId is informational but not applied on display
- TimeZoneConversionService exists but isn't used in Calendar/Grid views

### What This Means for Development:
- If adding timezone-aware display: Call `TimeZoneConversionService.ConvertEventToTimeZone()` before rendering
- If modifying event times: Remember they're stored in the timezone specified in TimeZoneId
- For reminders: Use StartTime directly (assumes it's in correct timezone)

---

## Common Debugging Scenarios

### Scenario 1: Event times wrong in calendar
**Check these sections:**
- EVENT_SYSTEM_ARCHITECTURE.md, Section 4.E
- EVENT_FLOW_DIAGRAM.md, "Common Issues" #1

### Scenario 2: Reminders not working
**Check these sections:**
- EVENT_SYSTEM_QUICK_REFERENCE.md, "Reminder/Notification System"
- EVENT_FLOW_DIAGRAM.md, "Reminder/Notification Flow"

### Scenario 3: Recurring events not displaying
**Check these sections:**
- EVENT_SYSTEM_QUICK_REFERENCE.md, EventRepository lines
- EVENT_FLOW_DIAGRAM.md, "Common Issues" #3

### Scenario 4: User timezone changes but times don't update
**Check these sections:**
- EVENT_SYSTEM_ARCHITECTURE.md, Section 4.E
- EVENT_FLOW_DIAGRAM.md, "Timezone Handling"

---

## Making Changes to the Event System

### Before Making Changes:
1. Read relevant section from EVENT_SYSTEM_ARCHITECTURE.md
2. Check EVENT_SYSTEM_QUICK_REFERENCE.md for file locations
3. Review EVENT_FLOW_DIAGRAM.md for visual understanding
4. Look at test file: `/Tempus.Tests/EventRepositoryTests.cs`

### Common Changes:

**Add a new event property:**
1. Add to Event.cs model
2. Add migration with `dotnet ef migrations add`
3. Update EventFormDialog.razor UI
4. Update any display components (EventsGrid, Calendar)
5. Run tests

**Fix timezone display:**
1. Modify Calendar.razor or EventsGrid.razor
2. Call `TimeZoneConversionService.ConvertEventToTimeZone()` before rendering
3. Test with multiple timezones

**Add new reminder interval:**
1. Add to `_reminderOptions` in EventFormDialog.razor (Line 464-473)
2. Update ReminderMinutes property parsing in NotificationSchedulerService
3. Test notification scheduling

---

## Questions to Ask While Reading Code

### When reading Event.cs:
- What properties are required vs nullable?
- When is CreatedAt set? (Answer: Always DateTime.UtcNow)
- Where is ReminderMinutes used?

### When reading EventFormDialog.razor:
- How are reminders saved? (Answer: String.Join in SaveReminders)
- When is timezone set? (Answer: Line 622-625 if not specified)
- How are start/end times handled? (Answer: DateTime.Now initially, user modified)

### When reading EventRepository.cs:
- How are recurring events expanded? (Answer: RecurrenceHelper.ExpandRecurringEvent)
- When is UpdatedAt set? (Answer: Line 154, DateTime.UtcNow)
- What's the date range fetch logic? (Answer: Lines 37-106)

### When reading TimeZoneConversionService.cs:
- Is it called automatically? (Answer: NO - must be called explicitly)
- What's the conversion process? (Answer: To UTC first, then to target)
- What timezones are available? (Answer: Lines 9-26 common, GetAvailableTimeZones() all)

---

## Support & Questions

If you have questions while reading these docs:

1. **"Where is [feature] implemented?"** 
   - Check EVENT_SYSTEM_QUICK_REFERENCE.md

2. **"Why does [behavior] happen?"**
   - Check EVENT_FLOW_DIAGRAM.md, "Key Decision Points"
   - Check EVENT_SYSTEM_ARCHITECTURE.md, Section 4.E

3. **"How do I fix [issue]?"**
   - Check EVENT_FLOW_DIAGRAM.md, "Common Issues"
   - Search in EVENT_SYSTEM_ARCHITECTURE.md for the component

4. **"Where should I add my code?"**
   - Read EVENT_SYSTEM_QUICK_REFERENCE.md
   - Review EVENT_FLOW_DIAGRAM.md, "Code Paths"

---

Last Updated: November 7, 2025
Application: Tempus (Calendar & Event Management System)
Framework: .NET 9, Blazor Server, Entity Framework Core
Database: SQL Server

