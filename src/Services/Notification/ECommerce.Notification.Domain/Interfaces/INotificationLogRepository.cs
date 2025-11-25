using ECommerce.Notification.Domain.Entities;

namespace ECommerce.Notification.Domain.Interfaces;

public interface INotificationLogRepository
{
    Task<NotificationLog> CreateAsync(NotificationLog log);
    Task<NotificationLog?> GetByIdAsync(string id);
    Task<List<NotificationLog>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 10);
    Task<long> GetCountByUserIdAsync(string userId);
    Task UpdateAsync(NotificationLog log);
}
