using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;

namespace ECommerce.Product.Infrastructure.Repositories;

public class TagRepository : MongoRepository<Domain.Entities.Tag>, ITagRepository
{
    public TagRepository(IMongoDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Tag?> GetByNameAsync(string name)
    {
        return await FindOneAsync(t => t.Name == name);
    }

    public async Task<Domain.Entities.Tag?> GetBySlugAsync(string slug)
    {
        return await FindOneAsync(t => t.Slug == slug);
    }
}
