using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MicroServiceProduct.Infrastructure.Persistence;

public class ProductsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        var config = builder.Build();
        var conn = config.GetConnectionString("DefaultConnection") ?? "Server=localhost;Port=3306;Database=products_db;User=root;Password=changeme;";

        var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
        optionsBuilder.UseMySql(conn, ServerVersion.AutoDetect(conn));

        return new ProductsDbContext(optionsBuilder.Options);
    }
}
