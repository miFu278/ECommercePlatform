using ECommerce.Product.Application.DTOs;

namespace ECommerce.Product.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(string id);
    Task<ProductDto?> GetBySlugAsync(string slug);
    Task<IEnumerable<ProductDto>> SearchAsync(string query);
    Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(string categoryId);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(string id, UpdateProductDto dto);
    Task DeleteAsync(string id);
    
    // Advanced features
    Task<PagedResultDto<ProductListDto>> SearchAndFilterAsync(ProductSearchDto searchDto);
    Task<IEnumerable<ProductListDto>> GetFeaturedAsync(int limit = 10);
    Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(string productId, int limit = 5);
    Task<bool> UpdateStockAsync(string id, int quantity);
}
