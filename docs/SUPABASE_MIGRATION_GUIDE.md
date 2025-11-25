# Hướng dẫn Migration sang Supabase PostgreSQL

## Tổng quan

Project đã được cấu hình để sử dụng PostgreSQL trên Supabase với các schema riêng biệt cho mỗi service:
- **user** schema: User Service
- **order** schema: Order Service  
- **payment** schema: Payment Service

## Connection String

Connection string Supabase đã được cấu hình trong các file `appsettings.Development.json`:

```
Host=db.nmresbaqqbjgemfjryen.supabase.co;Database=postgres;Username=postgres;Password=@0375331022Ttmp;SSL Mode=Require;Trust Server Certificate=true
```

## Các bước Migration

### 1. Tạo Migration cho từng service

#### User Service
```bash
cd src/Services/Users/ECommerce.User.API
dotnet ef migrations add InitialCreate --project ../ECommerce.User.Infrastructure --startup-project . --context UserDbContext
```

#### Order Service
```bash
cd src/Services/Order/ECommerce.Order.API
dotnet ef migrations add InitialCreate --project ../ECommerce.Order.Infrastructure --startup-project . --context OrderDbContext
```

#### Payment Service
```bash
cd src/Services/Payment/ECommerce.Payment.API
dotnet ef migrations add InitialCreate --project ../ECommerce.Payment.Infrastructure --startup-project . --context PaymentDbContext
```

### 2. Apply Migration lên Supabase

#### User Service
```bash
cd src/Services/Users/ECommerce.User.API
dotnet ef database update --project ../ECommerce.User.Infrastructure --startup-project . --context UserDbContext
```

#### Order Service
```bash
cd src/Services/Order/ECommerce.Order.API
dotnet ef database update --project ../ECommerce.Order.Infrastructure --startup-project . --context OrderDbContext
```

#### Payment Service
```bash
cd src/Services/Payment/ECommerce.Payment.API
dotnet ef database update --project ../ECommerce.Payment.Infrastructure --startup-project . --context PaymentDbContext
```

### 3. Kiểm tra trên Supabase

Sau khi chạy migration, bạn có thể kiểm tra trên Supabase Dashboard:

1. Truy cập: https://supabase.com/dashboard/project/nmresbaqqbjgemfjryen
2. Vào **Table Editor** hoặc **SQL Editor**
3. Kiểm tra các schema đã được tạo:
   - `user.users`, `user.roles`, `user.addresses`, etc.
   - `order.orders`, `order.order_items`, `order.order_status_history`
   - `payment.payments`, `payment.payment_history`

## Lưu ý quan trọng

### Schema Isolation
Mỗi service sử dụng schema riêng để tránh conflict:
- Các bảng của User service nằm trong schema `user`
- Các bảng của Order service nằm trong schema `order`
- Các bảng của Payment service nằm trong schema `payment`

### SSL Connection
Connection string đã bao gồm `SSL Mode=Require;Trust Server Certificate=true` để kết nối an toàn với Supabase.

### Environment Variables (Khuyến nghị)
Để bảo mật hơn, nên sử dụng environment variables thay vì hardcode password:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.nmresbaqqbjgemfjryen.supabase.co;Database=postgres;Username=postgres;Password=${SUPABASE_PASSWORD};SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

Sau đó set environment variable:
```bash
# Windows PowerShell
$env:SUPABASE_PASSWORD="@0375331022Ttmp"

# Windows CMD
set SUPABASE_PASSWORD=@0375331022Ttmp

# Linux/Mac
export SUPABASE_PASSWORD="@0375331022Ttmp"
```

## Rollback (nếu cần)

Nếu cần rollback migration:

```bash
# Rollback về migration trước đó
dotnet ef database update <PreviousMigrationName> --project ../ECommerce.*.Infrastructure --startup-project . --context *DbContext

# Xóa migration cuối cùng
dotnet ef migrations remove --project ../ECommerce.*.Infrastructure --startup-project . --context *DbContext
```

## Troubleshooting

### Lỗi: "relation already exists"
Nếu gặp lỗi này, có thể do đã có bảng tồn tại. Giải pháp:
1. Drop schema cũ trên Supabase
2. Chạy lại migration

### Lỗi: "password authentication failed"
Kiểm tra lại password trong connection string hoặc reset password trên Supabase Dashboard.

### Lỗi: "SSL connection required"
Đảm bảo connection string có `SSL Mode=Require;Trust Server Certificate=true`

## Kiểm tra kết nối

Bạn có thể test kết nối bằng cách chạy một trong các API:

```bash
# Test User Service
curl http://localhost:5000/api/users

# Test Order Service  
curl http://localhost:5003/api/orders

# Test Payment Service
curl http://localhost:5004/api/payments
```
