using ECommerce.Notification.Domain.Attributes;
using ECommerce.Notification.Domain.Entities;
using ECommerce.Notification.Domain.Interfaces;
using ECommerce.Notification.Infrastructure.Data;
using MongoDB.Driver;

namespace ECommerce.Notification.Infrastructure.Repositories;

public class EmailLogRepository : IEmailLogRepository
{
    private readonly IMongoCollection<EmailLog> _collection;

    public EmailLogRepository(IMongoDbContext context)
    {
        var collectionName = GetCollectionName();
        _collection = context.GetCollection<EmailLog>(collectionName);
    }

    public async Task<EmailLog> CreateAsync(EmailLog log)
    {
        await _collection.InsertOneAsync(log);
        return log;
    }

    public async Task<EmailLog?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<EmailLog>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 10)
    {
        return await _collection
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(EmailLog log)
    {
        await _collection.ReplaceOneAsync(x => x.Id == log.Id, log);
    }

    private static string GetCollectionName()
    {
        var attribute = (BsonCollectionAttribute?)Attribute.GetCustomAttribute(
            typeof(EmailLog), typeof(BsonCollectionAttribute));
        return attribute?.CollectionName ?? "email_logs";
    }
}
