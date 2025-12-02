# Deploy Backend lên Render.com (FREE)

## 🎯 Tổng quan

Render.com cung cấp:
- ✅ **Free tier**: 750 giờ/tháng (đủ cho 1 service chạy 24/7)
- ✅ **Free PostgreSQL**: 90 ngày, sau đó $7/tháng
- ✅ **Auto-deploy** từ GitHub
- ✅ **HTTPS** miễn phí
- ✅ **Docker support**

**Lưu ý**: Free tier có giới hạn:
- Service sẽ "ngủ" sau 15 phút không hoạt động
- Khởi động lại mất ~30 giây khi có request đầu tiên
- 512MB RAM

---

## 📋 Bước 1: Chuẩn bị Code

### 1.1. Fix Port Configuration

Render yêu cầu app lắng nghe trên port từ biến môi trường `PORT`.

**Cập nhật `Program.cs`:**

Thay thế phần configure Kestrel:

```csharp
// OLD CODE (xóa đi):
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenLocalhost(5010, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

// NEW CODE (thêm vào):
builder.WebHost.ConfigureKestrel(options =>
{
    // Get port from environment variable (Render requirement)
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    
    if (builder.Environment.IsDevelopment())
    {
        // Development: Multiple ports
        options.ListenLocalhost(5000, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
        options.ListenLocalhost(5010, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
    }
    else
    {
        // Production: Single port from environment
        options.ListenAnyIP(int.Parse(port), o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    }
});
```

### 1.2. Cập nhật appsettings.json

Thêm fallback cho connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=UserDb;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "ECommerceAPI",
    "Audience": "ECommerceClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### 1.3. Push code lên GitHub

```bash
git add .
git commit -m "Configure for Render deployment"
git push origin main
```

---

## 📋 Bước 2: Setup Render Account

1. Đăng ký tại: https://render.com
2. Connect GitHub account
3. Authorize Render to access your repository

---

## 📋 Bước 3: Deploy PostgreSQL Database (FREE 90 ngày)

1. Vào Dashboard → **New** → **PostgreSQL**
2. Điền thông tin:
   - **Name**: `ecommerce-userdb`
   - **Database**: `UserDb`
   - **User**: `ecommerce_user`
   - **Region**: Singapore (gần VN nhất)
   - **Plan**: **Free** (90 ngày)
3. Click **Create Database**
4. Đợi ~2 phút để database khởi tạo
5. Copy **Internal Database URL** (dạng: `postgresql://user:pass@host/db`)

---

## 📋 Bước 4: Deploy User Service

### 4.1. Tạo Web Service

1. Vào Dashboard → **New** → **Web Service**
2. Connect repository: `ECommercePlatform`
3. Điền thông tin:

**Basic Settings:**
- **Name**: `ecommerce-user-service`
- **Region**: Singapore
- **Branch**: `main`
- **Root Directory**: `ECommercePlatform` (nếu repo có nhiều folder)
- **Runtime**: **Docker**
- **Dockerfile Path**: `src/Services/Users/ECommerce.User.API/Dockerfile`

**Instance Type:**
- **Plan**: **Free** (512MB RAM, 0.1 CPU)

### 4.2. Environment Variables

Click **Advanced** → **Add Environment Variable**:

```bash
# Database
ConnectionStrings__DefaultConnection=<PASTE_INTERNAL_DATABASE_URL_HERE>

# JWT Settings (QUAN TRỌNG: Đổi secret key!)
Jwt__Secret=your-super-secret-jwt-key-minimum-32-characters-long-change-this
Jwt__Issuer=ECommerceAPI
Jwt__Audience=ECommerceClient
Jwt__AccessTokenExpirationMinutes=60
Jwt__RefreshTokenExpirationDays=7

# Environment
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Optional: Email service (nếu dùng)
# EmailSettings__SmtpServer=smtp.gmail.com
# EmailSettings__SmtpPort=587
# EmailSettings__SenderEmail=your-email@gmail.com
# EmailSettings__SenderPassword=your-app-password
```

**Lưu ý**: 
- Thay `<PASTE_INTERNAL_DATABASE_URL_HERE>` bằng Internal Database URL từ bước 3
- Đổi `Jwt__Secret` thành chuỗi ngẫu nhiên dài ít nhất 32 ký tự

### 4.3. Deploy

1. Click **Create Web Service**
2. Render sẽ tự động:
   - Clone repo
   - Build Docker image
   - Deploy service
3. Đợi ~5-10 phút cho lần deploy đầu tiên

---

## 📋 Bước 5: Kiểm tra Deployment

### 5.1. Check Service Status

Sau khi deploy xong, bạn sẽ thấy:
- **Status**: Live (màu xanh)
- **URL**: `https://ecommerce-user-service.onrender.com`

