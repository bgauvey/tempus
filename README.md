# Tempus - Award-Winning Time Management Platform

Tempus is a comprehensive time management application built with .NET 9 and Blazor Server. It features an award-winning business interface with modern design, smooth animations, and intelligent scheduling capabilities. Beyond basic calendar functionality, Tempus provides ICS file import/export, custom calendar ranges, advanced calendar settings, and integration capabilities with popular calendar services.

## Features

### Core Features
- **Event Management**: Create, edit, and delete meetings, appointments, tasks, and time blocks
- **Advanced Calendar Views**: Multiple calendar views (Monthly, Weekly, Work Week, Daily, Agenda) with customizable settings
- **Calendar Settings**: Comprehensive configuration including time formats (12/24-hour), date formats, work hours, time slot duration, and event visibility
- **Custom Calendar Ranges**: Define and manage custom date ranges for specialized scheduling
- **Calendar Import**: Import events from ICS and PST files (compatible with Google Calendar, Microsoft Outlook, Apple Calendar)
- **Dashboard**: Real-time overview of upcoming events, tasks, statistics, and analytics
- **Event Types**: Support for different event types (Meeting, Appointment, Task, Time Block, Reminder, Deadline)
- **Priority System**: Assign priorities to events (Low, Medium, High, Urgent)
- **Recurring Events**: Support for recurring event patterns with flexible recurrence rules
- **Attendee Management**: Add and track attendees for events with organizer designation, auto-add current user as organizer
- **Meeting Cost Calculator**: Calculate estimated meeting costs based on attendee count, duration, and hourly rates
- **Address Book**: Contact management with automatic integration into event attendees

### User Experience
- **Award-Winning UI**: Modern gradient designs with purple, blue, green, and pink color schemes
- **Smooth Animations**: Professional transitions and glass morphism effects
- **Responsive Design**: Optimized for all device sizes
- **Authentication**: User registration and login with secure identity management
- **GDPR Compliance**: Privacy controls, terms of service, and security features

## Technology Stack

- **.NET 9**: Latest version of the .NET framework
- **Blazor Server**: Interactive web UI framework with SignalR
- **Entity Framework Core 9**: ORM for database access
- **SQLite**: Default database provider (with SQL Server support available)
- **Radzen Blazor**: Modern UI component library (v5.9.0)
- **Ical.Net**: iCalendar format parsing and generation (v4.3.1)
- **Aspose.Email**: PST/OST file parsing for Outlook integration (v24.12.0)
- **QuestPDF**: PDF generation capabilities (v2025.7.3)
- **Google.Apis.Calendar.v3**: Google Calendar API client (v1.68.0.3536)
- **HaemmerElectronics.SeppPenner.CalDAVNet**: CalDAV protocol client for Apple Calendar (v1.0.3)
- **Microsoft.Graph**: Microsoft Graph API client for Outlook/Office 365 integration (v5.77.0)
- **Microsoft.Identity.Client**: MSAL library for Microsoft OAuth2 authentication (v4.67.1)
- **ASP.NET Core Identity**: Authentication and authorization

## Project Structure

```
tempus/
├── src/                            # Main source code
│   ├── Tempus.Core/               # Domain models and interfaces
│   │   ├── Models/                # Event, Attendee, CalendarSettings, Contact, etc.
│   │   ├── Enums/                 # EventType, Priority, CalendarView, TimeFormat, etc.
│   │   ├── Interfaces/            # Repository and service interfaces
│   │   └── Helpers/               # Utility classes
│   ├── Tempus.Infrastructure/     # Data access and external services
│   │   ├── Data/                  # TempusDbContext with Identity support
│   │   ├── Repositories/          # Repository implementations
│   │   ├── Services/              # ICS import/export, Settings service
│   │   └── Migrations/            # EF Core database migrations
│   ├── Tempus.Web/                # Blazor Server web application
│   │   ├── Components/
│   │   │   ├── Layout/            # MainLayout, navigation
│   │   │   └── Pages/             # Calendar, Dashboard, Settings, etc.
│   │   ├── Services/              # Application services
│   │   ├── wwwroot/               # Static files, CSS, JavaScript, favicons
│   │   ├── Program.cs             # Application startup and DI configuration
│   │   └── appsettings.json       # Configuration settings
│   └── Tempus.Tests/              # Unit tests
└── tests/                         # Integration tests
    └── Tempus.Web.Tests/          # Web application tests
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Visual Studio 2022 (17.8+) or Visual Studio Code with C# extension
- No database installation required (uses SQLite by default)

## Getting Started

### 1. Clone or Download

```bash
# If using git
git clone <repository-url>
cd Tempus

# Or simply extract the Tempus folder
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the Application

```bash
cd Tempus.Web
dotnet run
```

The application will start and be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5000`

### 5. Using Visual Studio

1. Open `Tempus.sln` in Visual Studio 2022
2. Set `Tempus.Web` as the startup project
3. Press `F5` or click the Run button

## Database Configuration

By default, Tempus uses **SQLite** for easy setup and portability. The database file (`tempus.db`) is created automatically in the `Tempus.Web` directory on first run.

### SQLite (Default)

No configuration needed! The database is automatically created and managed. SQLite is perfect for:
- Development and testing
- Single-user deployments
- Portable applications
- Quick prototyping

### Switching to SQL Server

If you need SQL Server for production or multi-user scenarios:

1. Update `appsettings.json` connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TempusDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
}
```

2. Update `Program.cs` to use SQL Server:
```csharp
// Change from UseSqlite to UseSqlServer
builder.Services.AddDbContext<TempusDbContext>(options =>
    options.UseSqlServer(connectionString));
```

3. Create and apply migrations:
```bash
cd Tempus.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Tempus.Web
dotnet ef database update --startup-project ../Tempus.Web
```

## Features Roadmap

### Current Features
- ✅ Event CRUD operations with full attendee management
- ✅ Multiple calendar views (Monthly, Weekly, Work Week, Daily, Year, Planner, Timeline, Grid)
- ✅ Full viewport calendar layout with optimized screen space usage
- ✅ Drag-and-drop event rescheduling with recurring event support
- ✅ Quick time block templates (Deep Work, Meetings, Focus Blocks, Breaks)
- ✅ Advanced calendar settings with integrated Integrations tab
- ✅ Custom calendar ranges for specialized scheduling
- ✅ ICS file import and export with daily agenda PDF generation
- ✅ Dashboard with real-time statistics and analytics
- ✅ Multiple event types and priorities
- ✅ Recurring events with flexible patterns (edit single or all occurrences)
- ✅ User authentication and authorization
- ✅ Contact management and address book with auto-creation
- ✅ Meeting cost calculator with hourly rate tracking
- ✅ Award-winning UI with animations and modern design
- ✅ Full theme support (multiple Radzen themes - light and dark modes)
- ✅ Theme-aware components (Settings, Profile, Calendar, Dashboard)
- ✅ GDPR compliance (Privacy, Terms of Service, Security)
- ✅ Responsive design for all devices
- ✅ PDF export capabilities (daily agenda via QuestPDF)
- ✅ Organizer designation and protection in meetings
- ✅ User avatar menu with profile management
- ✅ Time zone support for multi-location meetings
  - Selectable timezone for each event (supports all IANA timezones)
  - Automatic timezone conversion to user's local timezone in calendar and dashboard
  - Visual timezone indicators (🌍) when event is in different timezone
  - Common timezone quick-select list for easy selection
  - Original event timezone preserved for multi-location coordination
