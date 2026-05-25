using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Application.DTOs;

namespace Taller_Mecanico_Services.Application.UseCases.Servicios
{
    public class UpdateServicioUseCase
    {
        private readonly IServicioRepository _repository;
        private readonly ICategoriaServicioRepository _categoriaRepository;

        public UpdateServicioUseCase(IServicioRepository repository, ICategoriaServicioRepository categoriaRepository)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<Result> ExecuteAsync(int id, ServicioUpdateDTO dto)
        {
            if (dto == null)
            {
                return Result.Failure(ErrorCodes.ValidationRequired, "Los datos de actualización son requeridos.");
            }

            var servicio = await _repository.GetByIdAsync(id);
            if (servicio == null)
            {
                return Result.Failure(ErrorCodes.ServicioNotFound, "El servicio especificado no existe.");
            }

            // Validar que la categoría exista y esté activa
            var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaServicioId);
            if (categoria == null || !categoria.Estado)
            {
                return Result.Failure(ErrorCodes.CategoriaNotFound, "La categoría de servicio seleccionada no existe o no está activa.");
            }

            // Validar duplicado de nombre excluyendo el servicio actual
            var isDuplicate = await _repository.ExistsNombreInCategoriaAsync(dto.Nombre, dto.CategoriaServicioId, id);
            if (isDuplicate)
            {
                return Result.Failure(ErrorCodes.ServicioNombreDuplicado, "Ya existe otro servicio activo con este nombre en la categoría seleccionada.");
            }

            // Modificar la entidad
            var modResult = servicio.Modificar(
                dto.Nombre,
                dto.Descripcion,
                dto.PrecioBase,
                dto.DuracionEstimadaMinutos,
                dto.CategoriaServicioId
            );

            if (modResult.IsFailure)
            {
                return modResult;
            }

            // Activar/Desactivar
            if (dto.Estado && !servicio.Estado)
            {
                servicio.Activar();
            }
            else if (!dto.Estado && servicio.Estado)
            {
                servicio.Desactivar();
            }

            // Guardar cambios
            return await _repository.UpdateAsync(servicio);
        }
    }
}
