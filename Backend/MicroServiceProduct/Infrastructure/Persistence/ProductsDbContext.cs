using Microsoft.EntityFrameworkCore;
using MicroServiceProduct.Domain.Entities;

namespace MicroServiceProduct.Infrastructure.Persistence;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(eb =>
        {
            eb.HasKey(p => p.Id);
            eb.Property(p => p.Name).IsRequired().HasMaxLength(200);
            eb.Property(p => p.Description).HasMaxLength(1000);
            eb.Property(p => p.Price).HasColumnType("decimal(18,2)");
            eb.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
            eb.Property(p => p.CreatedAt).IsRequired();
            eb.Property(p => p.CreatedBy).HasMaxLength(150);
            eb.Property(p => p.IsDeleted).HasDefaultValue(false);
            eb.Property(p => p.DeletedAt).IsRequired(false);
            eb.Property(p => p.DeletedBy).HasMaxLength(150);
            // Global query filter to hide soft-deleted entities
            eb.HasQueryFilter(p => !p.IsDeleted);
        });
    }
}
