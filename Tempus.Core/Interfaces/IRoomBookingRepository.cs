using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IRoomBookingRepository
{
    Task<RoomBooking?> GetByIdAsync(Guid id);
    Task<RoomBooking?> GetByEventIdAsync(Guid eventId);
    Task<List<RoomBooking>> GetByRoomIdAndDateRangeAsync(Guid roomId, DateTime startDate, DateTime endDate);
    Task<RoomBooking> CreateAsync(RoomBooking booking);
    Task<RoomBooking> UpdateAsync(RoomBooking booking);
    Task DeleteAsync(Guid id);
    Task DeleteByEventIdAsync(Guid eventId);
}
