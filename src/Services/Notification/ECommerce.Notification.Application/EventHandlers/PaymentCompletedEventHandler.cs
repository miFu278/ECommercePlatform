using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.Events;
using ECommerce.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Notification.Application.EventHandlers;

public class PaymentCompletedEventHandler : IEventHandler<PaymentCompletedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentCompletedEventHandler> _logger;

    public PaymentCompletedEventHandler(IEmailService emailService, ILogger<PaymentCompletedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentCompletedEvent @event)
    {
        _logger.LogInformation("Handling PaymentCompletedEvent for Payment: {PaymentId}, Order: {OrderId}", 
            @event.PaymentId, @event.OrderId);

        try
        {
            // Get info from event - in production these would be populated
            var userEmail = @event.UserEmail ?? "user@example.com";
            var orderNumber = @event.OrderNumber ?? $"ORD-{@event.OrderId.ToString().Substring(0, 8)}";
            var transactionId = @event.TransactionId ?? @event.PaymentId.ToString();

            await _emailService.SendPaymentConfirmationAsync(
                userEmail,
                orderNumber,
                @event.Amount,
                @event.PaymentMethod ?? "Online Payment",
                transactionId
            );

            _logger.LogInformation("Payment confirmation email sent for Payment: {PaymentId}", @event.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment confirmation email for Payment: {PaymentId}", @event.PaymentId);
            // Don't throw - notification failures shouldn't break the system
        }
    }
}
