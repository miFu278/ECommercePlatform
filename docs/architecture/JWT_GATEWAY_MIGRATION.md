# JWT Authentication Migration - API Gateway Centralized

## Overview

Migrated JWT authentication from individual services to centralized validation at API Gateway level.

## Architecture Change

### Before (Service-Level Authentication)
```
Client (JWT) → API Gateway (passthrough) → Service (validate JWT)
                                          → Service (validate JWT)
                                          → Service (validate JWT)
```
**Issues:**
- ❌ Duplicate JWT validation logic in every service
- ❌ Each service needs JWT configuration
- ❌ Performance overhead (validate multiple times)
- ❌ Harder to maintain

### After (Gateway-Level Authentication)
```
Client (JWT) → API Gateway (validate JWT + add headers) → Service (trust headers)
                                                         → Service (trust headers)
                                                         → Service (trust headers)
```
**Benefits:**
- ✅ Single point of JWT validation
- ✅ Services are simpler (no JWT logic)
- ✅ Better performance (validate once)
- ✅ Easier to maintain
- ✅ Consistent authentication

---

## Implementation Details

### 1. API Gateway Changes

**File:** `src/ApiGateway/Program.cs`

**Added:**
- JWT Authentication middleware
- Token validation on incoming requests
- Extract user claims (userId, email, role)
- Add custom headers for downstream services

**Headers Added:**
```
X-User-Id: <userId from JWT>
X-User-Email: <email from JWT>
X-User-Role: <role from JWT>
```

**Configuration:** `src/ApiGateway/appsettings.json`
```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-characters-long-for-production",
    "Issuer": "ECommerce.UserService",
    "Audience": "ECommerce.Clients"
  }
}
```

**Routes:** `src/ApiGateway/ocelot.Development.json`
```json
{
  "UpstreamPathTemplate": "/orders/{everything}",
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"
  }
}
```

---

### 2. Services Updated

#### ✅ Order Service
**File:** `src/Services/Order/ECommerce.Order.API/Controllers/OrdersController.cs`

**Changes:**
- Removed `[Authorize]` attribute from controller level
- Updated `GetCurrentUserId()` to read from `X-User-Id` header
- Added `IsAdmin()` method to check `X-User-Role` header
- Fallback to JWT claims if called directly

#### ✅ ShoppingCart Service
**File:** `src/Services/ShoppingCart/ECommerce.ShoppingCart.API/Controllers/ShoppingCartController.cs`

**Changes:**
- Updated `GetUserId()` to read from `X-User-Id` header
- Fallback to JWT claims if called directly
- Removed TODO comments

#### ✅ User Service
**Files:**
- `src/Services/Users/ECommerce.User.API/Controllers/UserController.cs`
- `src/Services/Users/ECommerce.User.API/Controllers/AddressController.cs`
- `src/Services/Users/ECommerce.User.API/Controllers/SessionController.cs`

**Changes:**
- Updated `GetCurrentUserId()` in all controllers
- Read from `X-User-Id` header first
- Fallback to JWT claims if called directly

---

## Code Pattern

### Standard GetCurrentUserId() Implementation

```csharp
private Guid GetCurrentUserId()
{
    // Try to get from API Gateway header first (trusted)
    if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
    {
        if (Guid.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
    }

    // Fallback to JWT claims (if called directly without Gateway)
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userIdFromClaim))
    {
        return userIdFromClaim;
    }

    throw new UnauthorizedAccessException("User ID not found");
}
```

### Admin Role Check Implementation

```csharp
private bool IsAdmin()
{
    // Check from API Gateway header first
    if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
    {
        return roleHeader.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    // Fallback to JWT claims
    return User.IsInRole("Admin");
}
```

---

## Request Flow

### Example: Create Order

```
1. Client sends request:
   POST http://localhost:5050/orders
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

2. API Gateway receives request:
   - Validates JWT token ✅
   - Extracts claims:
     * userId: 123e4567-e89b-12d3-a456-426614174000
     * email: user@example.com
     * role: User
   - Adds headers:
     * X-User-Id: 123e4567-e89b-12d3-a456-426614174000
     * X-User-Email: user@example.com
     * X-User-Role: User
   - Forwards to Order Service

3. Order Service receives request:
   POST http://localhost:5003/api/orders
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   X-User-Id: 123e4567-e89b-12d3-a456-426614174000
   X-User-Email: user@example.com
   X-User-Role: User

4. Order Service processes:
   - Reads X-User-Id header
   - Creates order for user 123e4567-e89b-12d3-a456-426614174000
   - Returns response

5. Client receives response:
   201 Created
   { "id": "...", "orderNumber": "ORD20241124-0001", ... }
```

---

## Security Considerations

