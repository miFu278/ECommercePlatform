# Entity Abstractions

## Overview

Shared base classes and interfaces for all domain entities across microservices.

## BaseEntity

Base class for all entities with common properties:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Usage:**
```csharp
public class Category : BaseEntity
{
    public string Name { get; set; }
    // Id, CreatedAt, UpdatedAt inherited
}
```

## IAuditableEntity

Interface for entities that need to track **WHO** created/modified them:

```csharp
public interface IAuditableEntity
{
    Guid CreatedBy { get; set; }
    Guid? UpdatedBy { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
```

**Usage:**
```csharp
public class Product : BaseEntity, IAuditableEntity
{
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**Use Cases:**
- Compliance & audit trail
- Security & accountability
- Business logic (only creator can edit)
- Regulatory requirements

**Setup:** See [Interceptors/README.md](../Interceptors/README.md) for automatic population.

## ISoftDeletable

Interface for entities that support soft delete (mark as deleted instead of physical delete):

```csharp
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    bool IsDeleted => DeletedAt.HasValue;
}
```

**Usage:**
```csharp
public class User : BaseEntity, ISoftDeletable
{
    public DateTime? DeletedAt { get; set; }
    
    public string Email { get; set; }
}
```

**Benefits:**
- Data recovery possible
- Maintain referential integrity
- Audit trail preserved
- Compliance requirements

**Query Example:**
```csharp
// Exclude soft-deleted records
var activeUsers = await _dbSet
    .Where(u => u.DeletedAt == null)
    .ToListAsync();
```

## Combining Interfaces

You can combine multiple interfaces:

```csharp
public class Product : BaseEntity, IAuditableEntity, ISoftDeletable
{
    // From BaseEntity:
    // - Guid Id
    // - DateTime CreatedAt
    // - DateTime UpdatedAt
    
    // From IAuditableEntity:
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    // From ISoftDeletable:
    public DateTime? DeletedAt { get; set; }
    
    // Your properties:
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

## Entity Patterns

### Simple Entity (timestamps only)
```csharp
public class Category : BaseEntity
{
    public string Name { get; set; }
}
```

### Auditable Entity (track who)
```csharp
public class Product : BaseEntity, IAuditableEntity
{
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string Name { get; set; }
}
```

### Soft Deletable Entity
```csharp
public class User : BaseEntity, ISoftDeletable
{
    public DateTime? DeletedAt { get; set; }
    public string Email { get; set; }
}
```

### Full Audit Entity (track who + soft delete)
```csharp
public class Order : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public decimal Total { get; set; }
}
```

## EF Core Configuration

Configure in your entity configurations:

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // BaseEntity properties
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // IAuditableEntity properties
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(p => p.UpdatedBy).HasColumnName("updated_by");
        
        // ISoftDeletable properties
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(p => p.DeletedAt); // For filtering
        
        // Your properties...
    }
}
```

## Global Query Filters (Soft Delete)

Automatically exclude soft-deleted records:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply to all ISoftDeletable entities
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
            var condition = Expression.Equal(property, Expression.Constant(null));
            var lambda = Expression.Lambda(condition, parameter);
            
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
```

Now all queries automatically exclude soft-deleted records:
```csharp
// Automatically filters DeletedAt == null
var products = await _dbSet.ToListAsync();

// Include soft-deleted if needed
var allProducts = await _dbSet.IgnoreQueryFilters().ToListAsync();
```
