# Tempus Project Structure

This document explains the architecture and organization of the Tempus application.

## Architecture Overview

Tempus follows Clean Architecture principles with three main layers:

```
┌─────────────────────────────────────────┐
│         Tempus.Web (Blazor)             │
│         Presentation Layer              │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      Tempus.Infrastructure              │
│   Data Access & External Services       │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│          Tempus.Core                    │
│       Domain Models & Interfaces        │
└─────────────────────────────────────────┘
```

## Project Details

### 1. Tempus.Core (Domain Layer)

**Purpose**: Contains the core business logic, domain models, and interfaces. Has no dependencies on other projects.

```
Tempus.Core/
├── Models/
│   ├── Event.cs              # Main event entity
│   ├── Attendee.cs           # Event attendee
│   └── CalendarIntegration.cs # External calendar config
├── Enums/
│   ├── EventType.cs          # Meeting, Task, TimeBlock, etc.
│   └── Priority.cs           # Low, Medium, High, Urgent
└── Interfaces/
    ├── IEventRepository.cs   # Event data access interface
    └── IIcsImportService.cs  # ICS import/export interface
```

**Key Models**:

- **Event**: Core entity representing any calendar event
  - Properties: Id, Title, Description, StartTime, EndTime, Location, EventType, Priority, IsAllDay, IsRecurring, etc.
  - Navigation: Attendees collection

- **Attendee**: Represents people attending events
  - Properties: Id, Name, Email, IsOrganizer, Status

- **CalendarIntegration**: Configuration for external calendar connections
  - Properties: Provider, AccessToken, RefreshToken, TokenExpiry

### 2. Tempus.Infrastructure (Data Access Layer)

**Purpose**: Implements data access, external services, and infrastructure concerns.

```
Tempus.Infrastructure/
├── Data/
│   └── TempusDbContext.cs    # EF Core DbContext
├── Repositories/
│   └── EventRepository.cs    # Event data access implementation
└── Services/
    └── IcsImportService.cs   # ICS file parsing service
```

**Key Components**:

- **TempusDbContext**: Entity Framework Core database context
  - Configures entity mappings
  - Manages database connections
  - Currently uses SQLite (easily switchable)

- **EventRepository**: Implements IEventRepository
  - CRUD operations for events
  - Query methods (GetByDateRange, Search)
  - Includes related entities (Attendees)

- **IcsImportService**: Handles ICS file operations
  - Imports ICS files using Ical.Net library
  - Exports events to ICS format
  - Converts between ICS format and domain models

**Dependencies**:
- Microsoft.EntityFrameworkCore (9.0.0)
- Microsoft.EntityFrameworkCore.Sqlite (9.0.0)
- Ical.Net (4.2.0)

### 3. Tempus.Web (Presentation Layer)

**Purpose**: Blazor Server web application providing the user interface.

```
Tempus.Web/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor  # Main application layout
│   ├── Pages/
│   │   ├── Home.razor        # Dashboard
│   │   ├── Events.razor      # Event list/management
│   │   ├── Calendar.razor    # Calendar view
│   │   ├── Import.razor      # ICS import page
│   │   └── Integrations.razor # Calendar integrations
│   ├── App.razor             # Root component
│   ├── Routes.razor          # Routing configuration
│   └── _Imports.razor        # Global using statements
├── wwwroot/
│   └── css/
│       └── app.css           # Custom styles
├── Properties/
│   └── launchSettings.json   # Development settings
├── Program.cs                # Application startup
└── appsettings.json          # Configuration
```

**Key Pages**:

1. **Home.razor** (Dashboard)
   - Shows statistics (today's events, upcoming, total)
   - Lists upcoming events
   - Quick action buttons

2. **Events.razor**
   - Table view of all events
   - Search functionality
   - Edit and delete actions
   - Color-coded event types and priorities

3. **Calendar.razor**
   - Monthly calendar grid view
   - Date navigation
   - Event details on date selection
   - Visual event indicators

4. **Import.razor**
   - File upload interface
   - ICS parsing preview
   - Batch event import

5. **Integrations.razor**
   - External calendar connection setup (planned)
   - OAuth flow handling (planned)

**Dependencies**:
- MudBlazor (7.0.0) - Material Design components
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
SQLite Database
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

Configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tempus.db"
  }
}
```

Registered in `Program.cs`:
```csharp
builder.Services.AddDbContext<TempusDbContext>(options =>
    options.UseSqlite(connectionString));
```

### Dependency Injection

Services registered in `Program.cs`:
```csharp
// Repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();

// Services
builder.Services.AddScoped<IIcsImportService, IcsImportService>();

// UI Services
builder.Services.AddMudServices();
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
3. Add navigation link to `MainLayout.razor`
4. Inject required services
5. Implement UI using MudBlazor components

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

### Unit Tests (Recommended)

```
Tempus.Tests/
├── Core.Tests/
│   └── Models/          # Model validation tests
├── Infrastructure.Tests/
│   ├── Repositories/    # Repository logic tests
│   └── Services/        # Service logic tests (ICS parsing)
└── Web.Tests/
    └── Components/      # Component rendering tests
```

Use:
- xUnit for test framework
- Moq for mocking
- FluentAssertions for assertions
- bUnit for Blazor component testing

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
   - Add Google.Apis.Calendar NuGet package
   - OAuth2 flow in new service
   - Background sync job

2. **Advanced Analytics**
   - New Analytics project/folder
   - Chart components in Web
   - Aggregation queries in Repository

3. **Mobile App**
   - Extract shared logic to .NET MAUI project
   - Reuse Core and Infrastructure
   - Platform-specific UI only

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
- [MudBlazor Components](https://mudblazor.com/)
- [Ical.Net Documentation](https://github.com/rianjs/ical.net)

---

This structure provides a solid foundation for building a comprehensive time management application while maintaining clean separation of concerns and testability.
