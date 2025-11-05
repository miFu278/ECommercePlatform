using ECommerce.Product.Application.DTOs;

namespace ECommerce.Product.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync();
    Task<CategoryDto?> GetByIdAsync(string id);
    Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(string parentId);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateAsync(string id, UpdateCategoryDto dto);
    Task DeleteAsync(string id);
}
