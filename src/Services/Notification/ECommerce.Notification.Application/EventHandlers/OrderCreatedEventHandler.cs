using ECommerce.EventBus.Events;
using ECommerce.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Notification.Application.EventHandlers;

public class OrderCreatedEventHandler
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
            // TODO: Get user email from User Service
            var userEmail = $"user{@event.UserId}@example.com"; // Placeholder
            var customerName = "Khách hàng"; // Placeholder

            await _emailService.SendOrderConfirmationAsync(
                userEmail,
                @event.OrderNumber,
                @event.TotalAmount,
                customerName
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
