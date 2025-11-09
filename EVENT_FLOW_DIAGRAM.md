# Event System Flow Diagrams

## 1. Event Creation Flow

```
User clicks "Create Event"
        |
        v
EventFormDialog.razor
  - OnInitializedAsync() [Line 511-608]
    - Load user info, timezone
    - Setup default event (DateTime.Now)
  - User fills in form
    - StartTime / EndTime (via DatePicker)
    - ReminderMinutes selection [Line 292-299]
    - TimeZoneId selection [Line 50-72]
  - User clicks Save [Line 616]
    |
    v
  Save() method [Line 616-675]
    - Timezone set to user default if empty [Line 622-625]
    - Auto-create contacts for attendees [Line 677-709]
    - Calculate meeting cost [Line 631-634]
    - SaveReminders() [Line 757-767]
      - Converts list to: "15,60,1440" string
    - Set _event.CreatedAt = DateTime.UtcNow [Line 656]
    - Set _event.Id = Guid.NewGuid() [Line 655]
    |
    v
EventRepository.CreateAsync() [Line 108-148]
  - Process attendees [Line 117-137]
    - Generate IDs for new attendees
    - Set EventId foreign key
  - context.Events.Add(@event) [Line 140]
  - await context.SaveChangesAsync() [Line 143]
    |
    v
Database (SQL Server)
  - Events table
    | Id | Title | StartTime | EndTime | TimeZoneId | CreatedAt | ReminderMinutes |
    | ... | ... | 2025-11-07 14:00 | 2025-11-07 14:30 | America/New_York | 2025-11-07 19:00:00 (UTC) | 15,60,1440 |
```

---

## 2. Event Display Flow

```
User navigates to Calendar or Events page
        |
        v
Calendar.razor [Line 333-334]
  - var startDate = DateTime.Today.AddMonths(-1)
  - var endDate = DateTime.Today.AddMonths(2)
  - await EventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId)
    |
    v
EventRepository.GetEventsByDateRangeAsync() [Line 37-106]
  - Fetch non-recurring events in range [Line 42-49]
  - Fetch recurring events [Line 52-58]
  - Fetch recurrence exceptions [Line 61-68]
  - Expand recurring events using RecurrenceHelper [Line 84]
  - Filter out exceptions [Line 87-90]
  - Combine and return all events [Line 99-105]
    |
    v
Events returned to Calendar.razor
    |
    +-- Display in RadzenScheduler [Line 429-432]
    |   - Create Appointment objects
    |   - Start = event.StartTime
    |   - End = event.EndTime
    |   - NO timezone conversion applied
    |
    +-- Display in EventsGrid.razor [Line 71-86]
        - Column: StartTime
          * event.StartTime.ToString("MMM dd, yyyy")
          * event.StartTime.ToString("hh:mm tt")
          * NO timezone conversion applied
        - Column: EndTime (same format)
```

---

## 3. Timezone Handling

### Current Implementation:

```
Event Created
  |
  +--> StartTime = 2025-11-07 14:00 (as entered by user - LOCAL TIME)
  |     EndTime = 2025-11-07 14:30 (as entered by user - LOCAL TIME)
  |     TimeZoneId = "America/New_York"
  |     CreatedAt = 2025-11-07 19:00 (UTC)
  |
  +--> Stored in Database
  |     - StartTime/EndTime stored as-is (no conversion)
  |     - TimeZoneId stored separately
  |
  +--> Retrieved from Database
  |     - Same times returned
  |
  +--> Display on UI
  |     - Times shown as stored (2:00 PM)
  |     - NO automatic conversion to current user's timezone
  |
  v
Problem: If user changes timezone, display doesn't update
         Event "always at 2:00 PM" regardless of timezone context
```

### TimeZoneConversionService (Available but not used):

```
TimeZoneConversionService.ConvertTime() [Line 83-125]
  Input: DateTime, fromTimeZoneId, toTimeZoneId
  
  Process:
    1. Identify DateTime.Kind
    2. Convert to UTC first
    3. Convert from UTC to target timezone
  
  Output: DateTime in target timezone
  
  Status: AVAILABLE but NOT CALLED in Calendar/EventsGrid display
```

---

## 4. Reminder/Notification Flow

```
Event Created with Reminders
  |
  v
Event.ReminderMinutes = "15,60,1440" [string]
  - 15 minutes before
  - 60 minutes (1 hour) before
  - 1440 minutes (1 day) before
  |
  v
Stored in Events table
  |
  v
NotificationSchedulerService / NotificationBackgroundService
  (Runs periodically)
  |
  +-- Check events with reminders
  |
  +-- For each reminder time:
      |
      v
      Calculate: ScheduledFor = event.StartTime - reminderMinutes
      |
      v
      Create Notification record
        - ReminderMinutes = 15 (which reminder triggered)
        - ScheduledFor = DateTime when to trigger
        - IsSent = false (initially)
        - SentAt = null (initially)
        - CreatedAt = DateTime.UtcNow
      |
      v
      At scheduled time:
        - Send browser push notification / email
        - Set IsSent = true
        - Set SentAt = DateTime.UtcNow
      |
      v
      Display in UI
        - User sees notification
        - Can mark as read
        - ReadAt = DateTime.UtcNow
```

---

## 5. Database Schema (Relevant Portions)

