using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Persistence;

public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProductCatalogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
