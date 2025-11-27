using ECommerce.Payment.Domain.Enums;
using ECommerce.Shared.Abstractions.Entities;

namespace ECommerce.Payment.Domain.Entities;

public class PaymentEntity : AuditableEntity
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;

    // Order Reference
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    // Payment Details
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; }
    public PaymentProvider Provider { get; set; }

    // Amount
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";

    // Provider Details
    public string? ProviderTransactionId { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? ProviderCustomerId { get; set; }

    // Card Details (last 4 digits only)
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }

    // Timestamps
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    // Error Details
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    // Refund
    public decimal RefundedAmount { get; set; }
    public string? RefundReason { get; set; }

    // Metadata
    public string? Description { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }

    // Navigation
    public ICollection<PaymentHistory> History { get; set; } = new List<PaymentHistory>();
}
