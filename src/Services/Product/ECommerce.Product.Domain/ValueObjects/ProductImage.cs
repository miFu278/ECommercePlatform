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
    
    [BsonElement("displayOrder")]
    public int DisplayOrder { get; set; }
}
