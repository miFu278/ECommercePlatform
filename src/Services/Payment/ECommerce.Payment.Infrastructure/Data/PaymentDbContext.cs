using ECommerce.Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Payment.Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<PaymentEntity> Payments { get; set; }
    public DbSet<PaymentHistory> PaymentHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for Payment service
        modelBuilder.HasDefaultSchema("payment");

        // Payment configuration
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PaymentNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.PaymentNumber)
                .IsUnique();

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.RefundedAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");

            entity.Property(e => e.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ProviderTransactionId)
                .HasMaxLength(100);

            entity.Property(e => e.ProviderPaymentId)
                .HasMaxLength(100);

            entity.Property(e => e.ProviderCustomerId)
                .HasMaxLength(100);

            entity.Property(e => e.CardLast4)
                .HasMaxLength(4);

            entity.Property(e => e.CardBrand)
                .HasMaxLength(50);

            entity.Property(e => e.ErrorCode)
                .HasMaxLength(50);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500);

            entity.Property(e => e.RefundReason)
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(100);

            entity.Property(e => e.CustomerName)
                .HasMaxLength(100);

            // Relationships
            entity.HasMany(e => e.History)
                .WithOne(e => e.Payment)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PaymentHistory configuration
        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            entity.ToTable("payment_history");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.ChangedAt);

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            entity.Property(e => e.ChangedBy)
                .HasMaxLength(100);
        });
    }
}
