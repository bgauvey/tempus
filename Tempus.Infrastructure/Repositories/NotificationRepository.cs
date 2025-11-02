using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;

    public NotificationRepository(IDbContextFactory<TempusDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Include(n => n.Event)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    public async Task<List<Notification>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Include(n => n.Event)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Include(n => n.Event)
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        return notification;
    }

    public async Task UpdateAsync(Notification notification)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Notifications.Update(notification);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var notification = await context.Notifications
            .Include(n => n.Event)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification != null)
        {
            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAsReadAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var notification = await context.Notifications
            .Include(n => n.Event)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            context.Notifications.Update(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var unreadNotifications = await context.Notifications
            .Include(n => n.Event)
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync();
    }
}
