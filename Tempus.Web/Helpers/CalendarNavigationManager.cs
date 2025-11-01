using Tempus.Core.Models;

namespace Tempus.Web.Helpers;

/// <summary>
/// Manages navigation and view operations for the Calendar component
/// </summary>
public class CalendarNavigationManager
{
    public class ViewOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public List<ViewOption> GetAvailableViews(List<CustomCalendarRange> customRanges)
    {
        var views = new List<ViewOption>
        {
            new ViewOption { Value = "Grid", Text = "Grid" },
            new ViewOption { Value = "Day", Text = "Day" },
            new ViewOption { Value = "WorkWeek", Text = "Work Week" },
            new ViewOption { Value = "Week", Text = "Week" },
            new ViewOption { Value = "Month", Text = "Month" },
            new ViewOption { Value = "Year", Text = "Year" },
            new ViewOption { Value = "Planner", Text = "Planner" },
            new ViewOption { Value = "Timeline", Text = "Timeline" }
        };

        foreach (var range in customRanges)
        {
            views.Add(new ViewOption
            {
                Value = $"Custom_{range.Id}",
                Text = range.Name
            });
        }

        return views;
    }

    public DateTime NavigatePrevious(DateTime selectedDate, string currentView, CustomCalendarRange? selectedCustomRange)
    {
        return currentView switch
        {
            "Day" => selectedDate.AddDays(-1),
            "WorkWeek" => selectedDate.AddDays(-7),
            "Week" => selectedDate.AddDays(-7),
            "Month" => selectedDate.AddMonths(-1),
            _ when selectedCustomRange != null => selectedDate.AddDays(-selectedCustomRange.DaysCount),
            _ => selectedDate.AddMonths(-1)
        };
    }

    public DateTime NavigateNext(DateTime selectedDate, string currentView, CustomCalendarRange? selectedCustomRange)
    {
        return currentView switch
        {
            "Day" => selectedDate.AddDays(1),
            "WorkWeek" => selectedDate.AddDays(7),
            "Week" => selectedDate.AddDays(7),
            "Month" => selectedDate.AddMonths(1),
            _ when selectedCustomRange != null => selectedDate.AddDays(selectedCustomRange.DaysCount),
            _ => selectedDate.AddMonths(1)
        };
    }

    public string GetCurrentViewTitle(DateTime selectedDate, string currentView, CustomCalendarRange? selectedCustomRange, CalendarSettings? settings)
    {
        return currentView switch
        {
            "Grid" => "All Events",
            "Day" => selectedDate.ToString("MMMM dd, yyyy"),
            "WorkWeek" or "Week" => $"{GetWeekStart(selectedDate, settings).ToString("MMM dd")} - {GetWeekEnd(selectedDate, settings).ToString("MMM dd, yyyy")}",
            "Month" => selectedDate.ToString("MMMM yyyy"),
            "Year" => DateTime.Today.Year.ToString(),
            "Planner" => $"Year Planner - {DateTime.Today.Year}",
            "Timeline" => $"Timeline - {DateTime.Today.Year}",
            _ when selectedCustomRange != null => $"{selectedDate.ToString("MMM dd")} - {selectedDate.AddDays(selectedCustomRange.DaysCount - 1).ToString("MMM dd, yyyy")}",
            _ => selectedDate.ToString("MMMM yyyy")
        };
    }

    public int GetSelectedViewIndex(string currentView)
    {
        return currentView switch
        {
            "Month" => 0,
            "Week" => 1,
            "WorkWeek" => 1,
            "Day" => 2,
            "Year" => 3,
            "Planner" => 4,
            "Timeline" => 5,
            _ => 0
        };
    }

    public DateTime GetWeekStart(DateTime selectedDate, CalendarSettings? settings)
    {
        var startOfWeek = settings?.StartOfWeek ?? DayOfWeek.Sunday;
        var diff = ((int)selectedDate.DayOfWeek - (int)startOfWeek + 7) % 7;
        return selectedDate.AddDays(-diff).Date;
    }

    public DateTime GetWeekEnd(DateTime selectedDate, CalendarSettings? settings)
    {
        return GetWeekStart(selectedDate, settings).AddDays(6);
    }

    public DateTime GetWorkWeekStart(DateTime selectedDate)
    {
        var diff = selectedDate.DayOfWeek - DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return selectedDate.AddDays(-diff).Date;
    }

    public DateTime GetWorkWeekEnd(DateTime selectedDate)
    {
        return GetWorkWeekStart(selectedDate).AddDays(4);
    }

    public List<DateTime> GetWeekDays(DateTime selectedDate, CalendarSettings? settings)
    {
        var days = new List<DateTime>();
        var start = GetWeekStart(selectedDate, settings);
        var showWeekends = settings?.ShowWeekendInWeekView ?? true;

        for (int i = 0; i < 7; i++)
        {
            var day = start.AddDays(i);
            if (showWeekends || (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday))
            {
                days.Add(day);
            }
        }
        return days;
    }

    public List<DateTime> GetWorkWeekDays(DateTime selectedDate)
    {
        var days = new List<DateTime>();
        var start = GetWorkWeekStart(selectedDate);
        for (int i = 0; i < 5; i++)
        {
            days.Add(start.AddDays(i));
        }
        return days;
    }

    public List<DateTime> GetCustomRangeDays(DateTime selectedDate, CustomCalendarRange customRange)
    {
        var days = new List<DateTime>();
        var start = selectedDate.Date;

        for (int i = 0; i < customRange.DaysCount; i++)
        {
            var day = start.AddDays(i);
            if (customRange.ShowWeekends || (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday))
            {
                days.Add(day);
            }
        }
        return days;
    }

    public List<List<DateTime>> GetCalendarWeeks(DateTime selectedDate, CalendarSettings? settings)
    {
        var firstDayOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var startOfWeek = settings?.StartOfWeek ?? DayOfWeek.Sunday;

        var daysToSubtract = ((int)firstDayOfMonth.DayOfWeek - (int)startOfWeek + 7) % 7;
        var startDate = firstDayOfMonth.AddDays(-daysToSubtract);

        var daysToAdd = (6 - ((int)lastDayOfMonth.DayOfWeek - (int)startOfWeek + 7) % 7);
        var endDate = lastDayOfMonth.AddDays(daysToAdd);

        var weeks = new List<List<DateTime>>();
        var currentWeek = new List<DateTime>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            currentWeek.Add(date);
            if (currentWeek.Count == 7)
            {
                weeks.Add(currentWeek);
                currentWeek = new List<DateTime>();
            }
        }

        return weeks;
    }

    public (DateTime startDate, DateTime endDate) GetDateRangeForView(DateTime selectedDate, string currentView)
    {
        if (currentView == "Year" || currentView == "Planner" || currentView == "Timeline")
        {
            var startDate = new DateTime(DateTime.Today.Year, 1, 1);
            var endDate = new DateTime(DateTime.Today.Year, 12, 31);
            return (startDate, endDate);
        }
        else
        {
            var startOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var startDate = startOfMonth.AddDays(-7);
            var endDate = endOfMonth.AddDays(7);
            return (startDate, endDate);
        }
    }
}
