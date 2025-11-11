# ECommerce.Shared.Abstractions

Shared interfaces and abstractions used across all microservices. **Contains only interfaces and abstract classes - NO implementations or external dependencies.**

## Philosophy: Trường phái 1 - Pure Abstractions

Each microservice implements these interfaces independently, maintaining true service autonomy.

## Contents

### Entities
- `BaseEntity` - Base class for all entities with Id, CreatedAt, UpdatedAt
- `IAuditableEntity` - Interface for entities that track who created/updated them
- `ISoftDeletable` - Interface for entities that support soft delete

### Repositories
- `IRepository<T>` - Generic repository interface for CRUD operations
- `IUnitOfWork` - Base Unit of Work interface for transaction management

## Usage Pattern

### 1. In Service's Domain Layer
```csharp
using ECommerce.Shared.Abstractions.Repositories;

// Extend IUnitOfWork for your service
public interface IProductUnitOfWork : IUnitOfWork
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
}

// Define specific repository
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
}
```

### 2. In Service's Infrastructure Layer
```csharp
// Each service implements Repository<T> independently
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    // Full implementation here...
}

// Specific repository
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ProductDbContext context) : base(context) { }
    
    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);
    }
}

// Unit of Work implementation
public class ProductUnitOfWork : IProductUnitOfWork
{
    private readonly ProductDbContext _context;
    
    public ProductUnitOfWork(ProductDbContext context)
    {
        _context = context;
        Products = new ProductRepository(context);
        Categories = new CategoryRepository(context);
    }
    
    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
    
    // Transaction methods...
}
```

### 3. In Service's Application Layer
```csharp
public class ProductService
{
    private readonly IProductUnitOfWork _unitOfWork;
    
    public async Task CreateProductAsync(Product product)
    {
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

## Benefits

✅ **True Microservice Autonomy** - Each service owns its implementation
✅ **No Coupling** - No shared infrastructure dependencies
✅ **Flexibility** - Services can use different ORMs (EF Core, Dapper, etc.)
✅ **Consistency** - Same interface contracts everywhere
✅ **Testability** - Easy to mock interfaces

## What NOT to put here

❌ Concrete implementations (Repository<T>, UnitOfWork)
❌ External dependencies (EF Core, Dapper, etc.)
❌ Business logic
❌ Infrastructure concerns
