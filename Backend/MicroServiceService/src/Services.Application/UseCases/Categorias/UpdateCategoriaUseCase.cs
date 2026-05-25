using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Application.DTOs;

namespace Taller_Mecanico_Services.Application.UseCases.Categorias
{
    public class UpdateCategoriaUseCase
    {
        private readonly ICategoriaServicioRepository _repository;

        public UpdateCategoriaUseCase(ICategoriaServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(int id, CategoriaServicioUpdateDTO dto)
        {
            if (dto == null)
            {
                return Result.Failure(ErrorCodes.ValidationRequired, "Los datos de actualización son requeridos.");
            }

            var categoria = await _repository.GetByIdAsync(id);
            if (categoria == null)
            {
                return Result.Failure(ErrorCodes.CategoriaNotFound, "La categoría especificada no existe.");
            }

            // Validar nombre duplicado con otra categoría diferente
            var existing = await _repository.GetByNombreAsync(dto.Nombre);
            if (existing != null && existing.Id != id && existing.Estado)
            {
                return Result.Failure(ErrorCodes.CategoriaNombreDuplicado, "Ya existe otra categoría activa con este nombre.");
            }

            // Modificar la entidad
            var modResult = categoria.Modificar(dto.Nombre, dto.Descripcion);
            if (modResult.IsFailure)
            {
                return modResult;
            }

            // Activar/Desactivar según corresponda
            if (dto.Estado && !categoria.Estado)
            {
                categoria.Activar();
            }
            else if (!dto.Estado && categoria.Estado)
            {
                // Si se va a desactivar, verificar que no tenga servicios activos asociados
                var hasActiveServices = await _repository.HasActiveServicesAsync(id);
                if (hasActiveServices)
                {
                    return Result.Failure(ErrorCodes.CategoriaTieneServiciosActivos, "No se puede desactivar la categoría porque tiene servicios activos asociados.");
                }
                categoria.Desactivar();
            }

            // Guardar cambios
            return await _repository.UpdateAsync(categoria);
        }
    }
}
