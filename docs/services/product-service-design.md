# Product Service - Design & Implementation Guide

## 🎯 Overview

Product Service quản lý tất cả thông tin về sản phẩm, categories, variants, inventory và images.

## 📊 Database Design

### Core Entities

#### 1. Product (Sản phẩm chính)
```csharp
public class Product
{
    // Identity
    public Guid Id { get; set; }
    public string Sku { get; set; }  // Stock Keeping Unit - unique
    public string Name { get; set; }
    public string Slug { get; set; }  // URL-friendly name
    
    // Description
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    
    // Pricing
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }  // Original price (for discount display)
    public decimal Cost { get; set; }  // Cost price (for profit calculation)
    
    // Category
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    
    // Brand (optional)
    public string? Brand { get; set; }
    
    // Status
    public ProductStatus Status { get; set; }  // Draft, Active, Archived
    public bool IsVisible { get; set; }  // Show on storefront
    public bool IsFeatured { get; set; }  // Featured products
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // Inventory
    public bool TrackInventory { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public StockStatus StockStatus { get; set; }  // InStock, LowStock, OutOfStock
    
    // Shipping
    public decimal Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    // Navigation Properties
    public ICollection<ProductImage> Images { get; set; }
    public ICollection<ProductVariant> Variants { get; set; }
    public ICollection<ProductTag> ProductTags { get; set; }
    public ICollection<ProductAttribute> Attributes { get; set; }
}
```

**Key Points:**
- `Sku`: Unique identifier cho inventory
- `Slug`: SEO-friendly URL (e.g., "nike-air-max-2024")
- `CompareAtPrice`: Hiển thị giá gốc khi có discount
- `TrackInventory`: Có track stock không (digital products = false)
- Soft delete với `DeletedAt`

---

#### 2. Category (Danh mục)
```csharp
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    
    // Hierarchy
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; }
    
    // Display
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Product> Products { get; set; }
}
```

**Key Points:**
- Hierarchical structure (parent/child)
- Example: Electronics → Phones → Smartphones
- `DisplayOrder`: Thứ tự hiển thị

---

#### 3. ProductVariant (Biến thể sản phẩm)
```csharp
public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    
    // Variant Info
    public string Sku { get; set; }  // Unique SKU for variant
    public string Name { get; set; }  // e.g., "Red / Large"
    
    // Options (stored as JSON or separate table)
    public string Option1Name { get; set; }  // e.g., "Color"
    public string Option1Value { get; set; }  // e.g., "Red"
    public string? Option2Name { get; set; }  // e.g., "Size"
    public string? Option2Value { get; set; }  // e.g., "Large"
    public string? Option3Name { get; set; }
    public string? Option3Value { get; set; }
    
    // Pricing (override product price)
    public decimal? Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? Cost { get; set; }
    
    // Inventory
    public int StockQuantity { get; set; }
    public StockStatus StockStatus { get; set; }
    
    // Image
    public string? ImageUrl { get; set; }
    
    // Status
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Key Points:**
- Mỗi variant có SKU riêng
- Support up to 3 options (Color, Size, Material, etc.)
- Price có thể override product price
- Stock tracking per variant

**Example:**
```
Product: Nike Air Max
Variants:
  - Red/Small (SKU: NAM-RED-S)
  - Red/Large (SKU: NAM-RED-L)
  - Blue/Small (SKU: NAM-BLU-S)
  - Blue/Large (SKU: NAM-BLU-L)
```

---

#### 4. ProductImage (Hình ảnh)
```csharp
public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    
    // Image Info
    public string Url { get; set; }
    public string? AltText { get; set; }
    public string? Title { get; set; }
    
    // Storage
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string MimeType { get; set; }
    
    // Display
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }  // Main product image
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
}
```

**Key Points:**
- Multiple images per product
- `IsPrimary`: Main image hiển thị đầu tiên
- `DisplayOrder`: Thứ tự hiển thị
- Store metadata cho SEO (alt text)

---

#### 5. ProductTag (Tags)
```csharp
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    
    public ICollection<ProductTag> ProductTags { get; set; }
}

public class ProductTag
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    
    public Guid TagId { get; set; }
    public Tag Tag { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

**Key Points:**
- Many-to-many relationship
- Tags for filtering (e.g., "sale", "new", "trending")

---

#### 6. ProductAttribute (Thuộc tính)
```csharp
public class ProductAttribute
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    
    public string Name { get; set; }  // e.g., "Material", "Warranty"
    public string Value { get; set; }  // e.g., "Cotton", "2 years"
    
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Key Points:**
- Flexible attributes (không cố định)
- Example: Material, Warranty, Origin, etc.

---

### Enums

```csharp
public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    Archived = 2
}

