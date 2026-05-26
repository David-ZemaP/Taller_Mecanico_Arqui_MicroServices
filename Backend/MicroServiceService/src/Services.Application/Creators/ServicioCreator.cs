using Taller_Mecanico_Services.Domain.Interfaces;

namespace Taller_Mecanico_Services.Application.Creators;

/// <summary>
/// ConcreteCreator del patrón Factory Method para Servicio.
/// Delega la implementación concreta al repositorio en Infrastructure.
/// </summary>
public class ServicioCreator
{
    protected readonly IServicioRepository Repository;

    public ServicioCreator(IServicioRepository repository)
    {
        Repository = repository;
    }
}
