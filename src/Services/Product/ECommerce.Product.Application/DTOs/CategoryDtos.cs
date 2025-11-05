namespace ECommerce.Product.Application.DTOs;

public record CategoryDto(
    string? Id,
    string Name,
    string Slug,
    string? Description,
    string? ParentId,
    string? ImageUrl,
    int DisplayOrder,
    bool IsVisible,
    string? MetaTitle,
    string? MetaDescription,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateCategoryDto(
    string Name,
    string Slug,
    string? Description,
    string? ParentId,
    string? ImageUrl,
    int DisplayOrder,
    bool IsVisible,
    string? MetaTitle,
    string? MetaDescription
);

public record UpdateCategoryDto(
    string Name,
    string Slug,
    string? Description,
    string? ParentId,
    string? ImageUrl,
    int DisplayOrder,
    bool IsVisible,
    string? MetaTitle,
    string? MetaDescription
);
