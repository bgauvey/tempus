using Tempus.Core.Models;
using Tempus.Core.Enums;

namespace Tempus.Core.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, string userId);
    Task<List<Event>> GetAllAsync(string userId);
    Task<List<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, string userId);
    Task<List<Event>> GetEventsByDateRangeAndCalendarsAsync(DateTime startDate, DateTime endDate, List<Guid> calendarIds);
    Task<Event> CreateAsync(Event @event);
    Task<Event> UpdateAsync(Event @event);
    Task DeleteAsync(Guid id, string userId);
    Task<List<Event>> SearchAsync(string searchTerm, string userId);
    Task<List<Event>> AdvancedSearchAsync(AdvancedSearchFilter filter, string userId);

    // Bulk operations
    Task<List<Event>> GetByIdsAsync(List<Guid> ids, string userId);
    Task<int> BulkDeleteAsync(List<Guid> ids, string userId);
    Task<int> BulkUpdatePriorityAsync(List<Guid> ids, Priority priority, string userId);
    Task<int> BulkUpdateEventTypeAsync(List<Guid> ids, EventType eventType, string userId);
    Task<int> BulkUpdateColorAsync(List<Guid> ids, string color, string userId);
    Task<int> BulkCompleteAsync(List<Guid> ids, bool isCompleted, string userId);
    Task<int> BulkMoveAsync(List<Guid> ids, TimeSpan offset, string userId);
}
