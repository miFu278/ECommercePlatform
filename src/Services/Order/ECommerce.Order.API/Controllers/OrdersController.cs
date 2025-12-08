using System.Security.Claims;
using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Application.Interfaces;
using ECommerce.Order.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Order.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        IConfiguration configuration,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _configuration = configuration;
        _logger = logger;
    }
    
    private IConfiguration Configuration => _configuration;

    /// <summary>
    /// Create a new order from cart
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = GetCurrentUserId();
        var order = await _orderService.CreateOrderAsync(userId, dto);
        
        // If payment method is banking, create payment link
        string? paymentUrl = null;
        if (dto.PaymentMethod == PaymentMethod.Banking)
        {
            try
            {
                // Call Payment Service to create payment
                using var httpClient = new HttpClient();
                var paymentRequest = new
                {
                    orderId = order.Id,
                    method = 1, // PaymentMethod.Online enum value
                    paymentToken = (string?)null
                };
                
                var paymentServiceUrl = Configuration["Services:Payment:Http"] ?? "http://localhost:5050";
                var response = await httpClient.PostAsJsonAsync($"{paymentServiceUrl}/api/payments", paymentRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var paymentResult = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
                    // PayOS checkout URL is stored in ErrorMessage field temporarily
                    paymentUrl = paymentResult?.ErrorMessage;
                    
                    _logger.LogInformation("Payment link created for order {OrderId}: {PaymentUrl}", order.Id, paymentUrl);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Payment Service returned {StatusCode}: {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment link for order {OrderId}", order.Id);
                // Continue without payment link - user can pay later or use COD
            }
        }
        
        // Return order with payment URL if available
        var result = new
        {
            order.Id,
            order.OrderNumber,
            order.Status,
            order.TotalAmount,
            paymentUrl
        };
        
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, result);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        // Check if user owns this order or is admin
        var userId = GetCurrentUserId();
        if (order.UserId != userId && !IsAdmin())
        {
            return Forbid();
        }

        return Ok(order);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
    {
        var order = await _orderService.GetOrderByNumberAsync(orderNumber);
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        // Check if user owns this order or is admin
        var userId = GetCurrentUserId();
        if (order.UserId != userId && !IsAdmin())
        {
            return Forbid();
        }

        return Ok(order);
    }

    /// <summary>
    /// Get current user's orders
    /// </summary>
    [HttpGet("my-orders")]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.GetUserOrdersAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get all orders (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var result = await _orderService.GetAllOrdersAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get orders by status (Admin only)
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetOrdersByStatus(
        OrderStatus status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var result = await _orderService.GetOrdersByStatusAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusDto dto)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, dto);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderDto dto)
    {
        try
        {
            // Check if user owns this order
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            var userId = GetCurrentUserId();
            if (order.UserId != userId && !IsAdmin())
            {
                return Forbid();
            }

            await _orderService.CancelOrderAsync(id, dto.Reason);
            return Ok(new { message = "Order cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
    /// <summary>
    /// Get order statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<OrderStatisticsDto>> GetStatistics()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var statistics = await _orderService.GetStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Get dashboard data (Admin only)
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<OrderDashboardDto>> GetDashboard()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var dashboard = await _orderService.GetDashboardAsync();
        return Ok(dashboard);
    }
}

public class CancelOrderDto
{
    public string Reason { get; set; } = string.Empty;
}

// DTO to receive response from Payment Service
public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; } // Contains PayOS checkout URL
    public string? ProviderTransactionId { get; set; }
}
