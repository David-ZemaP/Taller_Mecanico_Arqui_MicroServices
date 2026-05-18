using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Interfaces;

namespace Taller_Mecanico_Users.Application.UseCases.Users
{
    public class GetUsersUseCase
    {
        private readonly IUsuarioLoginRepository _repository;

        public GetUsersUseCase(IUsuarioLoginRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UsuarioLogin>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}