- ✅ Calendar view preferences and saved layouts
  - Remember and restore last used calendar view across sessions
  - Support for all 6 calendar views (Month, Week, Day, Year, Year Planner, Year Timeline)
  - Automatic view preference saving when user switches views
  - Customizable hour range for day/week views (start/end hours)
  - Event display filters (show/hide completed tasks, cancelled events)
  - Display customization (event icons, colors, compact view mode)
  - Per-user personalized calendar experience
- ✅ Advanced search and filtering
  - Comprehensive search dialog with organized filter sections
  - Full-text search across titles, descriptions, locations, and attendees
  - Selectable search scope (search in title, description, location, attendees)
  - Date range filtering with flexible start/end dates
  - Multi-select event type and priority filtering with checkboxes
  - Status-based filtering (all, completed, incomplete events)
  - Recurring event inclusion toggle
  - Time-of-day filtering (earliest start, latest end times)
  - Flexible sorting options (StartTime, EndTime, Title, Priority, CreatedDate, UpdatedDate)
  - Ascending/descending sort order control
  - Configurable result limiting (max results)
  - Reset filter functionality to quickly clear all criteria
  - Visual search state indicator in calendar header
  - Clear search button to restore full calendar view
  - Case-insensitive search capabilities
  - Efficient database querying with EF Core
  - Resizable, draggable search dialog (700x600px)
  - Search results seamlessly replace calendar view
  - Timezone-aware search results display
- ✅ Bulk event operations
  - Selection mode for multi-event selection on calendar
  - Visual selection indicators (golden border, glow effect, checkmark)
  - Dynamic bulk operations toolbar appearing when events selected
  - Real-time selection counter with grammar-aware pluralization
  - Bulk event type changes (Meeting, Task, Reminder, Appointment, etc.)
  - Bulk priority updates (Low, Medium, High, Critical)
  - Bulk color changes with 10 predefined colors + custom color picker
  - Bulk event movement with configurable time offset (days, hours, minutes)
  - Bulk completion status toggling (mark complete/incomplete)
  - Bulk event deletion with confirmation dialog
  - Click-to-select in selection mode
  - Context menu integration for select/deselect operations
  - Auto-clear selection after successful operations
  - Efficient database operations using batched updates
  - Safe user ID filtering ensures data isolation
  - Loading states and error handling for all operations
- ✅ Industry benchmarking and best practices
  - Compare time management metrics against industry standards
  - Overall performance score (0-100) with visual arc gauge
  - 9 key benchmark comparisons across 5 categories
  - Meeting benchmarks (percentage, duration, attendee count)
  - Focus time benchmarks (percentage, block duration)
  - Work hours benchmarks (weekly, daily averages)
  - Task management benchmarks (completion rate)
  - Schedule quality benchmarks (fragmentation score)
  - Color-coded status indicators (Excellent, AtStandard, NearStandard, BelowStandard)
  - Context-aware recommendations for each metric
  - Top 3 priority recommendations highlighted
  - Variance calculations showing deviation from standards
  - Multiple analysis periods (7, 30, 90 days)
  - Actionable insights for productivity improvement
  - Industry best practices integration
  - Professional visualization with progress bars and charts
- ✅ Comprehensive notification system
  - Email notifications for meeting updates (created, updated, cancelled)
  - Browser/desktop push notifications with real-time alerts
  - Customizable notification preferences per event
  - Support for notification permissions and fallback handling
  - Real-time notification delivery for upcoming events and reminders
- ✅ Advanced calendar analytics and insights dashboard
  - Calendar health score (0-100) with visual gauge
  - Time usage breakdown by event type, day of week, and hour
  - Meeting analytics with cost tracking and top participants
  - Productivity metrics (focus time, task completion rate, fragmentation)
  - AI-powered recommendations for schedule optimization
  - Warning system for burnout prevention and scheduling issues
  - Flexible date range analysis (7, 30, 90, 365 days)
- ✅ Predictive analytics and trend forecasting
  - Historical trend analysis with linear regression
  - Future predictions for key metrics (events, costs, workload)
  - Pattern detection (busiest days, peak hours, meeting habits)
  - 4-week workload forecast with capacity planning
  - Trend direction indicators (increasing, decreasing, stable, volatile)
  - Confidence levels for predictions
  - Actionable recommendations based on forecasts
- ✅ Analytics report export (PDF, CSV, Excel formats)
- ✅ Advanced analytics visualizations (interactive charts, heatmaps, graphs)
  - Interactive donut chart for time distribution by event type
  - Column chart showing activity by day of week
  - Hour-by-hour activity heatmap with intensity colors
  - Bar chart for top cost contributors analysis
  - Pie chart for meeting frequency by type
  - Line charts displaying historical trend data with smooth curves
  - Area chart for workload forecast visualization
  - Dual-axis charts showing events and hours together
  - Hover tooltips with detailed information
  - Responsive chart sizing and layout
  - Color-coded trends (green=increasing, red=decreasing, blue=stable, orange=volatile)
- ✅ Google Calendar integration (OAuth2)
  - Secure OAuth2 authentication flow
  - Two-way event synchronization (import from Google, export to Google)
  - Automatic sync token management for incremental updates
  - Connection testing and validation
  - Calendar selection and management
  - Integration status tracking with last sync timestamps
  - Sync on demand or automatic background sync
  - Event deduplication using Google Event IDs
- ✅ Apple Calendar integration (CalDAV)
  - CalDAV protocol support for iCloud calendars
  - App-specific password authentication
  - Two-way event synchronization
  - Connection testing before saving credentials
  - Support for all-day and timed events
  - Event deduplication using CalDAV UIDs
  - Multiple calendar support
  - Secure credential storage
  - Step-by-step setup instructions
- ✅ Microsoft Outlook/Office 365 integration (Microsoft Graph API)
  - Secure OAuth2 authentication with Microsoft accounts
  - Two-way event synchronization (import from Outlook, export to Outlook)
  - Support for both personal and work/school accounts
  - Multiple calendar support
  - Connection testing and validation
  - Integration status tracking with last sync timestamps
  - Sync on demand or automatic background sync
  - Event deduplication using Outlook Event IDs
  - Automatic token refresh for seamless authentication

