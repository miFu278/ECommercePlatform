using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductSeo
{
    [BsonElement("metaTitle")]
    public string? MetaTitle { get; set; }
    
    [BsonElement("metaDescription")]
    public string? MetaDescription { get; set; }
    
    [BsonElement("metaKeywords")]
    public List<string> MetaKeywords { get; set; } = new();
}
