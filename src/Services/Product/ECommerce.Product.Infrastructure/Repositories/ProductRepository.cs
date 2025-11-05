using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Product.Infrastructure.Repositories;

public class ProductRepository : Repository<Domain.Entities.Product>, IProductRepository
{
    public ProductRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Include(p => p.Attributes)
            .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Product>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Tags.Any(pt => pt.TagId == tagId))
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(p => p.Name.ToLower().Contains(lowerSearchTerm) 
                     || p.LongDescription.ToLower().Contains(lowerSearchTerm)
                     || p.ShortDescription.ToLower().Contains(lowerSearchTerm)
                     || p.Slug.ToLower().Contains(lowerSearchTerm))
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Slug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
