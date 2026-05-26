using Google.Cloud.Firestore;
using Taller_Mecanico_Clientes.Domain.Entities;

namespace Taller_Mecanico_Clientes.Infrastructure.Persistence
{
    [FirestoreData]
    public class ClienteDocument
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("primerApellido")]
        public string PrimerApellido { get; set; } = string.Empty;

        [FirestoreProperty("segundoApellido")]
        public string? SegundoApellido { get; set; }

        [FirestoreProperty("ci")]
        public int Ci { get; set; }

        [FirestoreProperty("ciComplemento")]
        public string? CiComplemento { get; set; }

        [FirestoreProperty("telefono")]
        public int Telefono { get; set; }

        [FirestoreProperty("email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("usuarioLoginId")]
        public int? UsuarioLoginId { get; set; }

        [FirestoreProperty("creadoPor")]
        public string? CreadoPor { get; set; }

        [FirestoreProperty("actualizadoPor")]
        public string? ActualizadoPor { get; set; }

        [FirestoreProperty("eliminadoPor")]
        public string? EliminadoPor { get; set; }

        [FirestoreProperty("fechaActualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        [FirestoreProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [FirestoreProperty("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [FirestoreProperty("tipoCliente")]
        public string TipoCliente { get; set; } = "Regular";

        public Cliente ToEntity()
        {
            // Should never fail for valid Firestore data
            return Cliente.Reconstituir(
                id: this.Id,
                nombre: this.Nombre,
                primerApellido: this.PrimerApellido,
                segundoApellido: this.SegundoApellido,
                ci: this.Ci,
                ciComplemento: this.CiComplemento,
                telefono: this.Telefono,
                email: this.Email,
                usuarioLoginId: this.UsuarioLoginId,
                creadoPor: this.CreadoPor,
                actualizadoPor: this.ActualizadoPor,
                eliminadoPor: this.EliminadoPor,
                fechaActualizacion: this.FechaActualizacion,
                isDeleted: this.IsDeleted,
                fechaRegistro: this.FechaRegistro,
                tipoCliente: this.TipoCliente).Value!;
        }

        public static ClienteDocument FromEntity(Cliente entity)
        {
            return new ClienteDocument
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                PrimerApellido = entity.PrimerApellido,
                SegundoApellido = entity.SegundoApellido,
                Ci = entity.Ci,
                CiComplemento = entity.CiComplemento,
                Telefono = entity.Telefono,
                Email = entity.Email,
                UsuarioLoginId = entity.UsuarioLoginId,
                CreadoPor = entity.CreadoPor,
                ActualizadoPor = entity.ActualizadoPor,
                EliminadoPor = entity.EliminadoPor,
                FechaActualizacion = entity.FechaActualizacion,
                IsDeleted = entity.IsDeleted,
                FechaRegistro = entity.FechaRegistro,
                TipoCliente = entity.TipoCliente
            };
        }
    }
}
