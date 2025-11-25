using System.Security.Claims;
using ECommerce.ShoppingCart.Application.DTOs;
using ECommerce.ShoppingCart.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.ShoppingCart.API.Controllers;

[ApiController]
[Route("api/cart")]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _cartService;

    public ShoppingCartController(IShoppingCartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get shopping cart for current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartAsync(userId);
        if (cart == null)
        {
            return Ok(new CartDto { UserId = userId });
        }

        return Ok(cart);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartDto dto)
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.AddToCartAsync(userId, dto);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items/{productId}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(Guid productId, [FromBody] UpdateCartItemDto dto)
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.UpdateCartItemAsync(userId, productId, dto);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{productId}")]
    public async Task<ActionResult<CartDto>> RemoveFromCart(Guid productId)
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.RemoveFromCartAsync(userId, productId);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Clear entire cart
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// Merge anonymous cart with authenticated user cart (after login)
    /// </summary>
    [HttpPost("merge")]
    public async Task<IActionResult> MergeCarts([FromBody] MergeCartsDto dto)
    {
        var authenticatedUserId = GetUserId();
        var merged = await _cartService.MergeCartsAsync(dto.AnonymousUserId, authenticatedUserId);
        
        return Ok(new { merged });
    }

    private Guid GetUserId()
    {
        // Try to get from API Gateway header first (trusted)
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
        {
            if (Guid.TryParse(userIdHeader, out var userId))
            {
                return userId;
            }
        }

        // Fallback to JWT claims (if called directly without Gateway)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userIdFromClaim))
        {
            return userIdFromClaim;
        }

        throw new UnauthorizedAccessException("User ID not found");
    }
}

public class MergeCartsDto
{
    public Guid AnonymousUserId { get; set; }
}
