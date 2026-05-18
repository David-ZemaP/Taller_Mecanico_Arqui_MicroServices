using System;
using Microsoft.Extensions.Configuration;

namespace Taller_Mecanico_Users.Infrastructure.Security
{
    /// <summary>
    /// Lectura de configuración SMTP desde variables de entorno con fallback a appsettings.json.
    ///
    /// IMPORTANTE:
    /// - Si SMTP está deshabilitado (Enabled = false), NO se requiere que Host esté configurado.
    /// - Esto evita que la aplicación falle al arrancar cuando el envío de correos es opcional.
    /// </summary>
    public class SmtpSettings
    {
        public bool Enabled { get; }

        public string Host { get; }

        public int Port { get; }

        public string? Username { get; }

        public string? Password { get; }

        public string From { get; }

        public bool EnableSsl { get; }

        public int TimeoutMs { get; }

        public SmtpSettings(IConfiguration configuration)
        {
            // 1. Determinar si SMTP está habilitado
            Enabled =
                GetBoolEnv("SMTP_ENABLED")
                ?? (bool.TryParse(configuration["Smtp:Enabled"], out var enabled) && enabled);

            // 2. Host SMTP
            // Si SMTP está deshabilitado, permitimos un valor vacío para evitar excepciones.
            var host =
                Environment.GetEnvironmentVariable("SMTP_HOST")
                ?? configuration["Smtp:Host"];

            if (Enabled && string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException(
                    "SMTP está habilitado pero no se configuró Smtp:Host o SMTP_HOST.");
            }

            Host = host ?? string.Empty;

            // 3. Puerto SMTP
            Port =
                GetIntEnv("SMTP_PORT")
                ?? (int.TryParse(configuration["Smtp:Port"], out var port) ? port : 25);

            // 4. Credenciales opcionales
            Username =
                Environment.GetEnvironmentVariable("SMTP_USERNAME")
                ?? configuration["Smtp:Username"];

            Password =
                Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                ?? configuration["Smtp:Password"];

            // 5. Dirección remitente
            From =
                Environment.GetEnvironmentVariable("SMTP_FROM")
                ?? configuration["Smtp:From"]
                ?? Username
                ?? "no-reply@example.com";

            // 6. SSL/TLS
            EnableSsl =
                GetBoolEnv("SMTP_ENABLESSL")
                ?? (bool.TryParse(configuration["Smtp:EnableSsl"], out var ssl) && ssl);

            // 7. Timeout
            TimeoutMs =
                GetIntEnv("SMTP_TIMEOUTMS")
                ?? (int.TryParse(configuration["Smtp:TimeoutMs"], out var timeout)
                    ? timeout
                    : 100000);
        }

        private static bool? GetBoolEnv(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return bool.TryParse(value, out var result)
                ? result
                : null;
        }

        private static int? GetIntEnv(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return int.TryParse(value, out var result)
                ? result
                : null;
        }
    }
}






