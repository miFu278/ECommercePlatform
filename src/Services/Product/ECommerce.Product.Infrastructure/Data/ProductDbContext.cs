using Microsoft.EntityFrameworkCore;

namespace ECommerce.Product.Infrastructure.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Product> Products { get; set; } = null!;
    public DbSet<Domain.Entities.Category> Categories { get; set; } = null!;
    public DbSet<Domain.Entities.Tag> Tags { get; set; } = null!;
    public DbSet<Domain.Entities.ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<Domain.Entities.ProductImage> ProductImages { get; set; } = null!;
    public DbSet<Domain.Entities.ProductAttribute> ProductAttributes { get; set; } = null!;
    public DbSet<Domain.Entities.ProductTag> ProductTags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
    }
}
