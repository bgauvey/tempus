using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, string userId);
    Task<List<Event>> GetAllAsync(string userId);
    Task<List<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, string userId);
    Task<Event> CreateAsync(Event @event);
    Task<Event> UpdateAsync(Event @event);
    Task DeleteAsync(Guid id, string userId);
    Task<List<Event>> SearchAsync(string searchTerm, string userId);
}
