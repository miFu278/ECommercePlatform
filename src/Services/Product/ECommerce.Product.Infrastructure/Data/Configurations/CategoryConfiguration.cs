using ECommerce.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Product.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(c => c.Slug).HasColumnName("slug").IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(500);

        // Hierarchy
        builder.Property(c => c.ParentId).HasColumnName("parent_id");

        // Display
        builder.Property(c => c.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(c => c.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
        builder.Property(c => c.IsVisible).HasColumnName("is_visible").HasDefaultValue(true);

        // SEO
        builder.Property(c => c.MetaTitle).HasColumnName("meta_title").HasMaxLength(255);
        builder.Property(c => c.MetaDescription).HasColumnName("meta_description").HasMaxLength(500);

        // Timestamps
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.ParentId);
        builder.HasIndex(c => c.IsVisible);

        // Self-referencing relationship
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
