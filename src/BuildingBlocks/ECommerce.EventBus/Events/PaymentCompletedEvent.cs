using ECommerce.EventBus.Abstractions;

namespace ECommerce.EventBus.Events;

public class PaymentCompletedEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
}
