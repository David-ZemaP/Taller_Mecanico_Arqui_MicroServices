using Taller_Mecanico_Services.Application.DTOs;
using Taller_Mecanico_Services.Domain.Interfaces;

namespace Taller_Mecanico_Services.Application.UseCases.Servicios
{
    public class GetServiciosUseCase
    {
        private readonly IServicioRepository _repository;

        public GetServiciosUseCase(IServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ServicioResponseDTO>> ExecuteAsync(int? categoriaId = null, bool? estado = null, string? nombre = null, string? ordenarPor = null)
        {
            var servicios = await _repository.GetAllAsync(categoriaId, estado, nombre, ordenarPor);

            return servicios.Select(s => new ServicioResponseDTO
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Descripcion = s.Descripcion,
                PrecioBase = s.PrecioBase,
                DuracionEstimadaMinutos = s.DuracionEstimadaMinutos,
                CategoriaServicioId = s.CategoriaServicioId,
                CategoriaNombre = s.Categoria?.Nombre,
                Estado = s.Estado,
                FechaCreacion = s.FechaCreacion,
                FechaModificacion = s.FechaModificacion
            });
        }
    }
}
