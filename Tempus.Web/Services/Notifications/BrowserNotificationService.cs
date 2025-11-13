using Microsoft.JSInterop;

namespace Tempus.Web.Services.Notifications;

public interface IBrowserNotificationService
{
    Task<bool> IsSupportedAsync();
    Task<string> RequestPermissionAsync();
    Task<string> GetPermissionStatusAsync();
    Task ShowNotificationAsync(string title, string body, string? clickUrl = null);
}

public class BrowserNotificationService : IBrowserNotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<BrowserNotificationService> _logger;

    public BrowserNotificationService(IJSRuntime jsRuntime, ILogger<BrowserNotificationService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<bool> IsSupportedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("notificationsSupported");
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> RequestPermissionAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("requestNotificationPermission");
        }
        catch
        {
            return "denied";
        }
    }

    public async Task<string> GetPermissionStatusAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("getNotificationPermission");
        }
        catch
        {
            return "unsupported";
        }
    }

    public async Task ShowNotificationAsync(string title, string body, string? clickUrl = null)
    {
        try
        {
            var options = new
            {
                body = body,
                icon = "/favicon-192.png",
                badge = "/favicon-192.png",
                tag = "tempus-notification",
                requireInteraction = false,
                silent = false,
                clickUrl = clickUrl
            };

            await _jsRuntime.InvokeVoidAsync("showNotification", title, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing browser notification");
        }
    }
}
