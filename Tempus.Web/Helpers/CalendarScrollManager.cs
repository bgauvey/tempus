using Microsoft.JSInterop;
using Tempus.Core.Models;

namespace Tempus.Web.Helpers;

/// <summary>
/// Manages scroll position operations for the Calendar component
/// </summary>
public class CalendarScrollManager
{
    private readonly IJSRuntime _jsRuntime;

    public CalendarScrollManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ScrollToWorkHoursAsync(string currentView, CalendarSettings? settings)
    {
        try
        {
            var workStartHour = settings?.WorkHoursStart.Hours ?? 8;
            var workStartMinute = settings?.WorkHoursStart.Minutes ?? 0;

            var scrollPosition = (workStartHour * 60) + workStartMinute;
            var containerId = currentView == "Day" ? "day-view-container" : "multi-day-view-container";

            await _jsRuntime.InvokeVoidAsync("eval", $@"
                (function() {{
                    var container = document.getElementById('{containerId}');
                    if (container) {{
                        container.scrollTop = {scrollPosition};
                    }}
                }})();
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scrolling to work hours: {ex.Message}");
        }
    }

    public async Task<int> SaveScrollPositionAsync(string currentView)
    {
        try
        {
            var containerId = currentView == "Day" ? "day-view-container" : "multi-day-view-container";
            var scrollTop = await _jsRuntime.InvokeAsync<int>("eval", $@"
                (function() {{
                    var container = document.getElementById('{containerId}');
                    return container ? container.scrollTop : 0;
                }})();
            ");
            return scrollTop;
        }
        catch
        {
            return 0;
        }
    }

    public async Task RestoreScrollPositionAsync(string currentView, int scrollPosition)
    {
        try
        {
            var containerId = currentView == "Day" ? "day-view-container" : "multi-day-view-container";

            await Task.Delay(50);

            await _jsRuntime.InvokeVoidAsync("eval", $@"
                (function() {{
                    var container = document.getElementById('{containerId}');
                    if (container) {{
                        container.scrollTop = {scrollPosition};
                    }}
                }})();
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restoring scroll position: {ex.Message}");
        }
    }
}
