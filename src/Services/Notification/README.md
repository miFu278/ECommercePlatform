# Notification Service

Email notification microservice với SMTP.

## Features

- ✅ Send order confirmation emails
- ✅ Send payment receipt emails
- ✅ Send order shipped notifications
- ✅ Send order delivered notifications
- ✅ Send welcome emails
- ✅ Event-driven architecture
- ✅ HTML email templates (Vietnamese)
- ✅ SMTP support (Gmail, Outlook, Brevo, etc.)
- ✅ Stateless (no database)

## Tech Stack

- **Framework:** ASP.NET Core 8.0
- **Email:** SMTP (System.Net.Mail)
- **Event Bus:** In-Memory (ready for RabbitMQ)
- **Templates:** HTML (Vietnamese)

## Quick Start

### 1. Configure SMTP

#### Option A: Gmail (Development)

1. Enable 2-Step Verification in Gmail
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Update `appsettings.json`:

```json
{
  "Email": {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@gmail.com",
      "Password": "your-16-char-app-password",
      "FromEmail": "your-email@gmail.com",
      "FromName": "ECommerce Platform"
    }
  }
}
```

#### Option B: Brevo (Production)

1. Sign up at https://brevo.com
2. Get SMTP credentials from Settings → SMTP & API
3. Update `appsettings.json`:

```json
{
  "Email": {
    "Smtp": {
      "Host": "smtp-relay.brevo.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@example.com",
      "Password": "your-brevo-smtp-key",
      "FromEmail": "no-reply@yourdomain.com",
      "FromName": "ECommerce Platform"
    }
  }
}
```

### 2. Run Service

```bash
cd src/Services/Notification/ECommerce.Notification.API
dotnet run --urls "http://localhost:5005"
```

### 3. Open Swagger

http://localhost:5005

## Email Templates

### 1. Order Confirmation
**Trigger:** OrderCreatedEvent  
**Subject:** Xác nhận đơn hàng #{orderNumber}  
**Content:**
- Order number
- Total amount
- Customer name
- Order tracking link

### 2. Payment Receipt
**Trigger:** PaymentCompletedEvent  
**Subject:** Biên lai thanh toán - Đơn hàng #{orderNumber}  
**Content:**
- Payment number
- Order number
- Amount paid
- Payment timestamp

### 3. Order Shipped
**Trigger:** OrderShippedEvent  
**Subject:** Đơn hàng #{orderNumber} đã được giao cho đơn vị vận chuyển  
**Content:**
- Order number
- Tracking number
- Tracking link
- Estimated delivery

### 4. Order Delivered
**Trigger:** OrderDeliveredEvent  
**Subject:** Đơn hàng #{orderNumber} đã được giao thành công  
**Content:**
- Order number
- Delivery confirmation
- Review request link

### 5. Welcome Email
**Trigger:** UserRegisteredEvent  
**Subject:** Chào mừng bạn đến với ECommerce!  
**Content:**
- Welcome message
- Platform features
- Call-to-action button

## Event Subscriptions

| Event | Handler | Email Sent |
|-------|---------|------------|
| OrderCreatedEvent | OrderCreatedEventHandler | Order Confirmation |
| PaymentCompletedEvent | PaymentCompletedEventHandler | Payment Receipt |
| OrderShippedEvent | OrderShippedEventHandler | Shipping Notification |
| OrderDeliveredEvent | OrderDeliveredEventHandler | Delivery Confirmation |
| UserRegisteredEvent | UserRegisteredEventHandler | Welcome Email |

## Architecture

```
Event Bus (RabbitMQ/In-Memory)
    ↓
Event Handlers
    ↓
Email Service (SMTP)
    ↓
Gmail/Brevo/Custom SMTP Server
    ↓
Customer Email
```

## Configuration

### Environment Variables (Production)

```bash
export EMAIL__SMTP__HOST="smtp-relay.brevo.com"
export EMAIL__SMTP__PORT="587"
export EMAIL__SMTP__ENABLESSL="true"
export EMAIL__SMTP__USERNAME="your-email@example.com"
export EMAIL__SMTP__PASSWORD="your-smtp-password"
export EMAIL__SMTP__FROMEMAIL="no-reply@yourdomain.com"
export EMAIL__SMTP__FROMNAME="ECommerce Platform"
```

