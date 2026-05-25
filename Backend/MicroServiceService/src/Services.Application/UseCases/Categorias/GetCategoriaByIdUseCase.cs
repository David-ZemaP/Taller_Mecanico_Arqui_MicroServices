using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Application.DTOs;

namespace Taller_Mecanico_Services.Application.UseCases.Categorias
{
    public class GetCategoriaByIdUseCase
    {
        private readonly ICategoriaServicioRepository _repository;

        public GetCategoriaByIdUseCase(ICategoriaServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<CategoriaServicioResponseDTO>> ExecuteAsync(int id)
        {
            var categoria = await _repository.GetByIdAsync(id);
            if (categoria == null)
            {
                return Result<CategoriaServicioResponseDTO>.Failure(ErrorCodes.CategoriaNotFound, "La categoría especificada no fue encontrada.");
            }

            return Result<CategoriaServicioResponseDTO>.Success(new CategoriaServicioResponseDTO
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Estado = categoria.Estado,
                FechaCreacion = categoria.FechaCreacion,
                FechaModificacion = categoria.FechaModificacion
            });
        }
    }
}
