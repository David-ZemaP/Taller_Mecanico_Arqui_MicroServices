using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Interfaces;

namespace Taller_Mecanico_Services.Application.UseCases.Servicios
{
    public class DeleteServicioUseCase
    {
        private readonly IServicioRepository _repository;

        public DeleteServicioUseCase(IServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(int id)
        {
            var servicio = await _repository.GetByIdAsync(id);
            if (servicio == null)
            {
                return Result.Failure(ErrorCodes.ServicioNotFound, "El servicio especificado no existe.");
            }

            // Realizar eliminación lógica (estado = false)
            return await _repository.DeleteAsync(id);
        }
    }
}
