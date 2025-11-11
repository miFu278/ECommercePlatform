namespace ECommerce.Product.Domain.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Entities.Product>> GetAllAsync();
    Task<Entities.Product?> GetByIdAsync(string id);
    Task<Entities.Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Entities.Product>> GetByCategoryIdAsync(string categoryId);
    Task<IEnumerable<Entities.Product>> SearchAsync(string searchTerm);
    Task<Entities.Product> CreateAsync(Entities.Product product);
    Task<Entities.Product> UpdateAsync(string id, Entities.Product product);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsBySlugAsync(string slug, string? excludeId = null);
    
    // Advanced queries
    Task<(IEnumerable<Entities.Product> Items, int TotalCount)> SearchAndFilterAsync(
        string? searchTerm,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        List<string>? tags,
        bool? inStock,
        bool? isFeatured,
        bool? isActive,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize);
    
    Task<IEnumerable<Entities.Product>> GetFeaturedAsync(int limit = 10);
    Task<IEnumerable<Entities.Product>> GetRelatedProductsAsync(string productId, int limit = 5);
    Task<bool> UpdateStockAsync(string id, int quantity);
}
