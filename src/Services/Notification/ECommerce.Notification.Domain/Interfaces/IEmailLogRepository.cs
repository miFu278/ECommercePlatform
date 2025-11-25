using ECommerce.Notification.Domain.Entities;

namespace ECommerce.Notification.Domain.Interfaces;

public interface IEmailLogRepository
{
    Task<EmailLog> CreateAsync(EmailLog log);
    Task<EmailLog?> GetByIdAsync(string id);
    Task<List<EmailLog>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 10);
    Task UpdateAsync(EmailLog log);
}