```
Events Table
+----------------------------+--------+--------+
| Column                     | Type   | Notes  |
+----------------------------+--------+--------+
| Id                         | GUID   | PK     |
| Title                      | nvarchar(200) | Required |
| Description                | nvarchar(10000) | nullable |
| StartTime                  | datetime2 | No TZ info |
| EndTime                    | datetime2 | No TZ info |
| TimeZoneId                 | nvarchar(max) | IANA format |
| CreatedAt                  | datetime2 | Always UTC |
| UpdatedAt                  | datetime2 | Always UTC |
| ReminderMinutes            | nvarchar(max) | Comma-separated |
| UserId                     | nvarchar(450) | FK to Users |
| IsRecurring                | bit    | 0=false, 1=true |
| RecurrenceParentId         | GUID   | nullable FK |
| IsRecurrenceException      | bit    | Modified instance |
+----------------------------+--------+--------+

Notifications Table
+----------------------------+--------+--------+
| Column                     | Type   | Notes  |
+----------------------------+--------+--------+
| Id                         | GUID   | PK     |
| EventId                    | GUID   | FK (nullable) |
| UserId                     | nvarchar(450) | FK |
| ReminderMinutes            | int    | Which reminder (15, 60, etc) |
| ScheduledFor               | datetime2 | When to trigger |
| IsSent                     | bit    | Delivery status |
| SentAt                     | datetime2 | When sent |
| CreatedAt                  | datetime2 | Always UTC |
| IsRead                     | bit    | User read status |
| ReadAt                     | datetime2 | When user read |
+----------------------------+--------+--------+
```

---

## 6. Code Path for Common Operations

### A. Creating Event with Reminders

```
EventFormDialog.razor
  _reminderOptions = [15, 30, 60, 120, 1440, 2880, 10080]
                      |
                      v
  User selects: 15 minutes, 1 hour, 1 day
                |
                v
  _selectedReminders = [15, 60, 1440]
                      |
                      v
  Save() method
    |
    v
  SaveReminders() [Line 757-767]
    |
    v
  _event.ReminderMinutes = string.Join(",", [15, 60, 1440])
                         = "15,60,1440"
                      |
                      v
  EventRepository.CreateAsync(_event)
    |
    v
  Database: Events.ReminderMinutes = "15,60,1440"
```

### B. Fetching Events for Display

```
Calendar.razor needs events
  |
  v
Call EventRepository.GetEventsByDateRangeAsync(
  startDate = DateTime.Today.AddMonths(-1)
  endDate = DateTime.Today.AddMonths(2)
  userId = currentUser
)
  |
  v
Query 1: Non-recurring in range
Query 2: Recurring parent events
Query 3: Recurrence exceptions
  |
  v
RecurrenceHelper.ExpandRecurringEvent()
  - Creates instances for each occurrence
  - Uses parent's StartTime as template
  |
  v
Filter out exceptions
  |
  v
Return List<Event>
  |
  v
Create RadzenScheduler.AppointmentCollection
  For each event:
    new Appointment {
      Start = event.StartTime,        [NO conversion]
      End = event.EndTime,             [NO conversion]
      Text = event.Title,
      Description = event.Description
    }
  |
  v
Render on Calendar
```

### C. Time Display in EventsGrid

```
EventsGrid.razor Receives List<Event>
  |
  v
For each event in Events:
  <RadzenDataGridColumn Property="StartTime">
    <Template>
      <div>@evt.StartTime.ToString("MMM dd, yyyy")</div>
      <div>@evt.StartTime.ToString("hh:mm tt")</div>
    </Template>
  </RadzenDataGridColumn>
  |
  v
Renders as:
  Nov 07, 2025
  02:00 PM
  
[Note: Uses Direct DateTime.ToString() - NO formatting helper, NO timezone conversion]
```

---

## 7. Key Decision Points

### Where Timezone Should Be Applied

```
Option 1: At Storage (RECOMMENDED)
  Event Created -> Convert to UTC -> Store -> Retrieve -> Display UTC
  Pros: Consistent, unambiguous, server-agnostic
  Cons: Current code doesn't do this

Option 2: At Display (CURRENT)
  Event Created -> Store as-is -> Retrieve -> [NO CONVERSION] -> Display as stored
  Pros: Simple for single-timezone apps
  Cons: Breaks with multi-timezone support

Option 3: At Retrieval (BEST)
  Event Created -> Store with TZ ID -> Retrieve -> Convert to User TZ -> Display
  Pros: Flexible, user-centric, preserves intent
  Cons: Must call conversion service (currently not done)
```

### Current Implementation Decision
- **CreatedAt/UpdatedAt:** UTC (always)
- **StartTime/EndTime:** As-is, no conversion
- **TimeZoneId:** Stored separately
- **Display:** As-is, no conversion
- **Result:** Events show in original entry timezone, not user's current timezone

---

## 8. Common Issues When Working With This Code

### Issue 1: Event Times Wrong After Timezone Change
**Why:** Times are stored as-is, not converted on display
**Fix:** Call TimeZoneConversionService before rendering

### Issue 2: Reminders Not Firing
**Why:** NotificationSchedulerService must be running
**Check:** Services registered in Program.cs
**Debug:** Enable logging in NotificationSchedulerService

### Issue 3: Recurring Events Not Expanding
**Why:** RecurrenceHelper.ExpandRecurringEvent() may have issues
**Check:** EventRepository.GetEventsByDateRangeAsync() Line 84
**Debug:** Check RecurrenceHelper.cs logic

### Issue 4: All-Day Events Display Wrong
**Why:** Times set to midnight but may display with timezone offset
**Fix:** Line 437-438 (EventFormDialog.razor) sets to .Date

### Issue 5: User Timezone Not Saved
**Why:** ApplicationUser.TimeZone might not be populated
**Check:** ApplicationUser model and initialization
**Fix:** Set during user registration/profile setup

