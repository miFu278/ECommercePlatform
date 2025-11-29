using ECommerce.Product.Domain.Attributes;
using ECommerce.Product.Domain.Enums;
using ECommerce.Product.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.Entities;

[BsonCollection("products")]
public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("long_description")]
    public string LongDescription { get; set; } = string.Empty;

    // Pricing
    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    [BsonElement("compare_at_price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? CompareAtPrice { get; set; }

    [BsonElement("cost_price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal CostPrice { get; set; }

    // Inventory (flattened)
    [BsonElement("stock")]
    public int Stock { get; set; }

    [BsonElement("low_stock_threshold")]
    public int LowStockThreshold { get; set; }

    [BsonElement("track_inventory")]
    public bool TrackInventory { get; set; }

    // Category & Brand
    [BsonElement("category_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = string.Empty;

    [BsonElement("category_path")]
    public List<string> CategoryPath { get; set; } = new();

    [BsonElement("brand_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? BrandId { get; set; }

    [BsonElement("brand_name")]
    public string? BrandName { get; set; }

    // Images
    [BsonElement("images")]
    public List<ProductImage> Images { get; set; } = new();

    // Attributes (for variants)
    [BsonElement("attributes")]
    public List<ProductAttribute> Attributes { get; set; } = new();

    // Specifications (PC components specs)
    [BsonElement("specifications")]
    public ProductSpecifications? Specifications { get; set; }

    // SEO
    [BsonElement("seo")]
    public ProductSeo? Seo { get; set; }

    // Status & Marketing
    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    [BsonElement("is_featured")]
    public bool IsFeatured { get; set; }

    [BsonElement("is_published")]
    public bool IsPublished { get; set; }

    [BsonElement("published_at")]
    public DateTime? PublishedAt { get; set; }

    // Tags (string array instead of ObjectId references)
    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    // Rating (denormalized from reviews)
    [BsonElement("rating")]
    public ProductRating? Rating { get; set; }

    // Timestamps
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
