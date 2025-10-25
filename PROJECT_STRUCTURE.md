# Tempus Project Structure

This document explains the architecture and organization of the Tempus application.

## Architecture Overview

Tempus follows **Clean Architecture** principles with three main layers plus testing:

```
┌─────────────────────────────────────────┐
│    Tempus.Web (Blazor Server)           │
│         Presentation Layer              │
│   • Radzen UI Components                │
│   • Authentication & Authorization      │
│   • Application Services                │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      Tempus.Infrastructure              │
│   Data Access & External Services       │
│   • EF Core + SQLite/SQL Server         │
│   • ASP.NET Core Identity               │
│   • ICS Import/Export                   │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│          Tempus.Core                    │
│       Domain Models & Interfaces        │
│   • Domain Entities (Event, Contact)   │
│   • Enums & Value Objects               │
│   • Repository Interfaces               │
└─────────────────────────────────────────┘
                    ↑
┌─────────────────────────────────────────┐
│      Tempus.Tests + Integration         │
│           Testing Layer                 │
└─────────────────────────────────────────┘
```

## Project Details

### 1. Tempus.Core (Domain Layer)

**Purpose**: Contains the core business logic, domain models, and interfaces. Has no dependencies on other projects.

```
Tempus.Core/
├── Models/
│   ├── Event.cs               # Main event/calendar item entity
│   ├── Attendee.cs            # Event attendee
│   ├── CalendarIntegration.cs # External calendar config
│   ├── CalendarSettings.cs    # User calendar preferences
│   ├── Contact.cs             # Address book contact
│   ├── CustomCalendarRange.cs # Custom date ranges
│   └── ApplicationUser.cs     # Extended identity user
├── Enums/
│   ├── EventType.cs           # Meeting, Task, TimeBlock, etc.
│   ├── Priority.cs            # Low, Medium, High, Urgent
│   ├── CalendarView.cs        # Month, Week, WorkWeek, Day, Agenda
│   ├── TimeFormat.cs          # 12-hour, 24-hour
│   ├── DateFormat.cs          # Various date format options
│   ├── TimeSlotDuration.cs    # 15, 30, 60 minutes
│   ├── EventVisibility.cs     # Event visibility settings
│   ├── RecurrencePattern.cs   # Daily, Weekly, Monthly, Yearly
│   └── RecurrenceEndType.cs   # Never, AfterOccurrences, OnDate
├── Interfaces/
│   ├── IEventRepository.cs         # Event data access
│   ├── IContactRepository.cs       # Contact data access
│   ├── ISettingsRepository.cs      # Settings data access
│   ├── ICustomCalendarRangeRepository.cs
│   └── IIcsImportService.cs        # ICS import/export
└── Helpers/
    └── [Utility classes]
```

**Key Models**:

- **Event**: Core entity representing any calendar event
  - Properties: Id, Title, Description, StartTime, EndTime, Location, EventType, Priority, IsAllDay, IsRecurring
  - Recurrence: RecurrencePattern, RecurrenceInterval, RecurrenceEndType, RecurrenceEndDate
  - Navigation: Attendees collection
  - Relationships: Belongs to ApplicationUser

- **CalendarSettings**: User-specific calendar configuration
  - Properties: TimeFormat, DateFormat, DefaultView, WorkStartHour, WorkEndHour
  - TimeSlotDuration, ShowWeekends, EventVisibility
  - Per-user customization of calendar behavior

- **Contact**: Address book contact
  - Properties: Name, Email, Phone, Company, Address
  - Used for event attendee management

- **CustomCalendarRange**: User-defined date ranges
  - Properties: Name, StartDate, EndDate, Color
  - For specialized scheduling needs

- **Attendee**: Represents people attending events
  - Properties: Id, Name, Email, IsOrganizer, Status

- **CalendarIntegration**: Configuration for external calendar connections
  - Properties: Provider, AccessToken, RefreshToken, TokenExpiry
  - For future OAuth2 integrations

- **ApplicationUser**: Extended ASP.NET Identity user
  - Inherits from IdentityUser
  - Navigation: CalendarSettings, Events, Contacts

### 2. Tempus.Infrastructure (Data Access Layer)

**Purpose**: Implements data access, external services, and infrastructure concerns.

```
Tempus.Infrastructure/
├── Data/
│   └── TempusDbContext.cs          # EF Core DbContext with Identity
├── Repositories/
│   ├── EventRepository.cs          # Event data access
│   ├── ContactRepository.cs        # Contact data access
│   ├── SettingsRepository.cs       # Settings data access
│   └── CustomCalendarRangeRepository.cs
├── Services/
│   ├── IcsImportService.cs         # ICS import/export
│   └── SettingsService.cs          # Settings management
└── Migrations/
    └── [EF Core migrations]        # Database schema versions
```

