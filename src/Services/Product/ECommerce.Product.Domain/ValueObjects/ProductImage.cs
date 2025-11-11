using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductImage
{
    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;
    
    [BsonElement("altText")]
    public string AltText { get; set; } = string.Empty;
    
    [BsonElement("isPrimary")]
    public bool IsPrimary { get; set; }
    
    [BsonElement("order")]
    public int Order { get; set; }
}
