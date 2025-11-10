using CalDAVNet;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Enums;
using IcsCalendar = Ical.Net.Calendar;

namespace Tempus.Infrastructure.Services;

public class AppleCalendarService : IAppleCalendarService
{
    private readonly ICalendarIntegrationRepository _integrationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppleCalendarService> _logger;
    private const string Provider = "Apple";

    public AppleCalendarService(
        ICalendarIntegrationRepository integrationRepository,
        IEventRepository eventRepository,
        IConfiguration configuration,
        ILogger<AppleCalendarService> logger)
    {
        _integrationRepository = integrationRepository;
        _eventRepository = eventRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CalendarIntegration> ConnectAsync(string userId, string serverUrl, string username, string appSpecificPassword)
    {
        _logger.LogInformation("Connecting to Apple Calendar for user {UserId}", userId);

        // Test connection first
        var isValid = await TestConnectionAsync(serverUrl, username, appSpecificPassword);
        if (!isValid)
        {
            throw new InvalidOperationException("Failed to connect to Apple Calendar. Please check your credentials.");
        }

        // Get calendar list to select primary calendar
        var client = CreateCalDAVClient(serverUrl, username, appSpecificPassword);
        var calendars = await client.GetAllCalendars();
        var primaryCalendar = calendars.FirstOrDefault();

        if (primaryCalendar == null)
        {
            throw new InvalidOperationException("No calendars found in Apple Calendar account.");
        }

        // Create or update integration
        var existing = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (existing != null)
        {
            // Update existing integration
            existing.CalendarId = serverUrl; // Store server URL as calendar ID
            existing.CalendarName = "Apple Calendar"; // Default name for now
            existing.AccessToken = EncryptPassword(appSpecificPassword);
            existing.RefreshToken = EncryptPassword(username); // Store username in RefreshToken field
            existing.IsEnabled = true;
            existing.LastSyncedAt = DateTime.UtcNow;

            await _integrationRepository.UpdateAsync(existing);
            return existing;
        }
        else
        {
            // Create new integration
            var integration = new CalendarIntegration
            {
                UserId = userId,
                Provider = Provider,
                CalendarId = serverUrl, // Store server URL as calendar ID
                CalendarName = "Apple Calendar", // Default name for now
                AccessToken = EncryptPassword(appSpecificPassword),
                RefreshToken = EncryptPassword(username), // Store username in RefreshToken field
                IsEnabled = true,
                SyncEnabled = true,
                LastSyncedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _integrationRepository.CreateAsync(integration);
            return integration;
        }
    }

    public async Task<bool> TestConnectionAsync(string serverUrl, string username, string appSpecificPassword)
    {
        try
        {
            _logger.LogInformation("Testing CalDAV connection to {ServerUrl}", serverUrl);

            var client = CreateCalDAVClient(serverUrl, username, appSpecificPassword);
            var calendars = await client.GetAllCalendars();

            return calendars != null && calendars.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test CalDAV connection");
            return false;
        }
    }

    public async Task<int> SyncFromAppleAsync(string userId)
    {
        _logger.LogInformation("Syncing events from Apple Calendar for user {UserId}", userId);

        var integration = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (integration == null || !integration.IsEnabled || !integration.SyncEnabled)
        {
            return 0;
        }

        var username = DecryptPassword(integration.RefreshToken ?? string.Empty);
        var password = DecryptPassword(integration.AccessToken ?? string.Empty);
        var client = CreateCalDAVClient(integration.CalendarId, username, password);

        try
        {
            // Get the default calendar
            var calendar = await client.GetDefaultCalendar();
            if (calendar == null || calendar.Events == null)
            {
                _logger.LogWarning("No calendar or events found");
                return 0;
            }

            int imported = 0;

            foreach (var calEvent in calendar.Events)
            {
                await ImportCalDAVEventAsync(calEvent, userId);
                imported++;
            }

            // Update last synced timestamp
            integration.LastSyncedAt = DateTime.UtcNow;
            await _integrationRepository.UpdateAsync(integration);

            _logger.LogInformation("Imported {Count} events from Apple Calendar", imported);
            return imported;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing from Apple Calendar");
            throw;
        }
    }

    public async Task<int> SyncToAppleAsync(string userId)
    {
        _logger.LogInformation("Syncing events to Apple Calendar for user {UserId}", userId);

        var integration = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (integration == null || !integration.IsEnabled || !integration.SyncEnabled)
        {
            return 0;
        }

        var username = DecryptPassword(integration.RefreshToken ?? string.Empty);
        var password = DecryptPassword(integration.AccessToken ?? string.Empty);
        var client = CreateCalDAVClient(integration.CalendarId, username, password);

        // Get events from Tempus that should be exported
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow.AddMonths(6);
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);

        int exported = 0;

        foreach (var tempusEvent in events)
        {
            await ExportToAppleAsync(client, tempusEvent);
            exported++;
        }

        _logger.LogInformation("Exported {Count} events to Apple Calendar", exported);
        return exported;
    }

    public async Task<(int imported, int exported)> SyncBothWaysAsync(string userId)
    {
        _logger.LogInformation("Two-way sync with Apple Calendar for user {UserId}", userId);

        var imported = await SyncFromAppleAsync(userId);
        var exported = await SyncToAppleAsync(userId);

        return (imported, exported);
    }

    public async Task<List<(string id, string name)>> GetCalendarListAsync(string userId)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (integration == null || !integration.IsEnabled)
        {
            return new List<(string id, string name)>();
        }

        var username = DecryptPassword(integration.RefreshToken ?? string.Empty);
        var password = DecryptPassword(integration.AccessToken ?? string.Empty);
        var client = CreateCalDAVClient(integration.CalendarId, username, password);

        var calendars = await client.GetAllCalendars();

        return calendars
            .Select((c, i) => ($"calendar_{i}", $"Apple Calendar {i + 1}"))
            .ToList();
    }

