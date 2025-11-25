using ECommerce.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Order.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for Order service
        modelBuilder.HasDefaultSchema("order");

        // Order configuration
        modelBuilder.Entity<Domain.Entities.Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.OrderNumber)
                .IsUnique();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Subtotal)
                .HasPrecision(18, 2);

            entity.Property(e => e.ShippingCost)
                .HasPrecision(18, 2);

            entity.Property(e => e.Tax)
                .HasPrecision(18, 2);

            entity.Property(e => e.Discount)
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            entity.Property(e => e.ShippingFullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ShippingPhone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ShippingAddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.ShippingAddressLine2)
                .HasMaxLength(200);

            entity.Property(e => e.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ShippingState)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ShippingPostalCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ShippingCountry)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(100);

            entity.Property(e => e.PaymentTransactionId)
                .HasMaxLength(100);

            entity.Property(e => e.CancellationReason)
                .HasMaxLength(500);

            entity.Property(e => e.CustomerNotes)
                .HasMaxLength(500);

            entity.Property(e => e.AdminNotes)
                .HasMaxLength(500);

            // Relationships
            entity.HasMany(e => e.Items)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.StatusHistory)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);

            entity.Property(e => e.ProductId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Sku)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.Discount)
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalPrice)
                .HasPrecision(18, 2);
        });

        // OrderStatusHistory configuration
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("order_status_history");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ChangedAt);

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            entity.Property(e => e.ChangedBy)
                .HasMaxLength(100);
        });
    }
}
