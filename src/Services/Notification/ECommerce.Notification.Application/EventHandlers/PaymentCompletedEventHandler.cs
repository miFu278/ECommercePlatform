using ECommerce.EventBus.Events;
using ECommerce.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Notification.Application.EventHandlers;

public class PaymentCompletedEventHandler
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
        _logger.LogInformation("Handling PaymentCompletedEvent for Payment: {PaymentId}", @event.PaymentId);

        try
        {
            // TODO: Get user email and order number from services
            var userEmail = "user@example.com"; // Placeholder
            var orderNumber = "ORD20241124-0001"; // Placeholder
            var paymentNumber = $"PAY{@event.PaymentId.ToString().Substring(0, 8)}";

            await _emailService.SendPaymentReceiptAsync(
                userEmail,
                orderNumber,
                paymentNumber,
                @event.Amount
            );

            _logger.LogInformation("Payment receipt email sent for Payment: {PaymentId}", @event.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment receipt email for Payment: {PaymentId}", @event.PaymentId);
            // Don't throw - notification failures shouldn't break the system
        }
    }
}
