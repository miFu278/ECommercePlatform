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
public record ProductImageDto(
    string Url,
    string AltText,
    bool IsPrimary,
    int DisplayOrder
);

public record ProductAttributeDto(
    string Name,
    string Value
);

public record ProductSpecificationsDto(
    string? Brand,
    string? Model,
    string? Warranty
);

public record ProductSeoDto(
    string? MetaTitle,
    string? MetaDescription,
    List<string>? MetaKeywords
);

public record ProductInventoryDto(
    int StockQuantity,
    int LowStockThreshold,
    bool TrackInventory,
    bool AllowBackorder
);

public record ProductDimensionsDto(
    decimal Length,
    decimal Width,
    decimal Height,
    string Unit
);

public record ProductRatingDto(
    decimal Average,
    int Count
);
