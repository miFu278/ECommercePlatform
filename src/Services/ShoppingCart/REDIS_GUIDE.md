# Shopping Cart Service - Redis Implementation Guide

## 🎯 Why Redis for Shopping Cart?

### Perfect Use Case
Shopping cart is an **ideal candidate for Redis** because:

1. **Temporary Data** - Carts expire after 7 days
2. **High Read/Write** - Users frequently add/remove items
3. **Session-Based** - Tied to user session
4. **Fast Access** - In-memory = millisecond response times
5. **Simple Structure** - Key-value with JSON serialization
6. **TTL Support** - Automatic expiration

### Redis vs Database Comparison

| Feature | Redis | PostgreSQL | MongoDB |
|---------|-------|------------|---------|
| **Speed** | <1ms | 10-50ms | 5-20ms |
| **TTL** | Native | Manual cleanup | TTL indexes |
| **Scalability** | Excellent | Good | Good |
| **Persistence** | Optional | Always | Always |
| **Use Case** | Temporary data | Permanent data | Flexible data |

---

## 🏗️ Architecture

### Data Structure

```
Redis Key: cart:{userId}
Value: JSON serialized ShoppingCart object
TTL: 7 days (configurable)
```

### Example Redis Data

```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "items": [
    {
      "productId": "prod-001",
      "productName": "iPhone 15 Pro",
      "sku": "IPH15PRO",
      "price": 999.00,
      "quantity": 2,
      "stockQuantity": 50,
      "isAvailable": true
    }
  ],
  "createdAt": "2024-11-12T10:00:00Z",
  "updatedAt": "2024-11-12T10:05:00Z"
}
```

---

## 📦 Redis Setup

### 1. Install Redis (Docker)

```bash
# Start Redis
docker run -d --name redis -p 6379:6379 redis:7-alpine

# Or with docker-compose
docker-compose -f docker-compose.infrastructure.yml up redis -d
```

### 2. Verify Connection

```bash
# Connect to Redis CLI
docker exec -it redis redis-cli

# Test commands
PING
# Response: PONG

# Set a test key
SET test "Hello Redis"
GET test
# Response: "Hello Redis"

# Check TTL
TTL test
# Response: -1 (no expiration)

# Set with expiration (60 seconds)
SETEX test2 60 "Expires in 60s"
TTL test2
# Response: 60, 59, 58...
```

---

## 🔧 Implementation Details

### 1. Redis Connection (StackExchange.Redis)

```csharp
// Program.cs
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse("localhost:6379");
    configuration.AbortOnConnectFail = false; // Don't crash if Redis is down
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    return ConnectionMultiplexer.Connect(configuration);
});
```

**Key Points:**
- `Singleton` - One connection for entire app (connection pooling)
- `AbortOnConnectFail = false` - Graceful degradation
- Connection is thread-safe and reusable

### 2. Repository Pattern

```csharp
public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly RedisContext _context;
    private const string KeyPrefix = "cart:";
    
    private string GetKey(Guid userId) => $"{KeyPrefix}{userId}";
    
    public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId)
    {
        var key = GetKey(userId);
        var value = await _context.Database.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return null;
            
        return JsonSerializer.Deserialize<ShoppingCart>(value!);
    }
    
    public async Task SaveAsync(ShoppingCart cart, TimeSpan? expiration = null)
    {
        var key = GetKey(cart.UserId);
        var value = JsonSerializer.Serialize(cart);
        var ttl = expiration ?? TimeSpan.FromDays(7);
        
        await _context.Database.StringSetAsync(key, value, ttl);
    }
}
```

**Key Points:**
- Key naming: `cart:{userId}` for easy identification
- JSON serialization for complex objects
- TTL (Time To Live) for automatic expiration
- Async operations for better performance

### 3. Service Layer

```csharp
public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto dto)
{
    // 1. Get product info from Product Service
    var productInfo = await _productService.GetProductInfoAsync(dto.ProductId);
    
    // 2. Get or create cart from Redis
    var cart = await _cartRepository.GetByUserIdAsync(userId);
    if (cart == null)
    {
        cart = new ShoppingCart { UserId = userId };
    }
    
    // 3. Add item to cart
    cart.AddItem(new CartItem { ... });
    
    // 4. Save to Redis with TTL
    await _cartRepository.SaveAsync(cart);
    
    return _mapper.Map<CartDto>(cart);
}
```

---

## 🚀 Redis Operations

### Basic Operations

