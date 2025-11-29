using ECommerce.Product.Domain.Enums;
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
        return await FindOneAsync(p => p.Slug == slug && p.DeletedAt == null);
    }

    public async Task<IEnumerable<Domain.Entities.Product>> GetByCategoryIdAsync(string categoryId)
    {
        return await FindAsync(p => p.CategoryId == categoryId && p.DeletedAt == null);
    }

    public async Task<IEnumerable<Domain.Entities.Product>> SearchAsync(string searchTerm)
    {
        var filter = Builders<Domain.Entities.Product>.Filter.And(
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.DeletedAt, null),
            Builders<Domain.Entities.Product>.Filter.Or(
                Builders<Domain.Entities.Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(searchTerm, "i")),
                Builders<Domain.Entities.Product>.Filter.Regex(p => p.LongDescription, new BsonRegularExpression(searchTerm, "i")),
                Builders<Domain.Entities.Product>.Filter.Regex(p => p.Description, new BsonRegularExpression(searchTerm, "i")),
                Builders<Domain.Entities.Product>.Filter.Regex(p => p.Sku, new BsonRegularExpression(searchTerm, "i"))
            )
        );

        return await FindAsync(filter);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, string? excludeId = null)
    {
        var filter = Builders<Domain.Entities.Product>.Filter.And(
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.Slug, slug),
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.DeletedAt, null)
        );

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


    public async Task<(IEnumerable<Domain.Entities.Product> Items, int TotalCount)> SearchAndFilterAsync(
        string? searchTerm,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        List<string>? tags,
        bool? inStock,
        bool? isFeatured,
        ProductStatus? status,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize)
    {
        var filterBuilder = Builders<Domain.Entities.Product>.Filter;
        var filters = new List<FilterDefinition<Domain.Entities.Product>>
        {
            filterBuilder.Eq(p => p.DeletedAt, null)
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, new BsonRegularExpression(searchTerm, "i")),
                filterBuilder.Regex(p => p.Description, new BsonRegularExpression(searchTerm, "i")),
                filterBuilder.Regex(p => p.LongDescription, new BsonRegularExpression(searchTerm, "i")),
                filterBuilder.Regex(p => p.Sku, new BsonRegularExpression(searchTerm, "i"))
            ));
        }

        if (!string.IsNullOrWhiteSpace(categoryId))
            filters.Add(filterBuilder.Eq(p => p.CategoryId, categoryId));

        if (minPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price, minPrice.Value));

        if (maxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price, maxPrice.Value));

        if (tags != null && tags.Any())
            filters.Add(filterBuilder.AnyIn(p => p.Tags, tags));

        if (inStock.HasValue)
        {
            if (inStock.Value)
                filters.Add(filterBuilder.Or(
                    filterBuilder.Eq(p => p.TrackInventory, false),
                    filterBuilder.Gt(p => p.Stock, 0)
                ));
            else
                filters.Add(filterBuilder.And(
                    filterBuilder.Eq(p => p.TrackInventory, true),
                    filterBuilder.Lte(p => p.Stock, 0)
                ));
        }

        if (isFeatured.HasValue)
            filters.Add(filterBuilder.Eq(p => p.IsFeatured, isFeatured.Value));

        if (status.HasValue)
            filters.Add(filterBuilder.Eq(p => p.Status, status.Value));

        var combinedFilter = filterBuilder.And(filters);
        var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

        var sortDefinition = sortOrder.ToLower() == "asc"
            ? Builders<Domain.Entities.Product>.Sort.Ascending(GetSortField(sortBy))
            : Builders<Domain.Entities.Product>.Sort.Descending(GetSortField(sortBy));

        var skip = (pageNumber - 1) * pageSize;
        var items = await _collection
            .Find(combinedFilter)
            .Sort(sortDefinition)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();

        return (items, (int)totalCount);
    }


    public async Task<IEnumerable<Domain.Entities.Product>> GetFeaturedAsync(int limit = 10)
    {
        var filter = Builders<Domain.Entities.Product>.Filter.And(
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.DeletedAt, null),
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.IsFeatured, true),
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.Status, ProductStatus.Active)
        );

        return await _collection
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Entities.Product>> GetRelatedProductsAsync(string productId, int limit = 5)
    {
        var product = await GetByIdAsync(productId);
        if (product == null) return new List<Domain.Entities.Product>();

        var filter = Builders<Domain.Entities.Product>.Filter.And(
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.DeletedAt, null),
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.Status, ProductStatus.Active),
            Builders<Domain.Entities.Product>.Filter.Ne(p => p.Id, productId),
            Builders<Domain.Entities.Product>.Filter.Eq(p => p.CategoryId, product.CategoryId)
        );

        return await _collection
            .Find(filter)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(string id, int quantity)
    {
        var filter = Builders<Domain.Entities.Product>.Filter.Eq(p => p.Id, id);
        var update = Builders<Domain.Entities.Product>.Update
            .Set(p => p.Stock, quantity)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    private static string GetSortField(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "name" => "name",
            "price" => "price",
            "created_at" => "created_at",
            "rating" => "rating.average",
            _ => "created_at"
        };
    }
}
