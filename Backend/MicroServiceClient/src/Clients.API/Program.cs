using Taller_Mecanico_Clientes.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Registrar toda la infraestructura (incluyendo Firebase y Casos de Uso)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
