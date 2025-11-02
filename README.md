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
- **Ical.Net**: iCalendar format parsing and generation (v4.2.0)
- **Aspose.Email**: PST/OST file parsing for Outlook integration (v24.12.0)
- **QuestPDF**: PDF generation capabilities (v2025.7.3)
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

### Planned Features
- 🔄 Google Calendar integration (OAuth2 sync)
- 🔄 Microsoft Outlook integration
- 🔄 Apple Calendar (CalDAV) integration
- 🔄 AI-powered smart scheduling suggestions
- 🔄 Benchmarking against industry standards and best practices
- 🔄 Team and organizational analytics
- 🔄 Additional custom themes and theme editor
- 🔄 Multi-language support (i18n)
- 🔄 Mobile native app (MAUI)
- 🔄 Team collaboration features
- 🔄 Calendar sharing and permissions
- 🔄 Calendar view preferences and saved layouts
- 🔄 Bulk event operations
- 🔄 Advanced search and filtering

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
