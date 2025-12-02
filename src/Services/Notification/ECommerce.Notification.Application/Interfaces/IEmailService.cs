namespace ECommerce.Notification.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendWelcomeEmailAsync(string email, string name);
    Task SendOrderConfirmationAsync(string email, string orderNumber, string customerName, decimal totalAmount, List<OrderItemInfo>? items = null);
    Task SendPaymentConfirmationAsync(string email, string orderNumber, decimal amount, string paymentMethod, string transactionId);
    Task SendShippingNotificationAsync(string email, string orderNumber, string trackingNumber, string carrier);
    Task SendPasswordResetAsync(string email, string resetLink);
}

public class OrderItemInfo
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
