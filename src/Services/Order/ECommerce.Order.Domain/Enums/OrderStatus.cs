namespace ECommerce.Order.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,        // Order created, awaiting payment
    Processing = 1,     // Payment confirmed, preparing order
    Shipped = 2,        // Order shipped
    Delivered = 3,      // Order delivered to customer
    Cancelled = 4,      // Order cancelled
    Refunded = 5        // Order refunded
}
