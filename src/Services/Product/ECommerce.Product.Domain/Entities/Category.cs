using ECommerce.Product.Domain.Attributes;
using ECommerce.Product.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.Entities;

[BsonCollection("categories")]
public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    // Hierarchy
    [BsonElement("parent_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }

    [BsonElement("level")]
    public int Level { get; set; }

    [BsonElement("path")]
    public List<string> Path { get; set; } = new();

    // Display
    [BsonElement("image")]
    public string? Image { get; set; }

    [BsonElement("icon")]
    public string? Icon { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;

    // Filter metadata for dynamic filtering
    [BsonElement("filter_meta")]
    public List<CategoryFilterMeta> FilterMeta { get; set; } = new();

    // SEO
    [BsonElement("seo")]
    public CategorySeo? Seo { get; set; }

    // Timestamps
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
