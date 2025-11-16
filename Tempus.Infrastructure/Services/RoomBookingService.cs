using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Infrastructure.Services;

public class RoomBookingService : IRoomBookingService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<RoomBookingService> _logger;

    public RoomBookingService(
        IRoomRepository roomRepository,
        IRoomBookingRepository bookingRepository,
        IEventRepository eventRepository,
        ILogger<RoomBookingService> logger)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<bool> CheckRoomAvailabilityAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeEventId = null)
    {
        var bookings = await _bookingRepository.GetByRoomIdAndDateRangeAsync(
            roomId,
            startTime.AddDays(-1), // Expand range for safety
            endTime.AddDays(1));

        var conflicts = bookings.Where(b =>
            b.Status != RoomBookingStatus.Cancelled &&
            b.Event != null &&
            b.EventId != excludeEventId && // Exclude current event when updating
            b.Event.StartTime < endTime &&
            b.Event.EndTime > startTime);

        var hasConflict = conflicts.Any();

        if (hasConflict)
        {
            _logger.LogWarning("Room {RoomId} has conflicts for time {StartTime} to {EndTime}",
                roomId, startTime, endTime);
        }

        return !hasConflict;
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int? minCapacity, string userId)
    {
        var availableRooms = await _roomRepository.GetAvailableRoomsAsync(startTime, endTime, minCapacity, userId);

        _logger.LogInformation("Found {Count} available rooms for {StartTime} to {EndTime} with capacity >= {MinCapacity}",
            availableRooms.Count, startTime, endTime, minCapacity ?? 0);

        return availableRooms;
    }

    public async Task<RoomBooking> BookRoomAsync(Guid eventId, Guid roomId, string? notes = null)
    {
        // Verify the event exists (using empty userId - permission check should happen at controller level)
        var eventEntity = await _eventRepository.GetByIdAsync(eventId, string.Empty);
        if (eventEntity == null)
        {
            throw new InvalidOperationException($"Event {eventId} not found");
        }

        // Check availability
        var isAvailable = await CheckRoomAvailabilityAsync(roomId, eventEntity.StartTime, eventEntity.EndTime);
        if (!isAvailable)
        {
            throw new InvalidOperationException($"Room {roomId} is not available for the requested time");
        }

        var booking = new RoomBooking
        {
            RoomId = roomId,
            EventId = eventId,
            Status = RoomBookingStatus.Confirmed,
            ConfirmationDate = DateTime.UtcNow,
            Notes = notes
        };

        var createdBooking = await _bookingRepository.CreateAsync(booking);

        _logger.LogInformation("Booked room {RoomId} for event {EventId}", roomId, eventId);

        return createdBooking;
    }

    public async Task CancelRoomBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
        {
            throw new InvalidOperationException($"Room booking {bookingId} not found");
        }

        booking.Status = RoomBookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);

        _logger.LogInformation("Cancelled room booking {BookingId}", bookingId);
    }

    public async Task<List<RoomBooking>> GetRoomBookingsAsync(Guid roomId, DateTime startDate, DateTime endDate)
    {
        return await _bookingRepository.GetByRoomIdAndDateRangeAsync(roomId, startDate, endDate);
    }

    public async Task<List<DateTime>> GetConflictingTimesAsync(Guid roomId, DateTime startDate, DateTime endDate)
    {
        var bookings = await _bookingRepository.GetByRoomIdAndDateRangeAsync(roomId, startDate, endDate);

        var conflictTimes = bookings
            .Where(b => b.Status != RoomBookingStatus.Cancelled && b.Event != null)
            .Select(b => b.Event!.StartTime)
            .OrderBy(dt => dt)
            .ToList();

        return conflictTimes;
    }
}
