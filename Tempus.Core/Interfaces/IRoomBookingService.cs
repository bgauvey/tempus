using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IRoomBookingService
{
    Task<bool> CheckRoomAvailabilityAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeEventId = null);
    Task<List<Room>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int? minCapacity, string userId);
    Task<RoomBooking> BookRoomAsync(Guid eventId, Guid roomId, string? notes = null);
    Task CancelRoomBookingAsync(Guid bookingId);
    Task<List<RoomBooking>> GetRoomBookingsAsync(Guid roomId, DateTime startDate, DateTime endDate);
    Task<List<DateTime>> GetConflictingTimesAsync(Guid roomId, DateTime startDate, DateTime endDate);
}
