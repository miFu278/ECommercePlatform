# CORS Configuration Summary

## ✅ Đã cấu hình CORS

### 1. API Gateway (Port 5000)
**File:** `src/ApiGateway/Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite dev server
                "http://localhost:3000",  // Alternative frontend port
                "http://localhost:4173"   // Vite preview
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});

// Middleware order
app.UseCors("AllowFrontend");  // ✅ Before Authentication
app.UseAuthentication();
app.UseAuthorization();
```

### 2. User Service (Port 5010/5011)
**File:** `src/Services/Users/ECommerce.User.API/Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

### 3. Product Service (Port 5020/5021)
**File:** `src/Services/Product/ECommerce.Product.API/Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

## 🔧 Middleware Order (Quan trọng!)

Thứ tự middleware phải đúng để CORS hoạt động:

```csharp
app.UseCors("AllowFrontend");    // 1. CORS FIRST
app.UseAuthentication();          // 2. Then Authentication
app.UseAuthorization();           // 3. Then Authorization
app.UseOcelot();                  // 4. Finally Ocelot
```

## 🧪 Test CORS

### 1. Từ Browser Console:
```javascript
fetch('http://localhost:5000/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'test@example.com',
    password: 'password123'
  })
})
.then(res => res.json())
.then(data => console.log(data))
.catch(err => console.error(err));
```

### 2. Check Response Headers:
Trong Network tab, kiểm tra response headers:
```
Access-Control-Allow-Origin: http://localhost:5173
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: *
Access-Control-Allow-Credentials: true
```

## 🚨 Common Issues

### Issue 1: CORS error vẫn xuất hiện
**Nguyên nhân:** Middleware order sai
**Giải pháp:** Đảm bảo `UseCors()` được gọi TRƯỚC `UseAuthentication()`

### Issue 2: Credentials not allowed
**Nguyên nhân:** Sử dụng `AllowAnyOrigin()` với `AllowCredentials()`
**Giải pháp:** Dùng `WithOrigins()` thay vì `AllowAnyOrigin()`

### Issue 3: Preflight OPTIONS request failed
**Nguyên nhân:** Server không xử lý OPTIONS request
**Giải pháp:** CORS middleware tự động xử lý OPTIONS, đảm bảo nó được add đúng

## 🔒 Security Best Practices

### Development (Hiện tại)
```csharp
// API Gateway - Specific origins
policy.WithOrigins("http://localhost:5173")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();

// Backend Services - Allow all (vì chỉ API Gateway gọi)
policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
```

### Production (Khuyến nghị)
```csharp
// API Gateway
policy.WithOrigins(
        "https://yourdomain.com",
        "https://www.yourdomain.com"
    )
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .WithHeaders("Content-Type", "Authorization")
    .AllowCredentials();

// Backend Services - Chỉ cho phép API Gateway
policy.WithOrigins("http://apigateway:8080")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

## 📝 Environment-based CORS

Để linh hoạt hơn, có thể config CORS theo environment:

```csharp
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

**appsettings.Development.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:3000"
    ]
  }
}
```

**appsettings.Production.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://yourdomain.com"
    ]
  }
}
```

## ✅ Checklist

- [x] API Gateway có CORS policy "AllowFrontend"
- [x] User Service có CORS policy "AllowAll"
- [x] Product Service có CORS policy "AllowAll"
- [x] Middleware order đúng (CORS → Auth → Authorization)
- [x] AllowCredentials enabled cho API Gateway
- [x] Specific origins cho API Gateway (security)
- [ ] Test CORS từ frontend
- [ ] Kiểm tra preflight OPTIONS requests
- [ ] Update CORS cho production

## 🚀 Next Steps

1. **Restart API Gateway:**
   ```bash
   cd ECommercePlatform/src/ApiGateway
   dotnet run
   ```

2. **Test từ Frontend:**
   ```bash
   cd ECommerceUI
   npm run dev
   ```

3. **Kiểm tra Network tab** trong browser DevTools để xem CORS headers

4. **Nếu vẫn lỗi:** Check console errors và response headers
