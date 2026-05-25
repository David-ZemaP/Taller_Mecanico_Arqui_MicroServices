namespace Taller_Mecanico_Services.Domain.Common
{
    public static class ErrorCodes
    {
        // Errores Generales / Infraestructura
        public const string DbError = "DB_ERROR";
        public const string DbInsertFailed = "DB_INSERT_FAILED";
        public const string ValidationRequired = "VALIDATION_REQUIRED";
        public const string ValidationDuplicateValue = "VALIDATION_DUPLICATE_VALUE";
        public const string ValidationInvalidValue = "VALIDATION_INVALID_VALUE";
        public const string NotFound = "NOT_FOUND";

        // Errores de Categoría de Servicio
        public const string CategoriaNotFound = "CATEGORIA_NOT_FOUND";
        public const string CategoriaTieneServiciosActivos = "CATEGORIA_TIENE_SERVICIOS_ACTIVOS";
        public const string CategoriaNombreDuplicado = "CATEGORIA_NOMBRE_DUPLICADO";

        // Errores de Servicio
        public const string ServicioNotFound = "SERVICIO_NOT_FOUND";
        public const string ServicioNombreDuplicado = "SERVICIO_NOMBRE_DUPLICADO";
        public const string ServicioPrecioInvalido = "SERVICIO_PRECIO_INVALIDO";
        public const string ServicioDuracionInvalida = "SERVICIO_DURACION_INVALIDA";
    }
}
