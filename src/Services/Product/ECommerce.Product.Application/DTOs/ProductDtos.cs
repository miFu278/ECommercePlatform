using ECommerce.Product.Domain.Enums;

namespace ECommerce.Product.Application.DTOs;

public record ProductDto(
    string? Id,
    string Sku,
    string Name,
    string Slug,
    string ShortDescription,
    string LongDescription,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    string CategoryId,
    List<string> CategoryPath,
    List<ProductImageDto> Images,
    List<string> TagIds,
    List<ProductAttributeDto> Attributes,
    ProductSpecificationsDto? Specifications,
    ProductStatus Status,
    bool IsActive,
    bool IsFeatured,
    bool IsPublished,
    DateTime? PublishedAt,
    ProductSeoDto? Seo,
    ProductInventoryDto Inventory,
    decimal Weight,
    ProductDimensionsDto? Dimensions,
    ProductRatingDto? Rating,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateProductDto(
    string Sku,
    string Name,
    string Slug,
    string ShortDescription,
    string LongDescription,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    string CategoryId,
    List<string>? TagIds,
    List<ProductImageDto>? Images,
    List<ProductAttributeDto>? Attributes,
    ProductSpecificationsDto? Specifications,
    ProductSeoDto? Seo,
    ProductInventoryDto Inventory,
    bool IsActive,
    bool IsFeatured,
    decimal Weight,
    ProductDimensionsDto? Dimensions
);

public record UpdateProductDto(
    string Name,
    string Slug,
    string ShortDescription,
    string LongDescription,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    string CategoryId,
    List<string>? TagIds,
    List<ProductImageDto>? Images,
    List<ProductAttributeDto>? Attributes,
    ProductSpecificationsDto? Specifications,
    ProductSeoDto? Seo,
    ProductInventoryDto Inventory,
    bool IsActive,
    bool IsFeatured,
    decimal Weight,
    ProductDimensionsDto? Dimensions
);

// Nested DTOs
public record ProductImageDto
{
    public string Url { get; init; } = string.Empty;
    public string AltText { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public int Order { get; init; }
}

public record ProductAttributeDto
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record ProductSpecificationsDto
{
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? Warranty { get; init; }
}

public record ProductSeoDto
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public List<string>? MetaKeywords { get; init; }
}

public record ProductInventoryDto
{
    public int Stock { get; init; }
    public int LowStockThreshold { get; init; }
    public bool TrackInventory { get; init; }
    public bool AllowBackorder { get; init; }
}

public record ProductDimensionsDto
{
    public decimal Length { get; init; }
    public decimal Width { get; init; }
    public decimal Height { get; init; }
    public string Unit { get; init; } = string.Empty;
}

public record ProductRatingDto
{
    public decimal Average { get; init; }
    public int Count { get; init; }
}
