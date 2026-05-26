using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebService.DTOs;

namespace WebService.Adapters
{
    public class EmpleadosAdapter : IEmpleadosAdapter
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        // Static list to mock Empleados database in-memory
        private static readonly List<EmpleadoDto> _mockEmpleados = new()
        {
            new EmpleadoDto
            {
                EmpleadoId = 1,
                Nombre = "Juan",
                PrimerApellido = "Perez",
                SegundoApellido = "Gomez",
                Ci = 1234567,
                CiComplemento = "",
                Telefono = 77777777,
                Email = "mecanico1@gmail.com",
                FechaContratacion = new DateTime(2025, 1, 15),
                TipoEmpleado = "Mecanico",
                EstadoLaboral = "Activo",
                Especialidad = "Motor",
                SalarioPorHora = 15.0m,
                SalarioMensual = 2500.0m,
                NivelAcceso = "Parcial"
            },
            new EmpleadoDto
            {
                EmpleadoId = 2,
                Nombre = "Carlos",
                PrimerApellido = "Ramos",
                SegundoApellido = "Lopez",
                Ci = 7654321,
                CiComplemento = "1F",
                Telefono = 66666666,
                Email = "admin@gmail.com",
                FechaContratacion = new DateTime(2024, 6, 10),
                TipoEmpleado = "Administrador",
                EstadoLaboral = "Activo",
                Especialidad = "General",
                SalarioPorHora = 25.0m,
                SalarioMensual = 4000.0m,
                NivelAcceso = "Completo"
            }
        };
        private static int _nextEmpleadoId = 3;

