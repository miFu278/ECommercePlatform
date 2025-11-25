namespace ECommerce.Payment.Domain.Enums;

public enum PaymentMethod
{
    BankTransfer = 0,       // Chuyển khoản ngân hàng
    QRCode = 1,             // QR Code (PayOS, VNPay)
    EWallet = 2,            // Ví điện tử (Momo, ZaloPay)
    CreditCard = 3,         // Thẻ tín dụng
    DebitCard = 4,          // Thẻ ghi nợ
    CashOnDelivery = 5      // Thanh toán khi nhận hàng
}
