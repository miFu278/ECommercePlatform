using MongoDB.Bson.Serialization.Attributes;
using ECommerce.Product.Domain.Enums;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductAttribute
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;
}