    public async Task<bool> UpdateCredentialsAsync(string userId, string serverUrl, string username, string appSpecificPassword)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (integration == null)
        {
            return false;
        }

        // Test new credentials
        var isValid = await TestConnectionAsync(serverUrl, username, appSpecificPassword);
        if (!isValid)
        {
            return false;
        }

        // Update credentials
        integration.AccessToken = EncryptPassword(appSpecificPassword);
        integration.RefreshToken = EncryptPassword(username);

        await _integrationRepository.UpdateAsync(integration);
        return true;
    }

    private Client CreateCalDAVClient(string serverUrl, string username, string password)
    {
        return new Client(serverUrl, username, password);
    }

    private async Task ImportCalDAVEventAsync(CalendarEvent calEvent, string userId)
    {
        // Check if event already exists (by checking for CalDAV UID in description)
        var existingEvents = await _eventRepository.SearchAsync($"CalDAVId:{calEvent.Uid}", userId);

        var tempusEvent = existingEvents.FirstOrDefault() ?? new Core.Models.Event
        {
            UserId = userId
        };

        tempusEvent.Title = calEvent.Summary ?? "Untitled Event";
        tempusEvent.Description = $"CalDAVId:{calEvent.Uid}\n{calEvent.Description}";
        tempusEvent.Location = calEvent.Location;

        if (calEvent.Start != null && calEvent.End != null)
        {
            tempusEvent.StartTime = calEvent.Start.AsUtc;
            tempusEvent.EndTime = calEvent.End.AsUtc;
            tempusEvent.IsAllDay = calEvent.Start is CalDateTime cdt && cdt.HasTime == false;
        }

        tempusEvent.EventType = EventType.Meeting;
        tempusEvent.Color = "#007AFF"; // Apple blue

        if (existingEvents.Any())
        {
            await _eventRepository.UpdateAsync(tempusEvent);
        }
        else
        {
            await _eventRepository.CreateAsync(tempusEvent);
        }
    }

    private async Task ExportToAppleAsync(Client client, Core.Models.Event tempusEvent)
    {
        var calendar = new IcsCalendar();
        var calEvent = new CalendarEvent
        {
            Summary = tempusEvent.Title,
            Description = tempusEvent.Description,
            Location = tempusEvent.Location,
            Start = new CalDateTime(tempusEvent.StartTime),
            End = new CalDateTime(tempusEvent.EndTime),
            Uid = Guid.NewGuid().ToString()
        };

        calendar.Events.Add(calEvent);

        // Check if event already has a CalDAV ID (already synced)
        var calDAVIdMatch = System.Text.RegularExpressions.Regex.Match(
            tempusEvent.Description ?? "",
            @"CalDAVId:([^\n]+)");

        if (calDAVIdMatch.Success)
        {
            var calDAVId = calDAVIdMatch.Groups[1].Value.Trim();
            calEvent.Uid = calDAVId;

            try
            {
                await client.AddOrUpdateEvent(calEvent, calendar);
            }
            catch
            {
                // If update fails, try to create new event with new UID
                calEvent.Uid = Guid.NewGuid().ToString();
                await client.AddOrUpdateEvent(calEvent, calendar);
            }
        }
        else
        {
            // Create new event
            await client.AddOrUpdateEvent(calEvent, calendar);

            // Update Tempus event with CalDAV ID
            tempusEvent.Description = $"CalDAVId:{calEvent.Uid}\n{tempusEvent.Description}";
            await _eventRepository.UpdateAsync(tempusEvent);
        }
    }

    // Simple encryption/decryption helpers (in production, use proper encryption)
    private string EncryptPassword(string password)
    {
        // TODO: Implement proper encryption using Data Protection API
        // For now, just base64 encode (NOT SECURE FOR PRODUCTION)
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(bytes);
    }

    private string DecryptPassword(string encryptedPassword)
    {
        // TODO: Implement proper decryption using Data Protection API
        // For now, just base64 decode (NOT SECURE FOR PRODUCTION)
        if (string.IsNullOrEmpty(encryptedPassword))
            return string.Empty;

        var bytes = Convert.FromBase64String(encryptedPassword);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
