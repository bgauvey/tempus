# Tempus - Time Management Application

Tempus is a comprehensive time management application built with .NET 9 and Blazor. It goes beyond basic calendar functionality to provide intelligent scheduling, ICS file import/export, and integration capabilities with popular calendar services.

## Features

- **Event Management**: Create, edit, and delete meetings, appointments, tasks, and time blocks
- **Calendar View**: Visual calendar interface with monthly view and event overview
- **ICS Import/Export**: Import events from ICS files (compatible with Google Calendar, Outlook, Apple Calendar)
- **Dashboard**: Overview of upcoming events, tasks, and statistics
- **Event Types**: Support for different event types (Meeting, Appointment, Task, Time Block, Reminder, Deadline)
- **Priority System**: Assign priorities to events (Low, Medium, High, Urgent)
- **Search**: Quick search functionality across all events
- **Attendee Management**: Add and track attendees for events
- **Recurring Events**: Support for recurring event patterns

## Technology Stack

- **.NET 9**: Latest version of the .NET framework
- **Blazor Server**: Interactive web UI framework
- **Entity Framework Core 9**: ORM for database access
- **SQLite**: Lightweight database (easily switchable to SQL Server or PostgreSQL)
- **MudBlazor**: Material Design component library
- **Ical.Net**: iCalendar format parsing and generation

## Project Structure

```
Tempus/
├── Tempus.Core/                    # Domain models and interfaces
│   ├── Models/                     # Event, Attendee, CalendarIntegration
│   ├── Enums/                      # EventType, Priority
│   └── Interfaces/                 # Repository and service interfaces
├── Tempus.Infrastructure/          # Data access and external services
│   ├── Data/                       # DbContext
│   ├── Repositories/               # Repository implementations
│   └── Services/                   # ICS import/export service
└── Tempus.Web/                     # Blazor web application
    ├── Components/
    │   ├── Layout/                 # MainLayout
    │   └── Pages/                  # Razor pages/components
    └── wwwroot/                    # Static files
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Visual Studio 2022 (17.8+) or Visual Studio Code with C# extension
- (Optional) SQL Server or PostgreSQL if not using SQLite

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

By default, Tempus uses SQLite with a database file named `tempus.db` in the application directory. The database is created automatically on first run.

### Switching to SQL Server

1. Install the SQL Server package:
```bash
cd Tempus.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

2. Update `Tempus.Web/Program.cs`:
```csharp
builder.Services.AddDbContext<TempusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

3. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TempusDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

4. Create and apply migrations:
```bash
cd Tempus.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Tempus.Web
dotnet ef database update --startup-project ../Tempus.Web
```

## Features Roadmap

### Current Features
- ✅ Event CRUD operations
- ✅ Calendar view (monthly)
- ✅ ICS file import
- ✅ ICS file export
- ✅ Event search
- ✅ Dashboard with statistics
- ✅ Multiple event types and priorities

### Planned Features
- 🔄 Google Calendar integration (OAuth2)
- 🔄 Microsoft Outlook integration
- 🔄 Apple Calendar (CalDAV) integration
- 🔄 Weekly and daily calendar views
- 🔄 Time blocking visualization
- 🔄 Meeting cost calculator
- 🔄 Smart scheduling suggestions
- 🔄 Event reminders and notifications
- 🔄 Calendar analytics and insights
- 🔄 Export to multiple formats (PDF, Excel)
- 🔄 Mobile responsive design improvements
- 🔄 Dark mode support
- 🔄 Multi-language support

## Using the Application

### Creating Events

1. Navigate to the Dashboard or Events page
2. Click "Create New Event" or "Create Event"
3. Fill in event details (title, description, start/end time, location, etc.)
4. Select event type and priority
5. Add attendees if needed
6. Save the event

### Importing ICS Files

1. Navigate to "Import ICS" from the sidebar
2. Click the upload area or drag and drop an ICS file
3. Review the parsed events
4. Click "Save All Events" to import them into Tempus

### Exporting Events

Currently, the export functionality is available programmatically through the `IIcsImportService`. A UI for exporting will be added in a future update.

### Calendar Integration Setup

Calendar integrations (Google, Outlook, Apple) will require OAuth2 authentication setup. This feature is planned for future releases.

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

Delete the `tempus.db` file and restart the application to recreate the database.

### Package Restore Issues

```bash
dotnet nuget locals all --clear
dotnet restore
```

## Contributing

Contributions are welcome! Areas for improvement:

- Calendar integration implementations
- Additional calendar views (week, day, agenda)
- Mobile app development
- Performance optimizations
- UI/UX enhancements
- Test coverage

## License

This project is open source and available under the MIT License.

## Support

For issues, questions, or suggestions, please create an issue in the repository.

---

**Tempus** - Master your time, one event at a time.
