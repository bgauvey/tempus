using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class RoomBookingRepository : IRoomBookingRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<RoomBookingRepository> _logger;

    public RoomBookingRepository(IDbContextFactory<TempusDbContext> contextFactory, ILogger<RoomBookingRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<RoomBooking?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RoomBookings
            .Include(rb => rb.Room)
            .Include(rb => rb.Event)
            .FirstOrDefaultAsync(rb => rb.Id == id);
    }

    public async Task<RoomBooking?> GetByEventIdAsync(Guid eventId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RoomBookings
            .Include(rb => rb.Room)
            .Include(rb => rb.Event)
            .FirstOrDefaultAsync(rb => rb.EventId == eventId);
    }

    public async Task<List<RoomBooking>> GetByRoomIdAndDateRangeAsync(Guid roomId, DateTime startDate, DateTime endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RoomBookings
            .Include(rb => rb.Event)
            .Where(rb => rb.RoomId == roomId &&
                         rb.Event != null &&
                         rb.Event.StartTime >= startDate &&
                         rb.Event.StartTime <= endDate)
            .OrderBy(rb => rb.Event!.StartTime)
            .ToListAsync();
    }

    public async Task<RoomBooking> CreateAsync(RoomBooking booking)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.RoomBookings.Add(booking);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created room booking {BookingId} for room {RoomId} and event {EventId}",
            booking.Id, booking.RoomId, booking.EventId);
        return booking;
    }

    public async Task<RoomBooking> UpdateAsync(RoomBooking booking)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        booking.UpdatedAt = DateTime.UtcNow;
        context.RoomBookings.Update(booking);
        await context.SaveChangesAsync();

        _logger.LogInformation("Updated room booking {BookingId}", booking.Id);
        return booking;
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var booking = await context.RoomBookings.FindAsync(id);
        if (booking != null)
        {
            context.RoomBookings.Remove(booking);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted room booking {BookingId}", id);
        }
    }

    public async Task DeleteByEventIdAsync(Guid eventId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var bookings = await context.RoomBookings
            .Where(rb => rb.EventId == eventId)
            .ToListAsync();

        if (bookings.Any())
        {
            context.RoomBookings.RemoveRange(bookings);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} room bookings for event {EventId}", bookings.Count, eventId);
        }
    }
}
