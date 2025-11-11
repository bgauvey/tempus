using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Web.Services.Calendar;

/// <summary>
/// Manages calendar view state and navigation
/// </summary>
public class CalendarViewService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<CalendarViewService> _logger;

    public CalendarViewService(
        ISettingsService settingsService,
        ILogger<CalendarViewService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Converts CalendarView enum to scheduler index
    /// </summary>
    public int GetSchedulerIndexFromCalendarView(CalendarView view)
    {
        return view switch
        {
            CalendarView.Day => 0,
            CalendarView.Week => 1,
            CalendarView.Month => 2,
            CalendarView.Year => 3,
            _ => 1 // Default to week
        };
    }

    /// <summary>
    /// Converts scheduler index to CalendarView enum
    /// </summary>
    public CalendarView GetCalendarViewFromSchedulerIndex(int index)
    {
        return index switch
        {
            0 => CalendarView.Day,
            1 => CalendarView.Week,
            2 => CalendarView.Month,
            3 => CalendarView.Year,
            _ => CalendarView.Week // Default to week
        };
    }

    /// <summary>
    /// Changes the current view and persists the setting if RememberLastView is enabled
    /// </summary>
    public async Task<int> ChangeViewAsync(int newIndex, string userId, CalendarSettings? settings)
    {
        if (settings == null)
            return newIndex;

        var newView = GetCalendarViewFromSchedulerIndex(newIndex);
        _logger.LogDebug("Changing view to: {View} (index: {Index})", newView, newIndex);

        // Update settings if RememberLastView is enabled
        if (settings.RememberLastView)
        {
            settings.LastUsedView = newView;
            settings.LastViewChangeDate = DateTime.UtcNow;
            await _settingsService.CreateOrUpdateSettingsAsync(settings);
            _logger.LogDebug("Saved last used view: {View}", newView);
        }

        return newIndex;
    }

    /// <summary>
    /// Gets the initial view index based on user settings
    /// </summary>
    public int GetInitialViewIndex(CalendarSettings? settings)
    {
        if (settings == null)
            return 1; // Default to Week

        CalendarView viewToUse;

        // Check if we should remember the last view and if it was recently used (within 7 days)
        if (settings.RememberLastView &&
            settings.LastUsedView.HasValue &&
            settings.LastViewChangeDate.HasValue &&
            (DateTime.UtcNow - settings.LastViewChangeDate.Value).TotalDays <= 7)
        {
            viewToUse = settings.LastUsedView.Value;
            _logger.LogDebug("Using remembered last view: {View}", viewToUse);
        }
        else
        {
            viewToUse = settings.DefaultCalendarView;
            _logger.LogDebug("Using default view: {View}", viewToUse);
        }

        return GetSchedulerIndexFromCalendarView(viewToUse);
    }

    /// <summary>
    /// Gets the time zone abbreviation for display
    /// </summary>
    public string GetTimeZoneAbbreviation(TimeZoneInfo tz)
    {
        var now = DateTime.Now;

        // Get standard and daylight names
        var standardName = tz.StandardName;
        var daylightName = tz.DaylightName;

        // Check if currently in daylight saving time
        var isDaylightSaving = tz.IsDaylightSavingTime(now);

        // Try to extract abbreviation
        string abbreviation;

        if (isDaylightSaving && daylightName != standardName)
        {
            // Use daylight name
            abbreviation = ExtractAbbreviation(daylightName);
        }
        else
        {
            // Use standard name
            abbreviation = ExtractAbbreviation(standardName);
        }

        return abbreviation;
    }

    private string ExtractAbbreviation(string timeZoneName)
    {
        // Common abbreviations
        var abbreviations = new Dictionary<string, string>
        {
            { "Pacific Standard Time", "PST" },
            { "Pacific Daylight Time", "PDT" },
            { "Mountain Standard Time", "MST" },
            { "Mountain Daylight Time", "MDT" },
            { "Central Standard Time", "CST" },
            { "Central Daylight Time", "CDT" },
            { "Eastern Standard Time", "EST" },
            { "Eastern Daylight Time", "EDT" },
            { "Coordinated Universal Time", "UTC" },
            { "Greenwich Mean Time", "GMT" },
        };

        if (abbreviations.TryGetValue(timeZoneName, out var abbr))
            return abbr;

        // Fallback: Take first letter of each word
        var words = timeZoneName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            return string.Join("", words.Select(w => w[0]));
        }

        return timeZoneName.Length > 3 ? timeZoneName.Substring(0, 3) : timeZoneName;
    }

    /// <summary>
    /// Calculates the date range for loading events based on current view
    /// </summary>
    public (DateTime startDate, DateTime endDate) GetDateRangeForView(DateTime selectedDate, int viewIndex)
    {
        var view = GetCalendarViewFromSchedulerIndex(viewIndex);

        return view switch
        {
            CalendarView.Day => (selectedDate.Date, selectedDate.Date.AddDays(1)),
            CalendarView.Week => GetWeekRange(selectedDate),
            CalendarView.Month => GetMonthRange(selectedDate),
            CalendarView.Year => GetYearRange(selectedDate),
            _ => GetWeekRange(selectedDate)
        };
    }

    private (DateTime startDate, DateTime endDate) GetWeekRange(DateTime date)
    {
        // Get start of week (assuming Sunday as start)
        var startOfWeek = date.Date.AddDays(-(int)date.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        return (startOfWeek, endOfWeek);
    }

    private (DateTime startDate, DateTime endDate) GetMonthRange(DateTime date)
    {
        var startOfMonth = new DateTime(date.Year, date.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        return (startOfMonth, endOfMonth);
    }

    private (DateTime startDate, DateTime endDate) GetYearRange(DateTime date)
    {
        var startOfYear = new DateTime(date.Year, 1, 1);
        var endOfYear = startOfYear.AddYears(1);
        return (startOfYear, endOfYear);
    }
}
