using Taller_Mecanico_Clientes.Domain.Interfaces;

namespace Taller_Mecanico_Clientes.Application.Creators;

/// <summary>
/// ConcreteCreator del patrón Factory Method para Cliente.
/// Delega la implementación concreta a ClienteRepository en Infrastructure.
/// </summary>
public class ClienteCreator
{
    protected readonly IClienteRepository Repository;

    public ClienteCreator(IClienteRepository repository)
    {
        Repository = repository;
    }
}