### Planned Features

Listed in priority order from highest to lowest:

1. **Multiple Calendar Support**
   - Support for multiple personal calendars within a single account
   - Color-coding and filtering by calendar
   - Toggle visibility of individual calendars
   - Calendar-specific default settings

2. **Meeting Responses & RSVP**
   - Accept/Decline/Tentative responses for meeting invitations
   - Track attendee responses and send reminders to non-responders
   - Propose alternative meeting times
   - Guest list visibility controls

3. **Video Conferencing Integration**
   - Automatic Zoom/Teams/Google Meet link generation
   - One-click join buttons for video meetings
   - Virtual meeting room management
   - Phone dial-in number support

4. **Calendar Sharing & Advanced Permissions**
   - Share calendars with specific people with granular permissions
   - Delegate calendar management to assistants
   - Permission levels (view free/busy, view details, make changes, etc.)
   - Subscribe to public calendars (holidays, sports schedules, school calendars)

5. **Scheduling Assistant / Find a Time**
   - Visual availability grid showing when all attendees are free
   - Smart scheduling suggestions based on attendee availability
   - Automatic conflict detection and resolution
   - Meeting polls (Doodle-style integration)

6. **Natural Language Event Creation**
   - Create events with natural language ("Meeting with John tomorrow at 2pm")
   - Voice command support
   - Quick add via keyboard shortcuts

7. **Appointment Booking Pages**
   - Create public booking pages (like Calendly)
   - Let others schedule time with you automatically
   - Buffer time between appointments
   - Booking limits and rules

8. **Room & Resource Booking**
   - Conference room finder and booking system
   - Equipment/resource reservation (projectors, vehicles, etc.)
   - Room availability checking
   - Capacity management and tracking

9. **Out of Office / Availability Status**
   - Set out-of-office status with auto-responders
   - Define working hours and availability windows
   - Automatic meeting declination during blocked times
   - Focus time protection (auto-decline meetings)

10. **Event Attachments & Rich Content**
    - Attach files and documents to events
    - Add images to calendar events
    - Link to notes (OneNote, Evernote, Google Docs)
    - Meeting agendas as attachments

11. **Keyboard Shortcuts**
    - Quick navigation (j/k for prev/next day)
    - Fast event creation (c for create)
    - View switching shortcuts
    - Search activation and filtering

12. **Mobile Native App (MAUI)**
    - Native iOS and Android applications
    - Mobile-optimized UI and gestures
    - Push notifications
    - Widget support on mobile devices

13. **AI-Powered Smart Scheduling**
    - AI-suggested meeting times based on habits and patterns
    - Optimal scheduling recommendations
    - Automatic lunch break protection
    - Learning from scheduling preferences

14. **Third-Party Integrations**
    - Slack integration (schedule meetings, view calendar)
    - Microsoft Teams deep integration
    - Task management apps (Todoist, Asana, Trello)
    - CRM integrations (Salesforce, HubSpot)
    - Email platform integrations

15. **Working Location Management**
    - Set location status (Office, Home, Remote, Traveling)
    - Show location in calendar view
    - Location-based notifications and reminders

16. **Free/Busy Information Sharing**
    - Publish free/busy times
    - View others' availability without seeing event details
    - Privacy-respecting availability sharing

17. **Event Proposals & Polling**
    - Suggest multiple time options to attendees
    - Collect votes on preferred meeting times
    - Automatic scheduling when consensus reached

18. **Travel Time Calculation**
    - Automatic travel time insertion between events
    - Interactive maps for event locations
    - Directions and real-time traffic updates
    - Location suggestions based on history

19. **Offline Mode & Sync**
    - Work without internet connection
    - Sync when connection restored
    - Offline event creation and editing
    - Cached data management

20. **Event Templates**
    - Save recurring event configurations
    - One-click event creation from templates
    - Template sharing across team

21. **Email to Calendar**
    - Create events from email (forward to calendar)
    - Send meeting invitations via email with ICS attachments
    - Email threading with event discussions

22. **Birthday & Special Occasion Calendar**
    - Auto-import birthdays from contacts
    - Automatic yearly recurrence
    - Birthday and anniversary reminders

23. **Desktop & Mobile Widgets**
    - Today's agenda widget
    - Upcoming events widget
    - Quick event creation widget

24. **Event History & Audit Trail**
    - Track who made changes to events
    - Change history log with timestamps
    - Restore previous event versions
    - Compliance and accountability features

25. **World Clock / Multiple Timezone View**
    - Side-by-side timezone comparison
    - World clock widget for global teams
    - Meeting time translator across timezones

26. **Weather Integration**
    - Show weather forecast on calendar days
    - Weather-aware event planning
    - Severe weather alerts for outdoor events

27. **Speedy Meetings**
    - End meetings 5-10 minutes early automatically
    - Build in buffer time between meetings
    - Prevent back-to-back scheduling

28. **Team & Organizational Analytics**
    - Team-wide calendar analytics
    - Organizational meeting metrics
    - Department-level insights

29. **Goals & Habits Tracking**
    - Set recurring goals (exercise 3x/week)
    - Smart scheduling of goal time
    - Progress tracking and analytics

30. **Enhanced Print & Export**
    - Formatted print layouts with custom templates
    - Print range selection and customization
    - Calendar snapshots for presentations

31. **Additional Custom Themes**
    - Theme editor for custom color schemes
    - Dark mode variants
    - Accessibility-focused themes

32. **Multi-language Support (i18n)**
    - Localization for multiple languages
    - Date/time format localization
    - RTL language support

## Using the Application

### Navigation

The application features a modern sidebar navigation with the following sections:
- **Home**: Landing page with feature highlights
- **Dashboard**: Your personalized command center with statistics and upcoming events
- **Calendar**: Advanced calendar with multiple view options, drag-and-drop rescheduling, and quick time block templates
- **Analytics**: Comprehensive calendar analytics and insights dashboard with health scores, time usage breakdowns, meeting analytics, and AI-powered recommendations
- **Import ICS**: Import events from other calendar applications
- **Address Book**: Manage contacts for event attendees
- **Settings**: Configure calendar preferences, time formats, work hours, email and browser notifications, and integrations (accessible via user menu)

### Creating Events

1. Navigate to the Calendar or Dashboard page
2. Click "Create Event" or select a date/time slot
3. Fill in event details:
   - Title, description, location
   - Start and end date/time
   - Event type (Meeting, Appointment, Task, etc.)
   - Priority level (Low, Medium, High, Urgent)
   - Recurrence pattern (if recurring)
