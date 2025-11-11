using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductRating
{
    [BsonElement("average")]
    public decimal Average { get; set; }
    
    [BsonElement("count")]
    public int Count { get; set; }
}
