using ECommerce.Notification.Infrastructure.Configuration;
using MongoDB.Driver;

namespace ECommerce.Notification.Infrastructure.Data;

public interface IMongoDbContext
{
    IMongoCollection<T> GetCollection<T>(string name);
    IMongoDatabase GetDatabase();
}

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

    public IMongoDatabase GetDatabase() => _database;
}
