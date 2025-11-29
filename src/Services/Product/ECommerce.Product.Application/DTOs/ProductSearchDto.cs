using ECommerce.Product.Domain.Enums;

namespace ECommerce.Product.Application.DTOs;

/// <summary>
/// DTO for product search and filtering
/// </summary>
public class ProductSearchDto
{
    public string? SearchTerm { get; set; }
    public string? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string>? Tags { get; set; }
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public ProductStatus? Status { get; set; }
    public string? BrandName { get; set; }

    // PC component specific filters
    public string? SocketType { get; set; }
    public string? MemoryType { get; set; }
    public string? FormFactor { get; set; }
    public int? MinVramGb { get; set; }
    public int? MaxVramGb { get; set; }

    public string SortBy { get; set; } = "created_at";
    public string SortOrder { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
