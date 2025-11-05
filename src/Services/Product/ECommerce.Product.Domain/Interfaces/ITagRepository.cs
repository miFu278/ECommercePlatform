namespace ECommerce.Product.Domain.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Entities.Tag>> GetAllAsync();
    Task<Entities.Tag?> GetByIdAsync(string id);
    Task<Entities.Tag?> GetByNameAsync(string name);
    Task<Entities.Tag?> GetBySlugAsync(string slug);
    Task<Entities.Tag> CreateAsync(Entities.Tag tag);
    Task<Entities.Tag> UpdateAsync(string id, Entities.Tag tag);
    Task<bool> DeleteAsync(string id);
}
