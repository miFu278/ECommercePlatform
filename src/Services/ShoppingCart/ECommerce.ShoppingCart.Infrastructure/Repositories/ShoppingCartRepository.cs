using ECommerce.ShoppingCart.Domain.Interfaces;
using ECommerce.ShoppingCart.Infrastructure.Data;
using StackExchange.Redis;
using System.Text.Json;

namespace ECommerce.ShoppingCart.Infrastructure.Repositories;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly RedisContext _context;
    private const string KeyPrefix = "cart:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromDays(7);

    public ShoppingCartRepository(RedisContext context)
    {
        _context = context;
    }

    private string GetKey(Guid userId) => $"{KeyPrefix}{userId}";

    public async Task<Domain.Entities.ShoppingCart?> GetByUserIdAsync(Guid userId)
    {
        var key = GetKey(userId);
        var value = await _context.Database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        var cart = JsonSerializer.Deserialize<Domain.Entities.ShoppingCart>(value!);
        return cart;
    }

    public async Task SaveAsync(Domain.Entities.ShoppingCart cart, TimeSpan? expiration = null)
    {
        var key = GetKey(cart.UserId);
        var value = JsonSerializer.Serialize(cart);
        var ttl = expiration ?? DefaultExpiration;

        await _context.Database.StringSetAsync(key, value, ttl);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var key = GetKey(userId);
        await _context.Database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(Guid userId)
    {
        var key = GetKey(userId);
        return await _context.Database.KeyExistsAsync(key);
    }

    public async Task<TimeSpan?> GetExpirationAsync(Guid userId)
    {
        var key = GetKey(userId);
        return await _context.Database.KeyTimeToLiveAsync(key);
    }

    public async Task ExtendExpirationAsync(Guid userId, TimeSpan expiration)
    {
        var key = GetKey(userId);
        await _context.Database.KeyExpireAsync(key, expiration);
    }
}
