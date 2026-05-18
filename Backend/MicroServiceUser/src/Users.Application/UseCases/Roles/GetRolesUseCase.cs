using System.Collections.Generic;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Interfaces;

namespace Taller_Mecanico_Users.Application.UseCases.Roles
{
    /// <summary>
    /// Caso de uso para obtener todos los roles del sistema.
    /// </summary>
    public class GetRolesUseCase
    {
        private readonly IRolRepository _rolRepository;

        public GetRolesUseCase(IRolRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        public async Task<IEnumerable<Rol>> ExecuteAsync()
        {
            return await _rolRepository.GetAllAsync();
        }
    }
}