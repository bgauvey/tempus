using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

/// <summary>
/// Service for integrating Microsoft Outlook/Office 365 calendar using Microsoft Graph API
/// </summary>
public class OutlookCalendarService : IOutlookCalendarService
{
    private readonly ICalendarIntegrationRepository _integrationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IConfiguration _configuration;

    // Microsoft Graph scopes needed for calendar access
    private readonly string[] _scopes = new[]
    {
        "Calendars.ReadWrite",
        "offline_access"  // Required for refresh tokens
    };

    public OutlookCalendarService(
        ICalendarIntegrationRepository integrationRepository,
        IEventRepository eventRepository,
        IConfiguration configuration)
    {
        _integrationRepository = integrationRepository;
        _eventRepository = eventRepository;
        _configuration = configuration;
    }

    public string GetAuthorizationUrl(string userId, string redirectUri)
    {
        var clientId = _configuration["Outlook:ClientId"] ?? throw new InvalidOperationException("Outlook:ClientId not configured");
        var tenantId = _configuration["Outlook:TenantId"] ?? "common"; // "common" allows personal and work accounts

        // Build authorization URL manually
        var authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"
            + $"?client_id={Uri.EscapeDataString(clientId)}"
            + $"&response_type=code"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + $"&response_mode=query"
            + $"&scope={Uri.EscapeDataString(string.Join(" ", _scopes))}"
            + $"&state={Uri.EscapeDataString(userId)}";

        return authUrl;
    }

    public async Task<CalendarIntegration> ExchangeCodeForTokensAsync(string code, string redirectUri, string userId)
    {
        var clientId = _configuration["Outlook:ClientId"] ?? throw new InvalidOperationException("Outlook:ClientId not configured");
        var clientSecret = _configuration["Outlook:ClientSecret"] ?? throw new InvalidOperationException("Outlook:ClientSecret not configured");
        var tenantId = _configuration["Outlook:TenantId"] ?? "common";

        // Create confidential client application
        var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .WithRedirectUri(redirectUri)
            .Build();

        // Exchange authorization code for tokens
        var result = await app.AcquireTokenByAuthorizationCode(_scopes, code).ExecuteAsync();

        // Get user's primary calendar using the access token
        var graphClient = CreateGraphClient(result.AccessToken);
        var calendar = await graphClient.Me.Calendar.GetAsync();

        // Create or update integration
        var existingIntegration = await _integrationRepository.GetByProviderAsync(userId, "Outlook");

        if (existingIntegration != null)
        {
            existingIntegration.AccessToken = result.AccessToken;
            existingIntegration.RefreshToken = result.AccessToken; // MSAL handles refresh internally
            existingIntegration.TokenExpiry = result.ExpiresOn.UtcDateTime;
            existingIntegration.CalendarId = calendar?.Id ?? "primary";
            existingIntegration.CalendarName = calendar?.Name ?? "Calendar";
            existingIntegration.IsEnabled = true;

            await _integrationRepository.UpdateAsync(existingIntegration);
            return existingIntegration;
        }
        else
        {
            var integration = new CalendarIntegration
            {
                UserId = userId,
                Provider = "Outlook",
                AccessToken = result.AccessToken,
                RefreshToken = result.AccessToken,
                TokenExpiry = result.ExpiresOn.UtcDateTime,
                CalendarId = calendar?.Id ?? "primary",
                CalendarName = calendar?.Name ?? "Calendar",
                IsEnabled = true,
                SyncEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            await _integrationRepository.CreateAsync(integration);
            return integration;
        }
    }

    public async Task<bool> TestConnectionAsync(string userId)
    {
        try
        {
            var integration = await _integrationRepository.GetByProviderAsync(userId, "Outlook");
            if (integration == null || string.IsNullOrEmpty(integration.AccessToken))
                return false;

            var graphClient = await CreateAuthenticatedGraphClientAsync(integration);
            var calendar = await graphClient.Me.Calendar.GetAsync();

            return calendar != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> SyncFromOutlookAsync(string userId)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, "Outlook");
        if (integration == null || !integration.IsEnabled)
            throw new InvalidOperationException("Outlook calendar integration is not configured");

        var graphClient = await CreateAuthenticatedGraphClientAsync(integration);

        // Get events from the last 6 months to 1 year in the future
        var startDateTime = DateTime.UtcNow.AddMonths(-6);
        var endDateTime = DateTime.UtcNow.AddYears(1);

        var events = await graphClient.Me.Calendar.Events.GetAsync(config =>
        {
            config.QueryParameters.Filter = $"start/dateTime ge '{startDateTime:yyyy-MM-ddTHH:mm:ssZ}' and end/dateTime le '{endDateTime:yyyy-MM-ddTHH:mm:ssZ}'";
            config.QueryParameters.Top = 1000;
            config.QueryParameters.Select = new[] { "id", "subject", "body", "start", "end", "location", "isAllDay", "sensitivity", "categories", "attendees" };
        });

        int importedCount = 0;

        if (events?.Value != null)
        {
            foreach (var outlookEvent in events.Value)
            {
                await ImportOutlookEventAsync(outlookEvent, userId, integration.CalendarId ?? "primary");
                importedCount++;
            }
        }

        // Update last synced time
        integration.LastSyncedAt = DateTime.UtcNow;
        await _integrationRepository.UpdateAsync(integration);

        return importedCount;
    }

    public async Task<int> SyncToOutlookAsync(string userId)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, "Outlook");
        if (integration == null || !integration.IsEnabled)
            throw new InvalidOperationException("Outlook calendar integration is not configured");

