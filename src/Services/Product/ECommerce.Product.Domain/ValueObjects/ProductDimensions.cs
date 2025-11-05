using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductDimensions
{
    [BsonElement("length")]
    public decimal Length { get; set; }
    
    [BsonElement("width")]
    public decimal Width { get; set; }
    
    [BsonElement("height")]
    public decimal Height { get; set; }
    
    [BsonElement("unit")]
    public string Unit { get; set; } = "cm";
}
