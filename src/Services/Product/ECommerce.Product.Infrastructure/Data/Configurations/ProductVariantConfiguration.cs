using ECommerce.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Product.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");

        builder.Property(v => v.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(v => v.Sku).HasColumnName("sku").IsRequired().HasMaxLength(50);
        builder.Property(v => v.Name).HasColumnName("name").IsRequired().HasMaxLength(255);

        // Options
        builder.Property(v => v.Option1Name).HasColumnName("option1_name").IsRequired().HasMaxLength(50);
        builder.Property(v => v.Option1Value).HasColumnName("option1_value").IsRequired().HasMaxLength(100);
        builder.Property(v => v.Option2Name).HasColumnName("option2_name").HasMaxLength(50);
        builder.Property(v => v.Option2Value).HasColumnName("option2_value").HasMaxLength(100);
        builder.Property(v => v.Option3Name).HasColumnName("option3_name").HasMaxLength(50);
        builder.Property(v => v.Option3Value).HasColumnName("option3_value").HasMaxLength(100);

        // Pricing
        builder.Property(v => v.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
        builder.Property(v => v.CompareAtPrice).HasColumnName("compare_at_price").HasColumnType("decimal(18,2)");
        builder.Property(v => v.Cost).HasColumnName("cost").HasColumnType("decimal(18,2)");

        // Inventory
        builder.Property(v => v.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
        builder.Property(v => v.StockStatus).HasColumnName("stock_status").IsRequired();

        // Image
        builder.Property(v => v.ImageUrl).HasColumnName("image_url").HasMaxLength(500);

        // Status
        builder.Property(v => v.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(v => v.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);

        // Timestamps
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.HasIndex(v => v.ProductId);
        builder.HasIndex(v => v.IsActive);
    }
}
