using ECommerce.Product.Domain.Enums;
using ECommerce.Shared.Abstractions.Entities;

namespace ECommerce.Product.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Variant Info
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Options (stored as JSON or separate table)
        public string Option1Name { get; set; } = string.Empty;
        public string Option1Value { get; set; } = string.Empty;
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
    }
}