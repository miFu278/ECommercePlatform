# Shopping Cart Service - Quick Start Guide

## ✅ Prerequisites
- .NET 8 SDK
- Docker (for Redis)
- Redis running on port 6379

## 🚀 Start in 3 Steps

### Step 1: Start Redis (if not running)
```bash
cd docker
docker-compose -f docker-compose.infrastructure.yml up redis -d
```

**Redis Info:**
- Host: `localhost`
- Port: `6379`
- Password: `redis123`

### Step 2: Run API
```bash
cd src/Services/ShoppingCart/ECommerce.ShoppingCart.API
dotnet run
```

**Expected Output:**
```
✅ Redis connection successful!
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5002
```

### Step 3: Open Swagger
Open your browser: **http://localhost:5002/swagger**

---

## 🧪 Test API

### Option 1: Using Swagger UI
1. Open http://localhost:5002/swagger
2. Click on "GET /api/cart"
3. Click "Try it out"
4. Add header: `X-User-Id: 00000000-0000-0000-0000-000000000001`
5. Click "Execute"

### Option 2: Using curl
```bash
# Get empty cart
curl http://localhost:5002/api/cart -H "X-User-Id: 00000000-0000-0000-0000-000000000001"

# Response:
# {"userId":"00000000-0000-0000-0000-000000000001","items":[],"totalItems":0,"subtotal":0,"total":0}
```

### Option 3: Using HTTP file
Open `shopping-cart.http` in VS Code with REST Client extension

---

## 🔍 Verify Redis Connection

### Check Redis is running:
```bash
docker ps | grep redis
```

### Test Redis connection:
```bash
docker exec ecommerce-redis redis-cli -a redis123 PING
# Response: PONG
```

### View cart data in Redis:
```bash
docker exec ecommerce-redis redis-cli -a redis123

# Inside Redis CLI:
KEYS cart:*
GET cart:00000000-0000-0000-0000-000000000001
TTL cart:00000000-0000-0000-0000-000000000001
```

---

## 📊 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/cart` | Get cart |
| POST | `/api/cart/items` | Add item |
| PUT | `/api/cart/items/{productId}` | Update quantity |
| DELETE | `/api/cart/items/{productId}` | Remove item |
| DELETE | `/api/cart` | Clear cart |
| POST | `/api/cart/merge` | Merge carts |

---

## 🐛 Troubleshooting

### Issue: "Redis connection failed"
**Solution:**
```bash
# Check if Redis is running
docker ps | grep redis

# If not running, start it
cd docker
docker-compose -f docker-compose.infrastructure.yml up redis -d

# Verify connection
docker exec ecommerce-redis redis-cli -a redis123 PING
```

### Issue: "Port 5002 already in use"
**Solution:**
```bash
# Find process using port 5002
netstat -ano | findstr :5002

# Kill the process (replace PID)
taskkill /PID <PID> /F

# Or use different port
dotnet run --urls "http://localhost:5003"
```

### Issue: "launchSettings.json error"
**Solution:**
```bash
# Delete bin/obj folders
Remove-Item -Recurse -Force bin,obj

# Run without launch profile
dotnet run --no-launch-profile --urls "http://localhost:5002"
```

### Issue: "Cannot access localhost:5002"
**Solution:**
```bash
# Check if API is running
netstat -ano | findstr :5002

# Check firewall
# Make sure Windows Firewall allows port 5002

# Try 127.0.0.1 instead of localhost
curl http://127.0.0.1:5002/api/cart
```

---

## 📝 Test Scenarios

### 1. Add Item to Cart
```bash
curl -X POST http://localhost:5002/api/cart/items \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 00000000-0000-0000-0000-000000000001" \
  -d '{"productId":"11111111-1111-1111-1111-111111111111","quantity":2}'
```

### 2. Get Cart
```bash
curl http://localhost:5002/api/cart \
  -H "X-User-Id: 00000000-0000-0000-0000-000000000001"
```

### 3. Update Quantity
```bash
curl -X PUT http://localhost:5002/api/cart/items/11111111-1111-1111-1111-111111111111 \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 00000000-0000-0000-0000-000000000001" \
  -d '{"quantity":5}'
```

### 4. Remove Item
```bash
curl -X DELETE http://localhost:5002/api/cart/items/11111111-1111-1111-1111-111111111111 \
  -H "X-User-Id: 00000000-0000-0000-0000-000000000001"
```

### 5. Clear Cart
```bash
curl -X DELETE http://localhost:5002/api/cart \
  -H "X-User-Id: 00000000-0000-0000-0000-000000000001"
```

---

## 🎯 What's Working

✅ **API Running:** http://localhost:5002  
✅ **Swagger UI:** http://localhost:5002/swagger  
✅ **Redis Connected:** localhost:6379 (password: redis123)  
✅ **All Endpoints:** 6 endpoints working  
✅ **Validation:** FluentValidation active  
✅ **Error Handling:** Graceful degradation  

---

## 📚 Next Steps

1. **Test with HTTP file:** Open `shopping-cart.http`
2. **Integrate with Product Service:** Update ProductService.cs
3. **Add JWT Authentication:** Replace X-User-Id header
4. **Monitor Redis:** Use RedisInsight or redis-cli MONITOR
5. **Load Testing:** Test with multiple concurrent users

---

## 🔗 Related Documentation

- [README.md](README.md) - Overview
- [REDIS_GUIDE.md](REDIS_GUIDE.md) - Redis implementation details
- [SHOPPING_CART_COMPLETE.md](SHOPPING_CART_COMPLETE.md) - Complete documentation
- [shopping-cart.http](ECommerce.ShoppingCart.API/shopping-cart.http) - API tests

---

**Status:** ✅ Running and Ready to Use!  
**API:** http://localhost:5002  
**Swagger:** http://localhost:5002/swagger  
**Redis:** localhost:6379 (password: redis123)
