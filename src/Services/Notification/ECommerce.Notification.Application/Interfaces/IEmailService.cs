namespace ECommerce.Notification.Application.Interfaces;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string email, string orderNumber, decimal totalAmount, string customerName);
    Task SendPaymentReceiptAsync(string email, string orderNumber, string paymentNumber, decimal amount);
    Task SendOrderShippedAsync(string email, string orderNumber, string trackingNumber, string customerName);
    Task SendOrderDeliveredAsync(string email, string orderNumber, string customerName);
    Task SendWelcomeEmailAsync(string email, string firstName);
}
