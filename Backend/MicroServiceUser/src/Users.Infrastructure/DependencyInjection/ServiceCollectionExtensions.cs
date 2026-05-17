using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

// Application
using Taller_Mecanico_Users.Application.Interfaces;
using Taller_Mecanico_Users.Application.UseCases.Auth;
using Taller_Mecanico_Users.Application.UseCases.Users;
using Taller_Mecanico_Users.Application.UseCases.Roles;

// Domain
using Taller_Mecanico_Users.Domain.Interfaces;

// Infrastructure - Persistence
using Taller_Mecanico_Users.Infrastructure.Persistence;

// Infrastructure - Repositories
using Taller_Mecanico_Users.Infrastructure.Repositories;

// Infrastructure - Security
using Taller_Mecanico_Users.Infrastructure.Security;

// Infrastructure - Services
using Taller_Mecanico_Users.Infrastructure.Services;

namespace Taller_Mecanico_Users.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de la capa Infrastructure:
    /// - Persistencia PostgreSQL
    /// - Repositorios
    /// - Seguridad (JWT, BCrypt, Password Security)
    /// - Auditoría
    /// - Casos de uso
    /// - Configuración de autenticación JWT
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Método de extensión para registrar toda la infraestructura.
        /// Uso:
        /// builder.Services.AddInfrastructure(builder.Configuration);
        /// </summary>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ==========================================================
            // HttpContext (necesario para AuthenticationHelper)
            // ==========================================================
            services.AddHttpContextAccessor();

            // ==========================================================
            // Persistencia
            // ==========================================================
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

            // ==========================================================
            // Repositorios
            // ==========================================================
            services.AddScoped<IUsuarioLoginRepository, UsuarioLoginRepository>();
            services.AddScoped<IRolRepository, RolRepository>();

            // ==========================================================
            // Seguridad
            // ==========================================================
            services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<IPasswordSecurity, PasswordSecurityService>();

            // ==========================================================
            // JWT
            // ==========================================================
            services.AddSingleton<IJwtSettings, JwtSettings>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // ==========================================================
            // Servicios Transversales
            // ==========================================================
            services.AddScoped<IAuditService, AuditService>();
            services.AddSingleton<SmtpSettings>();
            services.AddScoped<IMailSender, SmtpMailSender>();

            // ==========================================================
            // Casos de Uso - Autenticación
            // ==========================================================
            services.AddScoped<LoginUseCase>();
            services.AddScoped<ChangePasswordUseCase>();
            services.AddScoped<ResetPasswordUseCase>();

            // ==========================================================
            // Casos de Uso - Usuarios
            // ==========================================================
            services.AddScoped<CreateUserUseCase>();
            services.AddScoped<GetUsersUseCase>();
            services.AddScoped<GetUserByIdUseCase>();
            services.AddScoped<UpdateUserUseCase>();
            services.AddScoped<DeleteUserUseCase>();

            // ==========================================================
            // Casos de Uso - Roles
            // ==========================================================
            services.AddScoped<GetRolesUseCase>();

            // Si luego creas GetRoleByIdUseCase, descomenta:
            // services.AddScoped<GetRoleByIdUseCase>();

            // ==========================================================
            // Configuración de autenticación JWT
            // ==========================================================
            ConfigureJwtAuthentication(services, configuration);

            return services;
        }

        /// <summary>
        /// Configura ASP.NET Core JWT Bearer Authentication.
        /// </summary>
        private static void ConfigureJwtAuthentication(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettingsSection = configuration.GetSection("JwtSettings");

            var secret = jwtSettingsSection["Secret"]
                ?? throw new InvalidOperationException(
                    "JwtSettings:Secret no está configurado.");

            var issuer = jwtSettingsSection["Issuer"]
                ?? throw new InvalidOperationException(
                    "JwtSettings:Issuer no está configurado.");

            var audience = jwtSettingsSection["Audience"]
                ?? throw new InvalidOperationException(
                    "JwtSettings:Audience no está configurado.");

            var key = Encoding.UTF8.GetBytes(secret);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(key),

                            ValidateIssuer = true,
                            ValidIssuer = issuer,

                            ValidateAudience = true,
                            ValidAudience = audience,

                            ValidateLifetime = true,

                            // Sin tolerancia extra al expirar el token
                            ClockSkew = TimeSpan.Zero
                        };
                });
        }
    }
}