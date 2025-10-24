using Tempus.Core.Models;

namespace Tempus.Web.Services;

public interface IPdfAgendaService
{
    /// <summary>
    /// Generates a daily agenda PDF for the specified date
    /// </summary>
    /// <param name="events">List of events for the day</param>
    /// <param name="date">The date for the agenda</param>
    /// <param name="userName">Name of the user</param>
    /// <returns>PDF as byte array</returns>
    byte[] GenerateDailyAgenda(List<Event> events, DateTime date, string userName);
}
