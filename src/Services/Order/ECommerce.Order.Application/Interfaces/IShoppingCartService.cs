namespace ECommerce.Order.Application.Interfaces;

public interface IShoppingCartService
{
    Task<CartDto?> GetCartAsync(Guid userId);
    Task ClearCartAsync(Guid userId);
}

public class CartDto
{
    public Guid UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
}

public class CartItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
}
