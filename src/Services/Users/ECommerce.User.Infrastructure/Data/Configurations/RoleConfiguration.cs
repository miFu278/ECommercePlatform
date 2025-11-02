using ECommerce.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.User.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique constraint
        builder.HasIndex(r => r.Name).IsUnique();

        // Seed data - Use fixed date to avoid migration changes
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Role { Id = 1, Name = "Admin", Description = "Full system access", CreatedAt = seedDate },
            new Role { Id = 2, Name = "Manager", Description = "Product and order management", CreatedAt = seedDate },
            new Role { Id = 3, Name = "Customer", Description = "Standard customer access", CreatedAt = seedDate },
            new Role { Id = 4, Name = "Guest", Description = "Limited browsing access", CreatedAt = seedDate }
        );
    }
}
