# 🚀 Deploy E-Commerce Platform lên Render

## Tổng quan

Guide này sẽ hướng dẫn deploy toàn bộ 6 microservices lên Render **MIỄN PHÍ**.

**Services cần deploy:**
1. User Service (PostgreSQL)
2. Product Service (MongoDB)
3. Shopping Cart Service (Redis)
4. Order Service (PostgreSQL)
5. Payment Service (PostgreSQL)
6. Notification Service (MongoDB)

---

## 📋 Bước 1: Chuẩn bị

### 1.1. Đăng ký Render
1. Truy cập: https://render.com
2. Sign up với GitHub account
3. Authorize Render truy cập GitHub repo của bạn

### 1.2. Push code lên GitHub
```bash
git add .
git commit -m "Add Dockerfiles for Render deployment"
git push origin main
```

---

## 🗄️ Bước 2: Setup Databases (FREE)

### 2.1. PostgreSQL (cho User, Order, Payment)

**Tạo PostgreSQL database:**
1. Vào Render Dashboard
2. Click **"New +"** → **"PostgreSQL"**
3. Điền thông tin:
   - **Name**: `ecommerce-postgres`
   - **Database**: `ecommerce`
   - **User**: `ecommerce_user`
   - **Region**: `Singapore` (gần VN nhất)
   - **Plan**: **Free** ✅
4. Click **"Create Database"**
5. Đợi ~2 phút để database khởi tạo
6. Copy **Internal Database URL** (dạng: `postgresql://user:pass@host/db`)

**Lưu ý:** Free tier PostgreSQL có:
- ✅ 1GB storage
- ✅ Expires sau 90 ngày (cần renew)
- ✅ Không sleep

### 2.2. Redis (External - Redis Cloud)

Render không có Redis free tier, nên dùng Redis Cloud:

1. Truy cập: https://redis.com/try-free/
2. Sign up free account
3. Create database:
   - **Name**: `ecommerce-cart`
   - **Region**: `ap-southeast-1` (Singapore)
   - **Plan**: **Free 30MB** ✅
4. Copy connection string (đã có rồi):
   ```
   redis-15540.crce264.ap-east-1-1.ec2.cloud.redislabs.com:15540
   ```

### 2.3. MongoDB (External - MongoDB Atlas)

Render không có MongoDB, dùng MongoDB Atlas:

1. Truy cập: https://www.mongodb.com/cloud/atlas/register
2. Sign up free account
3. Create cluster:
   - **Name**: `ecommerce-cluster`
   - **Provider**: `AWS`
   - **Region**: `Singapore (ap-southeast-1)`
   - **Tier**: **M0 Free** ✅
4. Create database user:
   - Username: `ecommerce_user`
   - Password: `[YOUR_PASSWORD]`
5. Whitelist IP: `0.0.0.0/0` (allow all)
6. Copy connection string:
   ```
   mongodb+srv://ecommerce_user:[PASSWORD]@cluster.mongodb.net/
   ```

---

## 🚀 Bước 3: Deploy Services

### 3.1. Deploy User Service

1. Vào Render Dashboard
2. Click **"New +"** → **"Web Service"**
3. Connect GitHub repository
4. Điền thông tin:

**Basic:**
- **Name**: `ecommerce-user-service`
- **Region**: `Singapore`
- **Branch**: `main`
- **Root Directory**: `.` (để trống)
- **Runtime**: `Docker`
- **Dockerfile Path**: `src/Services/Users/ECommerce.User.API/Dockerfile`

**Instance:**
- **Plan**: **Free** ✅

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=postgresql://[COPY_FROM_STEP_2.1]
Jwt__Secret=your-super-secret-key-min-32-characters-long-for-production
Jwt__Issuer=ECommerceUserService
Jwt__Audience=ECommerceClient
Jwt__ExpirationMinutes=60
```

5. Click **"Create Web Service"**
6. Đợi ~5-10 phút để build và deploy
7. Copy URL (dạng: `https://ecommerce-user-service.onrender.com`)

---

### 3.2. Deploy Product Service

Tương tự User Service:

**Basic:**
- **Name**: `ecommerce-product-service`
- **Dockerfile Path**: `src/Services/Product/ECommerce.Product.API/Dockerfile`

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
MongoDB__ConnectionString=mongodb+srv://[FROM_STEP_2.3]
MongoDB__DatabaseName=ecommerce_product
```

---

### 3.3. Deploy Shopping Cart Service

**Basic:**
- **Name**: `ecommerce-cart-service`
- **Dockerfile Path**: `src/Services/ShoppingCart/ECommerce.ShoppingCart.API/Dockerfile`

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__Redis=redis-15540.crce264.ap-east-1-1.ec2.cloud.redislabs.com:15540,password=[YOUR_PASSWORD],ssl=true,abortConnect=false
Services__ProductService__Url=https://ecommerce-product-service.onrender.com
```

---

### 3.4. Deploy Order Service