```csharp
// GET - Retrieve cart
var cart = await db.StringGetAsync("cart:user-123");

// SET - Save cart with 7-day expiration
await db.StringSetAsync("cart:user-123", jsonCart, TimeSpan.FromDays(7));

// DELETE - Remove cart
await db.KeyDeleteAsync("cart:user-123");

// EXISTS - Check if cart exists
bool exists = await db.KeyExistsAsync("cart:user-123");

// TTL - Get remaining time
TimeSpan? ttl = await db.KeyTimeToLiveAsync("cart:user-123");

// EXPIRE - Extend expiration
await db.KeyExpireAsync("cart:user-123", TimeSpan.FromDays(7));
```

### Advanced Operations

```csharp
// SCAN - Find all carts (for admin/cleanup)
var server = redis.GetServer(endpoint);
var keys = server.Keys(pattern: "cart:*");

// MGET - Get multiple carts at once
var keys = new RedisKey[] { "cart:user-1", "cart:user-2" };
var values = await db.StringGetAsync(keys);

// PIPELINE - Batch operations
var batch = db.CreateBatch();
var task1 = batch.StringSetAsync("cart:user-1", json1);
var task2 = batch.StringSetAsync("cart:user-2", json2);
batch.Execute();
await Task.WhenAll(task1, task2);
```

---

## 🎯 Key Features Implemented

### 1. Cart Expiration (TTL)
- Default: 7 days
- Auto-extends on activity
- Automatic cleanup by Redis

```csharp
// Save with custom expiration
await _cartRepository.SaveAsync(cart, TimeSpan.FromHours(24));

// Extend expiration
await _cartRepository.ExtendExpirationAsync(userId, TimeSpan.FromDays(7));
```

### 2. Cart Merging (Anonymous → Authenticated)
When user logs in, merge anonymous cart:

```csharp
public async Task<bool> MergeCartsAsync(Guid anonymousUserId, Guid authenticatedUserId)
{
    var anonymousCart = await GetByUserIdAsync(anonymousUserId);
    var authenticatedCart = await GetByUserIdAsync(authenticatedUserId);
    
    // Merge items
    foreach (var item in anonymousCart.Items)
    {
        authenticatedCart.AddItem(item);
    }
    
    await SaveAsync(authenticatedCart);
    await DeleteAsync(anonymousUserId); // Clean up
    
    return true;
}
```

### 3. Product Info Refresh
Cart refreshes product prices/availability on each GET:

```csharp
public async Task<CartDto?> GetCartAsync(Guid userId)
{
    var cart = await _cartRepository.GetByUserIdAsync(userId);
    
    // Refresh product info from Product Service
    await RefreshCartItemsAsync(cart);
    
    // Save updated cart
    await _cartRepository.SaveAsync(cart);
    
    return _mapper.Map<CartDto>(cart);
}
```

### 4. Stock Validation
Validates stock before adding/updating:

```csharp
if (productInfo.StockQuantity < dto.Quantity)
{
    throw new InvalidOperationException(
        $"Only {productInfo.StockQuantity} items available"
    );
}
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

## 🧪 Testing

### 1. Start Redis
```bash
docker run -d --name redis -p 6379:6379 redis:7-alpine
```

### 2. Start API
```bash
cd src/Services/ShoppingCart/ECommerce.ShoppingCart.API
dotnet run
```

### 3. Test with Redis CLI
```bash
# Connect to Redis
docker exec -it redis redis-cli

# Watch cart operations in real-time
MONITOR

# In another terminal, make API calls
# You'll see Redis commands in MONITOR output

# Check cart data
GET cart:00000000-0000-0000-0000-000000000001

# Check TTL
TTL cart:00000000-0000-0000-0000-000000000001

# List all carts
KEYS cart:*

# Delete specific cart
DEL cart:00000000-0000-0000-0000-000000000001

# Flush all data (careful!)
FLUSHDB
```

### 4. Test with HTTP file
Open `shopping-cart.http` and run tests

---

## 🔍 Redis Monitoring

### Redis CLI Commands

```bash
# Connection info
INFO server

# Memory usage
INFO memory

# Check connected clients
CLIENT LIST

# Monitor commands in real-time
MONITOR

# Get database size
DBSIZE

# Check specific key
TYPE cart:user-123
TTL cart:user-123
OBJECT ENCODING cart:user-123
```

### Performance Metrics

```bash
# Latency
redis-cli --latency

# Stats
INFO stats

# Slow log (queries > 10ms)
SLOWLOG GET 10
```

---

## ⚡ Performance Tips

### 1. Connection Pooling
```csharp
// ✅ Good - Singleton connection
builder.Services.AddSingleton<IConnectionMultiplexer>(...);

// ❌ Bad - New connection per request
builder.Services.AddScoped<IConnectionMultiplexer>(...);
```

### 2. Batch Operations
```csharp
// ✅ Good - Batch multiple operations
var batch = db.CreateBatch();
var tasks = new List<Task>();
foreach (var cart in carts)
{
    tasks.Add(batch.StringSetAsync($"cart:{cart.UserId}", json));
}
batch.Execute();
await Task.WhenAll(tasks);