4. **For Meeting-type events:**
   - Current user is automatically added as the organizer
   - Add attendees from your address book or create new contacts on-the-fly
   - New contacts are automatically saved to your address book
   - Organizers cannot be removed from the attendee list
   - Set hourly cost per attendee to calculate meeting expenses
   - View real-time meeting cost estimates
5. Save the event

### Meeting Cost Calculator

For meeting-type events, Tempus includes a powerful cost calculator:

1. When creating or editing a meeting, navigate to the **Meeting Cost Calculator** section
2. Set the **Hourly Cost Per Attendee** (defaults to $75/hour)
3. View the real-time **Estimated Meeting Cost** calculation based on:
   - Number of attendees
   - Meeting duration
   - Hourly rate per attendee
4. The calculated cost is saved with the meeting for future reporting and analytics

**Example:** A 2-hour meeting with 5 attendees at $75/hour = $750 total cost

This feature helps organizations understand the true cost of meetings and make informed decisions about scheduling.

### Using Time Zones for Multi-Location Meetings

Tempus supports time zone selection for events, making it easy to coordinate meetings across different locations:

1. **Setting Event Time Zone:**
   - When creating or editing an event, look for the **Time Zone** field in the Details tab
   - By default, events use your profile timezone (set in Settings)
   - Use the dropdown to search and select a different timezone
   - The dropdown includes a quick-select list of common timezones at the top
   - Click the refresh button (🔄) to reset to your default timezone

2. **How Time Zones Work:**
   - The event is stored with its original timezone (e.g., "America/New_York")
   - When viewing in Calendar or Dashboard, times are automatically converted to YOUR timezone
   - Events in different timezones show a 🌍 indicator with the timezone abbreviation
   - The original timezone is preserved, so all participants see the correct local time

3. **Example Scenario:**
   - You're in Los Angeles (PST) and create a meeting set for New York (EST) at 2:00 PM
   - Your calendar will show the event at 11:00 AM PST with 🌍 EST indicator
   - Colleagues in New York will see it at 2:00 PM EST
   - The meeting is correctly coordinated across time zones

4. **Best Practices:**
   - Set timezone for any meeting with participants in different locations
   - Your default timezone is set in **Settings** → **General** → **Time Zone**
   - Use common timezone names (Pacific, Eastern, Central, Mountain, UTC, etc.)
   - The system handles daylight saving time changes automatically

### Connecting External Calendars

Tempus supports two-way synchronization with Google Calendar, Microsoft Outlook/Office 365, and Apple Calendar (iCloud), allowing you to keep all your calendars in sync.

#### Google Calendar Integration

1. **Connect Your Google Calendar:**
   - Navigate to **Settings** → **Integrations** tab (or use the Integrations page from the sidebar)
   - Click **Connect Google Calendar** in the Google Calendar card
   - You'll be redirected to Google's authorization page
   - Sign in with your Google account and grant calendar access permissions
   - You'll be redirected back to Tempus with the connection established

2. **Sync Events:**
   - Click **Sync Now** to perform two-way synchronization:
     - Events from Google Calendar are imported to Tempus
     - Events from Tempus are exported to Google Calendar
   - The last sync timestamp is displayed on the integration card
   - Sync tokens are used for efficient incremental updates

3. **Disconnect:**
   - Click **Disconnect** to stop synchronization
   - A confirmation dialog will appear
   - Your existing events in Tempus remain unchanged

#### Apple Calendar Integration (CalDAV)

