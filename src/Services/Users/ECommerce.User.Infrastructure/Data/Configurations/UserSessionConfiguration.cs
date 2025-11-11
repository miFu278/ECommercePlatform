using ECommerce.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.User.Infrastructure.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.RefreshToken).HasColumnName("refresh_token").IsRequired().HasMaxLength(500);
        builder.Property(s => s.DeviceInfo).HasColumnName("device_info");
        builder.Property(s => s.IpAddress).HasColumnName("ip_address");
        builder.Property(s => s.UserAgent).HasColumnName("user_agent");
        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(s => s.LastAccessedAt).HasColumnName("last_accessed_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.RefreshToken).IsUnique();
        builder.HasIndex(s => s.ExpiresAt);

        // Relationships
        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(s => s.IsExpired);
        builder.Ignore(s => s.IsValid);
        
        // Session không cần UpdatedAt vì chỉ tạo và xóa, không update
        builder.Ignore(s => s.UpdatedAt);
    }
}