public enum StockStatus
{
    InStock = 0,
    LowStock = 1,
    OutOfStock = 2,
    Discontinued = 3
}
```

---

## 🏗️ Project Structure

```
src/Services/Products/
├── ECommerce.Product.API/
│   ├── Controllers/
│   │   ├── ProductController.cs
│   │   ├── CategoryController.cs
│   │   └── ProductImageController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── ECommerce.Product.API.csproj
│
├── ECommerce.Product.Application/
│   ├── DTOs/
│   │   ├── ProductDto.cs
│   │   ├── CreateProductDto.cs
│   │   ├── UpdateProductDto.cs
│   │   ├── ProductListDto.cs
│   │   ├── CategoryDto.cs
│   │   ├── CreateCategoryDto.cs
│   │   └── ProductVariantDto.cs
│   ├── Interfaces/
│   │   ├── IProductService.cs
│   │   ├── ICategoryService.cs
│   │   └── IImageService.cs
│   ├── Services/
│   │   ├── ProductService.cs
│   │   ├── CategoryService.cs
│   │   └── ImageService.cs
│   ├── Validators/
│   │   ├── CreateProductDtoValidator.cs
│   │   ├── UpdateProductDtoValidator.cs
│   │   └── CreateCategoryDtoValidator.cs
│   ├── Mappings/
│   │   └── ProductMappingProfile.cs
│   └── ECommerce.Product.Application.csproj
│
├── ECommerce.Product.Domain/
│   ├── Entities/
│   │   ├── Product.cs
│   │   ├── Category.cs
│   │   ├── ProductVariant.cs
│   │   ├── ProductImage.cs
│   │   ├── ProductTag.cs
│   │   ├── Tag.cs
│   │   └── ProductAttribute.cs
│   ├── Enums/
│   │   ├── ProductStatus.cs
│   │   └── StockStatus.cs
│   ├── Interfaces/
│   │   ├── IProductRepository.cs
│   │   └── ICategoryRepository.cs
│   └── ECommerce.Product.Domain.csproj
│
└── ECommerce.Product.Infrastructure/
    ├── Data/
    │   ├── ProductDbContext.cs
    │   └── Configurations/
    │       ├── ProductConfiguration.cs
    │       ├── CategoryConfiguration.cs
    │       └── ProductVariantConfiguration.cs
    ├── Repositories/
    │   ├── ProductRepository.cs
    │   └── CategoryRepository.cs
    ├── Services/
    │   └── LocalImageService.cs
    ├── Migrations/
    └── ECommerce.Product.Infrastructure.csproj
```

---

## 📝 Implementation Steps

### Phase 1: Setup Project Structure (30 mins)

1. **Create Projects:**
```bash
cd src/Services
mkdir Products
cd Products

dotnet new webapi -n ECommerce.Product.API
dotnet new classlib -n ECommerce.Product.Application
dotnet new classlib -n ECommerce.Product.Domain
dotnet new classlib -n ECommerce.Product.Infrastructure

# Add references
cd ECommerce.Product.API
dotnet add reference ../ECommerce.Product.Application
dotnet add reference ../ECommerce.Product.Infrastructure

cd ../ECommerce.Product.Application
dotnet add reference ../ECommerce.Product.Domain

cd ../ECommerce.Product.Infrastructure
dotnet add reference ../ECommerce.Product.Domain
dotnet add reference ../ECommerce.Product.Application
```

2. **Add NuGet Packages:**

**Domain:** (no packages needed)

**Application:**
```bash
cd ECommerce.Product.Application
dotnet add package AutoMapper
dotnet add package FluentValidation
```

**Infrastructure:**
```bash
cd ECommerce.Product.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

**API:**
```bash
cd ECommerce.Product.API
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet add package FluentValidation.AspNetCore
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

---

### Phase 2: Domain Layer (1 hour)

**Tasks:**
1. ✅ Create all entities in `Domain/Entities/`
2. ✅ Create enums in `Domain/Enums/`
3. ✅ Create repository interfaces in `Domain/Interfaces/`

**Tips:**
- Start with `Product.cs` and `Category.cs`
- Add navigation properties
- Add computed properties (e.g., `IsInStock`, `DiscountPercentage`)
- Use nullable types appropriately

---

### Phase 3: Infrastructure Layer (1.5 hours)

**Tasks:**
1. ✅ Create `ProductDbContext`
2. ✅ Create Entity Configurations (Fluent API)
3. ✅ Create Repositories
4. ✅ Create Migration
5. ✅ Seed initial data (categories)

**DbContext Example:**
```csharp
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
    }
}
```

**Configuration Example:**
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(p => p.Sku).IsUnique();
        
        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
            
        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
            
        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Query filter for soft delete
        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
```

---

### Phase 4: Application Layer (2 hours)

**Tasks:**
1. ✅ Create DTOs
2. ✅ Create Service Interfaces
3. ✅ Implement Services
4. ✅ Create Validators
5. ✅ Create AutoMapper Profiles

