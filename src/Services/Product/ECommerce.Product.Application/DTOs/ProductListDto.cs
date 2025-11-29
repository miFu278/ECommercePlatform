using ECommerce.Product.Domain.Enums;

namespace ECommerce.Product.Application.DTOs;

/// <summary>
/// Simplified DTO for product listing (less data for performance)
/// </summary>
public record ProductListDto(
    string? Id = null,
    string Sku = "",
    string Name = "",
    string Slug = "",
    string Description = "",
    decimal Price = 0,
    decimal? CompareAtPrice = null,
    string? PrimaryImageUrl = null,
    string CategoryId = "",
    string CategoryName = "",
    string? BrandName = null,
    ProductStatus Status = ProductStatus.Draft,
    bool IsFeatured = false,
    int Stock = 0,
    bool InStock = false,
    List<string>? Tags = null,
    ProductRatingDto? Rating = null,
    DateTime CreatedAt = default
)
{
    /// <summary>
    /// Discount percentage (if CompareAtPrice exists)
    /// </summary>
    public decimal? DiscountPercentage => CompareAtPrice.HasValue && CompareAtPrice.Value > 0
        ? Math.Round((CompareAtPrice.Value - Price) / CompareAtPrice.Value * 100, 2)
        : null;
}
