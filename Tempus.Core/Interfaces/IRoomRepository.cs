using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, string userId);
    Task<List<Room>> GetAllAsync(string userId);
    Task<List<Room>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int? minCapacity, string userId);
    Task<Room> CreateAsync(Room room);
    Task<Room> UpdateAsync(Room room);
    Task DeleteAsync(Guid id, string userId);
    Task<bool> RoomExistsAsync(string name, string userId, Guid? excludeId = null);
}
