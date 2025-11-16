using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class ResourceReservationService : IResourceReservationService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IResourceReservationRepository _reservationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<ResourceReservationService> _logger;

    public ResourceReservationService(
        IResourceRepository resourceRepository,
        IResourceReservationRepository reservationRepository,
        IEventRepository eventRepository,
        ILogger<ResourceReservationService> logger)
    {
        _resourceRepository = resourceRepository;
        _reservationRepository = reservationRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<bool> CheckResourceAvailabilityAsync(Guid resourceId, int quantity, DateTime startTime, DateTime endTime, Guid? excludeEventId = null)
    {
        var resource = await _resourceRepository.GetByIdAsync(resourceId, string.Empty); // Public check
        if (resource == null || !resource.IsAvailable)
        {
            return false;
        }

        var reservedQuantity = await _reservationRepository.GetReservedQuantityAsync(resourceId, startTime, endTime);

        // If we're updating an existing event, exclude its current reservation
        if (excludeEventId.HasValue)
        {
            var existingReservations = await _reservationRepository.GetByEventIdAsync(excludeEventId.Value);
            var existingQuantity = existingReservations
                .Where(r => r.ResourceId == resourceId)
                .Sum(r => r.QuantityReserved);
            reservedQuantity -= existingQuantity;
        }

        var availableQuantity = resource.Quantity - reservedQuantity;

        _logger.LogInformation("Resource {ResourceId} has {AvailableQuantity} of {TotalQuantity} available ({ReservedQuantity} reserved)",
            resourceId, availableQuantity, resource.Quantity, reservedQuantity);

        return availableQuantity >= quantity;
    }

    public async Task<int> GetAvailableQuantityAsync(Guid resourceId, DateTime startTime, DateTime endTime)
    {
        var resource = await _resourceRepository.GetByIdAsync(resourceId, string.Empty);
        if (resource == null)
        {
            return 0;
        }

        var reservedQuantity = await _reservationRepository.GetReservedQuantityAsync(resourceId, startTime, endTime);
        return Math.Max(0, resource.Quantity - reservedQuantity);
    }

    public async Task<List<Resource>> GetAvailableResourcesAsync(ResourceType? type, string userId)
    {
        return await _resourceRepository.GetAvailableResourcesAsync(type, userId);
    }

    public async Task<ResourceReservation> ReserveResourceAsync(Guid eventId, Guid resourceId, int quantity, string? notes = null)
    {
        // Verify the event exists (using empty userId - permission check should happen at controller level)
        var eventEntity = await _eventRepository.GetByIdAsync(eventId, string.Empty);
        if (eventEntity == null)
        {
            throw new InvalidOperationException($"Event {eventId} not found");
        }

        // Check availability
        var isAvailable = await CheckResourceAvailabilityAsync(resourceId, quantity, eventEntity.StartTime, eventEntity.EndTime);
        if (!isAvailable)
        {
            throw new InvalidOperationException($"Resource {resourceId} does not have {quantity} units available for the requested time");
        }

        var reservation = new ResourceReservation
        {
            ResourceId = resourceId,
            EventId = eventId,
            QuantityReserved = quantity,
            Status = RoomBookingStatus.Confirmed,
            Notes = notes
        };

        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        _logger.LogInformation("Reserved {Quantity} units of resource {ResourceId} for event {EventId}",
            quantity, resourceId, eventId);

        return createdReservation;
    }

    public async Task CancelReservationAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new InvalidOperationException($"Resource reservation {reservationId} not found");
        }

        reservation.Status = RoomBookingStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);

        _logger.LogInformation("Cancelled resource reservation {ReservationId}", reservationId);
    }

    public async Task<List<ResourceReservation>> GetResourceReservationsAsync(Guid resourceId, DateTime startDate, DateTime endDate)
    {
        return await _reservationRepository.GetByResourceIdAndDateRangeAsync(resourceId, startDate, endDate);
    }

    public async Task<Dictionary<DateTime, int>> GetResourceUtilizationAsync(Guid resourceId, DateTime startDate, DateTime endDate)
    {
        var reservations = await _reservationRepository.GetByResourceIdAndDateRangeAsync(resourceId, startDate, endDate);

        var resource = await _resourceRepository.GetByIdAsync(resourceId, string.Empty);
        if (resource == null)
        {
            return new Dictionary<DateTime, int>();
        }

        // Group by day and calculate utilization percentage
        var utilization = reservations
            .Where(r => r.Status != RoomBookingStatus.Cancelled && r.Event != null)
            .GroupBy(r => r.Event!.StartTime.Date)
            .ToDictionary(
                g => g.Key,
                g => (int)((double)g.Sum(r => r.QuantityReserved) / resource.Quantity * 100)
            );

        return utilization;
    }
}
