using ECommerce.Product.Domain.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.Entities
{
    [BsonCollection("tags")]
    public class Tag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        
        [BsonElement("slug")]
        public string Slug { get; set; } = string.Empty;

        // Timestamps
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}