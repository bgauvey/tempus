using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface ICustomRangeRepository
{
    Task<CustomCalendarRange?> GetByIdAsync(Guid id, string userId);
    Task<List<CustomCalendarRange>> GetAllAsync(string userId);
    Task<CustomCalendarRange> CreateAsync(CustomCalendarRange range);
    Task<CustomCalendarRange> UpdateAsync(CustomCalendarRange range);
    Task DeleteAsync(Guid id, string userId);
}
