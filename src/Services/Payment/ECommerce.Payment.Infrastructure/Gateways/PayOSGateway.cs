using ECommerce.Payment.Application.Interfaces;
using ECommerce.Payment.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;

namespace ECommerce.Payment.Infrastructure.Gateways;

public class PayOSGateway : IPaymentGateway
{
    private readonly PayOS _payOS;
    private readonly ILogger<PayOSGateway> _logger;

    public PayOSGateway(IConfiguration configuration, ILogger<PayOSGateway> logger)
    {
        _logger = logger;
        
        var clientId = configuration["PayOS:ClientId"] ?? throw new InvalidOperationException("PayOS ClientId not configured");
        var apiKey = configuration["PayOS:ApiKey"] ?? throw new InvalidOperationException("PayOS ApiKey not configured");
        var checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new InvalidOperationException("PayOS ChecksumKey not configured");
        
        _payOS = new PayOS(clientId, apiKey, checksumKey);
    }

    public async Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentGatewayRequest request)
    {
        try
        {
            // Generate unique order code (PayOS requires int)
            var orderCode = GenerateOrderCode();
            
            // Create payment data
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: (int)request.Amount, // PayOS uses int for VND
                description: request.Description,
                items: new List<ItemData>
                {
                    new ItemData(request.Description, 1, (int)request.Amount)
                },
                cancelUrl: request.CancelUrl ?? "http://localhost:3000/payment/cancel",
                returnUrl: request.ReturnUrl ?? "http://localhost:3000/payment/success"
            );

            // Create payment link
            var createPaymentResult = await _payOS.createPaymentLink(paymentData);

            _logger.LogInformation("PayOS payment link created: {CheckoutUrl}", createPaymentResult.checkoutUrl);

            return new PaymentGatewayResult
            {
                Success = true,
                TransactionId = orderCode.ToString(),
                PaymentId = createPaymentResult.paymentLinkId,
                Status = PaymentStatus.Pending,
                ErrorMessage = createPaymentResult.checkoutUrl // Store checkout URL in ErrorMessage temporarily
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayOS payment processing failed");
            
            return new PaymentGatewayResult
            {
                Success = false,
                Status = PaymentStatus.Failed,
                ErrorCode = "PAYOS_ERROR",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentGatewayResult> RefundPaymentAsync(string transactionId, decimal amount)
    {
        try
        {
            if (!long.TryParse(transactionId, out var orderCode))
            {
                throw new ArgumentException("Invalid transaction ID");
            }

            // PayOS refund (cancel payment)
            var cancelResult = await _payOS.cancelPaymentLink(orderCode);

            return new PaymentGatewayResult
            {
                Success = true,
                TransactionId = transactionId,
                Status = PaymentStatus.Refunded
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayOS refund failed for transaction {TransactionId}", transactionId);
            
            return new PaymentGatewayResult
            {
                Success = false,
                Status = PaymentStatus.Failed,
                ErrorCode = "REFUND_FAILED",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentGatewayResult> GetPaymentStatusAsync(string transactionId)
    {
        try
        {
            if (!long.TryParse(transactionId, out var orderCode))
            {
                throw new ArgumentException("Invalid transaction ID");
            }

            var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

            var status = paymentInfo.status switch
            {
                "PAID" => PaymentStatus.Completed,
                "PENDING" => PaymentStatus.Pending,
                "PROCESSING" => PaymentStatus.Processing,
                "CANCELLED" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Failed
            };

            return new PaymentGatewayResult
            {
                Success = true,
                TransactionId = transactionId,
                PaymentId = paymentInfo.id,
                Status = status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PayOS payment status for {TransactionId}", transactionId);
            
            return new PaymentGatewayResult
            {
                Success = false,
                Status = PaymentStatus.Failed,
                ErrorCode = "STATUS_CHECK_FAILED",
                ErrorMessage = ex.Message
            };
        }
    }

    private long GenerateOrderCode()
    {
        // Generate unique order code using timestamp
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
