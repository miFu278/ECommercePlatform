using ECommerce.Shared.Abstractions.Repositories;

namespace ECommerce.Product.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Entities.Category>
{
    Task<Entities.Category?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
