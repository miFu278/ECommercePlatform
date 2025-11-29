using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using MongoDB.Driver;

namespace ECommerce.Product.Infrastructure.Repositories;

public class CategoryRepository : MongoRepository<Domain.Entities.Category>, ICategoryRepository
{
    public CategoryRepository(IMongoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Domain.Entities.Category>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var filter = Builders<Domain.Entities.Category>.Filter.In(c => c.Id, ids);
        return await FindAsync(filter);
    }

    public async Task<IEnumerable<Domain.Entities.Category>> GetRootCategoriesAsync()
    {
        return await FindAsync(c => c.ParentId == null);
    }

    public async Task<IEnumerable<Domain.Entities.Category>> GetChildCategoriesAsync(string parentId)
    {
        return await FindAsync(c => c.ParentId == parentId);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, string? excludeId = null)
    {
        var filter = Builders<Domain.Entities.Category>.Filter.Eq(c => c.Slug, slug);
        
        if (!string.IsNullOrEmpty(excludeId))
        {
            filter = Builders<Domain.Entities.Category>.Filter.And(
                filter,
                Builders<Domain.Entities.Category>.Filter.Ne(c => c.Id, excludeId)
            );
        }
        
        var category = await FindOneAsync(filter);
        return category != null;
    }
}
