using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductInventory
{
    [BsonElement("stockQuantity")]
    public int StockQuantity { get; set; }
    
    [BsonElement("lowStockThreshold")]
    public int LowStockThreshold { get; set; }
    
    [BsonElement("trackInventory")]
    public bool TrackInventory { get; set; }
    
    [BsonElement("allowBackorder")]
    public bool AllowBackorder { get; set; }
}
