using ECommerce.Order.Grpc;
using ECommerce.Payment.Application.Interfaces;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Payment.Infrastructure.GrpcClients;

public class OrderGrpcClient : IOrderService, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly OrderGrpcService.OrderGrpcServiceClient _client;
    private readonly ILogger<OrderGrpcClient> _logger;

    public OrderGrpcClient(IConfiguration configuration, ILogger<OrderGrpcClient> logger)
    {
        _logger = logger;
        
        var orderServiceUrl = configuration["Services:Order:Grpc"] ?? "http://localhost:5013";
        _logger.LogInformation("Connecting to Order gRPC service at {Url}", orderServiceUrl);
        
        _channel = GrpcChannel.ForAddress(orderServiceUrl);
        _client = new OrderGrpcService.OrderGrpcServiceClient(_channel);
    }

    public async Task<OrderInfoDto?> GetOrderInfoAsync(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Getting order info for OrderId: {OrderId}", orderId);
            
            var request = new OrderInfoRequest { OrderId = orderId.ToString() };
            var response = await _client.GetOrderInfoAsync(request);

            if (!response.Found)
            {
                _logger.LogWarning("Order not found: {OrderId}", orderId);
                return null;
            }

            var orderInfo = new OrderInfoDto
            {
                Id = Guid.Parse(response.Id),
                OrderNumber = response.OrderNumber,
                UserId = Guid.Parse(response.UserId),
                Status = response.Status,
                PaymentStatus = response.PaymentStatus,
                Subtotal = (decimal)response.Subtotal,
                ShippingCost = (decimal)response.ShippingCost,
                Tax = (decimal)response.Tax,
                Discount = (decimal)response.Discount,
                TotalAmount = (decimal)response.TotalAmount,
                Currency = response.Currency,
                ShippingFullName = response.ShippingFullName,
                ShippingPhone = response.ShippingPhone,
                ShippingAddress = response.ShippingAddress,
                ShippingCity = response.ShippingCity,
                ShippingCountry = response.ShippingCountry
            };

            foreach (var item in response.Items)
            {
                orderInfo.Items.Add(new OrderItemInfoDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Sku = item.Sku,
                    Quantity = item.Quantity,
                    UnitPrice = (decimal)item.UnitPrice,
                    TotalPrice = (decimal)item.TotalPrice
                });
            }

            _logger.LogInformation("Order info retrieved: {OrderNumber}, Total: {Total}", 
                orderInfo.OrderNumber, orderInfo.TotalAmount);
            
            return orderInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order info for OrderId: {OrderId}", orderId);
            return null;
        }
    }

    public async Task<bool> UpdatePaymentStatusAsync(Guid orderId, string paymentStatus, string transactionId)
    {
        try
        {
            _logger.LogInformation("Updating payment status for OrderId: {OrderId}, Status: {Status}", 
                orderId, paymentStatus);
            
            var request = new UpdatePaymentStatusRequest
            {
                OrderId = orderId.ToString(),
                PaymentStatus = paymentStatus,
                PaymentTransactionId = transactionId
            };

            var response = await _client.UpdatePaymentStatusAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Payment status updated successfully for OrderId: {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("Failed to update payment status: {Message}", response.Message);
            }

            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status for OrderId: {OrderId}", orderId);
            return false;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
