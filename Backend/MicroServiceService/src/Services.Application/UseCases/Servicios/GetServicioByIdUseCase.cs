using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Application.DTOs;

namespace Taller_Mecanico_Services.Application.UseCases.Servicios
{
    public class GetServicioByIdUseCase
    {
        private readonly IServicioRepository _repository;

        public GetServicioByIdUseCase(IServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ServicioResponseDTO>> ExecuteAsync(int id)
        {
            var s = await _repository.GetByIdAsync(id);
            if (s == null)
            {
                return Result<ServicioResponseDTO>.Failure(ErrorCodes.ServicioNotFound, "El servicio especificado no fue encontrado.");
            }

            return Result<ServicioResponseDTO>.Success(new ServicioResponseDTO
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
