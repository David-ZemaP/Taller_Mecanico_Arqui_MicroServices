using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Entities;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Application.DTOs;

namespace Taller_Mecanico_Services.Application.UseCases.Servicios
{
    public class CreateServicioUseCase
    {
        private readonly IServicioRepository _repository;
        private readonly ICategoriaServicioRepository _categoriaRepository;

        public CreateServicioUseCase(IServicioRepository repository, ICategoriaServicioRepository categoriaRepository)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<Result<int>> ExecuteAsync(ServicioCreateDTO dto)
        {
            if (dto == null)
            {
                return Result<int>.Failure(ErrorCodes.ValidationRequired, "Los datos de creación son requeridos.");
            }

            // Validar que la categoría exista y esté activa
            var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaServicioId);
            if (categoria == null || !categoria.Estado)
            {
                return Result<int>.Failure(ErrorCodes.CategoriaNotFound, "La categoría de servicio seleccionada no existe o no está activa.");
            }

            // Validar nombre duplicado dentro de la misma categoría
            var isDuplicate = await _repository.ExistsNombreInCategoriaAsync(dto.Nombre, dto.CategoriaServicioId);
            if (isDuplicate)
            {
                return Result<int>.Failure(ErrorCodes.ServicioNombreDuplicado, "Ya existe un servicio activo con este nombre en la categoría seleccionada.");
            }

            // Crear entidad de dominio
            var servicioResult = Servicio.Crear(
                dto.Nombre,
                dto.Descripcion,
                dto.PrecioBase,
                dto.DuracionEstimadaMinutos,
                dto.CategoriaServicioId
            );

            if (servicioResult.IsFailure)
            {
                return Result<int>.Failure(servicioResult.ErrorCode!, servicioResult.ErrorMessage!);
            }

            var servicio = servicioResult.Value!;

            // Guardar en repositorio
            var addResult = await _repository.AddAsync(servicio);
            if (addResult.IsFailure)
            {
                return Result<int>.Failure(addResult.ErrorCode!, addResult.ErrorMessage!);
            }

            return Result<int>.Success(servicio.Id);
        }
    }
}
