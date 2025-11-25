using ECommerce.Shared.Abstractions.Entities;

namespace ECommerce.Order.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    
    // Product Info (snapshot at order time)
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    // Pricing
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Navigation
    public Order Order { get; set; } = null!;
}
