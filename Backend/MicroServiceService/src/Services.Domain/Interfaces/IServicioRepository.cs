using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Entities;

namespace Taller_Mecanico_Services.Domain.Interfaces
{
    public interface IServicioRepository
    {
        Task<Result> AddAsync(Servicio servicio);
        Task<Result> UpdateAsync(Servicio servicio);
        Task<Servicio?> GetByIdAsync(int id);
        Task<bool> ExistsNombreInCategoriaAsync(string nombre, int categoriaId, int? excludeId = null);
        Task<IEnumerable<Servicio>> GetAllAsync(int? categoriaId = null, bool? estado = null, string? nombre = null, string? ordenarPor = null);
        Task<Result> DeleteAsync(int id);
    }
}
