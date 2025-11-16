using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IResourceReservationRepository
{
    Task<ResourceReservation?> GetByIdAsync(Guid id);
    Task<List<ResourceReservation>> GetByEventIdAsync(Guid eventId);
    Task<List<ResourceReservation>> GetByResourceIdAndDateRangeAsync(Guid resourceId, DateTime startDate, DateTime endDate);
    Task<int> GetReservedQuantityAsync(Guid resourceId, DateTime startTime, DateTime endTime);
    Task<ResourceReservation> CreateAsync(ResourceReservation reservation);
    Task<ResourceReservation> UpdateAsync(ResourceReservation reservation);
    Task DeleteAsync(Guid id);
    Task DeleteByEventIdAsync(Guid eventId);
}
