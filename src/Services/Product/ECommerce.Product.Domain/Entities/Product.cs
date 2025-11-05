using ECommerce.Product.Domain.Attributes;
using ECommerce.Product.Domain.Enums;
using ECommerce.Product.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.Entities
{
    [BsonCollection("products")]
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        // Identity
        [BsonElement("sku")]
        public string Sku { get; set; } = string.Empty;
        
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        
        [BsonElement("slug")]
        public string Slug { get; set; } = string.Empty;

        // Description
        [BsonElement("shortDescription")]
        public string ShortDescription { get; set; } = string.Empty;
        
        [BsonElement("longDescription")]
        public string LongDescription { get; set; } = string.Empty;

        // Pricing
        [BsonElement("price")]
        public decimal Price { get; set; }
        
        [BsonElement("compareAtPrice")]
        public decimal? CompareAtPrice { get; set; }
        
        [BsonElement("costPrice")]
        public decimal CostPrice { get; set; }

        // Category
        [BsonElement("categoryId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; } = string.Empty;
        
        [BsonElement("categoryPath")]
        public List<string> CategoryPath { get; set; } = new();

        // Images
        [BsonElement("images")]
        public List<ProductImage> Images { get; set; } = new();

        // Tags
        [BsonElement("tagIds")]
        public List<string> TagIds { get; set; } = new();

        // Attributes (for variants like color, size)
        [BsonElement("attributes")]
        public List<ProductAttribute> Attributes { get; set; } = new();

        // Specifications
        [BsonElement("specifications")]
        public ProductSpecifications? Specifications { get; set; }

        // Status 
        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public ProductStatus Status { get; set; }
        
        [BsonElement("isActive")]
        public bool IsActive { get; set; }
        
        [BsonElement("isFeatured")]
        public bool IsFeatured { get; set; }
        
        [BsonElement("isPublished")]
        public bool IsPublished { get; set; }
        
        [BsonElement("publishedAt")]
        public DateTime? PublishedAt { get; set; }

        // SEO
        [BsonElement("seo")]
        public ProductSeo? Seo { get; set; }

        // Inventory
        [BsonElement("inventory")]
        public ProductInventory Inventory { get; set; } = new();

        // Dimensions
        [BsonElement("weight")]
        public decimal Weight { get; set; }
        
        [BsonElement("dimensions")]
        public ProductDimensions? Dimensions { get; set; }

        // Rating (calculated from reviews)
        [BsonElement("rating")]
        public ProductRating? Rating { get; set; }

        // Audit
        [BsonElement("createdBy")]
        public string? CreatedBy { get; set; }
        
        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Soft Delete
        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; }
        
        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }
    }
}




