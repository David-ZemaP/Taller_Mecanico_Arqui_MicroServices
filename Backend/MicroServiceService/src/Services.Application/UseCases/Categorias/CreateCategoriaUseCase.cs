using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Entities;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Application.DTOs;

namespace Taller_Mecanico_Services.Application.UseCases.Categorias
{
    public class CreateCategoriaUseCase
    {
        private readonly ICategoriaServicioRepository _repository;

        public CreateCategoriaUseCase(ICategoriaServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<int>> ExecuteAsync(CategoriaServicioCreateDTO dto)
        {
            if (dto == null)
            {
                return Result<int>.Failure(ErrorCodes.ValidationRequired, "Los datos de creación son requeridos.");
            }

            // Validar nombre duplicado
            var existing = await _repository.GetByNombreAsync(dto.Nombre);
            if (existing != null && existing.Estado)
            {
                return Result<int>.Failure(ErrorCodes.CategoriaNombreDuplicado, "Ya existe una categoría activa con este nombre.");
            }

            // Crear entidad de dominio
            var categoriaResult = CategoriaServicio.Crear(dto.Nombre, dto.Descripcion);
            if (categoriaResult.IsFailure)
            {
                return Result<int>.Failure(categoriaResult.ErrorCode!, categoriaResult.ErrorMessage!);
            }

            var categoria = categoriaResult.Value!;

            // Persistir en base de datos
            var addResult = await _repository.AddAsync(categoria);
            if (addResult.IsFailure)
            {
                return Result<int>.Failure(addResult.ErrorCode!, addResult.ErrorMessage!);
            }

            return Result<int>.Success(categoria.Id);
        }
    }
}
