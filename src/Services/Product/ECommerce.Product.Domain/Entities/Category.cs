using ECommerce.Product.Domain.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.Entities
{
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
        [BsonElement("parentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentId { get; set; }

        // Display
        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }
        
        [BsonElement("displayOrder")]
        public int DisplayOrder { get; set; }
        
        [BsonElement("isVisible")]
        public bool IsVisible { get; set; }

        // SEO
        [BsonElement("metaTitle")]
        public string? MetaTitle { get; set; }
        
        [BsonElement("metaDescription")]
        public string? MetaDescription { get; set; }

        // Timestamps
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}