using ECommerce.Order.Application.DTOs;

namespace ECommerce.Order.Application.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request);
}
