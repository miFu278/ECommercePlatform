using ECommerce.Shared.Abstractions.Repositories;

namespace ECommerce.Product.Domain.Interfaces;

public interface ITagRepository : IRepository<Entities.Tag>
{
    Task<Entities.Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Tag>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}
