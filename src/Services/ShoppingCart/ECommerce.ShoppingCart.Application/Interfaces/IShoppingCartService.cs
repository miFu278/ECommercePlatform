using ECommerce.ShoppingCart.Application.DTOs;

namespace ECommerce.ShoppingCart.Application.Interfaces;

public interface IShoppingCartService
{
    Task<CartDto?> GetCartAsync(Guid userId);
    Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto dto);
    Task<CartDto> UpdateCartItemAsync(Guid userId, Guid productId, UpdateCartItemDto dto);
    Task<CartDto> RemoveFromCartAsync(Guid userId, Guid productId);
    Task ClearCartAsync(Guid userId);
    Task<bool> MergeCartsAsync(Guid anonymousUserId, Guid authenticatedUserId);
}
