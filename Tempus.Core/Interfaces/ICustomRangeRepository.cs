using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface ICustomRangeRepository
{
    Task<CustomCalendarRange?> GetByIdAsync(Guid id);
    Task<List<CustomCalendarRange>> GetAllAsync();
    Task<CustomCalendarRange> CreateAsync(CustomCalendarRange range);
    Task<CustomCalendarRange> UpdateAsync(CustomCalendarRange range);
    Task DeleteAsync(Guid id);
}
