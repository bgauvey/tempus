using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface ISettingsService
{
    Task<CalendarSettings?> GetUserSettingsAsync(string userId);
    Task<CalendarSettings> CreateOrUpdateSettingsAsync(CalendarSettings settings);
    Task<CalendarSettings> GetOrCreateDefaultSettingsAsync(string userId);
}
