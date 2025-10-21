using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<List<Event>> GetAllAsync();
    Task<List<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Event> CreateAsync(Event @event);
    Task<Event> UpdateAsync(Event @event);
    Task DeleteAsync(Guid id);
    Task<List<Event>> SearchAsync(string searchTerm);
}
