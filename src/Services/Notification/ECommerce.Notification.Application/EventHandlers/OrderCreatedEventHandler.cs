using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.Events;
using ECommerce.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Notification.Application.EventHandlers;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(IEmailService emailService, ILogger<OrderCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation("Handling OrderCreatedEvent for Order: {OrderNumber}", @event.OrderNumber);

        try
        {
            // Get user email - in production, this would come from User Service
            var userEmail = @event.UserEmail ?? $"user{@event.UserId}@example.com";
            var customerName = @event.CustomerName ?? "Khách hàng";

            // Convert event items to email items
            var items = @event.Items?.Select(i => new OrderItemInfo
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            await _emailService.SendOrderConfirmationAsync(
                userEmail,
                @event.OrderNumber,
                customerName,
                @event.TotalAmount,
                items
            );

            _logger.LogInformation("Order confirmation email sent for Order: {OrderNumber}", @event.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for Order: {OrderNumber}", @event.OrderNumber);
            // Don't throw - notification failures shouldn't break the system
        }
    }
}
