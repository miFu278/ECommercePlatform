using ECommerce.Payment.Domain.Enums;

namespace ECommerce.Payment.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    
    public PaymentStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public string MethodDisplay { get; set; } = string.Empty;
    public PaymentProvider Provider { get; set; }
    public string ProviderDisplay { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    
    public string? ProviderTransactionId { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    
    public decimal RefundedAmount { get; set; }
    public string? RefundReason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePaymentDto
{
    public Guid OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public string? PaymentToken { get; set; }  // Stripe token or PayPal token
}

public class ProcessPaymentDto
{
    public Guid PaymentId { get; set; }
    public string? ProviderTransactionId { get; set; }
}

public class RefundPaymentDto
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
