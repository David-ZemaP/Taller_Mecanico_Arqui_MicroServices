using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebService.DTOs;

namespace WebService.Adapters
{
    public class ClientesAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        // Concurrent dictionaries to map Firestore string IDs to integer IDs for the frontend
        private static readonly ConcurrentDictionary<int, string> _clientIntToString = new();
        private static readonly ConcurrentDictionary<string, int> _clientStringToInt = new();
        private static int _nextClientId = 0;

        public ClientesAdapter(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private static int GetOrAddIntId(string stringId)
        {
            if (string.IsNullOrEmpty(stringId)) return 0;
            return _clientStringToInt.GetOrAdd(stringId, id =>
            {
                var newId = Interlocked.Increment(ref _nextClientId);
                _clientIntToString[newId] = id;
                return newId;
            });
        }

        private static string? GetStringId(int intId)
        {
            return _clientIntToString.TryGetValue(intId, out var stringId) ? stringId : null;
        }

        private class BackendCliente
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
            public DateTime FechaRegistro { get; set; }
        }

        // --- Used by Clientes/Perfil.cshtml.cs ---
        public async Task<(bool ok, ClienteDto? cliente, string? error)> GetClienteByIdAsync(int id)
        {
            var clienteLookup = await GetClienteAsync(id);
            if (clienteLookup == null)
                return (false, null, "Cliente no encontrado.");

            var dto = new ClienteDto
            {
                ClienteId = clienteLookup.ClienteId,
                Email = clienteLookup.Email ?? string.Empty,
                Activo = true,
                UsuarioLoginId = 0
            };
            return (true, dto, null);
        }

        // --- Used by Clientes/Index.cshtml.cs ---
        public async Task<List<ClienteLookupDto>> GetAllClientesAsync()
        {
            try
            {
                var response = await SendAsync(HttpMethod.Get, "api/clientes");
                if (!response.IsSuccessStatusCode)
                    return new List<ClienteLookupDto>();

                var backendList = await DeserializeAsync<List<BackendCliente>>(response);
                if (backendList == null) return new List<ClienteLookupDto>();

                return backendList.Select(MapToLookupDto).ToList();
            }
            catch
            {
                return new List<ClienteLookupDto>();
            }
        }

        public async Task<ClienteLookupDto?> GetClienteAsync(int id)
        {
            try
            {
                var stringId = GetStringId(id);
                if (string.IsNullOrEmpty(stringId)) return null;

                var response = await SendAsync(HttpMethod.Get, $"api/clientes/{stringId}");
                if (!response.IsSuccessStatusCode) return null;

                var backend = await DeserializeAsync<BackendCliente>(response);
                return backend != null ? MapToLookupDto(backend) : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ClienteLookupDto>> BuscarClientesAsync(string term)
        {
            var all = await GetAllClientesAsync();
            if (string.IsNullOrWhiteSpace(term)) return all;

            var normalized = term.ToLowerInvariant();
            return all.Where(c => 
                c.Nombres.ToLowerInvariant().Contains(normalized) || 
                c.PrimerApellido.ToLowerInvariant().Contains(normalized) || 
                c.SegundoApellido?.ToLowerInvariant().Contains(normalized) == true ||
                c.CiNumero.ToString().Contains(normalized)
            ).ToList();
        }

        public async Task<List<VehiculoListDto>> GetVehiculosByClienteAsync(int clienteId)
        {
            await Task.Delay(10);
            // Return mock vehicles for this client for layout & list compatibility
            if (clienteId == 1)
            {
                return new List<VehiculoListDto>
                {
                    new VehiculoListDto
                    {
                        VehiculoId = 1,
                        Placa = "2345ABC",
                        ClienteId = 1,
                        ClienteNombre = "Juan Perez",
                        Anio = 2020,
                        MarcaNombre = "Toyota",
                        ModeloNombre = "Corolla",
                        ColorNombre = "Blanco"
                    }
                };
            }
            if (clienteId == 2)
            {
                return new List<VehiculoListDto>
                {
                    new VehiculoListDto
                    {
                        VehiculoId = 2,
                        Placa = "9876XYZ",
                        ClienteId = 2,
                        ClienteNombre = "Carlos Ramos",
                        Anio = 2018,
                        MarcaNombre = "Nissan",
                        ModeloNombre = "Sentra",
                        ColorNombre = "Negro"
                    }
                };
            }
            return new List<VehiculoListDto>();
        }

        public async Task<(bool ok, string? error, ClienteLookupDto? cliente)> SaveClienteAsync(ClienteFormDto form)
        {
            try
            {
                HttpResponseMessage response;
                var body = new
                {
                    nombre = form.Nombres,
                    primerApellido = form.PrimerApellido,
                    segundoApellido = form.SegundoApellido,
                    ci = form.CiNumero,
                    ciComplemento = form.CiComplemento,
                    telefono = form.Telefono,
                    email = form.Email
                };

                if (form.ClienteId == 0)
                {
                    response = await SendAsync(HttpMethod.Post, "api/clientes", body);
                }
                else
                {
                    var stringId = GetStringId(form.ClienteId);
                    if (string.IsNullOrEmpty(stringId))
                        return (false, "ID del cliente inválido o no encontrado.", null);

                    response = await SendAsync(HttpMethod.Put, $"api/clientes/{stringId}", new
                    {
                        id = stringId,
                        nombre = form.Nombres,
                        primerApellido = form.PrimerApellido,
                        segundoApellido = form.SegundoApellido,
                        ci = form.CiNumero,
                        ciComplemento = form.CiComplemento,
                        telefono = form.Telefono,
                        email = form.Email
                    });
                }

                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response), null);

                var backend = await DeserializeAsync<BackendCliente>(response);
                return (true, null, backend != null ? MapToLookupDto(backend) : null);
            }
            catch (Exception ex)
            {
                return (false, $"Error al conectar con el servicio de clientes: {ex.Message}", null);
            }
        }

        public async Task<(bool ok, string? error)> DeleteClienteAsync(int id)
        {
            try
            {
                var stringId = GetStringId(id);
                if (string.IsNullOrEmpty(stringId))
                    return (false, "ID del cliente inválido.");

                var response = await SendAsync(HttpMethod.Delete, $"api/clientes/{stringId}");
                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error al conectar con el servicio de clientes: {ex.Message}");
            }
        }

        // --- Helpers ---
        private ClienteLookupDto MapToLookupDto(BackendCliente backend)
        {
            return new ClienteLookupDto
            {
                ClienteId = GetOrAddIntId(backend.Id),
                Nombres = backend.Nombre,
                PrimerApellido = backend.PrimerApellido,
                SegundoApellido = backend.SegundoApellido,
                CiNumero = backend.Ci,
                CiComplemento = backend.CiComplemento,
                Telefono = backend.Telefono,
                Email = backend.Email,
                FechaRegistro = backend.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? data = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                token = _httpContextAccessor.HttpContext?.User.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }

        private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        private async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content)) return $"Error: {response.StatusCode}";
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? content;
                if (doc.RootElement.TryGetProperty("error", out var err))
                    return err.GetString() ?? content;
            }
            catch { }
            return content;
        }
    }
}
