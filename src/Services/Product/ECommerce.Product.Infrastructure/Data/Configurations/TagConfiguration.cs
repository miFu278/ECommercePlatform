using ECommerce.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Product.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
        builder.Property(t => t.Slug).HasColumnName("slug").IsRequired().HasMaxLength(50);

        // Indexes
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}
