using ECommerce.Product.Application.DTOs;

namespace ECommerce.Product.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(string id);
    Task<IEnumerable<ProductDto>> SearchAsync(string query);
    Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(string categoryId);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(string id, UpdateProductDto dto);
    Task DeleteAsync(string id);
}
