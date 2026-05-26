using Taller_Mecanico_Users.Domain.Interfaces;

namespace Taller_Mecanico_Users.Application.Creators;

/// <summary>
/// ConcreteCreator del patrón Factory Method para UsuarioLogin.
/// Delega la implementación concreta al repositorio en Infrastructure.
/// </summary>
public class UsuarioCreator
{
    protected readonly IUsuarioLoginRepository Repository;

    public UsuarioCreator(IUsuarioLoginRepository repository)
    {
        Repository = repository;
    }
}
