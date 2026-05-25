using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Entities;

namespace Taller_Mecanico_Services.Domain.Interfaces
{
    public interface ICategoriaServicioRepository
    {
        Task<Result> AddAsync(CategoriaServicio categoria);
        Task<Result> UpdateAsync(CategoriaServicio categoria);
        Task<CategoriaServicio?> GetByIdAsync(int id);
        Task<CategoriaServicio?> GetByNombreAsync(string nombre);
        Task<IEnumerable<CategoriaServicio>> GetAllAsync(bool? estado = null, string? ordenarPor = null);
        Task<bool> HasActiveServicesAsync(int categoriaId);
        Task<Result> DeleteAsync(int id);
    }
}
