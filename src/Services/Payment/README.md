# Payment Service

Payment processing microservice với PayOS payment gateway (Việt Nam).

## Features

- ✅ Tạo payment cho order
- ✅ Xử lý thanh toán qua PayOS
- ✅ Webhook từ PayOS
- ✅ Refund payment
- ✅ Cancel payment
- ✅ Payment history tracking
- ✅ PostgreSQL database
- ✅ Event-driven (PaymentCompletedEvent)

## Tech Stack

- **Framework:** ASP.NET Core 8.0
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Payment Gateway:** PayOS (Vietnamese)
- **Currency:** VND (Việt Nam Đồng)
- **Validation:** FluentValidation
- **Mapping:** AutoMapper

## Quick Start

### 1. Setup Database

```bash
# Start PostgreSQL
docker run -d --name postgres-payment \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=ecommerce_payment \
  -p 5432:5432 \
  postgres:16-alpine
```

### 2. Configure PayOS

Xem [PAYOS_SETUP.md](./PAYOS_SETUP.md) để đăng ký và lấy API keys.

Update `appsettings.json`:
```json
{
  "PayOS": {
    "ClientId": "your-client-id",
    "ApiKey": "your-api-key",
    "ChecksumKey": "your-checksum-key"
  }
}
```

### 3. Run Migrations

```bash
cd src/Services/Payment/ECommerce.Payment.API

# Create migration
dotnet ef migrations add InitialCreate --project ../ECommerce.Payment.Infrastructure

# Apply migration
dotnet ef database update --project ../ECommerce.Payment.Infrastructure
```

### 4. Run API

```bash
dotnet run --urls "http://localhost:5004"
```

### 5. Open Swagger

http://localhost:5004

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/payments` | Create payment | User |
| GET | `/api/payments/{id}` | Get payment by ID | User |
| GET | `/api/payments/order/{orderId}` | Get by order ID | User |
| GET | `/api/payments/my-payments` | Get user's payments | User |
| POST | `/api/payments/{id}/refund` | Refund payment | Admin |
| POST | `/api/payments/{id}/cancel` | Cancel payment | User |
| POST | `/api/payments/webhook` | PayOS webhook | Public |

## Payment Flow

```
1. User tạo order → Order Service
2. User click "Thanh toán" → Payment Service
3. Payment Service → PayOS: Create payment link
4. PayOS → Return checkout URL
5. User → Redirect to PayOS checkout
6. User → Chọn ngân hàng, quét QR
7. PayOS → Process payment
8. PayOS → Webhook to Payment Service
9. Payment Service → Update status to Completed
10. Payment Service → Publish PaymentCompletedEvent
11. Order Service → Update order status
12. Notification Service → Send email
13. PayOS → Redirect user to return URL
```

## Payment Status

- **Pending** - Payment created, awaiting processing
- **Processing** - Payment being processed by PayOS
- **Completed** - Payment successful
- **Failed** - Payment failed
- **Cancelled** - Payment cancelled by user
- **Refunded** - Payment refunded
- **PartialRefund** - Partial refund issued

## Payment Methods

- **BankTransfer** - Chuyển khoản ngân hàng
- **QRCode** - QR Code (PayOS, VNPay)
- **EWallet** - Ví điện tử (Momo, ZaloPay)
- **CreditCard** - Thẻ tín dụng
- **DebitCard** - Thẻ ghi nợ
- **CashOnDelivery** - Thanh toán khi nhận hàng

## Database Schema

### payments
- id (uuid, PK)
- payment_number (varchar, unique)
- order_id (uuid)
- order_number (varchar)
- user_id (uuid)
- status (int)
- method (int)
- provider (int)
- amount, refunded_amount (decimal)
- currency (varchar, default 'VND')
- provider_transaction_id, provider_payment_id (varchar)
- card_last4, card_brand (varchar)
- timestamps (processed_at, completed_at, failed_at, refunded_at)
- error_code, error_message (varchar)
- refund_reason, description (varchar)
- customer_email, customer_name (varchar)

### payment_history
- id (uuid, PK)
- payment_id (uuid, FK)
- status (int)
- notes (varchar)
- changed_by (varchar)
- changed_at (timestamp)

## Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ecommerce_payment;Username=postgres;Password=postgres"
  },
  "PayOS": {
    "ClientId": "your-client-id",
    "ApiKey": "your-api-key",
    "ChecksumKey": "your-checksum-key",
    "ReturnUrl": "http://localhost:3000/payment/success",
    "CancelUrl": "http://localhost:3000/payment/cancel"
  }
}
```

## Testing

### 1. Create Payment
```http
POST http://localhost:5004/api/payments
Content-Type: application/json
X-User-Id: 00000000-0000-0000-0000-000000000001

{
  "orderId": "123e4567-e89b-12d3-a456-426614174000",
  "method": 1
}
```

### 2. Get Payment
```http
GET http://localhost:5004/api/payments/{id}
X-User-Id: 00000000-0000-0000-0000-000000000001
```

### 3. Test Webhook (PayOS calls this)
```http
POST http://localhost:5004/api/payments/webhook
Content-Type: application/json

{
  "code": "00",
  "desc": "success",
  "data": {
    "orderCode": 1234567890,
    "amount": 100000,
    "code": "00"
  }
}
```

## Integration

### With Order Service
- Order Service calls Payment Service to create payment
- Payment Service publishes PaymentCompletedEvent
- Order Service subscribes and updates order status

### With Notification Service
- Payment Service publishes PaymentCompletedEvent
- Notification Service sends payment receipt email

## Events Published

- **PaymentCompletedEvent** - When payment is successful
- **PaymentFailedEvent** - When payment fails
- **PaymentRefundedEvent** - When payment is refunded

## Security

- ✅ JWT authentication (via API Gateway)
- ✅ User can only access their own payments
- ✅ Admin can refund payments
- ✅ Webhook signature validation (TODO)
- ✅ HTTPS in production

## Production Checklist

- [ ] Configure PayOS production keys
- [ ] Setup webhook URL (public, HTTPS)
- [ ] Enable webhook signature validation
- [ ] Configure logging (Serilog)
- [ ] Setup monitoring & alerts
- [ ] Configure rate limiting
- [ ] Backup database
- [ ] Test refund flow
- [ ] Document payment reconciliation process

## Currency

**VND (Việt Nam Đồng)**
- Không có số thập phân
- Ví dụ: 100,000 VND
- Format: `100.000 ₫`

## Support

- PayOS Documentation: https://payos.vn/docs
- PayOS Dashboard: https://my.payos.vn
- PayOS Support: support@payos.vn