1. **Generate App-Specific Password:**
   - Go to [appleid.apple.com](https://appleid.apple.com) and sign in
   - Navigate to **Security** → **App-Specific Passwords**
   - Click **Generate Password**
   - Enter a label (e.g., "Tempus Calendar") and click **Create**
   - Copy the generated 16-character password (format: xxxx-xxxx-xxxx-xxxx)

2. **Connect Your Apple Calendar:**
   - Navigate to **Settings** → **Integrations** tab
   - Click **Connect Apple Calendar**
   - Fill in the connection form:
     - **CalDAV Server URL**: `https://caldav.icloud.com` (default for iCloud)
     - **Apple ID Email**: Your iCloud email address
     - **App-Specific Password**: The password you generated in step 1
   - Click **Test Connection** to verify credentials
   - Once test succeeds, click **Connect** to save the integration

3. **Sync Events:**
   - Click **Sync Now** on the Apple Calendar card
   - Two-way synchronization runs:
     - Events from Apple Calendar are imported to Tempus
     - Events from Tempus are exported to Apple Calendar
   - Both all-day and timed events are supported
   - Event updates are tracked using CalDAV UIDs

4. **Disconnect:**
   - Click **Disconnect** to stop synchronization
   - Confirm the action in the dialog
   - Existing events remain in Tempus

#### Microsoft Outlook/Office 365 Integration

1. **Register Azure AD Application (One-Time Setup):**
   - This step must be completed by the system administrator
   - Go to [Azure Portal](https://portal.azure.com) and sign in
   - Navigate to **Azure Active Directory** → **App registrations**
   - Click **New registration**
   - Enter application name (e.g., "Tempus Calendar Integration")
   - Select supported account types (personal, work, or both)
   - Add redirect URI: `https://your-domain/outlook-callback`
   - After registration, note the **Application (client) ID** and **Directory (tenant) ID**
   - Go to **Certificates & secrets** → **New client secret**
   - Copy the secret value immediately (it won't be shown again)
   - Go to **API permissions** → **Add permission** → **Microsoft Graph** → **Delegated permissions**
   - Add permissions: `Calendars.ReadWrite`, `offline_access`
   - Click **Grant admin consent** (if required by your organization)
   - Update your Tempus configuration with:
     - `Outlook:ClientId` = Application (client) ID
     - `Outlook:ClientSecret` = Client secret value
     - `Outlook:TenantId` = Directory (tenant) ID (or "common" for multi-tenant)

2. **Connect Your Outlook Calendar:**
   - Navigate to **Settings** → **Integrations** tab (or use the Integrations page from the sidebar)
   - Click **Connect Outlook Calendar** in the Outlook card
   - You'll be redirected to Microsoft's authorization page
   - Sign in with your Microsoft account (personal or work/school)
   - Grant calendar access permissions
   - You'll be redirected back to Tempus with the connection established

3. **Sync Events:**
   - Click **Sync Now** to perform two-way synchronization:
     - Events from Outlook Calendar are imported to Tempus
     - Events from Tempus are exported to Outlook Calendar
   - The last sync timestamp is displayed on the integration card
   - Access tokens are automatically refreshed when needed

4. **Disconnect:**
   - Click **Disconnect** to stop synchronization
   - A confirmation dialog will appear
   - Your existing events in Tempus remain unchanged

**Security Note:** Your calendar credentials are securely stored and never shared with third parties. Google and Outlook use OAuth2 tokens (no password storage), and Apple uses app-specific passwords (not your main iCloud password).

### Customizing Calendar Settings

1. Navigate to **Settings** from the user menu (top-right avatar)
2. Configure your preferences across multiple tabs:
   - **General**: Time format (12/24-hour), date format, time zone, week start day, default calendar view
   - **Work Hours**: Set working hours, lunch breaks, time slot duration, buffer times
   - **Event Defaults**: Default meeting duration, event visibility, default colors and locations
   - **Notifications**: Enable/disable email notifications, browser push notifications, default reminder times, and per-event notification preferences
   - **Integrations**: View upcoming calendar integrations (Google, Outlook, Apple Calendar)

### Using Calendar Views

The Calendar page supports multiple viewing modes:
- **Month View**: Overview of the entire month with event indicators
- **Week View**: Detailed week schedule with time slots and drag-and-drop support
- **Work Week View**: Monday-Friday focus for work scheduling
- **Day View**: Hour-by-hour breakdown of a single day with automatic scroll to work hours
- **Year View**: Annual overview with event density visualization
- **Year Planner**: Comprehensive yearly planning view
- **Year Timeline**: Timeline-based yearly view
- **Grid View**: List-based view with search and filter capabilities

**Key Features:**
- Drag-and-drop events to reschedule (supports recurring events)
- Quick time block templates for common activities
- Automatic scroll to work hours in day/week views
- Full viewport layout for maximum screen usage
- Download daily agenda as PDF

Navigate views using the toolbar buttons and customize settings in real-time.

### Using Advanced Search and Filtering

The Advanced Search feature allows you to quickly find specific events using multiple filter criteria:

1. **Opening Advanced Search:**
   - Navigate to the **Calendar** page
   - Click the **Advanced Search** button in the calendar header (next to Day Agenda)
   - A comprehensive search dialog will open (700x600px, resizable and draggable)

2. **Text Search Options:**
   - Enter search terms in the **Search term** field
   - Select which fields to search:
     - ✅ **Title** (default: enabled)
     - ✅ **Description** (default: enabled)
     - ✅ **Location** (default: enabled)
     - ✅ **Attendees** (default: disabled)
   - Search is case-insensitive for better results

3. **Date Range Filtering:**
   - Set **Start Date** to filter events starting from a specific date
   - Set **End Date** to filter events ending before a specific date
   - Leave either field empty for open-ended ranges

4. **Event Type Filtering:**
   - Select one or more event types to narrow results:
     - Meeting, Appointment, Task, TimeBlock, Reminder, Deadline, Other
   - Leave all unchecked to search across all event types

5. **Priority Filtering:**
   - Filter by priority levels: Low, Medium, High, Critical
   - Select multiple priorities to include all matching events

6. **Status and Options:**
   - Choose completion status:
     - **All**: Show both completed and incomplete events
     - **Completed**: Only show completed events
     - **Incomplete**: Only show incomplete events
   - Toggle **Include Recurring Events** to include/exclude recurring events

7. **Time of Day Filtering:**
   - Set **Earliest Start Time** to find events starting after a specific time
   - Set **Latest End Time** to find events ending before a specific time
   - Useful for finding morning meetings, afternoon tasks, etc.

8. **Sorting Results:**
   - **Sort By**: Choose from 6 sort options
     - StartTime (default), EndTime, Title, Priority, CreatedDate, UpdatedDate
   - **Order**: Select Ascending (Asc) or Descending (Desc)

9. **Result Limiting:**
   - Set **Max Results** to limit the number of events returned
   - Leave empty to show all matching events

10. **Using Search Results:**
    - Click **Search** to apply filters and view results
    - The calendar view updates to show only matching events
    - A **Clear Search** button appears in the header
    - Search results maintain timezone conversion and event formatting
    - Click **Clear Search** to restore the full calendar view

11. **Resetting Filters:**
    - Click **Reset** in the dialog to clear all filter criteria
    - Default settings are restored (search in title, description, location enabled)

**Example Use Cases:**
- Find all High priority Meetings in the next 30 days
- Search for events containing "Project Alpha" in any field
- Locate all Tasks completed in the last week
- Find afternoon meetings (start time after 1:00 PM)
- Search for events with specific attendees
- Identify all events at a particular location

**Tips:**
- Combine multiple filters for precise results
- Use date ranges to focus on specific time periods
- Sort by Priority to find urgent items quickly
- Use time-of-day filters to find scheduling conflicts
- The search is timezone-aware and respects your local timezone

### Using Bulk Event Operations

The Bulk Operations feature allows you to efficiently manage multiple events at once, saving time on repetitive tasks:

#### Entering Selection Mode

1. **Activate Selection Mode:**
   - Navigate to the **Calendar** page
   - Click the **Select Events** button in the calendar header
   - The button changes to **Exit Selection** with a warning style
   - Calendar enters selection mode

2. **Visual Changes:**
   - Events can now be selected by clicking
   - Context menu shows Select/Deselect options
   - Selected events display with golden border and glow effect
   - Checkmark (✓) appears before selected event titles

#### Selecting Events

1. **Click to Select:**
   - Simply click any event to select it
   - Click again to deselect
   - Selection state persists across view changes

2. **Context Menu Selection:**
   - Right-click any event
   - Choose **Select** or **Deselect** from context menu
   - Useful for precise selection control

3. **Selection Indicators:**
   - Selected events have 3px golden border (#FFD700)
   - Glowing shadow effect around selected events
   - Checkmark prefix in event title
   - Selection count shown in bulk toolbar

#### Using the Bulk Operations Toolbar

Once events are selected, a toolbar appears with available actions:

1. **Toolbar Display:**
   - Appears between calendar header and content
   - Shows "X event(s) selected" with real-time count
   - **Clear Selection** button to deselect all events
   - Action buttons for bulk operations

2. **Available Bulk Actions:**

   **Change Event Type:**
   - Click **Change Type** button
   - Select new event type from dropdown (Meeting, Task, Reminder, etc.)
   - Apply to all selected events

   **Set Priority:**
   - Click **Set Priority** button
   - Choose priority level (Low, Medium, High, Critical)
   - Updates all selected events

   **Set Color:**
   - Click **Set Color** button
   - Choose from 10 predefined colors (Blue, Green, Orange, Red, etc.)
   - Or use custom color picker for any color
   - Visual color swatches make selection easy

   **Move Events:**
   - Click **Move Events** button
   - Configure time offset using three fields:
     * **Days**: Number of days to move (positive = forward, negative = backward)
     * **Hours**: Hour adjustment (-23 to +23)
     * **Minutes**: Minute adjustment (-59 to +59)
   - Preview shows direction and amount (e.g., "2 days, 3 hours forward")
   - All selected events move by the same offset

   **Mark Complete:**
   - Click **Mark Complete** button (green)
   - Instantly marks all selected events as complete
   - No dialog confirmation needed

   **Mark Incomplete:**
   - Click **Mark Incomplete** button (gray)
   - Marks all selected events as incomplete
   - Useful for resetting completed tasks

   **Delete:**
   - Click **Delete** button (red)
   - Confirmation dialog appears showing event count
   - Confirm to permanently delete all selected events
   - Operation cannot be undone

#### Workflow Example

**Scenario: Reschedule all meetings from Monday to Tuesday**

1. Click **Select Events** to enter selection mode
2. Click all Monday meetings on the calendar
3. Bulk toolbar shows "5 events selected"
4. Click **Move Events** button
5. Enter **Days: 1** (leave hours and minutes at 0)
6. Preview shows "1 day forward"
7. Click **Apply**
8. All 5 meetings move to Tuesday at same times
9. Selection automatically clears
10. Click **Exit Selection** to return to normal mode

#### Best Practices

1. **Review Selection:**
   - Check selection count matches expectations
   - Selected events have clear visual indicators
   - Use Clear Selection if you made mistakes

2. **Use Bulk Operations For:**
   - Rescheduling multiple events (team meetings, deadlines)
   - Color-coding events by project or category
   - Changing priority for a group of tasks
   - Converting events to different types
   - Mass deletion of old or duplicate events
   - Marking multiple tasks complete at once

3. **Safety Features:**
   - Delete operations require confirmation
   - Cannot modify events from other users
   - Operations are transactional (all or nothing)
   - Loading states prevent accidental double-clicks
   - Automatic event refresh shows changes immediately

4. **Performance:**
   - Efficient database batching reduces server load
   - Works well with large selections (100+ events)
   - Operations complete quickly with instant feedback

5. **After Operations:**
   - Selection automatically clears on success
   - Calendar refreshes to show updated events
   - Console logs confirm operation completion
   - Stay in selection mode to perform more operations

#### Exiting Selection Mode

- Click **Exit Selection** button in calendar header
- Selection automatically clears
- Calendar returns to normal interaction mode
- Context menus show Edit/Duplicate/Delete options again

This feature is particularly useful for calendar maintenance, project management, and handling recurring tasks efficiently.

### Using Industry Benchmarks

The Industry Benchmarks feature compares your time management practices against established industry standards and best practices:

#### Accessing Benchmarks

1. Navigate to **Benchmarks** from the sidebar
2. View your overall performance score (0-100)
3. Review detailed comparisons across 5 categories
4. Select analysis period: 7 Days, 30 Days, or 90 Days
5. Click **Refresh** to update data

#### Understanding Your Performance Score

Your overall score is calculated based on 9 key metrics:

**Score Ranges:**
- **85-100 (Excellent)**: Outstanding time management practices
- **70-84 (Good)**: Solid performance with minor improvements possible
- **60-69 (Fair)**: Acceptable but needs attention in some areas
- **0-59 (Needs Improvement)**: Significant changes recommended

#### Benchmark Categories

**1. Meetings (3 Metrics)**

- **Meeting Time Percentage**
  - Industry Standard: 35% of work time
  - Maximum Healthy: 50%
  - Too many meetings reduce productivity and focus time
  - Too few may indicate lack of collaboration

- **Average Meeting Duration**
  - Optimal: 30 minutes
  - Maximum Effective: 60 minutes
  - Longer meetings tend to lose engagement
  - Consider breaking long meetings into focused sessions

- **Average Meeting Size**
  - Optimal: 7 attendees
  - Maximum Productive: 12 attendees
  - Smaller groups make better decisions
  - Large meetings reduce individual contribution

**2. Focus Time (2 Metrics)**

- **Focus Time Percentage**
  - Minimum: 20% of work time
  - Optimal: 40% of work time
  - Critical for deep work and complex tasks
  - Protected time for concentrated effort

- **Average Focus Block Duration**
  - Minimum Effective: 90 minutes
  - Optimal: 120 minutes (2 hours)
  - Brain needs time to enter deep work state
  - Short blocks don't allow for meaningful progress

**3. Work Hours (2 Metrics)**

- **Weekly Work Hours**
  - Standard: 40 hours per week
  - Maximum Healthy: 50 hours per week
  - Excessive hours increase burnout risk
  - Diminishing returns beyond 50 hours

- **Daily Work Hours**
  - Minimum Full-Time: 6 hours per day
  - Maximum Productive: 10 hours per day
  - Very long days reduce next-day performance
  - Consistent moderate days beat sporadic marathons

**4. Task Management (1 Metric)**

- **Task Completion Rate**
  - Optimal: 80% completion
  - Healthy commitment vs. completion balance
  - Lower rates suggest over-commitment
  - Higher rates indicate good task sizing

**5. Schedule Quality (1 Metric)**

- **Schedule Fragmentation**
  - Target: ≤3.0 fragmentation score
  - Measures context switching frequency
  - High fragmentation reduces efficiency
  - Add buffers between different activity types

#### Status Indicators

Each metric shows a color-coded status:

- 🟢 **Excellent**: Meeting or exceeding industry standards
- 🔵 **At Standard**: Within acceptable range for the metric
- 🟠 **Near Standard**: Close to target but needs attention
- 🔴 **Below Standard**: Requires immediate improvement

#### Using Recommendations

**Top 3 Recommendations**
- Highlighted in yellow box at the top
- Focus on worst-performing metrics first
- Actionable advice for each metric
- Prioritized by impact potential

**Metric-Specific Recommendations**
- Each metric includes contextual advice
- Consider your role and responsibilities
- Not all standards apply equally to all jobs
- Use as guidelines, not strict rules

**Example Recommendations:**
- "Consider declining unnecessary meetings or consolidating similar meetings"
- "Protect more time for deep work. Block 2-4 hour chunks for focused tasks"
- "You're working excessive hours, which increases burnout risk"
- "Low completion rate suggests overcommitment. Reduce concurrent tasks"
- "High fragmentation indicates too many context switches. Batch similar activities"

#### How to Use Benchmarks Effectively

1. **Establish Baseline**
   - Run initial 30-day benchmark
   - Identify your weakest areas
   - Note your overall score

2. **Set Priorities**
   - Focus on Red (Below Standard) metrics first
   - Pick 1-2 metrics to improve
   - Don't try to fix everything at once

3. **Implement Changes**
   - Follow specific recommendations
   - Make incremental adjustments
   - Track changes over time

4. **Monitor Progress**
   - Re-run benchmarks weekly or monthly
   - Watch for score improvements
   - Celebrate metrics moving to green

5. **Maintain Standards**
   - Once at standard, maintain practices
   - Don't let metrics slip
   - Build sustainable habits

#### Context Matters

**Consider Your Role:**
- **Managers**: Naturally have more meetings (40-50%)
- **Individual Contributors**: Need more focus time (50%+)
- **Executives**: Higher meeting percentages are normal
- **Developers**: Require longer focus blocks (2-4 hours)

**Industry Variations:**
- Sales roles: More meetings expected
- Research roles: More focus time required
- Support roles: Higher fragmentation acceptable
- Creative roles: Longer focus blocks essential

**Company Culture:**
- Startup pace may differ from enterprise
- Remote work affects meeting patterns
- Company size impacts collaboration needs
- Team dynamics influence optimal metrics

#### Best Practices

- **Regular Review**: Check benchmarks monthly
- **Trend Analysis**: Look for patterns over time
- **Team Discussion**: Share insights with manager
- **Goal Setting**: Set realistic improvement targets
- **Habit Formation**: Implement changes gradually
- **Balance**: Don't optimize one metric at expense of others
- **Flexibility**: Adjust for busy periods or special projects

The benchmarking feature helps you make data-driven decisions about your time management and identify opportunities for productivity improvements based on proven industry standards.

### Using Calendar Analytics

The Analytics dashboard provides comprehensive insights into your time usage and scheduling patterns:

1. Navigate to **Analytics** from the sidebar
2. Select your analysis period:
   - **Last 7 Days**: Quick weekly review
   - **Last 30 Days**: Monthly patterns and trends
   - **Last 90 Days**: Quarterly analysis
   - **Last Year**: Annual overview

**Key Metrics Displayed:**
- **Quick Stats**: Total events, meetings, scheduled hours, and meeting costs at a glance
- **Calendar Health Score**: A 0-100 score indicating schedule quality with color-coded gauge
  - Green (80-100): Excellent schedule balance
  - Blue (60-79): Good schedule with minor improvements needed
  - Pink (40-59): Fair schedule needing attention
  - Red (0-39): Needs significant improvement
- **Time Usage Breakdown**: See how your time is distributed across event types
- **Meeting Analytics**: Total meetings, average duration, top participants, and cost analysis
- **Productivity Insights**: Focus time blocks, task completion rate, and schedule fragmentation
- **AI-Powered Recommendations**: Smart suggestions for optimizing your schedule
- **Warnings**: Proactive alerts for potential burnout or scheduling issues

**Use Analytics To:**
- Identify time-wasting activities and inefficient meetings
- Track meeting costs and ROI
- Find optimal times for deep work
- Monitor work-life balance
- Prevent schedule overload and burnout
- Make data-driven decisions about your calendar

### Importing ICS Files

1. Navigate to "Import ICS" from the sidebar
2. Click the upload area or drag and drop an ICS file
3. Review the parsed events in the preview
4. Click "Save All Events" to import them into Tempus
5. Events will be integrated with your existing calendar

### Managing Contacts

1. Navigate to **Address Book**
2. Add contacts with name, email, phone, and company details
3. Use contacts when adding attendees to events
4. Search and filter contacts easily

### Using Notifications

Tempus features a comprehensive notification system to keep you informed about upcoming events:

1. **Enable Browser Notifications**:
   - Navigate to **Settings** → **Notifications** tab
   - Enable "Browser Notifications"
   - Grant permission when prompted by your browser
   - Configure default reminder times (e.g., 15 minutes before)

2. **Email Notifications**:
   - Enable "Email Notifications" in Settings
   - Receive emails when meetings are created, updated, or cancelled
   - Emails include full event details and calendar context

3. **Per-Event Notification Settings**:
   - When creating or editing an event, customize notification preferences
   - Choose specific reminder times (5, 10, 15, 30 minutes, 1 hour, etc.)
   - Mix email and browser notifications based on importance

4. **Browser Notification Features**:
   - Real-time desktop alerts for upcoming events
   - Works even when the browser is in the background
   - Click notification to focus on Tempus
   - Fallback to email if browser notifications are blocked

**Supported Browsers:**
- Google Chrome / Chromium-based browsers
- Mozilla Firefox
- Microsoft Edge
- Safari (macOS/iOS)

**Note:** Browser notifications require HTTPS and user permission. If you decline permission, you can re-enable it in your browser settings.

### Authentication

New users can register for an account:
1. Click **Register** in the navigation
2. Provide email and password
3. Complete registration
4. Log in to access your personalized calendar

### GDPR & Privacy

Tempus includes comprehensive privacy controls:
- **Privacy Policy**: View data handling practices
- **Terms of Service**: Understand usage terms
- **Security**: Information about data protection
- **GDPR Compliance**: Data rights and controls

## Development

### Adding New Features

The application follows clean architecture principles:

1. **Domain Models**: Add to `Tempus.Core/Models`
2. **Business Logic**: Add interfaces to `Tempus.Core/Interfaces`
3. **Data Access**: Implement in `Tempus.Infrastructure/Repositories`
4. **UI**: Add Razor components to `Tempus.Web/Components/Pages`

### Database Migrations

When modifying models:

```bash
cd Tempus.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../Tempus.Web
dotnet ef database update --startup-project ../Tempus.Web
```

## Troubleshooting

### Port Already in Use

If ports 5000 or 7001 are in use, you can change them in `Tempus.Web/Properties/launchSettings.json`

### Database Issues

**SQLite database locked or corrupted:**
1. Stop the application (Ctrl + C)
2. Navigate to `Tempus.Web` directory
3. Delete `tempus.db` file
4. Restart the application (database will be recreated automatically)

**SQL Server connection issues (if using SQL Server):**
- Verify the connection string in `appsettings.json`
- Ensure SQL Server is running and accessible
- Check firewall settings allow connections
- Verify credentials and database permissions

### Package Restore Issues

```bash
dotnet nuget locals all --clear
dotnet restore
```

## Recent Improvements

### Version 1.4 - Calendar View Preferences & Saved Layouts
- ✅ **Remember Last View**: Automatically save and restore user's preferred calendar view
  - RememberLastView setting to enable/disable automatic view restoration
  - LastUsedView tracks the most recently selected view
  - Seamless experience across sessions
- ✅ **Expanded Calendar View Support**: Complete support for all 6 calendar views
  - Month, Week, Day views
  - Year, Year Planner, Year Timeline views
  - Updated CalendarView enum with explicit indices
  - Proper mapping between settings and scheduler views
- ✅ **Layout Customization Options**: Foundation for personalized calendar display
  - Customizable hour range (CalendarStartHour, CalendarEndHour)
  - Event display filters (HiddenEventTypes, ShowCompletedTasks, ShowCancelledEvents)
  - Visual customization (ShowEventIcons, ShowEventColors, CompactView)
  - Per-user preferences stored in database
- ✅ **Automatic Preference Persistence**: No manual saving required
  - View changes automatically saved to user settings
  - LastViewChangeDate timestamp for tracking
  - Integrated with existing SettingsService
- ✅ **Enhanced User Experience**: Personalized calendar interface
  - Calendar opens to user's preferred view
  - Consistent experience across sessions
  - Foundation for advanced filtering and display options
  - Better workflow for power users

### Version 1.3 - Notifications & Time Zone Support
- ✅ **Time Zone Support for Multi-Location Meetings**: Complete timezone management system
  - Selectable timezone for each event with full IANA timezone database support
  - Automatic timezone conversion to user's local timezone in calendar and dashboard
  - Visual timezone indicators (🌍) with abbreviation when event is in different timezone
  - Common timezone quick-select dropdown with searchable full timezone list
  - Original event timezone preservation for accurate multi-location coordination
  - Timezone conversion service with proper daylight saving time handling
  - Reset button to quickly revert to user's default timezone
  - Timezone display in event tooltips showing both original and converted times
- ✅ **Browser Push Notifications**: Real-time desktop alerts for upcoming events and reminders
  - Native browser notification API integration
  - Support for Chrome, Firefox, Edge, and Safari
  - Permission management with fallback handling
  - Click notifications to focus on Tempus application
  - Background notification delivery
- ✅ **Enhanced Email Notifications**: Improved email system for meeting updates
  - Created, updated, and cancelled meeting alerts
  - Full event details in email body
  - Rich HTML formatting with calendar context
- ✅ **Notification Preferences**: Granular control over notification settings
  - Global enable/disable for email and browser notifications
  - Per-event notification customization
  - Configurable reminder times (5, 10, 15, 30 min, 1 hour, etc.)
  - Default notification settings in Settings page
- ✅ **Real-time Notification Service**: Background service for notification delivery
  - Automatic detection of upcoming events
  - Smart scheduling of notifications based on event start time
  - Handles notification permissions and browser compatibility
  - Graceful fallback when notifications are unavailable

### Version 1.2 - Enhanced UI/UX & Theme Support
- ✅ **Full Theme Support**: All pages now support multiple Radzen themes (light and dark modes)
- ✅ **Calendar Enhancements**: Full viewport layout, drag-and-drop rescheduling, recurring event support
- ✅ **Quick Time Block Templates**: Pre-configured templates for Deep Work, Meetings, Focus Blocks, and Breaks
- ✅ **PST File Import**: Import calendar events from Microsoft Outlook PST (Personal Storage Table) files
  - Automatic detection of calendar folders within PST files
  - Extraction of appointments, meetings, and events with all metadata
  - Attendee and organizer information preservation
  - Support for event location, description, and time details
  - Unified import interface supporting both ICS and PST formats
  - Large file support (up to 100MB for PST files)
- ✅ **Advanced Calendar Analytics Dashboard**: Comprehensive analytics and insights feature
  - Calendar health score (0-100) with visual gauge and color-coded status
  - Time usage breakdown by event type, day of week, and hour of day
  - Meeting analytics with cost tracking, top participants, and costly meeting identification
  - Productivity metrics including focus time blocks, task completion rate, and schedule fragmentation
  - AI-powered recommendations for schedule optimization
  - Warning system for burnout prevention and overbooked schedules
  - Flexible date range analysis (7, 30, 90, 365 days)
- ✅ **Predictive Analytics & Trend Forecasting**: Machine learning-powered predictions
  - Historical trend analysis using linear regression algorithms
  - Future predictions for events, meeting costs, and workload
  - Pattern detection for busiest days, peak hours, and meeting habits
  - 4-week workload forecast with intelligent capacity planning
  - Trend indicators (increasing, decreasing, stable, volatile patterns)
  - Confidence levels and actionable recommendations
- ✅ **Analytics Report Export**: Download reports in multiple formats (PDF, CSV, Excel)
- ✅ **Advanced Analytics Visualizations**: Interactive charts, heatmaps, and graphs
  - Donut chart for time distribution by event type with center summary
  - Column chart showing daily activity patterns across the week
  - Hour-by-hour heatmap with color intensity for peak activity hours
  - Bar chart analyzing top cost contributors
  - Pie chart displaying meeting frequency breakdown by type
  - Smooth line charts for historical trend visualization
  - Area chart with dual-axis for workload forecast (events and hours)
  - Interactive tooltips with detailed metrics on hover
  - Responsive design with automatic chart resizing
  - Color-coded trend indicators for quick insights
- ✅ **Settings Consolidation**: Added Integrations tab to Settings page, removed from sidebar
- ✅ **Theme-Aware Components**: Settings, Profile, Calendar, Dashboard, and Analytics pages fully support themes
- ✅ **Profile Page Enhancement**: Improved avatar display with scoped CSS to prevent menubar conflicts
- ✅ **Daily Agenda PDF**: Export daily schedule as professionally formatted PDF
- ✅ **Enhanced Calendar Views**: Added Year, Planner, Timeline, and Grid views
- ✅ **Automatic Work Hours Scroll**: Calendar automatically scrolls to configured work hours
- ✅ **Improved Navigation**: Streamlined sidebar with Settings in user menu, Analytics added to main navigation

### Version 1.1 - Meeting Cost Tracking & Bug Fixes
- ✅ **Meeting Cost Calculator**: Calculate and track meeting expenses based on attendee count and hourly rates
- ✅ **Organizer Management**: Automatic organizer designation with protection from removal
- ✅ **Auto-Contact Creation**: New attendees are automatically saved to address book
- ✅ **Entity Framework Fix**: Resolved critical concurrency error when adding attendees to existing meetings
- ✅ **Enhanced Error Reporting**: Better user notifications for errors and validation issues
- ✅ **User Avatar Menu**: Profile management with avatar display
- ✅ **Test Coverage**: Added comprehensive tests for event repository operations (12 tests passing)

## Contributing

Contributions are welcome! Areas for improvement:

- **Calendar Integrations**: Implement OAuth2 flows for Google, Outlook, Apple Calendar
- **Analytics & Insights**: Meeting cost analytics, productivity metrics, and advanced reporting
- **Mobile Development**: .NET MAUI mobile application
- **Performance**: Optimize calendar rendering for large event sets
- **UI/UX**: Additional themes, animations, and accessibility improvements
- **Testing**: Expand unit and integration test coverage
- **Localization**: Multi-language support (i18n)
- **Features**: Smart scheduling, time blocking visualizations, AI-powered suggestions

## License

This project is open source and available under the MIT License.

## Support

For issues, questions, or suggestions, please create an issue in the repository.

---

**Tempus** - Master your time, one event at a time.
