using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly TempusDbContext _context;

    public NotificationRepository(TempusDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, string userId)
    {
        return await _context.Notifications
            .Include(n => n.Event)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    public async Task<List<Notification>> GetAllAsync(string userId)
    {
        return await _context.Notifications
            .Include(n => n.Event)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync(string userId)
    {
        return await _context.Notifications
            .Include(n => n.Event)
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var notification = await GetByIdAsync(id, userId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAsReadAsync(Guid id, string userId)
    {
        var notification = await GetByIdAsync(id, userId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await UpdateAsync(notification);
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await GetUnreadAsync(userId);
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }
}