## Testing

### Manual Test (via Swagger)

Since this is event-driven, you can't test directly via API. Instead:

1. Run Notification Service
2. Run Order Service
3. Create an order → OrderCreatedEvent → Email sent

### Test Email Sending

Create a test endpoint (for development only):

```csharp
[HttpPost("test/order-confirmation")]
public async Task<IActionResult> TestOrderConfirmation()
{
    await _emailService.SendOrderConfirmationAsync(
        "test@example.com",
        "ORD20241124-0001",
        100000,
        "Nguyễn Văn A"
    );
    return Ok("Email sent");
}
```

## Email Limits

### Gmail SMTP
- **Free:** 500 emails/day
- **Workspace:** 2,000 emails/day

### Brevo SMTP
- **Free:** 300 emails/day
- **Lite:** 10,000 emails/month ($25)
- **Premium:** 20,000 emails/month ($65)

### Amazon SES
- **Free tier:** 62,000 emails/month (first 12 months)
- **After:** $0.10 per 1,000 emails

## Troubleshooting

### Email not sending

1. **Check SMTP credentials**
   - Verify username/password
   - Check if App Password is correct (Gmail)

2. **Check firewall**
   - Port 587 must be open
   - Check antivirus/firewall settings

3. **Check logs**
   ```bash
   # View logs
   dotnet run --urls "http://localhost:5005"
   # Look for errors in console
   ```

4. **Test SMTP connection**
   ```bash
   telnet smtp.gmail.com 587
   ```

### Email goes to spam

1. **Use professional email**
   - Use custom domain (no-reply@yourdomain.com)
   - Don't use Gmail for production

2. **Configure SPF/DKIM**
   - Add SPF record to DNS
   - Configure DKIM with email provider

3. **Use Brevo/SendGrid**
   - Better deliverability
   - Pre-configured SPF/DKIM

## Production Checklist

- [ ] Use professional email service (Brevo/SendGrid/SES)
- [ ] Configure custom domain
- [ ] Setup SPF/DKIM records
- [ ] Test email deliverability
- [ ] Monitor email sending
- [ ] Setup error alerts
- [ ] Configure rate limiting
- [ ] Add retry mechanism
- [ ] Log all email attempts
- [ ] Setup email templates in provider dashboard

## Migration to RabbitMQ

Currently using In-Memory Event Bus. To migrate to RabbitMQ:

1. Install RabbitMQ package
2. Update Program.cs to use RabbitMQ
3. Configure RabbitMQ connection
4. All event handlers work the same!

## Integration

### With Order Service
```csharp
// Order Service publishes event
_eventBus.Publish(new OrderCreatedEvent
{
    OrderId = order.Id,
    OrderNumber = order.OrderNumber,
    UserId = order.UserId,
    TotalAmount = order.TotalAmount
});

// Notification Service receives and sends email
```

### With Payment Service
```csharp
// Payment Service publishes event
_eventBus.Publish(new PaymentCompletedEvent
{
    PaymentId = payment.Id,
    OrderId = payment.OrderId,
    Amount = payment.Amount
});

// Notification Service receives and sends email
```

## Security

- ✅ SMTP credentials in environment variables
- ✅ SSL/TLS encryption
- ✅ No sensitive data in emails
- ✅ Graceful error handling (don't break system)
- ✅ Rate limiting (via SMTP provider)

## Performance

- **Email sending:** ~1-2 seconds per email
- **Async processing:** Non-blocking
- **Retry:** Handled by SMTP client
- **Stateless:** No database, easy to scale

## Support

- Gmail SMTP: https://support.google.com/mail/answer/7126229
- Brevo: https://help.brevo.com/hc/en-us
- SendGrid: https://docs.sendgrid.com/

## Language

All email templates are in **Vietnamese** (Tiếng Việt) for Vietnam market.
