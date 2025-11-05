using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Enums;

namespace Tempus.Infrastructure.Services;

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly ICalendarIntegrationRepository _integrationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleCalendarService> _logger;
    private const string Provider = "Google";

    public GoogleCalendarService(
        ICalendarIntegrationRepository integrationRepository,
        IEventRepository eventRepository,
        IConfiguration configuration,
        ILogger<GoogleCalendarService> logger)
    {
        _integrationRepository = integrationRepository;
        _eventRepository = eventRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string userId, string redirectUri)
    {
        var clientId = _configuration["GoogleCalendar:ClientId"];
        var clientSecret = _configuration["GoogleCalendar:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Google Calendar credentials not configured");
        }

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[] { CalendarService.Scope.Calendar },
            DataStore = null
        });

        var codeRequestUrl = flow.CreateAuthorizationCodeRequest(redirectUri);
        codeRequestUrl.State = userId;

        return codeRequestUrl.Build().ToString();
    }

    public async Task<CalendarIntegration> ExchangeCodeForTokensAsync(string code, string userId, string redirectUri)
    {
        var clientId = _configuration["GoogleCalendar:ClientId"];
        var clientSecret = _configuration["GoogleCalendar:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Google Calendar credentials not configured");
        }

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[] { CalendarService.Scope.Calendar },
            DataStore = null
        });

        var token = await flow.ExchangeCodeForTokenAsync(
            userId,
            code,
            redirectUri,
            CancellationToken.None);

        // Check if integration already exists
        var existingIntegration = await _integrationRepository.GetByProviderAsync(userId, Provider);

        var integration = existingIntegration ?? new CalendarIntegration
        {
            UserId = userId,
            Provider = Provider,
            CalendarId = "primary"
        };

        integration.AccessToken = token.AccessToken;
        integration.RefreshToken = token.RefreshToken ?? integration.RefreshToken;
        integration.TokenExpiry = DateTime.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3600);
        integration.IsEnabled = true;
        integration.SyncEnabled = true;

        // Get calendar name
        try
        {
            var service = await CreateCalendarServiceAsync(integration);
            var calendar = await service.Calendars.Get("primary").ExecuteAsync();
            integration.CalendarName = calendar.Summary ?? "Primary Calendar";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch calendar name, using default");
            integration.CalendarName = "Google Calendar";
        }

        if (existingIntegration != null)
        {
            return await _integrationRepository.UpdateAsync(integration);
        }
        else
        {
            return await _integrationRepository.CreateAsync(integration);
        }
    }

    public async Task<bool> RefreshAccessTokenAsync(CalendarIntegration integration)
    {
        if (string.IsNullOrEmpty(integration.RefreshToken))
        {
            _logger.LogWarning("No refresh token available for integration {IntegrationId}", integration.Id);
            return false;
        }

        var clientId = _configuration["GoogleCalendar:ClientId"];
        var clientSecret = _configuration["GoogleCalendar:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Google Calendar credentials not configured");
        }

        try
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { CalendarService.Scope.Calendar },
                DataStore = null
            });

            var token = await flow.RefreshTokenAsync(integration.UserId, integration.RefreshToken, CancellationToken.None);

            integration.AccessToken = token.AccessToken;
            integration.TokenExpiry = DateTime.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3600);

            await _integrationRepository.UpdateAsync(integration);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh access token for integration {IntegrationId}", integration.Id);
            return false;
        }
    }

    public async Task<int> SyncFromGoogleAsync(string userId)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (integration == null || !integration.IsEnabled || !integration.SyncEnabled)
        {
            return 0;
        }

        await EnsureValidTokenAsync(integration);
        var service = await CreateCalendarServiceAsync(integration);

        var request = service.Events.List(integration.CalendarId);
        request.TimeMinDateTimeOffset = DateTimeOffset.UtcNow.AddMonths(-1);
        request.TimeMaxDateTimeOffset = DateTimeOffset.UtcNow.AddMonths(6);
        request.ShowDeleted = false;
        request.SingleEvents = true;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        if (!string.IsNullOrEmpty(integration.SyncToken))
        {
            request.SyncToken = integration.SyncToken;
        }

        var events = await request.ExecuteAsync();
        int imported = 0;

        foreach (var googleEvent in events.Items)
        {
            try
            {
                await ImportGoogleEventAsync(googleEvent, userId);
                imported++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import Google event {EventId}", googleEvent.Id);
            }
        }

        integration.LastSyncedAt = DateTime.UtcNow;
        integration.SyncToken = events.NextSyncToken;
        await _integrationRepository.UpdateAsync(integration);

        return imported;
    }

    public async Task<int> SyncToGoogleAsync(string userId)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (integration == null || !integration.IsEnabled || !integration.SyncEnabled)
        {
            return 0;
        }

        await EnsureValidTokenAsync(integration);
        var service = await CreateCalendarServiceAsync(integration);

        // Get events modified since last sync
        var events = await _eventRepository.GetEventsByDateRangeAsync(
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow.AddMonths(6),
            userId);

        int exported = 0;

        foreach (var tempusEvent in events)
        {
            try
            {
                await ExportToGoogleAsync(service, integration.CalendarId, tempusEvent);
                exported++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export event {EventId} to Google", tempusEvent.Id);
            }
        }

        integration.LastSyncedAt = DateTime.UtcNow;
        await _integrationRepository.UpdateAsync(integration);

        return exported;
    }

    public async Task<(int imported, int exported)> SyncBothWaysAsync(string userId)
    {
        var imported = await SyncFromGoogleAsync(userId);
        var exported = await SyncToGoogleAsync(userId);
        return (imported, exported);
    }

    public async Task<List<(string id, string name)>> GetCalendarListAsync(string userId)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, Provider);
        if (integration == null)
        {
            throw new InvalidOperationException("Google Calendar not connected");
        }

        await EnsureValidTokenAsync(integration);
        var service = await CreateCalendarServiceAsync(integration);

        var calendars = await service.CalendarList.List().ExecuteAsync();

        return calendars.Items
            .Select(c => (c.Id, c.Summary ?? "Unnamed Calendar"))
            .ToList();
    }

    private Task<CalendarService> CreateCalendarServiceAsync(CalendarIntegration integration)
    {
        var credential = GoogleCredential.FromAccessToken(integration.AccessToken);

        var service = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Tempus Calendar"
        });

        return Task.FromResult(service);
    }

    private async Task EnsureValidTokenAsync(CalendarIntegration integration)
    {
        if (integration.TokenExpiry.HasValue && integration.TokenExpiry.Value <= DateTime.UtcNow.AddMinutes(5))
        {
            await RefreshAccessTokenAsync(integration);
        }
    }

    private async Task ImportGoogleEventAsync(Google.Apis.Calendar.v3.Data.Event googleEvent, string userId)
    {
        // Check if event already exists (by checking for Google event ID in notes/description)
        var existingEvents = await _eventRepository.SearchAsync($"GoogleId:{googleEvent.Id}", userId);

        var tempusEvent = existingEvents.FirstOrDefault() ?? new Core.Models.Event
        {
            UserId = userId
        };

        tempusEvent.Title = googleEvent.Summary ?? "Untitled Event";
        tempusEvent.Description = $"GoogleId:{googleEvent.Id}\n{googleEvent.Description}";
        tempusEvent.Location = googleEvent.Location;

        if (googleEvent.Start?.DateTimeDateTimeOffset.HasValue == true)
        {
            tempusEvent.StartTime = googleEvent.Start.DateTimeDateTimeOffset.Value.DateTime;
            tempusEvent.EndTime = googleEvent.End?.DateTimeDateTimeOffset?.DateTime ?? tempusEvent.StartTime.AddHours(1);
            tempusEvent.IsAllDay = false;
        }
        else if (googleEvent.Start?.Date != null)
        {
            tempusEvent.StartTime = DateTime.Parse(googleEvent.Start.Date);
            tempusEvent.EndTime = googleEvent.End?.Date != null
                ? DateTime.Parse(googleEvent.End.Date)
                : tempusEvent.StartTime.AddDays(1);
            tempusEvent.IsAllDay = true;
        }

        tempusEvent.EventType = EventType.Meeting;
        tempusEvent.Color = googleEvent.ColorId ?? "#4285F4";

        if (existingEvents.Any())
        {
            await _eventRepository.UpdateAsync(tempusEvent);
        }
        else
        {
            await _eventRepository.CreateAsync(tempusEvent);
        }
    }

    private async Task ExportToGoogleAsync(CalendarService service, string calendarId, Core.Models.Event tempusEvent)
    {
        var googleEvent = new Google.Apis.Calendar.v3.Data.Event
        {
            Summary = tempusEvent.Title,
            Description = tempusEvent.Description,
            Location = tempusEvent.Location,
            Start = new EventDateTime
            {
                DateTimeDateTimeOffset = tempusEvent.IsAllDay ? null : new DateTimeOffset(tempusEvent.StartTime),
                Date = tempusEvent.IsAllDay ? tempusEvent.StartTime.ToString("yyyy-MM-dd") : null,
                TimeZone = tempusEvent.TimeZoneId
            },
            End = new EventDateTime
            {
                DateTimeDateTimeOffset = tempusEvent.IsAllDay ? null : new DateTimeOffset(tempusEvent.EndTime),
                Date = tempusEvent.IsAllDay ? tempusEvent.EndTime.ToString("yyyy-MM-dd") : null,
                TimeZone = tempusEvent.TimeZoneId
            }
        };

        // Check if event already has a Google ID (already synced)
        var googleIdMatch = System.Text.RegularExpressions.Regex.Match(
            tempusEvent.Description ?? "",
            @"GoogleId:([^\n]+)");

        if (googleIdMatch.Success)
        {
            var googleId = googleIdMatch.Groups[1].Value.Trim();
            try
            {
                await service.Events.Update(googleEvent, calendarId, googleId).ExecuteAsync();
            }
            catch
            {
                // Event might have been deleted, create new
                var created = await service.Events.Insert(googleEvent, calendarId).ExecuteAsync();
                tempusEvent.Description = $"GoogleId:{created.Id}\n{tempusEvent.Description}";
                await _eventRepository.UpdateAsync(tempusEvent);
            }
        }
        else
        {
            var created = await service.Events.Insert(googleEvent, calendarId).ExecuteAsync();
            tempusEvent.Description = $"GoogleId:{created.Id}\n{tempusEvent.Description}";
            await _eventRepository.UpdateAsync(tempusEvent);
        }
    }
}