        var graphClient = await CreateAuthenticatedGraphClientAsync(integration);

        // Get events from Tempus that should be exported
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow.AddYears(1);
        var allEvents = await _eventRepository.GetAllAsync(userId);
        var tempusEvents = allEvents.Where(e =>
            e.StartTime >= startDate && e.EndTime <= endDate).ToList();

        int exportedCount = 0;

        foreach (var tempusEvent in tempusEvents)
        {
            // Skip if already synced (check for OutlookId in description)
            if (tempusEvent.Description?.Contains("OutlookId:") == true)
                continue;

            var outlookEvent = ConvertToOutlookEvent(tempusEvent);

            var createdEvent = await graphClient.Me.Calendar.Events.PostAsync(outlookEvent);

            if (createdEvent?.Id != null)
            {
                // Store Outlook event ID in description for future sync
                tempusEvent.Description = $"OutlookId:{createdEvent.Id}\n{tempusEvent.Description}";
                await _eventRepository.UpdateAsync(tempusEvent);
                exportedCount++;
            }
        }

        // Update last synced time
        integration.LastSyncedAt = DateTime.UtcNow;
        await _integrationRepository.UpdateAsync(integration);

        return exportedCount;
    }

    public async Task<(int imported, int exported)> SyncBothWaysAsync(string userId)
    {
        var imported = await SyncFromOutlookAsync(userId);
        var exported = await SyncToOutlookAsync(userId);

        return (imported, exported);
    }

    public async Task<List<(string id, string name)>> GetCalendarListAsync(string userId)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, "Outlook");
        if (integration == null || !integration.IsEnabled)
            throw new InvalidOperationException("Outlook calendar integration is not configured");

        var graphClient = await CreateAuthenticatedGraphClientAsync(integration);
        var calendars = await graphClient.Me.Calendars.GetAsync();

        var calendarList = new List<(string id, string name)>();

        if (calendars?.Value != null)
        {
            foreach (var calendar in calendars.Value)
            {
                if (!string.IsNullOrEmpty(calendar.Id) && !string.IsNullOrEmpty(calendar.Name))
                {
                    calendarList.Add((calendar.Id, calendar.Name));
                }
            }
        }

        return calendarList;
    }

    public async Task<bool> UpdateCalendarSelectionAsync(string userId, string calendarId, string calendarName)
    {
        var integration = await _integrationRepository.GetByProviderAsync(userId, "Outlook");
        if (integration == null)
            return false;

        integration.CalendarId = calendarId;
        integration.CalendarName = calendarName;

        await _integrationRepository.UpdateAsync(integration);
        return true;
    }

    #region Private Helper Methods

    private GraphServiceClient CreateGraphClient(string accessToken)
    {
        var authProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(accessToken));
        return new GraphServiceClient(authProvider);
    }

    private async Task<GraphServiceClient> CreateAuthenticatedGraphClientAsync(CalendarIntegration integration)
    {
        // Check if token needs refresh
        if (integration.TokenExpiry.HasValue && integration.TokenExpiry.Value <= DateTime.UtcNow.AddMinutes(5))
        {
            // Token expired or about to expire, refresh it
            var clientId = _configuration["Outlook:ClientId"] ?? throw new InvalidOperationException("Outlook:ClientId not configured");
            var clientSecret = _configuration["Outlook:ClientSecret"] ?? throw new InvalidOperationException("Outlook:ClientSecret not configured");
            var tenantId = _configuration["Outlook:TenantId"] ?? "common";

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();

            // MSAL maintains token cache automatically
#pragma warning disable CS0618 // GetAccountsAsync is obsolete but required without token cache serialization
            var accounts = await app.GetAccountsAsync();
#pragma warning restore CS0618
            AuthenticationResult result;

            if (accounts.Any())
            {
                result = await app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault()).ExecuteAsync();
            }
            else
            {
                // Fallback: use refresh token flow (not recommended but kept for compatibility)
                throw new InvalidOperationException("Token expired and no cached account available. User must re-authenticate.");
            }

            // Update stored token
            integration.AccessToken = result.AccessToken;
            integration.TokenExpiry = result.ExpiresOn.UtcDateTime;
            await _integrationRepository.UpdateAsync(integration);
        }

        // Verify access token is available
        if (string.IsNullOrEmpty(integration.AccessToken))
            throw new InvalidOperationException("Access token is not available. User must re-authenticate.");

        return CreateGraphClient(integration.AccessToken);
    }

    private async Task ImportOutlookEventAsync(Microsoft.Graph.Models.Event outlookEvent, string userId, string calendarId)
    {
        if (outlookEvent.Id == null) return;

        // Check if event already exists (by OutlookId in description)
        var existingEvents = await _eventRepository.SearchAsync($"OutlookId:{outlookEvent.Id}", userId);
        var existingEvent = existingEvents.FirstOrDefault();

        var tempusEvent = existingEvent ?? new Core.Models.Event
        {
            UserId = userId
        };

        tempusEvent.Title = outlookEvent.Subject ?? "Untitled Event";
        tempusEvent.Description = $"OutlookId:{outlookEvent.Id}\n{outlookEvent.Body?.Content ?? ""}";
        tempusEvent.Location = outlookEvent.Location?.DisplayName ?? "";

        // Handle start time
        if (outlookEvent.Start?.DateTime != null && DateTime.TryParse(outlookEvent.Start.DateTime, out var startTime))
        {
            tempusEvent.StartTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
        }

        // Handle end time
        if (outlookEvent.End?.DateTime != null && DateTime.TryParse(outlookEvent.End.DateTime, out var endTime))
        {
            tempusEvent.EndTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
        }

        tempusEvent.IsAllDay = outlookEvent.IsAllDay ?? false;

        // Map sensitivity to priority
        tempusEvent.Priority = outlookEvent.Sensitivity switch
        {
            Sensitivity.Normal => Core.Enums.Priority.Medium,
            Sensitivity.Personal => Core.Enums.Priority.Medium,
            Sensitivity.Private => Core.Enums.Priority.High,
            Sensitivity.Confidential => Core.Enums.Priority.High,
            _ => Core.Enums.Priority.Medium
        };

        // Default event type
        tempusEvent.EventType = outlookEvent.Attendees?.Any() == true
            ? Core.Enums.EventType.Meeting
            : Core.Enums.EventType.Appointment;

        tempusEvent.Color = "#1F77B4"; // Outlook blue

        if (existingEvent == null)
        {
            await _eventRepository.CreateAsync(tempusEvent);
        }
        else
        {
            await _eventRepository.UpdateAsync(tempusEvent);
        }
    }

    private Microsoft.Graph.Models.Event ConvertToOutlookEvent(Core.Models.Event tempusEvent)
    {
        var outlookEvent = new Microsoft.Graph.Models.Event
        {
            Subject = tempusEvent.Title,
            Body = new ItemBody
            {
                ContentType = BodyType.Text,
                Content = tempusEvent.Description?.Replace($"OutlookId:{tempusEvent.Id}\n", "") ?? ""
            },
            Start = new DateTimeTimeZone
            {
                DateTime = tempusEvent.StartTime.ToString("O"),
                TimeZone = "UTC"
            },
            End = new DateTimeTimeZone
            {
                DateTime = tempusEvent.EndTime.ToString("O"),
                TimeZone = "UTC"
            },
            Location = new Location
            {
                DisplayName = tempusEvent.Location ?? ""
            },
            IsAllDay = tempusEvent.IsAllDay,
            Sensitivity = tempusEvent.Priority switch
            {
                Core.Enums.Priority.Low => Sensitivity.Normal,
                Core.Enums.Priority.Medium => Sensitivity.Normal,
                Core.Enums.Priority.High => Sensitivity.Private,
                Core.Enums.Priority.Urgent => Sensitivity.Confidential,
                _ => Sensitivity.Normal
            }
        };

        return outlookEvent;
    }

    #endregion

    // Simple token provider for authentication
    private class TokenProvider : IAccessTokenProvider
    {
        private readonly string _accessToken;

        public TokenProvider(string accessToken)
        {
            _accessToken = accessToken;
        }

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_accessToken);
        }

        public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
    }
}
