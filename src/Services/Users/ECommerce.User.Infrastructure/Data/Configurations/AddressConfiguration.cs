using ECommerce.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.User.Infrastructure.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(a => a.AddressType).HasColumnName("address_type").HasMaxLength(20).HasDefaultValue("shipping");
        builder.Property(a => a.StreetAddress).HasColumnName("street_address").IsRequired().HasMaxLength(255);
        builder.Property(a => a.Apartment).HasColumnName("apartment").HasMaxLength(50);
        builder.Property(a => a.City).HasColumnName("city").IsRequired().HasMaxLength(100);
        builder.Property(a => a.StateProvince).HasColumnName("state_province").HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasColumnName("postal_code").IsRequired().HasMaxLength(20);
        builder.Property(a => a.Country).HasColumnName("country").IsRequired().HasMaxLength(100);
        builder.Property(a => a.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.IsDefault).HasFilter("is_default = true");

        // Relationships
        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(a => a.FullAddress);
    }
}
