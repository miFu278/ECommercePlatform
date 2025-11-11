namespace ECommerce.Product.Application.DTOs;

public record TagDto(string? Id, string Name, string Slug, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateTagDto(string Name, string Slug);

public record UpdateTagDto(string Name, string Slug);
