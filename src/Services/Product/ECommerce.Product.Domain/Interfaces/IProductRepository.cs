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
}
