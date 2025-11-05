# Product Service - MongoDB Migration Complete

## Summary

Successfully migrated Product Service from EF Core + PostgreSQL to MongoDB.

## Changes Made

### 1. Packages ✅
- ❌ Removed: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`
- ✅ Added: `MongoDB.Driver`, `MongoDB.Bson`

### 2. Entities ✅
All entities updated with MongoDB attributes:
- `[BsonId]` - Mark Id field
- `[BsonRepresentation(BsonType.String)]` - Store Guid as string
- `[BsonElement("field_name")]` - Custom field names

**Updated Entities:**
- Product
- Category
- Tag
- ProductVariant
- ProductImage
- ProductAttribute
- ProductTag

### 3. Data Layer ✅
- ❌ Deleted: `ProductDbContext` (EF Core)
- ❌ Deleted: `Configurations/` folder (EF Core entity configs)
- ❌ Deleted: `Migrations/` folder
- ✅ Created: `MongoDbContext` - MongoDB context

### 4. Repositories ✅
- ❌ Deleted: `Repository<T>` (EF Core base)
- ✅ Created: `MongoRepository<T>` (MongoDB base)
- ✅ Updated: `ProductRepository` - MongoDB queries
- ✅ Updated: `CategoryRepository` - MongoDB queries
- ✅ Updated: `TagRepository` - MongoDB queries

### 5. Unit of Work ✅
- ✅ Updated: `UnitOfWork` - MongoDB transactions
- Note: MongoDB commits changes immediately, SaveChanges() kept for interface compatibility

### 6. Configuration ✅
**Program.cs:**
- MongoDB client registration
- MongoDbContext registration
- Connection string from appsettings

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://admin:admin123@localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "ECommerce_Product"
  }
}
```

## MongoDB Connection

**Docker Compose:**
- Host: `localhost` (dev) or `mongodb` (docker)
- Port: `27017`
- Username: `admin`
- Password: `admin123`
- Database: `ECommerce_Product`

**Connection String:**
```
mongodb://admin:admin123@localhost:27017
```

## MongoDB Benefits for Product Catalog

✅ **Flexible Schema** - Products can have different attributes
✅ **Nested Documents** - Variants, images, attributes stored together
✅ **High Read Performance** - Optimized for product catalog queries
✅ **Horizontal Scaling** - Easy to scale out
✅ **No Migrations** - Schema-less, no migration files needed

## Collections

```
ECommerce_Product (Database)
├── products (Collection)
│   ├── _id: Guid
│   ├── name, sku, slug
│   ├── variants: [] (embedded)
│   ├── images: [] (embedded)
│   ├── attributes: [] (embedded)
│   └── tags: [] (embedded)
├── categories (Collection)
│   ├── _id: Guid
│   ├── name, slug
│   └── children: [] (embedded)
└── tags (Collection)
    ├── _id: Guid
    └── name
```

## Query Examples

### Find Product by ID:
```csharp
var product = await _unitOfWork.Products.GetByIdAsync(productId);
// Returns product with all nested documents (variants, images, etc.)
```

### Search Products:
```csharp
var products = await _unitOfWork.Products.SearchAsync("iPhone");
// Uses MongoDB regex for text search
```

### Find by Category:
```csharp
var products = await _unitOfWork.Products.GetByCategoryIdAsync(categoryId);
```

### Find by Tag:
```csharp
var products = await _unitOfWork.Products.GetByTagIdAsync(tagId);
// Uses $elemMatch for array queries
```

## Running the Service

### 1. Start MongoDB (Docker):
```bash
cd docker
docker-compose -f docker-compose.infrastructure.yml up mongodb -d
```

### 2. Run Product Service:
```bash
cd src/Services/Product/ECommerce.Product.API
dotnet run
```

### 3. Verify MongoDB Connection:
```bash
# Connect to MongoDB
mongosh mongodb://admin:admin123@localhost:27017

# Switch to database
use ECommerce_Product

# List collections
show collections

# Query products
db.products.find().pretty()
```

## Seeding Data (Optional)

Create a seed script to populate initial data:

```csharp
// In Program.cs or separate seeder
public static async Task SeedDataAsync(MongoDbContext context)
{
    // Check if data exists
    var productCount = await context.Products.CountDocumentsAsync(_ => true);
    if (productCount > 0) return;

    // Seed categories
    var electronics = new Category
    {
        Id = Guid.NewGuid(),
        Name = "Electronics",
        Slug = "electronics",
        IsVisible = true
    };
    await context.Categories.InsertOneAsync(electronics);

    // Seed products
    var product = new Product
    {
        Id = Guid.NewGuid(),
        Name = "iPhone 15 Pro",
        Slug = "iphone-15-pro",
        Sku = "IPH15PRO",
        Price = 999,
        CategoryId = electronics.Id,
        Status = ProductStatus.Active,
        IsVisible = true
    };
    await context.Products.InsertOneAsync(product);
}
```

## Next Steps

1. ✅ Build successful
2. ⏳ Test CRUD operations
3. ⏳ Create seed data
4. ⏳ Test search functionality
5. ⏳ Performance testing

## Notes

- MongoDB stores nested documents, so no need for Include() like EF Core
- Transactions are optional in MongoDB (single document operations are atomic)
- Use indexes for better query performance
- Consider using MongoDB Atlas for production
