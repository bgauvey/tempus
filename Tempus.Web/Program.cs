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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

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

// Add separate DbContext pool for ASP.NET Core Identity
// This ensures Identity operations don't interfere with repository operations
builder.Services.AddDbContextPool<TempusDbContext>(options =>
    options.UseSqlServer(connectionString));

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
builder.Services.AddScoped<ICustomRangeRepository, CustomRangeRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
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

app.Run();

// Make Program accessible to tests
public partial class Program { }
