using ECommerce.Payment.Domain.Entities;
using ECommerce.Payment.Domain.Enums;

namespace ECommerce.Payment.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentEntity?> GetByIdAsync(Guid id);
    Task<PaymentEntity?> GetByPaymentNumberAsync(string paymentNumber);
    Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<PaymentEntity>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<IEnumerable<PaymentEntity>> GetByStatusAsync(PaymentStatus status, int page = 1, int pageSize = 10);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByUserIdAsync(Guid userId);
    Task<PaymentEntity> CreateAsync(PaymentEntity payment);
    Task UpdateAsync(PaymentEntity payment);
    Task<bool> ExistsAsync(Guid id);
    Task<string> GeneratePaymentNumberAsync();
}
