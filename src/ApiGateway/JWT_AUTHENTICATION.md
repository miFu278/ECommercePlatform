# JWT Authentication in API Gateway

## Overview

API Gateway validates JWT tokens and adds user information to headers for downstream services.

## Flow

```
1. Client → API Gateway (with JWT in Authorization header)
2. API Gateway validates JWT
3. API Gateway extracts user info (userId, email, role)
4. API Gateway adds custom headers:
   - X-User-Id
   - X-User-Email
   - X-User-Role
5. API Gateway forwards request to downstream service
6. Service reads headers (trusts Gateway)
```

## Configuration

### API Gateway (appsettings.json)
```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-characters-long-for-production",
    "Issuer": "ECommerce.UserService",
    "Audience": "ECommerce.Clients"
  }
}
```

### Ocelot Routes (ocelot.Development.json)
```json
{
  "UpstreamPathTemplate": "/orders/{everything}",
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"
  }
}
```

## Custom Headers

API Gateway adds these headers to downstream requests:

| Header | Description | Example |
|--------|-------------|---------|
| `X-User-Id` | User's unique identifier | `123e4567-e89b-12d3-a456-426614174000` |
| `X-User-Email` | User's email address | `user@example.com` |
| `X-User-Role` | User's role | `Admin` or `User` |

## Service Implementation

### Reading User Info from Headers

```csharp
private Guid GetCurrentUserId()
{
    // Trust API Gateway header
    if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
    {
        if (Guid.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
    }

    // Fallback to JWT (if called directly)
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userIdFromClaim))
    {
        return userIdFromClaim;
    }

    throw new UnauthorizedAccessException("User ID not found");
}

private bool IsAdmin()
{
    // Trust API Gateway header
    if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
    {
        return roleHeader.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    // Fallback to JWT
    return User.IsInRole("Admin");
}
```

## Security Considerations

### ✅ Advantages
- Single point of JWT validation (API Gateway)
- Services are simpler (no JWT validation logic)
- Consistent authentication across all services
- Easy to add/remove authentication

### ⚠️ Important
- **Internal network must be secure** - Services trust Gateway headers
- **Never expose services directly** - Always go through Gateway
- **Use same JWT secret** across all services (for fallback validation)

## Testing

### 1. Login to get JWT
```bash
POST http://localhost:5050/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}

# Response
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Call protected endpoint
```bash
GET http://localhost:5050/orders/my-orders
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Gateway adds headers to downstream
```
GET http://localhost:5003/api/orders/my-orders
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-User-Email: user@example.com
X-User-Role: User
```

## Routes Configuration

### Public Routes (No Authentication)
- `/auth/login` - Login
- `/auth/register` - Register
- `/products/*` - Browse products (read-only)

### Protected Routes (Requires JWT)
- `/users/*` - User profile
- `/cart/*` - Shopping cart
- `/orders/*` - Orders

### Admin Routes (Requires Admin role)
- `/orders` - Get all orders
- `/orders/status/{status}` - Get orders by status
- `/orders/{id}/status` - Update order status

## Troubleshooting

### 401 Unauthorized
- Check JWT token is valid
- Check token is not expired
- Check JWT secret matches across services

### 403 Forbidden
- Check user has required role
- Check X-User-Role header is set correctly

### Missing User Info
- Check API Gateway is adding headers
- Check service is reading headers correctly
- Check JWT claims are correct

## Migration from Service-Level JWT

### Before (Each service validates JWT)
```csharp
[Authorize]
public class OrdersController : ControllerBase
{
    // Each service has JWT validation
}
```

### After (Trust API Gateway)
```csharp
public class OrdersController : ControllerBase
{
    // No [Authorize] attribute
    // Read X-User-Id from headers
    private Guid GetCurrentUserId()
    {
        return Guid.Parse(Request.Headers["X-User-Id"]);
    }
}
```

## Benefits

1. **Centralized Authentication** - One place to manage JWT validation
2. **Simpler Services** - Services don't need JWT libraries
3. **Better Performance** - Validate once at Gateway, not in every service
4. **Easier Maintenance** - Change JWT config in one place
5. **Consistent Security** - All services use same authentication logic
