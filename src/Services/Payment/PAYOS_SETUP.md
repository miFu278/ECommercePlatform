# PayOS Payment Gateway Setup

## Giới thiệu

PayOS là cổng thanh toán của Việt Nam, hỗ trợ:
- ✅ Chuyển khoản ngân hàng (Bank Transfer)
- ✅ QR Code thanh toán
- ✅ Ví điện tử (Momo, ZaloPay)
- ✅ Thẻ ATM nội địa
- ✅ Thẻ tín dụng/ghi nợ quốc tế

## Đăng ký PayOS

### 1. Tạo tài khoản
1. Truy cập: https://payos.vn
2. Đăng ký tài khoản doanh nghiệp
3. Xác thực thông tin

### 2. Lấy API Keys
1. Đăng nhập vào Dashboard: https://my.payos.vn
2. Vào **Cài đặt** → **API Keys**
3. Lấy 3 thông tin:
   - **Client ID**
   - **API Key**
   - **Checksum Key**

## Cấu hình

### appsettings.json

```json
{
  "PayOS": {
    "ClientId": "your-client-id",
    "ApiKey": "your-api-key",
    "ChecksumKey": "your-checksum-key",
    "ReturnUrl": "http://localhost:3000/payment/success",
    "CancelUrl": "http://localhost:3000/payment/cancel"
  }
}
```

### Environment Variables (Production)

```bash
export PAYOS__CLIENTID="your-client-id"
export PAYOS__APIKEY="your-api-key"
export PAYOS__CHECKSUMKEY="your-checksum-key"
```

## Flow thanh toán

### 1. Tạo đơn hàng
```
Client → Order Service → Create Order
```

### 2. Tạo payment link
```
Client → Payment Service → PayOS Gateway
                         → Generate Payment Link
                         → Return checkout URL
```

### 3. Khách hàng thanh toán
```
Client → Redirect to PayOS checkout URL
      → Chọn phương thức thanh toán
      → Quét QR hoặc nhập thông tin thẻ
      → PayOS xử lý thanh toán
```

### 4. Webhook callback
```
PayOS → Payment Service Webhook
     → Update payment status
     → Update order status
     → Send notification
```

### 5. Return URL
```
PayOS → Redirect to Return URL
     → Client hiển thị kết quả
```

## API Endpoints

### Tạo Payment
```http
POST /api/payments
Content-Type: application/json
Authorization: Bearer <jwt_token>

{
  "orderId": "123e4567-e89b-12d3-a456-426614174000",
  "method": 1,
  "returnUrl": "http://localhost:3000/payment/success",
  "cancelUrl": "http://localhost:3000/payment/cancel"
}

Response:
{
  "id": "...",
  "paymentNumber": "PAY20241124-0001",
  "checkoutUrl": "https://pay.payos.vn/web/...",
  "status": "Pending"
}
```

### Webhook (PayOS gọi)
```http
POST /api/payments/webhook
Content-Type: application/json

{
  "code": "00",
  "desc": "success",
  "data": {
    "orderCode": 1234567890,
    "amount": 100000,
    "description": "Order ORD20241124-0001",
    "accountNumber": "1234567890",
    "reference": "FT123456",
    "transactionDateTime": "2024-11-24 10:00:00",
    "currency": "VND",
    "paymentLinkId": "...",
    "code": "00",
    "desc": "Thành công",
    "counterAccountBankId": "",
    "counterAccountBankName": "Vietcombank",
    "counterAccountName": "NGUYEN VAN A",
    "counterAccountNumber": "9876543210",
    "virtualAccountName": "",
    "virtualAccountNumber": ""
  },
  "signature": "..."
}
```

## Testing

### Test Mode (Sandbox)

PayOS cung cấp môi trường test:

1. **Test Bank Account:**
   - Số tài khoản: `9704198526191432198`
   - Tên: `NGUYEN VAN A`
   - Ngân hàng: `MB Bank`
   - OTP: `123456`

2. **Test QR Code:**
   - Quét QR → Tự động thanh toán thành công

3. **Test Webhook:**
   ```bash
   curl -X POST http://localhost:5004/api/payments/webhook \
     -H "Content-Type: application/json" \
     -d '{
       "code": "00",
       "desc": "success",
       "data": {
         "orderCode": 1234567890,
         "amount": 100000,
         "code": "00"
       }
     }'
   ```

## Phí giao dịch

| Phương thức | Phí |
|-------------|-----|
| Chuyển khoản ngân hàng | 1.5% |
| QR Code | 1.5% |
| Thẻ ATM nội địa | 1.8% |
| Thẻ quốc tế | 2.5% |
| Ví điện tử | 2.0% |

## Giới hạn

- **Số tiền tối thiểu:** 5,000 VND
- **Số tiền tối đa:** 500,000,000 VND/giao dịch
- **Thời gian hết hạn link:** 15 phút (mặc định)

## Troubleshooting

### Lỗi: "Invalid signature"
- Kiểm tra ChecksumKey
- Đảm bảo không có khoảng trắng thừa

### Lỗi: "Order code already exists"
- Order code phải unique
- Sử dụng timestamp hoặc UUID

### Lỗi: "Amount invalid"
- PayOS chỉ chấp nhận số nguyên (VND)
- Không có số thập phân

### Webhook không nhận được
- Kiểm tra URL public (dùng ngrok cho local)
- Kiểm tra firewall
- Xem logs trong PayOS Dashboard

## Production Checklist

- [ ] Đăng ký tài khoản doanh nghiệp
- [ ] Xác thực thông tin công ty
- [ ] Cấu hình webhook URL (public)
- [ ] Test thanh toán thật
- [ ] Setup monitoring
- [ ] Backup API keys
- [ ] Configure rate limiting
- [ ] Setup error alerts

## Links

- **Website:** https://payos.vn
- **Dashboard:** https://my.payos.vn
- **Documentation:** https://payos.vn/docs
- **Support:** support@payos.vn
- **Hotline:** 1900 6666

## Security

⚠️ **Quan trọng:**
- Không commit API keys vào Git
- Sử dụng environment variables
- Validate webhook signature
- Log tất cả transactions
- Monitor suspicious activities

## Example Flow

```
1. User tạo order → Order Service
2. Order Service → Payment Service: Create payment
3. Payment Service → PayOS: Create payment link
4. PayOS → Return checkout URL
5. User → Redirect to PayOS checkout
6. User → Chọn ngân hàng, quét QR
7. PayOS → Process payment
8. PayOS → Webhook to Payment Service
9. Payment Service → Update payment status
10. Payment Service → Update order status
11. Payment Service → Send email notification
12. PayOS → Redirect user to return URL
13. User → See success message
```

## Currency: VND

Tất cả số tiền sử dụng **VND (Việt Nam Đồng)**:
- Không có số thập phân
- Ví dụ: 100,000 VND (không phải 100000.00)
- Format: `100.000 ₫` hoặc `100,000 VND`
