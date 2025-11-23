namespace ECommerce.ShoppingCart.Domain.Interfaces;

public interface IShoppingCartRepository
{
    Task<Entities.ShoppingCart?> GetByUserIdAsync(Guid userId);
    Task SaveAsync(Entities.ShoppingCart cart, TimeSpan? expiration = null);
    Task DeleteAsync(Guid userId);
    Task<bool> ExistsAsync(Guid userId);
    Task<TimeSpan?> GetExpirationAsync(Guid userId);
    Task ExtendExpirationAsync(Guid userId, TimeSpan expiration);
}
