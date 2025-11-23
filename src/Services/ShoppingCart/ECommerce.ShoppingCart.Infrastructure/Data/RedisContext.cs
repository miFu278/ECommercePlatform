using StackExchange.Redis;

namespace ECommerce.ShoppingCart.Infrastructure.Data;

public class RedisContext
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisContext(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public IDatabase Database => _database;
    public IConnectionMultiplexer Connection => _redis;

    public IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints().First();
        return _redis.GetServer(endpoint);
    }
}