**Key DTOs:**

```csharp
// For listing products
public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string CategoryName { get; set; }
    public StockStatus StockStatus { get; set; }
    public bool IsFeatured { get; set; }
}

// For product details
public class ProductDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public CategoryDto Category { get; set; }
    public string? Brand { get; set; }
    public int StockQuantity { get; set; }
    public StockStatus StockStatus { get; set; }
    public List<ProductImageDto> Images { get; set; }
    public List<ProductVariantDto> Variants { get; set; }
    public List<ProductAttributeDto> Attributes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// For creating product
public class CreateProductDto
{
    public string Sku { get; set; }
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public Guid CategoryId { get; set; }
    public string? Brand { get; set; }
    public bool TrackInventory { get; set; }
    public int StockQuantity { get; set; }
    public decimal Weight { get; set; }
}
```

---

### Phase 5: API Layer (1.5 hours)

**Tasks:**
1. ✅ Create Controllers
2. ✅ Configure DI in Program.cs
3. ✅ Setup Swagger
4. ✅ Add XML documentation

**Key Endpoints:**

**ProductController:**
- `GET /api/products` - Get all products (paginated, filtered)
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/slug/{slug}` - Get product by slug
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product (soft delete)
- `GET /api/products/featured` - Get featured products
- `GET /api/products/search` - Search products

**CategoryController:**
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `GET /api/categories/{id}/products` - Get products by category
- `POST /api/categories` - Create category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

---

## 🎨 Advanced Features (Optional)

### 1. Search & Filtering
```csharp
public class ProductSearchDto
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Brand { get; set; }
    public List<string>? Tags { get; set; }
    public bool? InStock { get; set; }
    public string? SortBy { get; set; }  // price, name, date
    public string? SortOrder { get; set; }  // asc, desc
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

### 2. Slug Generation
```csharp
public static string GenerateSlug(string name)
{
    return name.ToLowerInvariant()
        .Replace(" ", "-")
        .Replace("&", "and")
        // Remove special characters
        .Where(c => char.IsLetterOrDigit(c) || c == '-')
        .Aggregate("", (current, c) => current + c);
}
```

### 3. Stock Management
```csharp
public void UpdateStockStatus()
{
    if (StockQuantity <= 0)
        StockStatus = StockStatus.OutOfStock;
    else if (StockQuantity <= LowStockThreshold)
        StockStatus = StockStatus.LowStock;
    else
        StockStatus = StockStatus.InStock;
}
```

### 4. Image Upload
```csharp
public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task<bool> DeleteImageAsync(string url);
}
```

---

## 🧪 Testing Strategy

### Unit Tests
- Product entity business logic
- Service methods
- Validators

### Integration Tests
- Repository methods
- API endpoints
- Database operations

### Test Data
```csharp
// Seed categories
Electronics
  - Phones
  - Laptops
  - Accessories
Clothing
  - Men
  - Women
  - Kids

// Seed products
iPhone 15 Pro
  - Variants: 128GB, 256GB, 512GB
  - Colors: Black, White, Blue
Nike Air Max
  - Sizes: 38, 39, 40, 41, 42
  - Colors: Red, Blue, Black
```

---

## 📚 Resources

### Similar to User Service
- Same project structure
- Same patterns (Repository, Service, DTO)
- Same validation approach
- Same Swagger setup

### Key Differences
- More complex entities (variants, images)
- File upload handling
- More complex queries (search, filter)
- Hierarchical data (categories)

---

## ✅ Checklist

### Domain Layer
- [ ] Product entity
- [ ] Category entity
- [ ] ProductVariant entity
- [ ] ProductImage entity
- [ ] ProductTag & Tag entities
- [ ] ProductAttribute entity
- [ ] Enums (ProductStatus, StockStatus)
- [ ] Repository interfaces

### Infrastructure Layer
- [ ] ProductDbContext
- [ ] Entity Configurations
- [ ] Repositories implementation
- [ ] Migration
- [ ] Seed data

### Application Layer
- [ ] DTOs (List, Detail, Create, Update)
- [ ] Service interfaces
- [ ] Service implementations
- [ ] Validators
- [ ] AutoMapper profiles

### API Layer
- [ ] ProductController
- [ ] CategoryController
- [ ] Program.cs configuration
- [ ] Swagger setup
- [ ] XML documentation

### Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing with Swagger

---

## 🚀 Next Steps After Product Service

1. **Shopping Cart Service** - Use Product data
2. **Order Service** - Create orders with products
3. **Payment Service** - Process payments
4. **API Gateway** - Centralize all services

---

**Good luck! Bạn có thể tham khảo User Service để implement tương tự.** 💪

Nếu gặp vấn đề gì, cứ hỏi nhé! 😊
