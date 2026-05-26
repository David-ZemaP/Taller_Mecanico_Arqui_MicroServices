using Microsoft.EntityFrameworkCore;
using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Domain.Repositories;
using MicroServiceProduct.Infrastructure.Persistence;
using MicroServiceProduct.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure EF Core with MySQL (connection string in appsettings.json)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(conn))
{
    builder.Services.AddDbContext<ProductsDbContext>(options =>
        options.UseMySql(conn, ServerVersion.AutoDetect(conn)));
}

// Dependency injection for repository and services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Automatically apply EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Error al aplicar migraciones de base de datos en Productos.");
    }
}

// Configure the HTTP request pipeline.
// Always map OpenAPI/UI so the Swagger UI is available inside containers.
app.MapOpenApi();

app.UseHttpsRedirection();

// Serve static files (for the Swagger-like test UI)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
