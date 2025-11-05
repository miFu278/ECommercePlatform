using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductSpecifications
{
    [BsonElement("brand")]
    public string? Brand { get; set; }
    
    [BsonElement("model")]
    public string? Model { get; set; }
    
    [BsonElement("warranty")]
    public string? Warranty { get; set; }
}
