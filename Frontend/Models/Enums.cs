namespace WebService.Models
{
    public enum NivelAcceso
    {
        Parcial,
        Completo,
        Gerente,
        Administrador,
        Mecanico,
        Cliente
    }

    public enum EstadoLaboral
    {
        Activo = 1,
        Inactivo = 2,
        Suspendido = 3
    }

    public enum TipoEmpleado
    {
        Mecanico = 1,
        Recepcionista = 2,
        Gerente = 3,
        Administrativo = 4
    }
}
