using ECommerce.ShoppingCart.Application.Interfaces;
using System.Text.Json;

namespace ECommerce.ShoppingCart.Infrastructure.Services;

/// <summary>
/// Mock implementation - Replace with actual HTTP client to Product Service
/// </summary>
public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductInfo?> GetProductInfoAsync(Guid productId)
    {
        try
        {
            // TODO: Replace with actual Product Service endpoint
            var response = await _httpClient.GetAsync($"/api/products/{productId}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (product == null)
                return null;

            return new ProductInfo
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                ImageUrl = product.Images?.FirstOrDefault()?.Url,
                Price = product.Price,
                CompareAtPrice = product.CompareAtPrice,
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsActive && product.StockQuantity > 0
            };
        }
        catch
        {
            // Log error
            return null;
        }
    }

    public async Task<Dictionary<Guid, ProductInfo>> GetProductsInfoAsync(List<Guid> productIds)
    {
        var result = new Dictionary<Guid, ProductInfo>();

        // TODO: Optimize with batch endpoint
        foreach (var productId in productIds)
        {
            var productInfo = await GetProductInfoAsync(productId);
            if (productInfo != null)
            {
                result[productId] = productInfo;
            }
        }

        return result;
    }

    // Response models matching Product Service
    private class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public List<ProductImage>? Images { get; set; }
    }

    private class ProductImage
    {
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
