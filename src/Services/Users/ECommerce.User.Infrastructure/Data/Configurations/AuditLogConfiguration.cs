using ECommerce.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.User.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.Action).HasColumnName("action").IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).HasColumnName("entity_type").HasMaxLength(100);
        builder.Property(a => a.EntityId).HasColumnName("entity_id").HasMaxLength(255);
        builder.Property(a => a.OldValues).HasColumnName("old_values");
        builder.Property(a => a.NewValues).HasColumnName("new_values");
        builder.Property(a => a.IpAddress).HasColumnName("ip_address");
        builder.Property(a => a.UserAgent).HasColumnName("user_agent");
        builder.Property(a => a.Timestamp).HasColumnName("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
