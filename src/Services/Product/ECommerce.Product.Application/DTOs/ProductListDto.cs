using ECommerce.Product.Domain.Enums;

namespace ECommerce.Product.Application.DTOs;

/// <summary>
/// Simplified DTO for product listing (less data for performance)
/// </summary>
public record ProductListDto(
    string? Id,
    string Sku,
    string Name,
    string Slug,
    string ShortDescription,
    decimal Price,
    decimal? CompareAtPrice,
    string? PrimaryImageUrl,
    string CategoryId,
    string CategoryName,
    bool IsActive,
    bool IsFeatured,
    int Stock,
    bool InStock,
    ProductRatingDto? Rating,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Discount percentage (if CompareAtPrice exists)
    /// </summary>
    public decimal? DiscountPercentage => CompareAtPrice.HasValue && CompareAtPrice.Value > 0
        ? Math.Round((CompareAtPrice.Value - Price) / CompareAtPrice.Value * 100, 2)
        : null;
}
