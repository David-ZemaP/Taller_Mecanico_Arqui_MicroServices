using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Interfaces;

namespace Taller_Mecanico_Services.Application.UseCases.Categorias
{
    public class DeleteCategoriaUseCase
    {
        private readonly ICategoriaServicioRepository _repository;

        public DeleteCategoriaUseCase(ICategoriaServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(int id)
        {
            var categoria = await _repository.GetByIdAsync(id);
            if (categoria == null)
            {
                return Result.Failure(ErrorCodes.CategoriaNotFound, "La categoría especificada no existe.");
            }

            // Validar que no tenga servicios activos asociados
            var hasActiveServices = await _repository.HasActiveServicesAsync(id);
            if (hasActiveServices)
            {
                return Result.Failure(ErrorCodes.CategoriaTieneServiciosActivos, "No se puede eliminar la categoría porque tiene servicios activos asociados.");
            }

            // Realizar eliminación lógica (estado = false)
            return await _repository.DeleteAsync(id);
        }
    }
}
