using ECommerce.Order.Application.Interfaces;
using ECommerce.Order.Domain.Enums;
using ECommerce.Order.Grpc;
using Grpc.Core;

namespace ECommerce.Order.API.Grpc;

public class OrderGrpcService : ECommerce.Order.Grpc.OrderGrpcService.OrderGrpcServiceBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderGrpcService> _logger;

    public OrderGrpcService(IOrderService orderService, ILogger<OrderGrpcService> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public override async Task<OrderInfoResponse> GetOrderInfo(OrderInfoRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetOrderInfo called for OrderId: {OrderId}", request.OrderId);

        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            return new OrderInfoResponse { Found = false };
        }

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return new OrderInfoResponse { Found = false };
        }

        var response = new OrderInfoResponse
        {
            Found = true,
            Id = order.Id.ToString(),
            OrderNumber = order.OrderNumber,
            UserId = order.UserId.ToString(),
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            Subtotal = (double)order.Subtotal,
            ShippingCost = (double)order.ShippingCost,
            Tax = (double)order.Tax,
            Discount = (double)order.Discount,
            TotalAmount = (double)order.TotalAmount,
            Currency = order.Currency,
            ShippingFullName = order.ShippingAddress?.FullName ?? string.Empty,
            ShippingPhone = order.ShippingAddress?.Phone ?? string.Empty,
            ShippingAddress = $"{order.ShippingAddress?.AddressLine1}, {order.ShippingAddress?.AddressLine2}",
            ShippingCity = order.ShippingAddress?.City ?? string.Empty,
            ShippingCountry = order.ShippingAddress?.Country ?? string.Empty
        };

        foreach (var item in order.Items)
        {
            response.Items.Add(new OrderItemInfo
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Sku = item.Sku,
                Quantity = item.Quantity,
                UnitPrice = (double)item.UnitPrice,
                TotalPrice = (double)item.TotalPrice
            });
        }

        return response;
    }

    public override async Task<UpdatePaymentStatusResponse> UpdatePaymentStatus(
        UpdatePaymentStatusRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC UpdatePaymentStatus called for OrderId: {OrderId}, Status: {Status}", 
            request.OrderId, request.PaymentStatus);

        try
        {
            if (!Guid.TryParse(request.OrderId, out var orderId))
            {
                return new UpdatePaymentStatusResponse 
                { 
                    Success = false, 
                    Message = "Invalid order ID" 
                };
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return new UpdatePaymentStatusResponse 
                { 
                    Success = false, 
                    Message = "Order not found" 
                };
            }

            // Update order status based on payment status
            var newStatus = request.PaymentStatus switch
            {
                "Completed" => OrderStatus.Processing,
                "Failed" => OrderStatus.Pending,
                "Cancelled" => OrderStatus.Cancelled,
                _ => order.Status
            };

            if (request.PaymentStatus == "Completed" && order.Status == OrderStatus.Pending)
            {
                await _orderService.UpdateOrderStatusAsync(orderId, new Application.DTOs.UpdateOrderStatusDto
                {
                    Status = newStatus,
                    Notes = $"Payment completed. Transaction ID: {request.PaymentTransactionId}"
                });
            }

            return new UpdatePaymentStatusResponse 
            { 
                Success = true, 
                Message = "Payment status updated successfully" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status for order {OrderId}", request.OrderId);
            return new UpdatePaymentStatusResponse 
            { 
                Success = false, 
                Message = ex.Message 
            };
        }
    }
}