**Key Components**:

- **TempusDbContext**: Entity Framework Core database context
  - Inherits from `IdentityDbContext<ApplicationUser>` for authentication
  - Configures entity mappings and relationships
  - Manages database connections
  - **Default**: SQLite (tempus.db) for portability
  - **Alternative**: SQL Server (easily switchable)
  - DbSets: Events, Contacts, CalendarSettings, CustomCalendarRanges, CalendarIntegrations

- **EventRepository**: Implements IEventRepository
  - Full CRUD operations for events
  - Query methods: GetByDateRange, Search, GetUpcoming
  - Includes related entities (Attendees)
  - Supports filtering by user, event type, priority

- **ContactRepository**: Implements IContactRepository
  - Contact management for address book
  - Search and filter capabilities
  - Integration with event attendees

- **SettingsRepository**: Implements ISettingsRepository
  - Per-user calendar settings persistence
  - Default settings creation
  - Settings validation

- **CustomCalendarRangeRepository**: Implements ICustomCalendarRangeRepository
  - User-defined date range management
  - CRUD operations for custom ranges

- **IcsImportService**: Handles ICS file operations
  - Imports ICS files using Ical.Net library
  - Exports events to ICS format
  - Converts between iCalendar format and domain models
  - Supports recurring events and attendees

- **SettingsService**: Business logic for settings
  - Settings retrieval and updates
  - Default value management
  - Validation logic

**Dependencies**:
- Microsoft.EntityFrameworkCore (9.0.10)
- Microsoft.EntityFrameworkCore.Sqlite (9.0.10)
- Microsoft.EntityFrameworkCore.SqlServer (9.0.10)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.10)
- Ical.Net (4.2.0)

### 3. Tempus.Web (Presentation Layer)

**Purpose**: Blazor Server web application providing the award-winning user interface.

```
Tempus.Web/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor       # Main app layout with sidebar
│   │   ├── NavMenu.razor          # Navigation menu
│   │   └── LoginDisplay.razor     # User login status
│   ├── Pages/
│   │   ├── Home.razor             # Landing page with features
│   │   ├── Dashboard.razor        # User dashboard with analytics
│   │   ├── Calendar.razor         # Advanced calendar (Month/Week/Day/Agenda)
│   │   ├── Settings.razor         # Calendar settings configuration
│   │   ├── AddressBook.razor      # Contact management
│   │   ├── Addresses.razor        # Contact list view
│   │   ├── Import.razor           # ICS file import
│   │   ├── Integrations.razor     # External calendar setup
│   │   ├── Login.razor            # User login
│   │   ├── Register.razor         # User registration
│   │   ├── About.razor            # About page
│   │   ├── Features.razor         # Features showcase
│   │   ├── Contact.razor          # Contact form
│   │   ├── Privacy.razor          # Privacy policy
│   │   ├── TermsOfService.razor   # Terms of service
│   │   ├── Gdpr.razor             # GDPR compliance
│   │   └── Security.razor         # Security information
│   ├── App.razor                  # Root component
│   ├── Routes.razor               # Routing configuration
│   └── _Imports.razor             # Global using statements
├── Services/
│   └── [Application services]     # Business logic services
├── wwwroot/
│   ├── css/
│   │   └── app.css                # Custom styles and animations
│   ├── js/
│   │   └── [JavaScript interop]
│   ├── favicon.svg/.png           # App icons
│   └── site.webmanifest           # PWA manifest
├── Properties/
│   └── launchSettings.json        # Development settings
├── Program.cs                     # Application startup & DI
└── appsettings.json               # Configuration
```

**Key Pages**:

1. **Home.razor** (Landing Page)
   - Award-winning UI with gradient designs
   - Feature highlights and marketing content
   - Call-to-action buttons
   - Redirects authenticated users to Dashboard

