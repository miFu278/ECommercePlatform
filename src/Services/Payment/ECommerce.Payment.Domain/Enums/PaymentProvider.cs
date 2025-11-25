namespace ECommerce.Payment.Domain.Enums;

public enum PaymentProvider
{
    PayOS = 0,      // Vietnamese payment gateway
    VNPay = 1,      // Vietnamese payment gateway
    Momo = 2,       // Vietnamese e-wallet
    ZaloPay = 3,    // Vietnamese e-wallet
    Manual = 4      // For COD, Bank Transfer
}
