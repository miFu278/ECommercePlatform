using ECommerce.Payment.Domain.Entities;
using ECommerce.Payment.Domain.Enums;

namespace ECommerce.Payment.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByPaymentNumberAsync(string paymentNumber);
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, int page = 1, int pageSize = 10);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByUserIdAsync(Guid userId);
    Task<Payment> CreateAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task<bool> ExistsAsync(Guid id);
    Task<string> GeneratePaymentNumberAsync();
}
