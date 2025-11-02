namespace ECommerce.Product.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }
}