using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Product.Infrastructure.Repositories;

public class CategoryRepository : Repository<Domain.Entities.Category>, ICategoryRepository
{
    public CategoryRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Category?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ParentId == null)
            .Include(c => c.Children)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.Slug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
