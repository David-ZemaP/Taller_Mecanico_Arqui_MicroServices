using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Domain Interfaces
using Taller_Mecanico_Services.Domain.Interfaces;

// Infrastructure - Persistence
using Taller_Mecanico_Services.Infrastructure.Persistence;

// Infrastructure - Repositories
using Taller_Mecanico_Services.Infrastructure.Repositories;

// Application - UseCases
using Taller_Mecanico_Services.Application.UseCases.Categorias;
using Taller_Mecanico_Services.Application.UseCases.Servicios;

namespace Taller_Mecanico_Services.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Persistencia
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

            // Repositorios
            services.AddScoped<ICategoriaServicioRepository, CategoriaServicioRepository>();
            services.AddScoped<IServicioRepository, ServicioRepository>();

            // Casos de Uso - Categorías
            services.AddScoped<CreateCategoriaUseCase>();
            services.AddScoped<UpdateCategoriaUseCase>();
            services.AddScoped<GetCategoriasUseCase>();
            services.AddScoped<GetCategoriaByIdUseCase>();
            services.AddScoped<DeleteCategoriaUseCase>();

            // Casos de Uso - Servicios
            services.AddScoped<CreateServicioUseCase>();
            services.AddScoped<UpdateServicioUseCase>();
            services.AddScoped<GetServiciosUseCase>();
            services.AddScoped<GetServicioByIdUseCase>();
            services.AddScoped<DeleteServicioUseCase>();

            return services;
        }
    }
}
