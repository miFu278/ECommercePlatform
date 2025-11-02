namespace ECommerce.Product.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Image info
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public string? Title { get; set; }

        // Storage
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } // in bytes
        public string MimeType { get; set; } = string.Empty;

        // Display
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }

    }
}