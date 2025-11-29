namespace ECommerce.Product.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Entities.Category>> GetAllAsync();
    Task<Entities.Category?> GetByIdAsync(string id);
    Task<IEnumerable<Entities.Category>> GetByIdsAsync(IEnumerable<string> ids);
    Task<IEnumerable<Entities.Category>> GetRootCategoriesAsync();
    Task<IEnumerable<Entities.Category>> GetChildCategoriesAsync(string parentId);
    Task<Entities.Category> CreateAsync(Entities.Category category);
    Task<Entities.Category> UpdateAsync(string id, Entities.Category category);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsBySlugAsync(string slug, string? excludeId = null);
}
