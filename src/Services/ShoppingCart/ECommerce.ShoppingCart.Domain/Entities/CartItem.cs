namespace ECommerce.ShoppingCart.Domain.Entities;

public class CartItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Product availability
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }

    // Computed properties
    public decimal Subtotal => Price * Quantity;
    public decimal TotalDiscount => DiscountAmount * Quantity;
    public decimal Total => Subtotal - TotalDiscount;
    public bool HasDiscount => DiscountAmount > 0;
}
