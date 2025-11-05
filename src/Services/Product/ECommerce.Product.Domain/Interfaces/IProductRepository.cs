using ECommerce.Shared.Abstractions.Repositories;

namespace ECommerce.Product.Domain.Interfaces;

public interface IProductRepository : IRepository<Entities.Product>
{
    Task<Entities.Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Product>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
