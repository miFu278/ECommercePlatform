namespace ECommerce.Product.Application.DTOs;

/// <summary>
/// DTO for product search and filtering
/// </summary>
public class ProductSearchDto
{
    /// <summary>
    /// Search term (searches in name, description, SKU)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Filter by tags (comma-separated)
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Filter by in-stock status
    /// </summary>
    public bool? InStock { get; set; }

    /// <summary>
    /// Filter by featured products only
    /// </summary>
    public bool? IsFeatured { get; set; }

    /// <summary>
    /// Filter by active products only
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Sort by field (name, price, createdAt, rating)
    /// </summary>
    public string SortBy { get; set; } = "createdAt";

    /// <summary>
    /// Sort order (asc, desc)
    /// </summary>
    public string SortOrder { get; set; } = "desc";

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;
}
