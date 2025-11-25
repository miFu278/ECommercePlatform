using ECommerce.Notification.Domain.Attributes;
using ECommerce.Notification.Domain.Entities;
using ECommerce.Notification.Domain.Interfaces;
using ECommerce.Notification.Infrastructure.Data;
using MongoDB.Driver;

namespace ECommerce.Notification.Infrastructure.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly IMongoCollection<NotificationLog> _collection;

    public NotificationLogRepository(IMongoDbContext context)
    {
        var collectionName = GetCollectionName();
        _collection = context.GetCollection<NotificationLog>(collectionName);
    }

    public async Task<NotificationLog> CreateAsync(NotificationLog log)
    {
        await _collection.InsertOneAsync(log);
        return log;
    }

    public async Task<NotificationLog?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<NotificationLog>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 10)
    {
        return await _collection
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> GetCountByUserIdAsync(string userId)
    {
        return await _collection.CountDocumentsAsync(x => x.UserId == userId);
    }

    public async Task UpdateAsync(NotificationLog log)
    {
        log.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == log.Id, log);
    }

    private static string GetCollectionName()
    {
        var attribute = (BsonCollectionAttribute?)Attribute.GetCustomAttribute(
            typeof(NotificationLog), typeof(BsonCollectionAttribute));
        return attribute?.CollectionName ?? "notification_logs";
    }
}
