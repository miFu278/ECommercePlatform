using System.Linq.Expressions;
using ECommerce.Product.Domain.Attributes;
using ECommerce.Product.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Product.Infrastructure.Repositories;

public abstract class MongoRepository<TDocument> where TDocument : class
{
    protected readonly IMongoCollection<TDocument> _collection;

    protected MongoRepository(IMongoDbContext context)
    {
        var collectionName = GetCollectionName(typeof(TDocument));
        _collection = context.GetCollection<TDocument>(collectionName);
    }

    private static string GetCollectionName(Type documentType)
    {
        var attribute = documentType.GetCustomAttributes(typeof(BsonCollectionAttribute), true)
            .FirstOrDefault() as BsonCollectionAttribute;
        return attribute?.CollectionName ?? documentType.Name.ToLowerInvariant();
    }

    public virtual async Task<IEnumerable<TDocument>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    public virtual async Task<TDocument?> GetByIdAsync(string id)
    {
        FilterDefinition<TDocument> filter;
        if (ObjectId.TryParse(id, out var objectId))
            filter = Builders<TDocument>.Filter.Eq("_id", objectId);
        else
            filter = Builders<TDocument>.Filter.Eq("_id", id);

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<TDocument> CreateAsync(TDocument document)
    {
        await _collection.InsertOneAsync(document);
        return document;
    }

    public virtual async Task<TDocument> UpdateAsync(string id, TDocument document)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id), "Id cannot be null or empty");

        FilterDefinition<TDocument> filter;
        if (ObjectId.TryParse(id, out var objectId))
            filter = Builders<TDocument>.Filter.Eq("_id", objectId);
        else
            filter = Builders<TDocument>.Filter.Eq("_id", id);

        var result = await _collection.ReplaceOneAsync(filter, document);
        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"Document with ID {id} was not found for update.");

        return document;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        FilterDefinition<TDocument> filter;
        if (ObjectId.TryParse(id, out var objectId))
            filter = Builders<TDocument>.Filter.Eq("_id", objectId);
        else
            filter = Builders<TDocument>.Filter.Eq("_id", id);

        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        FilterDefinition<TDocument> filter;
        if (ObjectId.TryParse(id, out var objectId))
            filter = Builders<TDocument>.Filter.Eq("_id", objectId);
        else
            filter = Builders<TDocument>.Filter.Eq("_id", id);

        var count = await _collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 });
        return count > 0;
    }

    public virtual async Task<long> CountAsync()
        => await _collection.CountDocumentsAsync(FilterDefinition<TDocument>.Empty);

    public virtual async Task<IEnumerable<TDocument>> FindAsync(FilterDefinition<TDocument> filter)
        => await _collection.Find(filter).ToListAsync();

    public virtual async Task<IEnumerable<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate)
        => await _collection.Find(predicate).ToListAsync();

    public virtual async Task<TDocument?> FindOneAsync(FilterDefinition<TDocument> filter)
        => await _collection.Find(filter).FirstOrDefaultAsync();

    public virtual async Task<TDocument?> FindOneAsync(Expression<Func<TDocument, bool>> predicate)
        => await _collection.Find(predicate).FirstOrDefaultAsync();
}
