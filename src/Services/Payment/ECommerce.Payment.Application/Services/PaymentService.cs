using AutoMapper;
using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.Events;
using ECommerce.Payment.Application.DTOs;
using ECommerce.Payment.Application.Interfaces;
using ECommerce.Payment.Domain.Enums;
using ECommerce.Payment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Payment.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IEventBus _eventBus;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPaymentGateway paymentGateway,
        IEventBus eventBus,
        ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentGateway = paymentGateway;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(Guid userId, CreatePaymentDto dto)
    {
        // TODO: Get order info from Order Service
        // For now, use dummy data
        var orderNumber = $"ORD{DateTime.UtcNow:yyyyMMdd}-0001";
        var amount = 100000m; // 100,000 VND

        var payment = new Domain.Entities.PaymentEntity
        {
            Id = Guid.NewGuid(),
            PaymentNumber = await _unitOfWork.Payments.GeneratePaymentNumberAsync(),
            OrderId = dto.OrderId,
            OrderNumber = orderNumber,
            UserId = userId,
            Status = PaymentStatus.Pending,
            Method = dto.Method,
            Provider = PaymentProvider.PayOS,
            Amount = amount,
            Currency = "VND",
            Description = $"Thanh toán đơn hàng {orderNumber}",
            CreatedAt = DateTime.UtcNow
        };

        // Add initial history
        payment.History.Add(new Domain.Entities.PaymentHistory
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Status = PaymentStatus.Pending,
            Notes = "Payment created",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Payments.CreateAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Process payment with gateway
        var gatewayRequest = new PaymentGatewayRequest
        {
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentToken = dto.PaymentToken,
            CustomerEmail = $"user{userId}@example.com",
            CustomerName = "Customer",
            Description = payment.Description
        };

        var gatewayResult = await _paymentGateway.ProcessPaymentAsync(gatewayRequest);

        if (gatewayResult.Success)
        {
            payment.ProviderTransactionId = gatewayResult.TransactionId;
            payment.ProviderPaymentId = gatewayResult.PaymentId;
            payment.Status = PaymentStatus.Processing;
            payment.ProcessedAt = DateTime.UtcNow;

            payment.History.Add(new Domain.Entities.PaymentHistory
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                Status = PaymentStatus.Processing,
                Notes = $"Payment processing. Checkout URL: {gatewayResult.ErrorMessage}",
                ChangedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
        }

        var result = _mapper.Map<PaymentDto>(payment);
        
        // Store checkout URL temporarily in ErrorMessage
        if (gatewayResult.Success && !string.IsNullOrEmpty(gatewayResult.ErrorMessage))
        {
            result.ErrorMessage = gatewayResult.ErrorMessage; // Checkout URL
        }

        return result;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto dto)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(dto.PaymentId);
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.ProviderTransactionId = dto.ProviderTransactionId;

        payment.History.Add(new Domain.Entities.PaymentHistory
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Status = PaymentStatus.Completed,
            Notes = "Payment completed successfully",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Publish event
        await _eventBus.PublishAsync(new PaymentCompletedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Currency = payment.Currency
        });

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id);
        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto?> GetPaymentByOrderIdAsync(Guid orderId)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PagedResultDto<PaymentDto>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        var payments = await _unitOfWork.Payments.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await _unitOfWork.Payments.GetCountByUserIdAsync(userId);

        return new PagedResultDto<PaymentDto>
        {
            Items = _mapper.Map<List<PaymentDto>>(payments),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaymentDto> RefundPaymentAsync(Guid id, RefundPaymentDto dto)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id);
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Only completed payments can be refunded");
        }

        // Process refund with gateway
        if (!string.IsNullOrEmpty(payment.ProviderTransactionId))
        {
            var refundResult = await _paymentGateway.RefundPaymentAsync(
                payment.ProviderTransactionId, 
                dto.Amount);

            if (!refundResult.Success)
            {
                throw new InvalidOperationException($"Refund failed: {refundResult.ErrorMessage}");
            }
        }

        payment.Status = dto.Amount >= payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartialRefund;
        payment.RefundedAmount = dto.Amount;
        payment.RefundReason = dto.Reason;
        payment.RefundedAt = DateTime.UtcNow;

        payment.History.Add(new Domain.Entities.PaymentHistory
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Status = payment.Status,
            Notes = $"Refunded {dto.Amount:N0} VND. Reason: {dto.Reason}",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CancelPaymentAsync(Guid id)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id);
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel completed payment. Use refund instead.");
        }

        payment.Status = PaymentStatus.Cancelled;
        payment.UpdatedAt = DateTime.UtcNow;

        payment.History.Add(new Domain.Entities.PaymentHistory
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Status = PaymentStatus.Cancelled,
            Notes = "Payment cancelled by user",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }
}