        public EmpleadosAdapter(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        // ─── Empleados CRUD (Mocked in-memory) ─────────────────────────────────

        public async Task<(bool ok, IEnumerable<EmpleadoDto>? empleados, string? error)> GetAllEmpleadosAsync()
        {
            await Task.Delay(50); // Simulate network latency
            return (true, _mockEmpleados.ToList(), null);
        }

        public async Task<(bool ok, EmpleadoDto? empleado, string? error)> GetEmpleadoByIdAsync(int id)
        {
            await Task.Delay(50);
            var emp = _mockEmpleados.FirstOrDefault(e => e.EmpleadoId == id);
            if (emp == null) return (false, null, "Empleado no encontrado.");
            return (true, emp, null);
        }

        public async Task<(bool ok, int? empleadoId, string? error)> CrearEmpleadoAsync(EmpleadoFormDto form)
        {
            await Task.Delay(50);
            var newId = System.Threading.Interlocked.Increment(ref _nextEmpleadoId);
            var emp = new EmpleadoDto
            {
                EmpleadoId = newId,
                Nombre = form.Nombres,
                PrimerApellido = form.PrimerApellido,
                SegundoApellido = form.SegundoApellido,
                Ci = form.CiNumero,
                CiComplemento = form.CiComplemento,
                Telefono = form.Telefono,
                Email = form.Email,
                FechaContratacion = form.FechaContratacion,
                TipoEmpleado = form.TipoEmpleado,
                EstadoLaboral = form.EstadoLaboral,
                Especialidad = form.Especialidad,
                SalarioPorHora = form.SalarioPorHora,
                SalarioMensual = form.SalarioMensual,
                NivelAcceso = form.TipoEmpleado == "Administrador" ? (form.NivelAcceso ?? "Completo") : "Parcial"
            };
            _mockEmpleados.Add(emp);
            return (true, newId, null);
        }

        public async Task<(bool ok, string? error)> ActualizarEmpleadoAsync(int id, EmpleadoFormDto form)
        {
            await Task.Delay(50);
            var emp = _mockEmpleados.FirstOrDefault(e => e.EmpleadoId == id);
            if (emp == null) return (false, "Empleado no encontrado.");

            emp.Nombre = form.Nombres;
            emp.PrimerApellido = form.PrimerApellido;
            emp.SegundoApellido = form.SegundoApellido;
            emp.Ci = form.CiNumero;
            emp.CiComplemento = form.CiComplemento;
            emp.Telefono = form.Telefono;
            emp.Email = form.Email;
            emp.FechaContratacion = form.FechaContratacion;
            emp.TipoEmpleado = form.TipoEmpleado;
            emp.EstadoLaboral = form.EstadoLaboral;
            emp.Especialidad = form.Especialidad;
            emp.SalarioPorHora = form.SalarioPorHora;
            emp.SalarioMensual = form.SalarioMensual;
            emp.NivelAcceso = form.TipoEmpleado == "Administrador" ? (form.NivelAcceso ?? "Completo") : "Parcial";

            return (true, null);
        }

        public async Task<(bool ok, string? error)> EliminarEmpleadoAsync(int id)
        {
            await Task.Delay(50);
            var emp = _mockEmpleados.FirstOrDefault(e => e.EmpleadoId == id);
            if (emp == null) return (false, "Empleado no encontrado.");
            _mockEmpleados.Remove(emp);
            return (true, null);
        }

        // ─── Usuarios/Cuentas (Conectados al Microservicio Real) ───────────────

        public async Task<(bool ok, IEnumerable<UsuarioDto>? usuarios, string? error)> GetAllUsuariosAsync()
        {
            try
            {
                var response = await SendAsync(HttpMethod.Get, "api/users");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var result = await DeserializeAsync<IEnumerable<UsuarioDto>>(response);
                return (true, result, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, UsuarioDto? usuario, string? error)> GetUsuarioByIdAsync(int id)
        {
            try
            {
                var response = await SendAsync(HttpMethod.Get, $"api/users/{id}");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var result = await DeserializeAsync<UsuarioDto>(response);
                return (true, result, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, UsuarioDto? usuario, string? error)> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return (false, null, "Correo de usuario inválido.");

                var (ok, usuarios, error) = await GetAllUsuariosAsync();
                if (!ok || usuarios == null)
                    return (false, null, error ?? "No se pudieron obtener los usuarios del backend.");

                var matchedUser = usuarios.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                if (matchedUser == null)
                    return (false, null, "No se encontró cuenta de usuario para este correo.");

                return (true, matchedUser, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error al buscar usuario por correo: {ex.Message}");
            }
        }

        public async Task<(bool ok, UsuarioDto? usuario, string? error)> GetUsuarioByEmpleadoIdAsync(int empleadoId)
        {
            try
            {
                // Buscar el empleado en el mock para obtener su email
                var emp = _mockEmpleados.FirstOrDefault(e => e.EmpleadoId == empleadoId);
                if (emp == null || string.IsNullOrWhiteSpace(emp.Email))
                    return (false, null, "Empleado no encontrado en el mock.");

                // Obtener todos los usuarios del backend
                var (ok, usuarios, error) = await GetAllUsuariosAsync();
                if (!ok || usuarios == null)
                    return (false, null, error ?? "No se pudieron obtener los usuarios del backend.");

                // Buscar el usuario que tiene el email del empleado
                var matchedUser = usuarios.FirstOrDefault(u => u.Email.Equals(emp.Email, StringComparison.OrdinalIgnoreCase));
                if (matchedUser == null)
                    return (false, null, "No se encontró cuenta de usuario para este empleado.");

                return (true, matchedUser, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error al asociar empleado con usuario: {ex.Message}");
            }
        }

        public async Task<(bool ok, string? plainPassword, string? createdEmail, IReadOnlyList<string>? notificationRecipients, string? error)> CreateUsuarioAsync(int empleadoId, string email, string? password)
        {
            try
            {
                // Registrar el usuario en el backend. Nota: request.Email y request.Password son pasados
                var body = new { email, password };
                var response = await SendAsync(HttpMethod.Post, "api/users", body);
                if (!response.IsSuccessStatusCode)
                    return (false, null, null, null, await ReadErrorAsync(response));

                var json = await response.Content.ReadAsStringAsync();
                string? plain = null;
                string? createdEmail = null;
                IReadOnlyList<string>? recipients = null;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("plainPassword", out var pw))
                        plain = pw.GetString();
                    if (doc.RootElement.TryGetProperty("email", out var createdEmailProperty) && createdEmailProperty.ValueKind == JsonValueKind.String)
                        createdEmail = createdEmailProperty.GetString();
                    if (doc.RootElement.TryGetProperty("notificationRecipients", out var recs) && recs.ValueKind == JsonValueKind.Array)
                    {
                        recipients = recs
                            .EnumerateArray()
                            .Where(e => e.ValueKind == JsonValueKind.String)
                            .Select(e => e.GetString())
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .Select(e => e!)
                            .ToList();
                    }
                }
                return (true, plain, createdEmail, recipients, null);
            }
            catch (Exception)
            {
                return (false, null, null, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? error)> UpdateUsuarioAsync(int id, string email, bool activo)
        {
            try
            {
                var body = new { email, activo };
                var response = await SendAsync(HttpMethod.Put, $"api/users/{id}", body);
                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? plainPassword, string? error)> ResetPasswordAsync(int id)
        {
            try
            {
                var response = await SendAsync(HttpMethod.Post, $"api/users/{id}/reset-password");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var json = await response.Content.ReadAsStringAsync();
                string? plain = null;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("plainPassword", out var pw))
                        plain = pw.GetString();
                }
                return (true, plain, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? error)> UpdateUsuarioRolAsync(int usuarioId, string rolNombre)
        {
            try
            {
                var body = new { rolNombre };
                var response = await SendAsync(HttpMethod.Put, $"api/users/{usuarioId}/rol", body);
                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        // ─── Helpers privados ─────────────────────────────────────────────────

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? body = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                token = _ctx.HttpContext?.User.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body is not null)
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json");

            return await _http.SendAsync(request);
        }

        private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOpts);
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                        return msg.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                    if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                        return err.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                }
            }
            catch { }
            return $"Error HTTP {(int)response.StatusCode}.";
        }
    }
}
