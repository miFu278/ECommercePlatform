using System.Security.Claims;
using ECommerce.Payment.Application.DTOs;
using ECommerce.Payment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Payment.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create payment for order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        var userId = GetCurrentUserId();
        var payment = await _paymentService.CreatePaymentAsync(userId, dto);
        
        return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        // Check if user owns this payment
        var userId = GetCurrentUserId();
        if (payment.UserId != userId && !IsAdmin())
        {
            return Forbid();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Get payment by order ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(Guid orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        if (payment == null)
        {
            return NotFound(new { message = "Payment not found for this order" });
        }

        // Check if user owns this payment
        var userId = GetCurrentUserId();
        if (payment.UserId != userId && !IsAdmin())
        {
            return Forbid();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Get current user's payments
    /// </summary>
    [HttpGet("my-payments")]
    public async Task<ActionResult<PagedResultDto<PaymentDto>>> GetMyPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var result = await _paymentService.GetUserPaymentsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Refund payment (Admin only)
    /// </summary>
    [HttpPost("{id}/refund")]
    public async Task<ActionResult<PaymentDto>> RefundPayment(Guid id, [FromBody] RefundPaymentDto dto)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        try
        {
            var payment = await _paymentService.RefundPaymentAsync(id, dto);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel payment
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<PaymentDto>> CancelPayment(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            // Check if user owns this payment
            var userId = GetCurrentUserId();
            if (payment.UserId != userId && !IsAdmin())
            {
                return Forbid();
            }

            var result = await _paymentService.CancelPaymentAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PayOS Webhook - Called by PayOS when payment status changes
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookDto webhook)
    {
        try
        {
            _logger.LogInformation("PayOS Webhook received: Code={Code}, OrderCode={OrderCode}", 
                webhook.Code, webhook.Data?.OrderCode);

            // Verify webhook signature (PayOS uses checksum)
            if (!string.IsNullOrEmpty(webhook.Signature))
            {
                // TODO: Implement signature verification with PayOS checksum key
                // var isValid = VerifySignature(webhook);
                // if (!isValid) return BadRequest(new { message = "Invalid signature" });
            }

            if (webhook.Code == "00" && webhook.Data != null)
            {
                // Payment successful - find payment by transaction ID (orderCode)
                var transactionId = webhook.Data.OrderCode.ToString();
                
                // Find payment by provider transaction ID
                var payments = await _paymentService.GetUserPaymentsAsync(Guid.Empty, 1, 1000);
                var payment = payments.Items.FirstOrDefault(p => 
                    p.ProviderTransactionId == transactionId || 
                    p.PaymentNumber.Contains(transactionId));

                if (payment != null)
                {
                    // Process the payment completion
                    await _paymentService.ProcessPaymentAsync(new ProcessPaymentDto
                    {
                        PaymentId = payment.Id,
                        ProviderTransactionId = transactionId
                    });

                    _logger.LogInformation("Payment {PaymentId} completed via webhook", payment.Id);
                }
                else
                {
                    _logger.LogWarning("Payment not found for transaction: {TransactionId}", transactionId);
                }

                return Ok(new { message = "Webhook processed successfully" });
            }

            return Ok(new { message = "Webhook received" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayOS webhook");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// PayOS Return URL - Called when user returns from PayOS
    /// </summary>
    [HttpGet("return")]
    public async Task<IActionResult> PayOSReturn(
        [FromQuery] string code,
        [FromQuery] string id,
        [FromQuery] bool cancel,
        [FromQuery] string status,
        [FromQuery] long orderCode)
    {
        _logger.LogInformation("PayOS Return: Code={Code}, Status={Status}, OrderCode={OrderCode}, Cancel={Cancel}", 
            code, status, orderCode, cancel);

        if (cancel)
        {
            return Redirect($"/payment/cancelled?orderCode={orderCode}");
        }

        if (status == "PAID" || code == "00")
        {
            // Payment successful
            return Redirect($"/payment/success?orderCode={orderCode}");
        }

        return Redirect($"/payment/failed?orderCode={orderCode}&status={status}");
    }

    private Guid GetCurrentUserId()
    {
        // Try to get from API Gateway header first (trusted)
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
        {
            if (Guid.TryParse(userIdHeader, out var userId))
            {
                return userId;
            }
        }

        // Fallback to JWT claims (if called directly without Gateway)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userIdFromClaim))
        {
            return userIdFromClaim;
        }

        throw new UnauthorizedAccessException("User ID not found");
    }

    private bool IsAdmin()
    {
        // Check from API Gateway header first
        if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
        {
            return roleHeader.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        // Fallback to JWT claims
        return User.IsInRole("Admin");
    }
}

public class PayOSWebhookDto
{
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public PayOSWebhookData? Data { get; set; }
    public string? Signature { get; set; }
}

public class PayOSWebhookData
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string TransactionDateTime { get; set; } = string.Empty;
}
