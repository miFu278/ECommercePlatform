using ECommerce.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Product.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Domain.Entities.Product>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        // Identity
        builder.Property(p => p.Sku).HasColumnName("sku").IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
        builder.Property(p => p.Slug).HasColumnName("slug").IsRequired().HasMaxLength(255);

        // Description
        builder.Property(p => p.ShortDescription).HasColumnName("short_description").HasMaxLength(500);
        builder.Property(p => p.LongDescription).HasColumnName("long_description").HasColumnType("text");

        // Pricing
        builder.Property(p => p.Price).HasColumnName("price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.CompareAtPrice).HasColumnName("compare_at_price").HasColumnType("decimal(18,2)");
        builder.Property(p => p.Cost).HasColumnName("cost").HasColumnType("decimal(18,2)");

        // Category
        builder.Property(p => p.CategoryId).HasColumnName("category_id").IsRequired();

        // Brand
        builder.Property(p => p.Brand).HasColumnName("brand").HasMaxLength(100);

        // Status
        builder.Property(p => p.Status).HasColumnName("status").IsRequired();
        builder.Property(p => p.IsVisible).HasColumnName("is_visible").HasDefaultValue(true);
        builder.Property(p => p.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);

        // SEO
        builder.Property(p => p.MetaTitle).HasColumnName("meta_title").HasMaxLength(255);
        builder.Property(p => p.MetaDescription).HasColumnName("meta_description").HasMaxLength(500);
        builder.Property(p => p.MetaKeywords).HasColumnName("meta_keywords").HasMaxLength(255);

        // Inventory
        builder.Property(p => p.TrackInventory).HasColumnName("track_inventory").HasDefaultValue(true);
        builder.Property(p => p.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
        builder.Property(p => p.LowStockThreshold).HasColumnName("low_stock_threshold").HasDefaultValue(10);
        builder.Property(p => p.StockStatus).HasColumnName("stock_status").IsRequired();

        // Shipping
        builder.Property(p => p.Weight).HasColumnName("weight").HasColumnType("decimal(10,2)").HasDefaultValue(0);
        builder.Property(p => p.Length).HasColumnName("length").HasColumnType("decimal(10,2)");
        builder.Property(p => p.Width).HasColumnName("width").HasColumnType("decimal(10,2)");
        builder.Property(p => p.Height).HasColumnName("height").HasColumnType("decimal(10,2)");

        // Timestamps
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(p => p.UpdatedBy).HasColumnName("updated_by");

        // Indexes
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsVisible);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.DeletedAt);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Attributes)
            .WithOne(a => a.Product)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
