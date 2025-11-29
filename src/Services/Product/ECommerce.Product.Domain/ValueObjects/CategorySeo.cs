using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class CategorySeo
{
    [BsonElement("meta_title")]
    public string? MetaTitle { get; set; }

    [BsonElement("meta_description")]
    public string? MetaDescription { get; set; }
}
