using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IResourceReservationService
{
    Task<bool> CheckResourceAvailabilityAsync(Guid resourceId, int quantity, DateTime startTime, DateTime endTime, Guid? excludeEventId = null);
    Task<int> GetAvailableQuantityAsync(Guid resourceId, DateTime startTime, DateTime endTime);
    Task<List<Resource>> GetAvailableResourcesAsync(ResourceType? type, string userId);
    Task<ResourceReservation> ReserveResourceAsync(Guid eventId, Guid resourceId, int quantity, string? notes = null);
    Task CancelReservationAsync(Guid reservationId);
    Task<List<ResourceReservation>> GetResourceReservationsAsync(Guid resourceId, DateTime startDate, DateTime endDate);
    Task<Dictionary<DateTime, int>> GetResourceUtilizationAsync(Guid resourceId, DateTime startDate, DateTime endDate);
}
