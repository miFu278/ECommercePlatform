namespace ECommerce.Payment.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,        // Payment initiated, awaiting processing
    Processing = 1,     // Payment being processed by gateway
    Completed = 2,      // Payment successful
    Failed = 3,         // Payment failed
    Cancelled = 4,      // Payment cancelled by user
    Refunded = 5,       // Payment refunded
    PartialRefund = 6   // Partial refund issued
}
