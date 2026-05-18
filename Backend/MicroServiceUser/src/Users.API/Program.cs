using DotNetEnv;
using Microsoft.OpenApi;
using Taller_Mecanico_Users.API.Middleware;
using Taller_Mecanico_Users.Infrastructure.DependencyInjection;
using Taller_Mecanico_Users.Domain.Interfaces;
using Taller_Mecanico_Users.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

LoadDotEnvIfPresent();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega el token así: Bearer {tu_token}"
    });
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed default admin user (if necessary)
await SeedDefaultAdminAsync(app.Services);

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseRequirePasswordChange();
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

static async Task SeedDefaultAdminAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    try
    {
        var rolRepo = scope.ServiceProvider.GetRequiredService<IRolRepository>();

        var gerente = await rolRepo.GetByNombreAsync("Gerente");
        if (gerente == null)
        {
            logger?.LogWarning("Rol 'Gerente' no encontrado. Seed de administrador omitido.");
            return;
        }

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();

        const string adminEmail = "administrador.principal@taller.com";
        const string adminPlainPassword = "ap100000";

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Verificar existencia
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM usuariologin WHERE LOWER(email) = LOWER(@Email) LIMIT 1;";
        var pEmail = checkCmd.CreateParameter();
        pEmail.ParameterName = "@Email";
        pEmail.Value = adminEmail;
        checkCmd.Parameters.Add(pEmail);

        var exists = false;
        using (var reader = await ((System.Data.Common.DbCommand)checkCmd).ExecuteReaderAsync())
        {
            if (await reader.ReadAsync()) exists = true;
        }

        if (exists)
        {
            logger?.LogInformation("Administrador ya existe. Seed omitido.");
            return;
        }

        // Insertar usuario administrador directamente en la tabla usuariologin
        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO usuariologin
                (email, passwordhash, rolid, ultimoacceso, activo, requierecambiopassword, fecharegistro, fechaactualizacion, creadopor, actualizadopor, inactivadopor)
            VALUES
                (@Email, @PasswordHash, @RolId, @UltimoAcceso, @Activo, @ReqCambio, @FechaRegistro, @FechaActualizacion, @CreadoPor, @ActualizadoPor, @Inactivadopor);
        ";

        var passwordHash = passwordHasher.HashPassword(adminPlainPassword);

        void AddParam(System.Data.IDbCommand cmd, string name, object? value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? System.DBNull.Value;
            cmd.Parameters.Add(param);
        }

        AddParam(insertCmd, "@Email", adminEmail);
        AddParam(insertCmd, "@PasswordHash", passwordHash);
        AddParam(insertCmd, "@RolId", gerente.RolId);
        AddParam(insertCmd, "@UltimoAcceso", null);
        AddParam(insertCmd, "@Activo", true);
        AddParam(insertCmd, "@ReqCambio", false);
        AddParam(insertCmd, "@FechaRegistro", DateTime.UtcNow);
        AddParam(insertCmd, "@FechaActualizacion", null);
        AddParam(insertCmd, "@CreadoPor", "system");
        AddParam(insertCmd, "@ActualizadoPor", null);
        AddParam(insertCmd, "@Inactivadopor", null);

        var rows = await ((System.Data.Common.DbCommand)insertCmd).ExecuteNonQueryAsync();
        logger?.LogInformation("Seed administrador completado. Filas insertadas: {rows}", rows);
    }
    catch (Exception ex)
    {
        var fallbackLogger = services.GetService<ILogger<Program>>();
        fallbackLogger?.LogError(ex, "Error durante SeedDefaultAdminAsync");
    }
}
