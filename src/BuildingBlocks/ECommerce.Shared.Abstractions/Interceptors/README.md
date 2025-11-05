# Audit Interceptor Setup Guide

## What is IAuditableEntity?

`IAuditableEntity` tracks **WHO** created/modified entities, not just WHEN.

### Use Cases:
- **Compliance**: "Who changed this price?"
- **Security**: "Who deleted this record?"
- **Business Logic**: "Only creator can edit"
- **Audit Trail**: Complete history of changes

## Setup Instructions

### 1. Implement ICurrentUserService

Create in your service's Infrastructure layer:

```csharp
// Infrastructure/Services/CurrentUserService.cs
using ECommerce.Shared.Abstractions.Interceptors;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
```

### 2. Register Services in DI

```csharp
// Program.cs or Startup.cs
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

### 3. Add Interceptor to DbContext

```csharp
// Program.cs
builder.Services.AddDbContext<YourDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(connectionString);
    
    // Add audit interceptor
    var currentUserService = serviceProvider.GetService<ICurrentUserService>();
    options.AddInterceptors(new AuditableEntityInterceptor(currentUserService));
});
```

### 4. Mark Entities as Auditable

```csharp
using ECommerce.Shared.Abstractions.Entities;

public class Product : BaseEntity, IAuditableEntity, ISoftDeletable
{
    // Inherited from BaseEntity:
    // - Guid Id
    // - DateTime CreatedAt
    // - DateTime UpdatedAt
    
    // From IAuditableEntity:
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    // From ISoftDeletable:
    public DateTime? DeletedAt { get; set; }
    
    // Your properties...
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

## How It Works

The interceptor automatically populates audit fields:

### On Create (EntityState.Added):
```csharp
var product = new Product { Name = "iPhone", Price = 999 };
await _unitOfWork.Products.AddAsync(product);
await _unitOfWork.SaveChangesAsync();

// Automatically set by interceptor:
// product.Id = Guid.NewGuid()
// product.CreatedAt = DateTime.UtcNow
// product.UpdatedAt = DateTime.UtcNow
// product.CreatedBy = currentUserId (from JWT token)
// product.UpdatedBy = currentUserId
```

### On Update (EntityState.Modified):
```csharp
product.Price = 899;
_unitOfWork.Products.Update(product);
await _unitOfWork.SaveChangesAsync();

// Automatically set by interceptor:
// product.UpdatedAt = DateTime.UtcNow
// product.UpdatedBy = currentUserId (from JWT token)
```

### On Delete (Soft Delete):
```csharp
_unitOfWork.Products.Remove(product);
await _unitOfWork.SaveChangesAsync();

// Automatically set by interceptor:
// product.DeletedAt = DateTime.UtcNow
// EntityState changed from Deleted to Modified (soft delete)
```

## Benefits

✅ **Automatic** - No manual code needed
✅ **Consistent** - Same behavior across all entities
✅ **Secure** - Uses authenticated user from JWT
✅ **Audit Trail** - Complete history of who did what
✅ **Compliance** - Meets regulatory requirements

## Query Examples

### Find who created a product:
```csharp
var product = await _unitOfWork.Products.GetByIdAsync(productId);
var creator = await _unitOfWork.Users.GetByIdAsync(product.CreatedBy);
Console.WriteLine($"Created by: {creator.FullName}");
```

### Find all products created by a user:
```csharp
var products = await _unitOfWork.Products.FindAsync(p => p.CreatedBy == userId);
```

### Authorization check:
```csharp
if (product.CreatedBy != currentUserId && !user.IsAdmin)
{
    throw new ForbiddenException("Only creator or admin can edit");
}
```

## Without Authentication (Background Jobs, Migrations)

If no user is authenticated (e.g., background jobs), `CreatedBy`/`UpdatedBy` will be `Guid.Empty` or you can set a system user ID:

```csharp
// Option 1: Check for system operations
public class CurrentUserService : ICurrentUserService
{
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    public Guid? GetCurrentUserId()
    {
        var userId = GetFromHttpContext();
        return userId ?? SystemUserId; // Use system user if no auth
    }
}

// Option 2: Allow null and handle in business logic
public Guid? GetCurrentUserId()
{
    return GetFromHttpContext(); // Returns null if no auth
}
```

## Testing

Mock `ICurrentUserService` in tests:

```csharp
var mockCurrentUser = new Mock<ICurrentUserService>();
mockCurrentUser.Setup(x => x.GetCurrentUserId())
    .Returns(Guid.Parse("test-user-id"));

var interceptor = new AuditableEntityInterceptor(mockCurrentUser.Object);
```
