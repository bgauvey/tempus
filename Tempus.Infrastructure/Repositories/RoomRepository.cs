using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<RoomRepository> _logger;

    public RoomRepository(IDbContextFactory<TempusDbContext> contextFactory, ILogger<RoomRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Room?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Rooms
            .Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<List<Room>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Rooms
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int? minCapacity, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get all rooms that match capacity requirement
        var query = context.Rooms
            .Where(r => r.UserId == userId && r.IsAvailable);

        if (minCapacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        }

        var rooms = await query.ToListAsync();

        // Check for booking conflicts
        var availableRooms = new List<Room>();
        foreach (var room in rooms)
        {
            var hasConflict = await context.RoomBookings
                .Where(rb => rb.RoomId == room.Id &&
                             rb.Status != Core.Enums.RoomBookingStatus.Cancelled &&
                             rb.Event != null &&
                             rb.Event.StartTime < endTime &&
                             rb.Event.EndTime > startTime)
                .AnyAsync();

            if (!hasConflict)
            {
                availableRooms.Add(room);
            }
        }

        return availableRooms;
    }

    public async Task<Room> CreateAsync(Room room)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created room {RoomId} with name {RoomName}", room.Id, room.Name);
        return room;
    }

    public async Task<Room> UpdateAsync(Room room)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        room.UpdatedAt = DateTime.UtcNow;
        context.Rooms.Update(room);
        await context.SaveChangesAsync();

        _logger.LogInformation("Updated room {RoomId}", room.Id);
        return room;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var room = await context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (room != null)
        {
            context.Rooms.Remove(room);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted room {RoomId} with name {RoomName}", id, room.Name);
        }
    }

    public async Task<bool> RoomExistsAsync(string name, string userId, Guid? excludeId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Rooms.Where(r => r.Name == name && r.UserId == userId);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
