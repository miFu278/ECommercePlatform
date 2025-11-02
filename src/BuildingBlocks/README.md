# BuildingBlocks - Shared Libraries

Shared libraries và utilities được sử dụng chung bởi tất cả microservices.

## Projects

### 1. ECommerce.Common
Shared models, exceptions, extensions, và constants.

**Contents:**
- `Models/` - ApiResponse, PagedResult, ErrorResponse
- `Exceptions/` - Custom exceptions (NotFoundException, ValidationException, etc.)
- `Extensions/` - String, DateTime extensions
- `Constants/` - Application constants

**Usage:**
```csharp
// API Response
var response = ApiResponse<User>.SuccessResponse(user, "User retrieved successfully");

// Pagination
var pagedResult = PagedResult<Product>.Create(products, totalCount, pageNumber, pageSize);

// Exceptions
throw new NotFoundException("User", userId);
throw new ValidationException("email", "Invalid email format");

// Extensions
string slug = productName.ToSlug(); // "wireless-headphones"
string masked = email.MaskEmail(); // "j***n@example.com"
string relative = createdAt.ToRelativeTime(); // "2 hours ago"
```

---

### 2. ECommerce.EventBus
Event-driven communication abstractions và integration events.

**Contents:**
- `Abstractions/` - IntegrationEvent base class, IEventBus interface
- `Events/` - Predefined integration events

**Events:**
- `UserRegisteredEvent` - When user registers
- `OrderCreatedEvent` - When order is created
- `PaymentProcessedEvent` - When payment is processed

**Usage:**
```csharp
// Publish event
var @event = new UserRegisteredEvent
{
    UserId = user.Id,
    Email = user.Email,
    FirstName = user.FirstName,
    LastName = user.LastName
};

await _eventBus.PublishAsync(@event);
```

---

### 3. ECommerce.Logging
Centralized logging extensions và utilities.

**Contents:**
- `LoggingExtensions.cs` - Logging helper methods

**Usage:**
```csharp
// Log user action
_logger.LogUserAction(userId, "Login", "Successful login");

// Log API request
_logger.LogApiRequest("POST", "/api/users/register", userId);

// Log API response
_logger.LogApiResponse("POST", "/api/users/register", 201, elapsedMs);
```

---

## Installation

Add reference to your service:

```bash
# Add Common library
dotnet add reference ../../BuildingBlocks/ECommerce.Common/ECommerce.Common.csproj

# Add EventBus library
dotnet add reference ../../BuildingBlocks/ECommerce.EventBus/ECommerce.EventBus.csproj

# Add Logging library
dotnet add reference ../../BuildingBlocks/ECommerce.Logging/ECommerce.Logging.csproj
```

---

## Constants Reference

### Roles
```csharp
AppConstants.Roles.Admin
AppConstants.Roles.Manager
AppConstants.Roles.Customer
AppConstants.Roles.Guest
```

### Cache Keys
```csharp
AppConstants.CacheKeys.UserPrefix // "user:"
AppConstants.CacheKeys.ProductPrefix // "product:"
AppConstants.CacheKeys.CartPrefix // "cart:"
```

### Security
```csharp
AppConstants.Security.MaxFailedLoginAttempts // 5
AppConstants.Security.LockoutMinutes // 15
AppConstants.Security.AccessTokenExpirationMinutes // 60
AppConstants.Security.RefreshTokenExpirationDays // 7
```

### Pagination
```csharp
AppConstants.Pagination.DefaultPageNumber // 1
AppConstants.Pagination.DefaultPageSize // 20
AppConstants.Pagination.MaxPageSize // 100
```

---

## Exception Handling

All custom exceptions inherit from `BaseException`:

```csharp
public abstract class BaseException : Exception
{
    public string Code { get; set; }
    public int StatusCode { get; set; }
}
```

**Available Exceptions:**
- `NotFoundException` (404)
- `ValidationException` (400)
- `BusinessException` (400)
- `UnauthorizedException` (401)
- `ForbiddenException` (403)

**Example:**
```csharp
try
{
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
        throw new NotFoundException("User", userId);
}
catch (BaseException ex)
{
    return StatusCode(ex.StatusCode, new ErrorDetail
    {
        Message = ex.Message,
        Errors = new List<ErrorResponse>
        {
            new() { Code = ex.Code, Message = ex.Message }
        }
    });
}
```

---

## String Extensions

```csharp
// Slug generation
"Wireless Headphones".ToSlug() // "wireless-headphones"

// Truncate
"Long text here...".Truncate(10) // "Long text..."

// Email validation
"user@example.com".IsValidEmail() // true

// Masking
"john@example.com".MaskEmail() // "j***n@example.com"
"+1234567890".MaskPhoneNumber() // "***7890"
```

---

## DateTime Extensions

```csharp
// Relative time
DateTime.UtcNow.AddHours(-2).ToRelativeTime() // "2 hours ago"

// Date checks
DateTime.UtcNow.IsToday() // true
DateTime.UtcNow.AddDays(-1).IsYesterday() // true

// Date ranges
var startOfDay = DateTime.UtcNow.StartOfDay();
var endOfMonth = DateTime.UtcNow.EndOfMonth();
```

---

## Best Practices

1. **Use ApiResponse for all API endpoints**
   ```csharp
   return Ok(ApiResponse<User>.SuccessResponse(user));
   ```

2. **Use PagedResult for list endpoints**
   ```csharp
   var result = PagedResult<Product>.Create(products, count, page, size);
   return Ok(ApiResponse<PagedResult<Product>>.SuccessResponse(result));
   ```

3. **Throw custom exceptions instead of returning error codes**
   ```csharp
   if (user == null)
       throw new NotFoundException("User", userId);
   ```

4. **Use constants instead of magic strings**
   ```csharp
   [Authorize(Roles = AppConstants.Roles.Admin)]
   ```

5. **Publish events for cross-service communication**
   ```csharp
   await _eventBus.PublishAsync(new OrderCreatedEvent { ... });
   ```

---

**Status**: ✅ Complete  
**Build Status**: ✅ All projects build successfully  
**Dependencies**: None (Pure .NET 9.0 + Microsoft.Extensions.Logging.Abstractions)
