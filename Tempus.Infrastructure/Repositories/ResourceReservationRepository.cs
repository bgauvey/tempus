using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class ResourceReservationRepository : IResourceReservationRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<ResourceReservationRepository> _logger;

    public ResourceReservationRepository(IDbContextFactory<TempusDbContext> contextFactory, ILogger<ResourceReservationRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<ResourceReservation?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResourceReservations
            .Include(rr => rr.Resource)
            .Include(rr => rr.Event)
            .FirstOrDefaultAsync(rr => rr.Id == id);
    }

    public async Task<List<ResourceReservation>> GetByEventIdAsync(Guid eventId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResourceReservations
            .Include(rr => rr.Resource)
            .Where(rr => rr.EventId == eventId)
            .ToListAsync();
    }

    public async Task<List<ResourceReservation>> GetByResourceIdAndDateRangeAsync(Guid resourceId, DateTime startDate, DateTime endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ResourceReservations
            .Include(rr => rr.Event)
            .Where(rr => rr.ResourceId == resourceId &&
                         rr.Event != null &&
                         rr.Event.StartTime >= startDate &&
                         rr.Event.StartTime <= endDate)
            .OrderBy(rr => rr.Event!.StartTime)
            .ToListAsync();
    }

    public async Task<int> GetReservedQuantityAsync(Guid resourceId, DateTime startTime, DateTime endTime)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var reservations = await context.ResourceReservations
            .Include(rr => rr.Event)
            .Where(rr => rr.ResourceId == resourceId &&
                         rr.Status != RoomBookingStatus.Cancelled &&
                         rr.Event != null &&
                         rr.Event.StartTime < endTime &&
                         rr.Event.EndTime > startTime)
            .ToListAsync();

        return reservations.Sum(rr => rr.QuantityReserved);
    }

    public async Task<ResourceReservation> CreateAsync(ResourceReservation reservation)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.ResourceReservations.Add(reservation);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created resource reservation {ReservationId} for resource {ResourceId} and event {EventId}",
            reservation.Id, reservation.ResourceId, reservation.EventId);
        return reservation;
    }

    public async Task<ResourceReservation> UpdateAsync(ResourceReservation reservation)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        reservation.UpdatedAt = DateTime.UtcNow;
        context.ResourceReservations.Update(reservation);
        await context.SaveChangesAsync();

        _logger.LogInformation("Updated resource reservation {ReservationId}", reservation.Id);
        return reservation;
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var reservation = await context.ResourceReservations.FindAsync(id);
        if (reservation != null)
        {
            context.ResourceReservations.Remove(reservation);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted resource reservation {ReservationId}", id);
        }
    }

    public async Task DeleteByEventIdAsync(Guid eventId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var reservations = await context.ResourceReservations
            .Where(rr => rr.EventId == eventId)
            .ToListAsync();

        if (reservations.Any())
        {
            context.ResourceReservations.RemoveRange(reservations);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} resource reservations for event {EventId}", reservations.Count, eventId);
        }
    }
}
