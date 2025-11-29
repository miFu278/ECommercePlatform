using ECommerce.Product.Domain.Enums;

namespace ECommerce.Product.Application.DTOs;

public record ProductDto(
    string? Id,
    string Sku,
    string Name,
    string Slug,
    string Description,
    string LongDescription,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    int Stock,
    int LowStockThreshold,
    bool TrackInventory,
    string CategoryId,
    List<string> CategoryPath,
    string? BrandId,
    string? BrandName,
    List<ProductImageDto> Images,
    List<ProductAttributeDto> Attributes,
    ProductSpecificationsDto? Specifications,
    ProductSeoDto? Seo,
    ProductStatus Status,
    bool IsFeatured,
    bool IsPublished,
    DateTime? PublishedAt,
    List<string> Tags,
    ProductRatingDto? Rating,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateProductDto(
    string Sku,
    string Name,
    string Slug,
    string Description,
    string LongDescription,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    int Stock,
    int LowStockThreshold,
    bool TrackInventory,
    string CategoryId,
    string? BrandId,
    string? BrandName,
    List<ProductImageDto>? Images,
    List<ProductAttributeDto>? Attributes,
    ProductSpecificationsDto? Specifications,
    ProductSeoDto? Seo,
    bool IsFeatured,
    List<string>? Tags
);


public record UpdateProductDto(
    string Name,
    string Slug,
    string Description,
    string LongDescription,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    int Stock,
    int LowStockThreshold,
    bool TrackInventory,
    string CategoryId,
    string? BrandId,
    string? BrandName,
    List<ProductImageDto>? Images,
    List<ProductAttributeDto>? Attributes,
    ProductSpecificationsDto? Specifications,
    ProductSeoDto? Seo,
    ProductStatus Status,
    bool IsFeatured,
    List<string>? Tags
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
    // General
    public string? Model { get; init; }
    public string? Weight { get; init; }
    public ProductDimensionsDto? Dimensions { get; init; }
    public string? Warranty { get; init; }

    // CPU
    public string? SocketType { get; init; }
    public int? CoreCount { get; init; }
    public int? ThreadCount { get; init; }
    public decimal? BaseClockGhz { get; init; }
    public decimal? BoostClockGhz { get; init; }
    public int? TdpW { get; init; }

    // GPU
    public int? VramGb { get; init; }
    public string? GpuChipset { get; init; }

    // Monitor
    public int? RefreshRateHz { get; init; }
    public decimal? ScreenSizeInch { get; init; }
    public string? PanelType { get; init; }
    public string? Resolution { get; init; }

    // Storage/RAM
    public int? CapacityGb { get; init; }
    public string? MemoryType { get; init; }
    public int? MemorySpeedMhz { get; init; }

    // Mainboard
    public string? Chipset { get; init; }
    public string? FormFactor { get; init; }
    public int? MemorySlots { get; init; }

    // PSU
    public int? Wattage { get; init; }
    public string? EfficiencyRating { get; init; }
    public string? Modular { get; init; }

    // Case
    public string? CaseType { get; init; }
    public int? MaxGpuLengthMm { get; init; }
    public int? MaxCoolerHeightMm { get; init; }
}

public record ProductSeoDto
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public List<string>? MetaKeywords { get; init; }
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
