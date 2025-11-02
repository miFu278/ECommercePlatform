using ECommerce.EventBus.Abstractions;

namespace ECommerce.EventBus.Events;

public class PaymentProcessedEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty; // succeeded, failed
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}
