namespace Taller_Mecanico_Services.Application.DTOs
{
    public class ServicioCreateDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public int DuracionEstimadaMinutos { get; set; }
        public int CategoriaServicioId { get; set; }
    }

    public class ServicioUpdateDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public int DuracionEstimadaMinutos { get; set; }
        public int CategoriaServicioId { get; set; }
        public bool Estado { get; set; }
    }

    public class ServicioResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public int DuracionEstimadaMinutos { get; set; }
        public int CategoriaServicioId { get; set; }
        public string? CategoriaNombre { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
