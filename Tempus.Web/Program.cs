using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tempus.Infrastructure.Data;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Repositories;
using Tempus.Infrastructure.Services;
using Radzen;
using Tempus.Web.Components;
using Tempus.Web.Components.Account;
using Tempus.Web.Services;
using Tempus.Web.Services.Notifications;
using Tempus.Web.Services.Pdf;
using Tempus.Web.Services.Help;
using Serilog;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Tempus application");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Configure application settings
builder.Services.Configure<Tempus.Core.Configuration.ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings"));

// Configure email settings
builder.Services.Configure<Tempus.Core.Configuration.EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Add Radzen services
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "TempusTheme";
    options.Duration = TimeSpan.FromDays(365);
});

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=TempusDb;Trusted_Connection=True;TrustServerCertificate=True";

// Add pooled DbContextFactory for better performance in Blazor Server
// This creates a pool of reusable contexts to avoid concurrency issues
builder.Services.AddPooledDbContextFactory<TempusDbContext>(options =>
    options.UseSqlServer(connectionString));

// Some libraries (like Identity's EF stores) expect TempusDbContext to be resolvable
// from DI. We register a scoped TempusDbContext that uses the pooled factory to
// create a context per scope so existing code that requests TempusDbContext will
// be satisfied while still benefiting from the pooled factory for manual usage.
// Use transient DbContext so each injection resolves a new instance. In Blazor Server
// circuits multiple threads can use services from the same scope concurrently; a
// transient DbContext reduces the chance the same instance is used concurrently.
// For best safety, refactor services/repositories to take IDbContextFactory and
// create a DbContext per operation.
builder.Services.AddTransient<TempusDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<TempusDbContext>>().CreateDbContext());

// Add Identity services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<TempusDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Register repositories and services
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();
builder.Services.AddScoped<ICustomRangeRepository, CustomRangeRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ICalendarIntegrationRepository, CalendarIntegrationRepository>();
builder.Services.AddScoped<IIcsImportService, IcsImportService>();
builder.Services.AddScoped<IPstImportService, PstImportService>();
builder.Services.AddScoped<IPdfAgendaService, PdfAgendaService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAnalyticsReportService, AnalyticsReportService>();
builder.Services.AddScoped<ITrendForecastService, TrendForecastService>();
builder.Services.AddScoped<ITimeZoneConversionService, TimeZoneConversionService>();
builder.Services.AddScoped<IBrowserNotificationService, BrowserNotificationService>();
builder.Services.AddScoped<IBenchmarkService, BenchmarkService>();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddScoped<IAppleCalendarService, AppleCalendarService>();
builder.Services.AddScoped<IOutlookCalendarService, OutlookCalendarService>();
builder.Services.AddScoped<IHelpService, HelpService>();
builder.Services.AddScoped<INotificationSchedulerService, NotificationSchedulerService>();
builder.Services.AddScoped<IRSVPService, RSVPService>();
builder.Services.AddScoped<ISchedulingAssistantService, SchedulingAssistantService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IVideoConferenceService, VideoConferenceService>();
builder.Services.AddScoped<ITeamService, TeamService>();

// Register calendar services for refactored Calendar component
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarStateService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarEventService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarViewService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarFilterService>();
builder.Services.AddScoped<Tempus.Web.Services.Calendar.CalendarHelperService>();

// Register background service for notification checking
builder.Services.AddHostedService<NotificationBackgroundService>();

// Configure OpenTelemetry
const string serviceName = "Tempus";
const string serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["host.name"] = Environment.MachineName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            // Record all incoming requests
            options.RecordException = true;
            // Enrich spans with additional info
            options.EnrichWithHttpRequest = (activity, httpRequest) =>
            {
                activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
            };
            options.EnrichWithHttpResponse = (activity, httpResponse) =>
            {
                activity.SetTag("http.response.status_code", httpResponse.StatusCode);
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSource("Tempus.*") // Add custom activity sources
        .SetSampler(new AlwaysOnSampler()) // Sample all traces in development
        .AddConsoleExporter()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter("Tempus.*") // Add custom meters
        .AddConsoleExporter()
        .AddOtlpExporter());

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TempusDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Temporarily disable HTTPS redirection for debugging
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Radzen.Blazor.RadzenButton).Assembly);

// Add Identity endpoints for login/logout
app.MapAdditionalIdentityEndpoints();

// Map API controllers
app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program accessible to tests
public partial class Program { }
