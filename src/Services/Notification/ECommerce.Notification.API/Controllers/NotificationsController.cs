using ECommerce.Notification.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Notification.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IEmailService emailService, ILogger<NotificationsController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Send email notification
    /// </summary>
    [HttpPost("email")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailDto dto)
    {
        try
        {
            await _emailService.SendEmailAsync(dto.To, dto.Subject, dto.Body, dto.IsHtml);
            return Ok(new { message = "Email sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", dto.To);
            return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
        }
    }

    /// <summary>
    /// Send order confirmation email
    /// </summary>
    [HttpPost("order-confirmation")]
    public async Task<IActionResult> SendOrderConfirmation([FromBody] OrderConfirmationDto dto)
    {
        try
        {
            var items = dto.Items?.Select(i => new OrderItemInfo
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            
            await _emailService.SendOrderConfirmationAsync(
                dto.Email, 
                dto.OrderNumber, 
                dto.CustomerName, 
                dto.TotalAmount,
                items);
            
            return Ok(new { message = "Order confirmation email sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation to {Email}", dto.Email);
            return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
        }
    }

    /// <summary>
    /// Send payment confirmation email
    /// </summary>
    [HttpPost("payment-confirmation")]
    public async Task<IActionResult> SendPaymentConfirmation([FromBody] PaymentConfirmationDto dto)
    {
        try
        {
            await _emailService.SendPaymentConfirmationAsync(
                dto.Email,
                dto.OrderNumber,
                dto.Amount,
                dto.PaymentMethod,
                dto.TransactionId);
            
            return Ok(new { message = "Payment confirmation email sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment confirmation to {Email}", dto.Email);
            return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
        }
    }

    /// <summary>
    /// Send welcome email
    /// </summary>
    [HttpPost("welcome")]
    public async Task<IActionResult> SendWelcomeEmail([FromBody] WelcomeEmailDto dto)
    {
        try
        {
            await _emailService.SendWelcomeEmailAsync(dto.Email, dto.Name);
            return Ok(new { message = "Welcome email sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", dto.Email);
            return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
        }
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "Notification Service" });
    }
}

// DTOs
public class SendEmailDto
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}

public class OrderConfirmationDto
{
    public string Email { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class PaymentConfirmationDto
{
    public string Email { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}

public class WelcomeEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
