# Migration Guide: Using Shared Abstractions

## Overview

This guide shows how to migrate existing services to use shared abstractions from `ECommerce.Shared.Abstractions`.

## Step 1: Add Reference

```bash
dotnet add <YourService>.Domain.csproj reference ../../../BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj
dotnet add <YourService>.Infrastructure.csproj reference ../../../BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj
```

## Step 2: Refactor Entities

### Before:
```csharp
public class Product
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
```

### After:
```csharp
using ECommerce.Shared.Abstractions.Entities;

public class Product : BaseEntity, IAuditableEntity, ISoftDeletable
{
    // Id, CreatedAt, UpdatedAt inherited from BaseEntity
    // DeletedAt from ISoftDeletable
    // CreatedBy, UpdatedBy from IAuditableEntity
    
    // Your specific properties...
}
```

### Entity Patterns:

**Simple Entity (only timestamps):**
```csharp
public class Category : BaseEntity
{
    // Inherits: Id, CreatedAt, UpdatedAt
}
```

**Soft Deletable:**
```csharp
public class User : BaseEntity, ISoftDeletable
{
    // Inherits: Id, CreatedAt, UpdatedAt
    public DateTime? DeletedAt { get; set; }
}
```

**Auditable + Soft Deletable:**
```csharp
public class Product : BaseEntity, IAuditableEntity, ISoftDeletable
{
    // Inherits: Id, CreatedAt, UpdatedAt
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

## Step 3: Refactor Repository Interfaces

### Before:
```csharp
// Domain/Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    // ... all methods
}

// Domain/Interfaces/IProductRepository.cs
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
}
```

### After:
```csharp
// DELETE Domain/Interfaces/IRepository.cs - use shared one!

// Domain/Interfaces/IProductRepository.cs
using ECommerce.Shared.Abstractions.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
}
```

## Step 4: Refactor UnitOfWork Interface

### Before:
```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

### After:
```csharp
using ECommerce.Shared.Abstractions.Repositories;

public interface IUnitOfWork : ECommerce.Shared.Abstractions.Repositories.IUnitOfWork
{
    // Only declare service-specific repositories
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
}
```

## Step 5: Implement Repository Base Class

Create in Infrastructure layer:

```csharp
// Infrastructure/Repositories/Repository.cs
using ECommerce.Shared.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly YourDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(YourDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync([id], ct);
    }

    // Implement all IRepository<T> methods...
}
```

## Step 6: Implement Specific Repositories

```csharp
// Infrastructure/Repositories/ProductRepository.cs
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);
    }
}
```

## Step 7: Implement UnitOfWork

```csharp
// Infrastructure/Repositories/UnitOfWork.cs
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProductDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ProductDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Categories = new CategoryRepository(_context);
    }

    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

## Step 8: Update Application Layer

### Before (using repository directly):
```csharp
public class ProductService
{
    private readonly IProductRepository _productRepo;
    
    public async Task CreateAsync(Product product)
    {
        await _productRepo.AddAsync(product);
        await _productRepo.SaveChangesAsync(); // ❌ Wrong!
    }
}
```

### After (using UnitOfWork):
```csharp
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task CreateAsync(Product product)
    {
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(); // ✅ Correct!
    }
}
```

## Step 9: Update DI Registration

### Before:
```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
```

### After:
```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## Benefits

✅ **No Code Duplication** - Shared interfaces across all services
✅ **Consistency** - Same patterns everywhere
✅ **Proper Transaction Management** - UnitOfWork handles SaveChanges
✅ **Service Autonomy** - Each service implements its own Repository<T>
✅ **Testability** - Easy to mock interfaces
✅ **Maintainability** - Change interface once, all services benefit

## Checklist

- [ ] Add reference to ECommerce.Shared.Abstractions
- [ ] Refactor entities to use BaseEntity, IAuditableEntity, ISoftDeletable
- [ ] Delete local IRepository<T> interface
- [ ] Update specific repository interfaces to extend shared IRepository<T>
- [ ] Update IUnitOfWork to extend shared IUnitOfWork
- [ ] Create Repository<T> base implementation
- [ ] Update specific repositories to extend Repository<T>
- [ ] Create UnitOfWork implementation
- [ ] Update Application services to use IUnitOfWork
- [ ] Update DI registration
- [ ] Remove SaveChanges() calls from repository methods
- [ ] Test everything!