### 5.2. Test API

```bash
# Health check
curl https://ecommerce-user-service.onrender.com/health

# Swagger UI (nếu enable trong Production)
https://ecommerce-user-service.onrender.com/swagger
```

### 5.3. Test Register

```bash
curl -X POST https://ecommerce-user-service.onrender.com/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123456",
    "firstName": "Test",
    "lastName": "User",
    "phoneNumber": "0123456789"
  }'
```

---

## 📋 Bước 6: Deploy Các Services Khác

Lặp lại Bước 4 cho các services khác:

### Product Service (MongoDB)

1. Tạo MongoDB trên **MongoDB Atlas** (free 512MB):
   - https://www.mongodb.com/cloud/atlas/register
   - Tạo cluster → Get connection string
   
2. Deploy Product Service:
   - **Name**: `ecommerce-product-service`
   - **Dockerfile Path**: `src/Services/Product/ECommerce.Product.API/Dockerfile`
   - **Environment Variables**:
     ```bash
     ConnectionStrings__MongoDB=<MONGODB_CONNECTION_STRING>
     ASPNETCORE_ENVIRONMENT=Production
     ```

### Shopping Cart Service (Redis)

1. Tạo Redis trên **Upstash** (free 10,000 commands/day):
   - https://upstash.com
   - Tạo database → Get connection string

2. Deploy Cart Service:
   - **Name**: `ecommerce-cart-service`
   - **Dockerfile Path**: `src/Services/ShoppingCart/ECommerce.ShoppingCart.API/Dockerfile`
   - **Environment Variables**:
     ```bash
     ConnectionStrings__Redis=<UPSTASH_REDIS_URL>
     ASPNETCORE_ENVIRONMENT=Production
     ```

---

## 🔧 Troubleshooting

### Lỗi: "Application failed to respond"

**Nguyên nhân**: App không lắng nghe đúng port

**Giải pháp**: Kiểm tra Program.cs đã cập nhật đúng như Bước 1.1

### Lỗi: "Database connection failed"

**Nguyên nhân**: Connection string sai

**Giải pháp**: 
1. Vào PostgreSQL dashboard → Copy **Internal Database URL**
2. Paste vào environment variable `ConnectionStrings__DefaultConnection`

### Lỗi: "Service keeps sleeping"

**Nguyên nhân**: Free tier tự động sleep sau 15 phút

**Giải pháp**: 
- Upgrade lên paid plan ($7/month)
- Hoặc dùng cron job để ping service mỗi 10 phút:
  ```bash
  # Dùng cron-job.org (free)
  https://cron-job.org
  # Tạo job ping: https://ecommerce-user-service.onrender.com/health
  ```

### Build quá lâu

**Nguyên nhân**: Dockerfile build từ đầu mỗi lần

**Giải pháp**: Render cache Docker layers tự động, lần sau sẽ nhanh hơn

---

## 💰 Chi phí Ước tính

### Option 1: Hoàn toàn FREE (90 ngày)
- User Service: FREE
- PostgreSQL: FREE (90 ngày)
- MongoDB Atlas: FREE (512MB)
- Upstash Redis: FREE (10K commands/day)
- **Total**: $0/tháng (3 tháng đầu)

### Option 2: Sau 90 ngày
- User Service: FREE (hoặc $7/month cho không sleep)
- PostgreSQL: $7/month
- MongoDB Atlas: FREE
- Upstash Redis: FREE
- **Total**: $7-14/tháng

---

## 🚀 Auto-Deploy

Render tự động deploy khi bạn push code lên GitHub:

```bash
git add .
git commit -m "Update feature"
git push origin main
# Render sẽ tự động build và deploy!
```

---

## 📊 Monitoring

### View Logs
1. Vào service dashboard
2. Click **Logs** tab
3. Xem real-time logs

### Metrics
- CPU usage
- Memory usage
- Request count
- Response time

---

## 🔐 Security Checklist

- [ ] Đổi `Jwt__Secret` thành chuỗi ngẫu nhiên mạnh
- [ ] Không commit secrets vào Git
- [ ] Enable HTTPS (Render tự động)
- [ ] Restrict CORS trong Production
- [ ] Enable rate limiting
- [ ] Setup database backups

---

## 🎓 Next Steps

1. **Deploy API Gateway**: Để route requests đến các services
2. **Setup Custom Domain**: Thay vì dùng `.onrender.com`
3. **Enable Monitoring**: Application Insights, Sentry
4. **Setup CI/CD**: GitHub Actions cho testing trước khi deploy
5. **Database Backups**: Tự động backup PostgreSQL

---

## 📞 Support

- Render Docs: https://render.com/docs
- Community: https://community.render.com
- Status: https://status.render.com

---

**Chúc bạn deploy thành công! 🎉**
