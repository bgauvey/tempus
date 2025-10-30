# Tempus - Award-Winning Time Management Platform

Tempus is a comprehensive time management application built with .NET 9 and Blazor Server. It features an award-winning business interface with modern design, smooth animations, and intelligent scheduling capabilities. Beyond basic calendar functionality, Tempus provides ICS file import/export, custom calendar ranges, advanced calendar settings, and integration capabilities with popular calendar services.

## Features

### Core Features
- **Event Management**: Create, edit, and delete meetings, appointments, tasks, and time blocks
- **Advanced Calendar Views**: Multiple calendar views (Monthly, Weekly, Work Week, Daily, Agenda) with customizable settings
- **Calendar Settings**: Comprehensive configuration including time formats (12/24-hour), date formats, work hours, time slot duration, and event visibility
- **Custom Calendar Ranges**: Define and manage custom date ranges for specialized scheduling
- **ICS Import/Export**: Import events from ICS files (compatible with Google Calendar, Outlook, Apple Calendar)
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
- ✅ Email notifications for meeting updates (created, updated, cancelled)

### Planned Features
- 🔄 Google Calendar integration (OAuth2 sync)
- 🔄 Microsoft Outlook integration
- 🔄 Apple Calendar (CalDAV) integration
- 🔄 AI-powered smart scheduling suggestions
- 🔄 Push notifications and browser notifications
- 🔄 Advanced calendar analytics and insights
- 🔄 Meeting cost analytics and reports dashboard
- 🔄 Export to additional formats (Excel, CSV)
- 🔄 Additional custom themes and theme editor
- 🔄 Multi-language support (i18n)
- 🔄 Mobile native app (MAUI)
- 🔄 Team collaboration features
- 🔄 Calendar sharing and permissions
- 🔄 Time zone support for multi-location meetings
- 🔄 Calendar view preferences and saved layouts
- 🔄 Bulk event operations
- 🔄 Advanced search and filtering

## Using the Application

### Navigation

The application features a modern sidebar navigation with the following sections:
- **Home**: Landing page with feature highlights
- **Dashboard**: Your personalized command center with statistics and upcoming events
- **Calendar**: Advanced calendar with multiple view options, drag-and-drop rescheduling, and quick time block templates
- **Import ICS**: Import events from other calendar applications
- **Address Book**: Manage contacts for event attendees
- **Settings**: Configure calendar preferences, time formats, work hours, notifications, and integrations (accessible via user menu)

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

### Customizing Calendar Settings

1. Navigate to **Settings** from the user menu (top-right avatar)
2. Configure your preferences across multiple tabs:
   - **General**: Time format (12/24-hour), date format, time zone, week start day, default calendar view
   - **Work Hours**: Set working hours, lunch breaks, time slot duration, buffer times
   - **Event Defaults**: Default meeting duration, event visibility, default colors and locations
   - **Notifications**: Email and desktop notifications, default reminder times
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

### Version 1.2 - Enhanced UI/UX & Theme Support
- ✅ **Full Theme Support**: All pages now support multiple Radzen themes (light and dark modes)
- ✅ **Calendar Enhancements**: Full viewport layout, drag-and-drop rescheduling, recurring event support
- ✅ **Quick Time Block Templates**: Pre-configured templates for Deep Work, Meetings, Focus Blocks, and Breaks
- ✅ **Settings Consolidation**: Added Integrations tab to Settings page, removed from sidebar
- ✅ **Theme-Aware Components**: Settings, Profile, Calendar, and Dashboard pages fully support themes
- ✅ **Profile Page Enhancement**: Improved avatar display with scoped CSS to prevent menubar conflicts
- ✅ **Daily Agenda PDF**: Export daily schedule as professionally formatted PDF
- ✅ **Enhanced Calendar Views**: Added Year, Planner, Timeline, and Grid views
- ✅ **Automatic Work Hours Scroll**: Calendar automatically scrolls to configured work hours
- ✅ **Improved Navigation**: Streamlined sidebar with Settings in user menu

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
