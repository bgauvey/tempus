using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, string userId);
    Task<List<Notification>> GetAllAsync(string userId);
    Task<List<Notification>> GetUnreadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Notification> CreateAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(Guid id, string userId);
    Task MarkAsReadAsync(Guid id, string userId);
    Task MarkAllAsReadAsync(string userId);
}
