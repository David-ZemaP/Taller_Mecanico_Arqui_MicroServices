namespace Taller_Mecanico_Clientes.Domain.Common
{
    public static class ErrorCodes
    {
        public const string DbError = "DB_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string ValidationRequired = "VALIDATION_REQUIRED";
        public const string ValidationInvalidValue = "VALIDATION_INVALID_VALUE";
        public const string ClienteNotFound = "CLIENTE_NOT_FOUND";
        public const string ClienteEmailDuplicado = "CLIENTE_EMAIL_DUPLICADO";
    }
}
