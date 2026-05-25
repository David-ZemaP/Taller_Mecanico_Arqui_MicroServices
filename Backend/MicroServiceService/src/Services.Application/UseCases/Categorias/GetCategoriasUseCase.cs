using Taller_Mecanico_Services.Application.DTOs;
using Taller_Mecanico_Services.Domain.Interfaces;

namespace Taller_Mecanico_Services.Application.UseCases.Categorias
{
    public class GetCategoriasUseCase
    {
        private readonly ICategoriaServicioRepository _repository;

        public GetCategoriasUseCase(ICategoriaServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoriaServicioResponseDTO>> ExecuteAsync(bool? estado = null, string? ordenarPor = null)
        {
            var categorias = await _repository.GetAllAsync(estado, ordenarPor);

            return categorias.Select(c => new CategoriaServicioResponseDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Estado = c.Estado,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            });
        }
    }
}
