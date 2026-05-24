namespace Taller_Mecanico_Clientes.Domain.Entities
{
    public class Cliente
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int Ci { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string Email { get; set; } = string.Empty;
        public int? UsuarioLoginId { get; set; }
        public string? CreadoPor { get; set; }
        public string? ActualizadoPor { get; set; }
        public string? EliminadoPor { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string TipoCliente { get; set; } = "Regular";
    }
}
