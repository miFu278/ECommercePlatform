using ECommerce.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Product.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.Url).HasColumnName("url").IsRequired().HasMaxLength(500);
        builder.Property(i => i.AltText).HasColumnName("alt_text").HasMaxLength(255);
        builder.Property(i => i.Title).HasColumnName("title").HasMaxLength(255);

        // Storage
        builder.Property(i => i.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(255);
        builder.Property(i => i.FileSize).HasColumnName("file_size").IsRequired();
        builder.Property(i => i.MimeType).HasColumnName("mime_type").IsRequired().HasMaxLength(100);

        // Display
        builder.Property(i => i.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
        builder.Property(i => i.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);

        // Timestamps
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.IsPrimary);
    }
}
