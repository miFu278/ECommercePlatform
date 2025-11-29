using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class CategoryFilterMeta
{
    [BsonElement("field_name")]
    public string FieldName { get; set; } = string.Empty;

    [BsonElement("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("value_options")]
    public List<string> ValueOptions { get; set; } = new();
}
