using DotNetEnv;
using Microsoft.OpenApi;
using Taller_Mecanico_Services.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

LoadDotEnvIfPresent();
builder.Configuration.AddEnvironmentVariables();

// Registrar controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Services API",
        Version = "v1",
        Description = "Microservicio para el catálogo de categorías y servicios del Taller Mecánico."
    });
});

// Registrar todas las dependencias de Infraestructura, Aplicación y Dominio
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Services API v1");
});

app.UseAuthorization();
app.MapControllers();

app.Run();

static void LoadDotEnvIfPresent()
{
    var candidatePaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env")
    };

    foreach (var path in candidatePaths)
    {
        if (File.Exists(path))
        {
            Env.Load(path);
            break;
        }
    }
}
