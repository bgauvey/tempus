using Tempus.Core.Models;

namespace Tempus.Web.Services.Pdf;

/// <summary>
/// Options for customizing print output
/// </summary>
public class PrintOptions
{
    public bool IncludeDescription { get; set; } = true;
    public bool IncludeLocation { get; set; } = true;
    public bool IncludeAttendees { get; set; } = true;
    public bool IncludeNotesSection { get; set; } = true;
    public bool IncludeCompletedEvents { get; set; } = false;
    public bool ShowTimeSlots { get; set; } = true;
    public bool CompactView { get; set; } = false;
    public string TimeFormat { get; set; } = "12"; // "12" or "24"
}

public interface IPdfAgendaService
{
    /// <summary>
    /// Generates a daily agenda PDF for the specified date
    /// </summary>
    /// <param name="events">List of events for the day</param>
    /// <param name="date">The date for the agenda</param>
    /// <param name="userName">Name of the user</param>
    /// <param name="options">Optional print customization options</param>
    /// <returns>PDF as byte array</returns>
    byte[] GenerateDailyAgenda(List<Event> events, DateTime date, string userName, PrintOptions? options = null);

    /// <summary>
    /// Generates a weekly agenda PDF for the specified week
    /// </summary>
    /// <param name="events">List of events for the week</param>
    /// <param name="weekStart">The start date of the week</param>
    /// <param name="userName">Name of the user</param>
    /// <param name="options">Optional print customization options</param>
    /// <returns>PDF as byte array</returns>
    byte[] GenerateWeeklyAgenda(List<Event> events, DateTime weekStart, string userName, PrintOptions? options = null);

    /// <summary>
    /// Generates a monthly calendar PDF for the specified month
    /// </summary>
    /// <param name="events">List of events for the month</param>
    /// <param name="month">The month to generate</param>
    /// <param name="year">The year</param>
    /// <param name="userName">Name of the user</param>
    /// <param name="options">Optional print customization options</param>
    /// <returns>PDF as byte array</returns>
    byte[] GenerateMonthlyCalendar(List<Event> events, int month, int year, string userName, PrintOptions? options = null);

    /// <summary>
    /// Generates a custom date range agenda PDF
    /// </summary>
    /// <param name="events">List of events for the date range</param>
    /// <param name="startDate">Start of the date range</param>
    /// <param name="endDate">End of the date range</param>
    /// <param name="userName">Name of the user</param>
    /// <param name="options">Optional print customization options</param>
    /// <returns>PDF as byte array</returns>
    byte[] GenerateDateRangeAgenda(List<Event> events, DateTime startDate, DateTime endDate, string userName, PrintOptions? options = null);
}
