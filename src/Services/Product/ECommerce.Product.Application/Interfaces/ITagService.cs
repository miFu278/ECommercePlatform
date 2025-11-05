using ECommerce.Product.Application.DTOs;

namespace ECommerce.Product.Application.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(string id);
    Task<TagDto?> GetByNameAsync(string name);
    Task<TagDto> CreateAsync(CreateTagDto dto);
    Task<TagDto> UpdateAsync(string id, UpdateTagDto dto);
    Task DeleteAsync(string id);
}