2. **Dashboard.razor** (Command Center)
   - Real-time statistics (today's events, upcoming, total)
   - Event analytics and insights
   - Upcoming events list
   - Quick action buttons
   - Calendar preview

3. **Calendar.razor** (Advanced Calendar)
   - Multiple views: Month, Week, Work Week, Day, Agenda
   - Radzen Scheduler component integration
   - Inline event creation and editing
   - Drag-and-drop event rescheduling
   - Real-time settings integration
   - Auto-scroll to work hours
   - Event color coding by type/priority

4. **Settings.razor** (Calendar Configuration)
   - Time format selection (12/24-hour)
   - Date format options
   - Work hours configuration
   - Time slot duration (15/30/60 minutes)
   - Default calendar view
   - Event visibility controls
   - Weekend display toggle
   - Real-time preview updates

5. **AddressBook.razor** (Contact Management)
   - Contact CRUD operations
   - Search and filter functionality
   - Integration with event attendees
   - Data grid with sorting

6. **Import.razor** (ICS Import)
   - File upload interface (drag & drop)
   - ICS parsing and validation
   - Event preview before import
   - Batch event import
   - Error handling and feedback

7. **Authentication Pages**
   - **Login.razor**: User authentication
   - **Register.razor**: New user registration
   - ASP.NET Core Identity integration

8. **Legal & Compliance**
   - **Privacy.razor**: Privacy policy
   - **TermsOfService.razor**: Terms and conditions
   - **Gdpr.razor**: GDPR data rights
   - **Security.razor**: Security practices

9. **Marketing Pages**
   - **About.razor**: About Tempus
   - **Features.razor**: Feature showcase
   - **Contact.razor**: Contact form

**UI Features**:
- **Radzen Blazor Components**: Modern, professional UI components
- **Gradient Design**: Purple, blue, green, pink color schemes
- **Smooth Animations**: Transitions and glass morphism effects
- **Responsive Layout**: Mobile-first design
- **Sidebar Navigation**: Easy access to all features
- **Toast Notifications**: User feedback on actions
- **Modal Dialogs**: Event creation and editing

**Dependencies**:
- Radzen.Blazor (5.9.0) - Modern UI component library
- QuestPDF (2025.7.3) - PDF generation
- Microsoft.EntityFrameworkCore.Design (9.0.0)
- References Core and Infrastructure projects

## Data Flow

### Creating an Event

```
User Interface (Events.razor)
    ↓
IEventRepository.CreateAsync(event)
    ↓
EventRepository.CreateAsync() 
    ↓
TempusDbContext.Events.Add()
    ↓
SaveChangesAsync()
    ↓
SQL Server Database
```

### Importing ICS File

```
User Upload (Import.razor)
    ↓
IIcsImportService.ImportFromStreamAsync(stream)
    ↓
IcsImportService parses using Ical.Net
    ↓
Converts CalendarEvent → Event models
    ↓
Returns List<Event> for preview
    ↓
User confirms import
    ↓
IEventRepository.CreateAsync() for each event
    ↓
Database
```

## Configuration

### Database Connection

**Default (SQLite)**: Configured in `Program.cs`:
```csharp
builder.Services.AddDbContext<TempusDbContext>(options =>
    options.UseSqlite("Data Source=tempus.db"));
```

Database file is created automatically in the `Tempus.Web` directory.

**Alternative (SQL Server)**: Update `appsettings.json`:
```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=TempusDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
  }
}
```

Then update `Program.cs`:
```csharp
builder.Services.AddDbContext<TempusDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### Dependency Injection

Services registered in `Program.cs`:
```csharp
// Database Context
builder.Services.AddDbContext<TempusDbContext>(options =>
    options.UseSqlite("Data Source=tempus.db"));

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<TempusDbContext>()
.AddDefaultTokenProviders();

// Repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ICustomCalendarRangeRepository, CustomCalendarRangeRepository>();

// Services
builder.Services.AddScoped<IIcsImportService, IcsImportService>();
builder.Services.AddScoped<SettingsService>();

// UI Services
builder.Services.AddRadzenComponents();

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
```

## Extension Points

### Adding New Event Types

1. Add to `Tempus.Core/Enums/EventType.cs`
2. Update color mapping in `Events.razor` GetEventTypeColor()
3. Update UI dropdowns/selectors

### Adding New Calendar Integration

1. Create interface in `Tempus.Core/Interfaces/`
2. Implement service in `Tempus.Infrastructure/Services/`
3. Add OAuth configuration to `appsettings.json`
4. Create UI in `Integrations.razor`
5. Register service in `Program.cs`

Example structure:
```csharp
// Core Interface
public interface IGoogleCalendarService
{
    Task<List<Event>> SyncEventsAsync();
    Task<bool> AuthenticateAsync(string authCode);
}

// Infrastructure Implementation
public class GoogleCalendarService : IGoogleCalendarService
{
    // Implementation using Google.Apis.Calendar
}
```

### Adding New Views

1. Create Razor component in `Components/Pages/`
2. Add `@page "/route"` directive
3. Add navigation link to `MainLayout.razor` or `NavMenu.razor`
4. Inject required services (`@inject IEventRepository EventRepo`)
5. Implement UI using Radzen Blazor components
6. Add authentication if needed (`@attribute [Authorize]`)

### Database Schema Changes

1. Modify models in `Tempus.Core/Models/`
2. Update DbContext configuration if needed
3. Create migration:
   ```bash
   dotnet ef migrations add MigrationName --project Tempus.Infrastructure --startup-project Tempus.Web
   ```
4. Apply migration:
   ```bash
   dotnet ef database update --project Tempus.Infrastructure --startup-project Tempus.Web
   ```

## Testing Strategy

### Current Test Projects

```
src/Tempus.Tests/            # Unit tests
tests/Tempus.Web.Tests/      # Integration and UI tests
```

The project includes comprehensive test coverage using:
- **xUnit**: Test framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **bUnit**: Blazor component testing

### Recommended Test Structure

```
Tempus.Tests/
├── Models/              # Domain model tests
├── Repositories/        # Repository logic tests
├── Services/            # Service logic tests (ICS parsing, settings)
└── Helpers/             # Helper class tests

Tempus.Web.Tests/
├── Components/          # Blazor component rendering tests
├── Pages/               # Page integration tests
└── Services/            # Application service tests
```

### Example Test Categories

1. **Model Tests**: Validation, business rules, relationships
2. **Repository Tests**: CRUD operations, queries, filtering
3. **Service Tests**: ICS import/export, settings management
4. **Component Tests**: UI rendering, user interactions, state management
5. **Integration Tests**: End-to-end workflows, authentication

### Integration Tests

Test database operations with in-memory database:
```csharp
var options = new DbContextOptionsBuilder<TempusDbContext>()
    .UseInMemoryDatabase(databaseName: "TestDb")
    .Options;
```

## Performance Considerations

### Database Optimization

- Indexes on frequently queried columns (StartTime, EventType)
- Eager loading with `.Include()` for related entities
- Pagination for large event lists

### Blazor Performance

- Use `@key` directive in loops
- Implement virtualization for long lists
- Minimize re-renders with `ShouldRender()`

## Security Considerations

### Future OAuth Implementation

- Store tokens encrypted
- Use HTTPS only for OAuth callbacks
- Implement token refresh logic
- Never log sensitive tokens

### Data Protection

- Validate all user inputs
- Sanitize event descriptions (XSS prevention)
- Implement proper authentication/authorization
- Use parameterized queries (EF Core handles this)

## Roadmap Implications

### Planned Features Impact

1. **Google Calendar Integration**
   - Add `Google.Apis.Calendar` NuGet package to Infrastructure
   - Implement `GoogleCalendarService` with OAuth2 flow
   - Add background sync job using `Hangfire` or `Quartz.NET`
   - Store OAuth tokens in CalendarIntegration table
   - UI updates in Integrations.razor

2. **Advanced Analytics Dashboard**
   - New analytics repository methods for aggregations
   - Chart.js or Radzen Charts for visualizations
   - Time tracking metrics (meetings vs. focus time)
   - Productivity insights and trends
   - Export analytics to PDF using QuestPDF

3. **.NET MAUI Mobile App**
   - Create new `Tempus.Mobile` project
   - Reuse Core and Infrastructure layers (no changes needed)
   - Implement platform-specific UI for iOS/Android
   - Shared business logic through repositories
   - Consider Azure Mobile Services for sync

4. **Real-time Collaboration**
   - Add SignalR hubs for real-time updates
   - Shared calendar functionality
   - Live event editing notifications
   - Presence indicators

5. **Dark Mode Theme**
   - CSS variable-based theming in app.css
   - Radzen theme customization
   - User preference storage in CalendarSettings
   - Theme toggle component

## Development Workflow

1. **Make Changes**: Edit code in appropriate layer
2. **Test Locally**: Run with `dotnet run`
3. **Build**: `dotnet build` to verify compilation
4. **Database Changes**: Create migrations if models changed
5. **Review**: Check all layers affected by changes

## Additional Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Radzen Blazor Components](https://blazor.radzen.com/)
- [Radzen Scheduler](https://blazor.radzen.com/scheduler)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Ical.Net Documentation](https://github.com/rianjs/ical.net)
- [QuestPDF Documentation](https://www.questpdf.com/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)

## Summary

Tempus demonstrates a modern, production-ready architecture with:

✅ **Clean Architecture**: Clear separation of concerns across layers
✅ **Award-Winning UI**: Modern design with Radzen Blazor components
✅ **Authentication**: ASP.NET Core Identity integration
✅ **Flexible Data**: SQLite default with SQL Server support
✅ **Comprehensive Features**: Events, contacts, settings, calendar views
✅ **Testable**: Unit and integration test support
✅ **Extensible**: Easy to add new features and integrations

This structure provides a solid foundation for building a comprehensive time management application while maintaining clean separation of concerns, testability, and scalability.

---

**Last Updated**: October 2025
**Version**: 1.0
**Technology Stack**: .NET 9, Blazor Server, EF Core 9, Radzen Blazor 5.9
