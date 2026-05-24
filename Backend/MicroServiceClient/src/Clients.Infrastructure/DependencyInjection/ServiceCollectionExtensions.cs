using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taller_Mecanico_Clientes.Domain.Interfaces;
using Taller_Mecanico_Clientes.Infrastructure.Repositories;
using Taller_Mecanico_Clientes.Application.UseCases.Clientes;

namespace Taller_Mecanico_Clientes.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar Firebase
            var firebaseConfig = configuration.GetSection("Firebase");
            var projectId = firebaseConfig["ProjectId"];
            var credentialsPath = firebaseConfig["CredentialsPath"];

            GoogleCredential? credential = null;
            if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
            {
                credential = GoogleCredential.FromFile(credentialsPath);
            }

            if (FirebaseApp.DefaultInstance == null)
            {
                if (credential != null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = projectId
                    });
                }
                else
                {
                    // Usar variable de entorno GOOGLE_APPLICATION_CREDENTIALS o configuración predeterminada de Google
                    FirebaseApp.Create();
                }
            }

            // Registrar FirestoreDb
            services.AddSingleton(provider => 
            {
                var app = FirebaseApp.DefaultInstance ?? throw new InvalidOperationException("FirebaseApp no fue inicializado correctamente.");
                var firestoreBuilder = new FirestoreDbBuilder
                {
                    ProjectId = app.Options.ProjectId
                };
                
                if (credential != null)
                {
                    firestoreBuilder.Credential = credential;
                }

                return firestoreBuilder.Build();
            });

            // Registrar Repositorio
            services.AddScoped<IClienteRepository, ClienteRepository>();

            // Registrar Casos de Uso
            services.AddScoped<GetClientesUseCase>();
            services.AddScoped<GetClienteByIdUseCase>();
            services.AddScoped<CreateClienteUseCase>();
            services.AddScoped<UpdateClienteUseCase>();
            services.AddScoped<DeleteClienteUseCase>();

            return services;
        }
    }
}
