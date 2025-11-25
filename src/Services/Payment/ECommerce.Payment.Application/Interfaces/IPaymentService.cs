using ECommerce.Payment.Application.DTOs;
using ECommerce.Payment.Domain.Enums;

namespace ECommerce.Payment.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> CreatePaymentAsync(Guid userId, CreatePaymentDto dto);
    Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto dto);
    Task<PaymentDto?> GetPaymentByIdAsync(Guid id);
    Task<PaymentDto?> GetPaymentByOrderIdAsync(Guid orderId);
    Task<PagedResultDto<PaymentDto>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<PaymentDto> RefundPaymentAsync(Guid id, RefundPaymentDto dto);
    Task<PaymentDto> CancelPaymentAsync(Guid id);
}
