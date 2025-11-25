using ECommerce.Payment.Domain.Enums;

namespace ECommerce.Payment.Application.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentGatewayRequest request);
    Task<PaymentGatewayResult> RefundPaymentAsync(string transactionId, decimal amount);
    Task<PaymentGatewayResult> GetPaymentStatusAsync(string transactionId);
}

public class PaymentGatewayRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? PaymentToken { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }  // For PayOS redirect
    public string? CancelUrl { get; set; }  // For PayOS redirect
}

public class PaymentGatewayResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
}