**Basic:**
- **Name**: `ecommerce-order-service`
- **Dockerfile Path**: `src/Services/Order/ECommerce.Order.API/Dockerfile`

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=postgresql://[FROM_STEP_2.1]
Services__ShoppingCart=https://ecommerce-cart-service.onrender.com
Services__Payment=https://ecommerce-payment-service.onrender.com
Services__Product__RestUrl=https://ecommerce-product-service.onrender.com
Services__Product__GrpcUrl=https://ecommerce-product-service.onrender.com
Services__User__RestUrl=https://ecommerce-user-service.onrender.com
Services__User__GrpcUrl=https://ecommerce-user-service.onrender.com
Jwt__Secret=your-super-secret-key-min-32-characters-long-for-production
```

---

### 3.5. Deploy Payment Service

**Basic:**
- **Name**: `ecommerce-payment-service`
- **Dockerfile Path**: `src/Services/Payment/ECommerce.Payment.API/Dockerfile`

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=postgresql://[FROM_STEP_2.1]
PayOS__ClientId=your-payos-client-id
PayOS__ApiKey=your-payos-api-key
PayOS__ChecksumKey=your-payos-checksum-key
PayOS__ReturnUrl=https://your-frontend.com/payment/success
PayOS__CancelUrl=https://your-frontend.com/payment/cancel
Jwt__Secret=your-super-secret-key-min-32-characters-long-for-production
```

---

### 3.6. Deploy Notification Service

**Basic:**
- **Name**: `ecommerce-notification-service`
- **Dockerfile Path**: `src/Services/Notification/ECommerce.Notification.API/Dockerfile`

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
MongoDB__ConnectionString=mongodb+srv://[FROM_STEP_2.3]
MongoDB__DatabaseName=ecommerce_notification
Email__Provider=SMTP
Email__Smtp__Host=smtp.gmail.com
Email__Smtp__Port=587
Email__Smtp__EnableSsl=true
Email__Smtp__Username=your-email@gmail.com
Email__Smtp__Password=your-app-password
Email__Smtp__FromEmail=your-email@gmail.com
Email__Smtp__FromName=ECommerce Platform
```

---

## ✅ Bước 4: Verify Deployment

### 4.1. Check Service Health

Truy cập các URLs:
- User Service: `https://ecommerce-user-service.onrender.com/swagger`
- Product Service: `https://ecommerce-product-service.onrender.com/swagger`
- Cart Service: `https://ecommerce-cart-service.onrender.com/swagger`
- Order Service: `https://ecommerce-order-service.onrender.com/swagger`
- Payment Service: `https://ecommerce-payment-service.onrender.com/swagger`
- Notification Service: `https://ecommerce-notification-service.onrender.com/swagger`

### 4.2. Test API

**Register User:**
```bash
curl -X POST https://ecommerce-user-service.onrender.com/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123",
    "firstName": "Test",
    "lastName": "User"
  }'
```

---

## 📊 Monitoring

### View Logs
1. Vào Render Dashboard
2. Click vào service
3. Tab **"Logs"** để xem real-time logs

### View Metrics
1. Tab **"Metrics"** để xem:
   - CPU usage
   - Memory usage
   - Request count
   - Response time

---

## ⚠️ Lưu ý quan trọng

### Free Tier Limitations

**Web Services:**
- ✅ 750 hours/month mỗi service
- ⚠️ **Sleep sau 15 phút không có request**
- ⚠️ Cold start ~30 giây khi wake up
- ✅ 512MB RAM
- ✅ 0.1 CPU

**PostgreSQL:**
- ✅ 1GB storage
- ⚠️ **Expires sau 90 ngày** (cần renew)
- ✅ Không sleep

**Workaround cho Sleep:**
1. Dùng cron job ping services mỗi 10 phút:
   ```bash
   # Crontab
   */10 * * * * curl https://ecommerce-user-service.onrender.com/health
   ```

2. Hoặc dùng UptimeRobot (free): https://uptimerobot.com

---

## 💰 Cost Breakdown

| Service | Cost |
|---------|------|
| User Service | **$0** |
| Product Service | **$0** |
| Shopping Cart Service | **$0** |
| Order Service | **$0** |
| Payment Service | **$0** |
| Notification Service | **$0** |
| PostgreSQL | **$0** |
| Redis Cloud | **$0** |
| MongoDB Atlas | **$0** |
| **TOTAL** | **$0/month** ✅ |

---

## 🔄 Auto Deploy

Render tự động deploy khi push code lên GitHub:

```bash
git add .
git commit -m "Update feature"
git push origin main
```

Render sẽ tự động:
1. Detect changes
2. Build Docker image
3. Deploy new version
4. Zero-downtime deployment

---

## 🆙 Upgrade to Paid (Optional)

Nếu cần performance tốt hơn:

**Starter Plan ($7/service/month):**
- ✅ Không sleep
- ✅ 512MB RAM
- ✅ 0.5 CPU
- ✅ Custom domain

**Standard Plan ($25/service/month):**
- ✅ 2GB RAM
- ✅ 1 CPU
- ✅ Auto-scaling

---

## 🐛 Troubleshooting

### Service không start được

**Check logs:**
```bash
# Vào Render Dashboard → Service → Logs
```

**Common issues:**
1. **Port mismatch**: Đảm bảo `ASPNETCORE_URLS=http://+:8080`
2. **Database connection**: Check connection string
3. **Missing env vars**: Check environment variables

### Cold start chậm

**Solutions:**
1. Dùng UptimeRobot ping mỗi 10 phút
2. Upgrade to Starter plan ($7/month)
3. Optimize Docker image size

### Database connection timeout

**Solutions:**
1. Check database IP whitelist
2. Verify connection string
3. Check database status

---

## 📚 Resources

- Render Docs: https://render.com/docs
- Render Status: https://status.render.com
- Support: https://render.com/support

---

**Deployment Date**: November 2025  
**Maintained By**: Development Team
