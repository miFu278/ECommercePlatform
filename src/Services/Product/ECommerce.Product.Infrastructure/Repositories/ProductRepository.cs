using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Product.Infrastructure.Repositories;

public class ProductRepository : MongoRepository<Domain.Entities.Product>, IProductRepository
{
    public ProductRepository(IMongoDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Product?> GetBySlugAsync(string slug)
    {
        return await FindOneAsync(p => p.Slug == slug);
    }

    public async Task<IEnumerable<Domain.Entities.Product>> GetByCategoryIdAsync(string categoryId)
    {
        return await FindAsync(p => p.CategoryId == categoryId);
    }

    public async Task<IEnumerable<Domain.Entities.Product>> SearchAsync(string searchTerm)
    {
        var filter = Builders<Domain.Entities.Product>.Filter.Or(
            Builders<Domain.Entities.Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(searchTerm, "i")),
            Builders<Domain.Entities.Product>.Filter.Regex(p => p.LongDescription, new BsonRegularExpression(searchTerm, "i")),
            Builders<Domain.Entities.Product>.Filter.Regex(p => p.ShortDescription, new BsonRegularExpression(searchTerm, "i")),
            Builders<Domain.Entities.Product>.Filter.Regex(p => p.Sku, new BsonRegularExpression(searchTerm, "i"))
        );
        
        return await FindAsync(filter);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, string? excludeId = null)
    {
        var filter = Builders<Domain.Entities.Product>.Filter.Eq(p => p.Slug, slug);
        
        if (!string.IsNullOrEmpty(excludeId))
        {
            filter = Builders<Domain.Entities.Product>.Filter.And(
                filter,
                Builders<Domain.Entities.Product>.Filter.Ne(p => p.Id, excludeId)
            );
        }
        
        var product = await FindOneAsync(filter);
        return product != null;
    }
}
