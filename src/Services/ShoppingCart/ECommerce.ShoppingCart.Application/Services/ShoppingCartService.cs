using AutoMapper;
using ECommerce.ShoppingCart.Application.DTOs;
using ECommerce.ShoppingCart.Application.Interfaces;
using ECommerce.ShoppingCart.Domain.Entities;
using ECommerce.ShoppingCart.Domain.Interfaces;

namespace ECommerce.ShoppingCart.Application.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public ShoppingCartService(
        IShoppingCartRepository cartRepository,
        IProductService productService,
        IMapper mapper)
    {
        _cartRepository = cartRepository;
        _productService = productService;
        _mapper = mapper;
    }

    public async Task<CartDto?> GetCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        
        if (cart == null)
            return null;

        // Refresh product info and availability
        await RefreshCartItemsAsync(cart);
        await _cartRepository.SaveAsync(cart);

        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto dto)
    {
        // Get product info from Product Service
        var productInfo = await _productService.GetProductInfoAsync(dto.ProductId);
        if (productInfo == null)
            throw new InvalidOperationException("Product not found");

        if (!productInfo.IsAvailable)
            throw new InvalidOperationException("Product is not available");

        if (productInfo.StockQuantity < dto.Quantity)
            throw new InvalidOperationException($"Only {productInfo.StockQuantity} items available in stock");

        // Get or create cart
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Domain.Entities.ShoppingCart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Create cart item
        var cartItem = new CartItem
        {
            ProductId = productInfo.Id,
            ProductName = productInfo.Name,
            Sku = productInfo.Sku,
            ImageUrl = productInfo.ImageUrl,
            Price = productInfo.Price,
            CompareAtPrice = productInfo.CompareAtPrice,
            DiscountAmount = CalculateDiscount(productInfo.Price, productInfo.CompareAtPrice),
            Quantity = dto.Quantity,
            StockQuantity = productInfo.StockQuantity,
            IsAvailable = productInfo.IsAvailable,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        cart.AddItem(cartItem);
        await _cartRepository.SaveAsync(cart);

        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid productId, UpdateCartItemDto dto)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            throw new InvalidOperationException("Cart not found");

        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Item not found in cart");

        // Check stock availability
        var productInfo = await _productService.GetProductInfoAsync(productId);
        if (productInfo == null)
            throw new InvalidOperationException("Product not found");

        if (productInfo.StockQuantity < dto.Quantity)
            throw new InvalidOperationException($"Only {productInfo.StockQuantity} items available in stock");

        cart.UpdateItemQuantity(productId, dto.Quantity);
        await _cartRepository.SaveAsync(cart);

        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            throw new InvalidOperationException("Cart not found");

        cart.RemoveItem(productId);
        
        if (cart.Items.Count == 0)
        {
            await _cartRepository.DeleteAsync(userId);
            return _mapper.Map<CartDto>(cart);
        }

        await _cartRepository.SaveAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        await _cartRepository.DeleteAsync(userId);
    }

    public async Task<bool> MergeCartsAsync(Guid anonymousUserId, Guid authenticatedUserId)
    {
        var anonymousCart = await _cartRepository.GetByUserIdAsync(anonymousUserId);
        if (anonymousCart == null || anonymousCart.Items.Count == 0)
            return false;

        var authenticatedCart = await _cartRepository.GetByUserIdAsync(authenticatedUserId);
        if (authenticatedCart == null)
        {
            // Simply move anonymous cart to authenticated user
            anonymousCart.UserId = authenticatedUserId;
            await _cartRepository.SaveAsync(anonymousCart);
            await _cartRepository.DeleteAsync(anonymousUserId);
            return true;
        }

        // Merge items
        foreach (var item in anonymousCart.Items)
        {
            authenticatedCart.AddItem(item);
        }

        await _cartRepository.SaveAsync(authenticatedCart);
        await _cartRepository.DeleteAsync(anonymousUserId);
        return true;
    }

    private async Task RefreshCartItemsAsync(Domain.Entities.ShoppingCart cart)
    {
        if (cart.Items.Count == 0)
            return;

        var productIds = cart.Items.Select(x => x.ProductId).ToList();
        var productsInfo = await _productService.GetProductsInfoAsync(productIds);

        foreach (var item in cart.Items)
        {
            if (productsInfo.TryGetValue(item.ProductId, out var productInfo))
            {
                item.Price = productInfo.Price;
                item.CompareAtPrice = productInfo.CompareAtPrice;
                item.DiscountAmount = CalculateDiscount(productInfo.Price, productInfo.CompareAtPrice);
                item.StockQuantity = productInfo.StockQuantity;
                item.IsAvailable = productInfo.IsAvailable;
            }
            else
            {
                item.IsAvailable = false;
            }
        }
    }

    private decimal CalculateDiscount(decimal price, decimal? compareAtPrice)
    {
        if (compareAtPrice.HasValue && compareAtPrice.Value > price)
            return compareAtPrice.Value - price;
        
        return 0;
    }
}