// ❌ Bad - Sequential operations
foreach (var cart in carts)
{
    await db.StringSetAsync($"cart:{cart.UserId}", json);
}
```

### 3. Compression (Optional)
For large carts, compress JSON:

```csharp
using System.IO.Compression;

public static byte[] Compress(string json)
{
    var bytes = Encoding.UTF8.GetBytes(json);
    using var output = new MemoryStream();
    using (var gzip = new GZipStream(output, CompressionMode.Compress))
    {
        gzip.Write(bytes, 0, bytes.Length);
    }
    return output.ToArray();
}
```

---

## 🛡️ Error Handling

### Graceful Degradation

```csharp
public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId)
{
    try
    {
        var key = GetKey(userId);
        var value = await _context.Database.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return null;
            
        return JsonSerializer.Deserialize<ShoppingCart>(value!);
    }
    catch (RedisConnectionException ex)
    {
        // Log error
        _logger.LogError(ex, "Redis connection failed");
        
        // Fallback to database or return empty cart
        return null;
    }
    catch (RedisTimeoutException ex)
    {
        _logger.LogError(ex, "Redis timeout");
        return null;
    }
}
```

---

## 🔐 Security Considerations

### 1. User Isolation
```csharp
// ✅ Always use userId in key
private string GetKey(Guid userId) => $"cart:{userId}";

// ❌ Never use predictable keys
private string GetKey() => "cart"; // All users share same cart!
```

### 2. Input Validation
```csharp
// FluentValidation
RuleFor(x => x.Quantity)
    .GreaterThan(0)
    .LessThanOrEqualTo(100);
```

### 3. Redis Authentication (Production)
```csharp
var configuration = ConfigurationOptions.Parse("localhost:6379");
configuration.Password = "your-redis-password";
```

---

## 📈 Scaling Redis

### 1. Redis Cluster
For high availability:

```bash
# 3 master + 3 replica nodes
docker-compose -f redis-cluster.yml up
```

### 2. Redis Sentinel
For automatic failover:

```csharp
var configuration = new ConfigurationOptions
{
    ServiceName = "mymaster",
    EndPoints = { "sentinel1:26379", "sentinel2:26379" }
};
```

### 3. Redis Cloud
- AWS ElastiCache
- Azure Cache for Redis
- Redis Enterprise Cloud

---

## 🎓 Redis Best Practices

### ✅ Do's
1. Use connection pooling (Singleton)
2. Set appropriate TTL
3. Use key prefixes (`cart:`, `session:`)
4. Handle connection failures gracefully
5. Monitor memory usage
6. Use batch operations for multiple keys
7. Compress large values if needed

### ❌ Don'ts
1. Don't create new connections per request
2. Don't store large objects (>1MB)
3. Don't use Redis as primary database
4. Don't forget to set TTL
5. Don't use blocking operations in async code
6. Don't store sensitive data without encryption

---

## 🆚 When NOT to Use Redis

Use **Database** instead if:
- Data must persist permanently
- Need complex queries/joins
- Need ACID transactions
- Data > 1MB per record
- Need full-text search

Use **MongoDB** instead if:
- Need flexible schema
- Need aggregation pipelines
- Data is document-oriented
- Need geospatial queries

---

## 📚 Resources

### Official Docs
- [Redis Documentation](https://redis.io/docs/)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)

### Commands Reference
- [Redis Commands](https://redis.io/commands/)
- [Data Types](https://redis.io/docs/data-types/)

### Tools
- [RedisInsight](https://redis.com/redis-enterprise/redis-insight/) - GUI client
- [redis-cli](https://redis.io/docs/ui/cli/) - Command line

---

## ✅ Summary

### What We Built
- ✅ Shopping Cart Service with Redis
- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ Cart expiration (7-day TTL)
- ✅ Cart merging (anonymous → authenticated)
- ✅ Product info refresh
- ✅ Stock validation
- ✅ FluentValidation
- ✅ Error handling

### Redis Benefits
- ⚡ **Fast** - <1ms response time
- 🔄 **TTL** - Automatic expiration
- 📦 **Simple** - Key-value storage
- 🚀 **Scalable** - Easy to scale horizontally
- 💰 **Cost-effective** - Less database load

### Next Steps
1. ✅ Test with Redis CLI
2. ⏳ Integrate with Product Service
3. ⏳ Add JWT authentication
4. ⏳ Add caching layer
5. ⏳ Production deployment

---

**Status:** ✅ Complete and Ready to Test  
**Redis Version:** 7.x  
**Package:** StackExchange.Redis 2.8.16