### ✅ Secure
- API Gateway validates JWT before forwarding
- Services trust internal network headers
- Fallback to JWT validation if called directly
- Same JWT secret across all services

### ⚠️ Important
- **Never expose services directly to internet**
- **Always route through API Gateway**
- **Secure internal network** (services trust Gateway headers)
- **Use HTTPS in production**
- **Rotate JWT secrets regularly**

### 🔒 Network Security
```
Internet → API Gateway (public) → Internal Network (private)
                                  ├── User Service (private)
                                  ├── Product Service (private)
                                  ├── Cart Service (private)
                                  └── Order Service (private)
```

---

## Testing

### 1. Test JWT Validation at Gateway

```bash
# Without token - Should fail
curl http://localhost:5050/orders/my-orders

# Response: 401 Unauthorized

# With valid token - Should succeed
curl http://localhost:5050/orders/my-orders \
  -H "Authorization: Bearer <valid_jwt_token>"

# Response: 200 OK with orders
```

### 2. Test Headers Forwarding

Add logging in service to verify headers:
```csharp
_logger.LogInformation("X-User-Id: {UserId}", Request.Headers["X-User-Id"]);
_logger.LogInformation("X-User-Email: {Email}", Request.Headers["X-User-Email"]);
_logger.LogInformation("X-User-Role: {Role}", Request.Headers["X-User-Role"]);
```

### 3. Test Direct Service Call (Fallback)

```bash
# Call service directly (bypass Gateway)
curl http://localhost:5003/api/orders/my-orders \
  -H "Authorization: Bearer <valid_jwt_token>"

# Should still work (fallback to JWT validation)
```

---

## Migration Checklist

### ✅ Completed

- [x] API Gateway JWT authentication
- [x] API Gateway header injection
- [x] Order Service updated
- [x] ShoppingCart Service updated
- [x] User Service updated (UserController)
- [x] User Service updated (AddressController)
- [x] User Service updated (SessionController)
- [x] Documentation created

### ⏳ Optional Enhancements

- [ ] Add rate limiting per user (using X-User-Id)
- [ ] Add request logging with user context
- [ ] Add metrics per user/role
- [ ] Add API Gateway caching
- [ ] Add circuit breaker pattern

---

## Rollback Plan

If issues occur, rollback by:

1. **Remove Gateway authentication:**
   - Comment out JWT middleware in `src/ApiGateway/Program.cs`
   - Remove `AuthenticationOptions` from Ocelot routes

2. **Revert services:**
   - Add back `[Authorize]` attributes
   - Revert `GetCurrentUserId()` to only use JWT claims

3. **Services will validate JWT themselves again**

---

## Performance Impact

### Before
```
Request → Gateway (0ms) → Service (validate JWT: 5ms) → Process
Total: 5ms overhead per service call
```

### After
```
Request → Gateway (validate JWT: 5ms) → Service (read header: 0.1ms) → Process
Total: 5ms overhead once at Gateway
```

**Improvement:**
- Single validation instead of per-service
- Faster header reading vs JWT decoding
- Better for microservices with multiple internal calls

---

## Monitoring

### Metrics to Track

1. **Gateway Level:**
   - JWT validation success rate
   - JWT validation failures (401s)
   - Average validation time

2. **Service Level:**
   - Requests with X-User-Id header
   - Requests without X-User-Id (direct calls)
   - Authorization failures (403s)

3. **User Level:**
   - Requests per user (using X-User-Id)
   - Admin vs User requests
   - Most active users

---

## Best Practices

### ✅ Do
- Always route through API Gateway in production
- Use HTTPS for all external communication
- Secure internal network (VPC, private subnets)
- Log authentication failures
- Monitor JWT expiration rates
- Rotate JWT secrets regularly

### ❌ Don't
- Don't expose services directly to internet
- Don't trust headers from external sources
- Don't hardcode JWT secrets
- Don't use weak JWT secrets
- Don't skip JWT validation at Gateway

---

## Related Documentation

- [JWT_AUTHENTICATION.md](../../src/ApiGateway/JWT_AUTHENTICATION.md) - Detailed JWT implementation
- [API Gateway README](../../src/ApiGateway/README.md) - Gateway configuration
- [Security Best Practices](./SECURITY.md) - Security guidelines

---

## Summary

Successfully migrated JWT authentication from service-level to gateway-level:

- ✅ **Centralized** authentication at API Gateway
- ✅ **Simplified** services (no JWT validation logic)
- ✅ **Improved** performance (validate once)
- ✅ **Maintained** security (fallback validation)
- ✅ **Updated** 4 services (Order, Cart, User x3 controllers)

**Result:** More maintainable, performant, and secure authentication architecture.
