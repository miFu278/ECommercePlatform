# Shopping Cart Service

## Overview
Shopping Cart microservice built with **Redis** for fast, temporary cart storage.

## Tech Stack
- **.NET 8** - Framework
- **Redis** - In-memory data store
- **StackExchange.Redis** - Redis client
- **Clean Architecture** - Project structure
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping

## Why Redis?
Shopping cart is perfect for Redis because:
- ⚡ **Fast** - In-memory, <1ms response
- 🔄 **TTL** - Auto-expires after 7 days
- 📦 **Simple** - Key-value storage
- 🚀 **Scalable** - Easy horizontal scaling

## Project Structure
```
ECommerce.ShoppingCart/
├── Domain/                      # Entities & Interfaces
│   ├── Entities/
│   │   ├── ShoppingCart.cs
│   │   └── CartItem.cs
│   └── Interfaces/
│       └── IShoppingCartRepository.cs
├── Application/                 # Business Logic
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   ├── Validators/
│   └── Mappings/
├── Infrastructure/              # Redis Implementation
│   ├── Data/
│   │   └── RedisContext.cs
│   ├── Repositories/
│   │   └── ShoppingCartRepository.cs
│   └── Services/
│       └── ProductService.cs    # HTTP client to Product Service
└── API/                         # REST API
    ├── Controllers/
    ├── Program.cs
    └── appsettings.json
```

## Features
- ✅ Add items to cart
- ✅ Update item quantity
- ✅ Remove items
- ✅ Clear cart
- ✅ Cart expiration (7 days)
- ✅ Cart merging (anonymous → authenticated)
- ✅ Product info refresh
- ✅ Stock validation
- ✅ FluentValidation
- ✅ Swagger documentation

## Quick Start

### 1. Start Redis
```bash
docker run -d --name redis -p 6379:6379 redis:7-alpine
```

### 2. Update appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=redis123"
  }
}
```

### 3. Run API
```bash
cd src/Services/ShoppingCart/ECommerce.ShoppingCart.API
dotnet run --urls "http://localhost:5002"
```

### 4. Open Swagger
http://localhost:5002/swagger

### 4. Test with Redis CLI
```bash
# Connect to Redis
docker exec -it redis redis-cli

# Watch operations
MONITOR

# Check cart
GET cart:00000000-0000-0000-0000-000000000001

# Check TTL
TTL cart:00000000-0000-0000-0000-000000000001

# List all carts
KEYS cart:*
```

## API Endpoints

### Cart Operations
```http
GET    /api/cart                      # Get cart
POST   /api/cart/items                # Add item
PUT    /api/cart/items/{productId}    # Update quantity
DELETE /api/cart/items/{productId}    # Remove item
DELETE /api/cart                      # Clear cart
POST   /api/cart/merge                # Merge carts
```

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Services": {
    "ProductService": {
      "Url": "http://localhost:5001"
    }
  }
}
```

## Redis Data Structure

### Key Format
```
cart:{userId}
```

### Value (JSON)
```json
{
  "userId": "guid",
  "items": [
    {
      "productId": "guid",
      "productName": "string",
      "price": 999.00,
      "quantity": 2,
      "stockQuantity": 50,
      "isAvailable": true
    }
  ],
  "createdAt": "2024-11-12T10:00:00Z",
  "updatedAt": "2024-11-12T10:05:00Z",
  "expiresAt": "2024-11-19T10:00:00Z"
}
```

### TTL
- Default: 7 days
- Auto-extends on activity
- Automatic cleanup by Redis

## Testing

### HTTP Tests
See `shopping-cart.http` for complete test scenarios

### Redis CLI Tests
```bash
# Get cart
GET cart:00000000-0000-0000-0000-000000000001

# Check expiration
TTL cart:00000000-0000-0000-0000-000000000001

# Delete cart
DEL cart:00000000-0000-0000-0000-000000000001

# List all carts
KEYS cart:*

# Monitor operations
MONITOR
```

## Integration with Other Services

### Product Service
- Get product info (price, stock, availability)
- Validate product exists
- Check stock quantity

### User Service (Future)
- Get userId from JWT token
- Validate user authentication

### Order Service (Future)
- Convert cart to order
- Clear cart after checkout

## Error Handling

### Graceful Degradation
```csharp
try
{
    var cart = await _cartRepository.GetByUserIdAsync(userId);
}
catch (RedisConnectionException)
{
    // Log error and return empty cart
    return new CartDto { UserId = userId };
}
```

### Validation Errors
```json
{
  "errors": {
    "Quantity": ["Quantity must be greater than 0"]
  }
}
```

## Performance

### Redis Operations
- GET: <1ms
- SET: <1ms
- DEL: <1ms

### Optimizations
- Connection pooling (Singleton)
- Batch operations for multiple carts
- JSON serialization
- Optional compression for large carts

## Security

### User Isolation
- Each cart has unique key: `cart:{userId}`
- Users can only access their own cart

### Input Validation
- FluentValidation for all DTOs
- Quantity limits (1-100)
- Product ID validation

### Redis Authentication (Production)
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=your-password"
  }
}
```

## Monitoring

### Redis Metrics
```bash
# Memory usage
INFO memory

# Connected clients
CLIENT LIST

# Slow queries
SLOWLOG GET 10

# Database size
DBSIZE
```

### Application Metrics
- Cart operations per second
- Average cart size
- Cart expiration rate
- Product service latency

## Deployment

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "ECommerce.ShoppingCart.API.dll"]
```

### Environment Variables
```bash
ConnectionStrings__Redis=redis:6379
Services__ProductService__Url=http://product-service:5001
```

### Redis Production
- AWS ElastiCache
- Azure Cache for Redis
- Redis Enterprise Cloud

## Documentation
- [REDIS_GUIDE.md](REDIS_GUIDE.md) - Complete Redis implementation guide
- [shopping-cart.http](ECommerce.ShoppingCart.API/shopping-cart.http) - API tests

## Next Steps
1. ✅ Test with Redis CLI
2. ⏳ Integrate with Product Service
3. ⏳ Add JWT authentication
4. ⏳ Add health checks
5. ⏳ Production deployment

## Status
✅ **Complete and Ready to Test**

---

**Port:** 5002  
**Redis:** localhost:6379  
**Swagger:** http://localhost:5002/swagger
