namespace ECommerce.ShoppingCart.Application.Interfaces;

/// <summary>
/// Interface for communicating with Product Service
/// </summary>
public interface IProductService
{
    Task<ProductInfo?> GetProductInfoAsync(Guid productId);
    Task<Dictionary<Guid, ProductInfo>> GetProductsInfoAsync(List<Guid> productIds);
}

public class ProductInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
}
