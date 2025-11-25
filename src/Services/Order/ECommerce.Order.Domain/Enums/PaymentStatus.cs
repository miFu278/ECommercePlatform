namespace ECommerce.Order.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,        // Awaiting payment
    Paid = 1,           // Payment successful
    Failed = 2,         // Payment failed
    Refunded = 3        // Payment refunded
}
