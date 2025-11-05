using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface ICalendarIntegrationRepository
{
    Task<CalendarIntegration?> GetByIdAsync(Guid id);
    Task<List<CalendarIntegration>> GetAllByUserIdAsync(string userId);
    Task<CalendarIntegration?> GetByProviderAsync(string userId, string provider);
    Task<CalendarIntegration> CreateAsync(CalendarIntegration integration);
    Task<CalendarIntegration> UpdateAsync(CalendarIntegration integration);
    Task DeleteAsync(Guid id);
}
