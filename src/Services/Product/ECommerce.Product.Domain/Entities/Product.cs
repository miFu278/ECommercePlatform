using ECommerce.Product.Domain.Enums;

namespace ECommerce.Product.Domain.Entities
{
    public class Product
    {
        // Identity
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        // Description
        public string ShortDescription { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;

        // Pricing
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; } // Original price before discount
        public decimal Cost { get; set; } // Cost to the business

        // Category
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Brand
        public string? Brand { get; set; }

        // Status 
        public ProductStatus Status { get; set; } // Draft, Active, Archived
        public bool IsVisible { get; set; } // Show on storefront
        public bool IsFeatured { get; set; } // Featured product

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        // Inventory
        public bool TrackInventory { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public StockStatus StockStatus { get; set; } // InStock, OutOfStock, LowStock

        // Shipping
        public decimal Weight { get; set; } // In kilograms
        public decimal? Length { get; set; } // In centimeters
        public decimal? Width { get; set; } // In centimeters
        public decimal? Height { get; set; } // In centimeters

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // Soft delete
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation Properties
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductTag> Tags { get; set; } = new List<ProductTag>();
        public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    }
}




