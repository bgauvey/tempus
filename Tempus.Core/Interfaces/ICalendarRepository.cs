using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface ICalendarRepository
{
    Task<Calendar?> GetByIdAsync(Guid id, string userId);
    Task<List<Calendar>> GetAllAsync(string userId);
    Task<Calendar?> GetDefaultCalendarAsync(string userId);
    Task<Calendar> CreateAsync(Calendar calendar);
    Task<Calendar> UpdateAsync(Calendar calendar);
    Task DeleteAsync(Guid id, string userId);
    Task<int> GetEventCountAsync(Guid calendarId, string userId);
    Task<bool> SetDefaultCalendarAsync(Guid calendarId, string userId);
    Task<int> UpdateSortOrderAsync(List<(Guid Id, int SortOrder)> updates, string userId);
    Task<Calendar> EnsureDefaultCalendarExistsAsync(string userId, string userEmail);
}
