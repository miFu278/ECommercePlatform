namespace ECommerce.Product.Application.DTOs;

public record CategoryDto(
    string? Id,
    string Name,
    string Slug,
    string? Description,
    string? ParentId,
    int Level,
    List<string> Path,
    string? Image,
    string? Icon,
    int Order,
    bool IsActive,
    List<CategoryFilterMetaDto> FilterMeta,
    CategorySeoDto? Seo,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateCategoryDto(
    string Name,
    string Slug,
    string? Description,
    string? ParentId,
    string? Image,
    string? Icon,
    int Order,
    bool IsActive,
    List<CategoryFilterMetaDto>? FilterMeta,
    CategorySeoDto? Seo
);

public record UpdateCategoryDto(
    string Name,
    string Slug,
    string? Description,
    string? ParentId,
    string? Image,
    string? Icon,
    int Order,
    bool IsActive,
    List<CategoryFilterMetaDto>? FilterMeta,
    CategorySeoDto? Seo
);

public record CategoryFilterMetaDto
{
    public string FieldName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public List<string> ValueOptions { get; init; } = new();
}

public record CategorySeoDto
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}
