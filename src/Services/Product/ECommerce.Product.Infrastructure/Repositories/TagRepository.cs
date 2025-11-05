using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Product.Infrastructure.Repositories;

public class TagRepository : Repository<Domain.Entities.Tag>, ITagRepository
{
    public TagRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Tag>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => names.Contains(t.Name))
            .ToListAsync(cancellationToken);
    }
}
