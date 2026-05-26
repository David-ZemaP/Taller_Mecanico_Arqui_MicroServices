using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebService.DTOs;

namespace WebService.Adapters
{
    public class OrdenTrabajoAdapter : IOrdenTrabajoAdapter
    {
        private readonly HttpClient _httpProducts;
        private readonly HttpClient _httpServices;
        private readonly IClientesAdapter _clientesAdapter;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // --- Mock Data store for Vehicles, Work Orders, Catalogs ---
        private static readonly List<VehiculoListDto> _mockVehiculos = new()
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
            },
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
        private static int _nextVehiculoId = 3;

        private static readonly List<MarcaDto> _mockMarcas = new()
        {
            new() { MarcaId = 1, Nombre = "Toyota" },
            new() { MarcaId = 2, Nombre = "Nissan" },
            new() { MarcaId = 3, Nombre = "Suzuki" },
            new() { MarcaId = 4, Nombre = "Hyundai" }
        };
        private static readonly List<ModeloDto> _mockModelos = new()
        {
            new() { ModeloId = 1, MarcaId = 1, Nombre = "Corolla" },
            new() { ModeloId = 2, MarcaId = 1, Nombre = "Hilux" },
            new() { ModeloId = 3, MarcaId = 2, Nombre = "Sentra" },
            new() { ModeloId = 4, MarcaId = 2, Nombre = "Frontier" },
            new() { ModeloId = 5, MarcaId = 3, Nombre = "Swift" },
            new() { ModeloId = 6, MarcaId = 4, Nombre = "Tucson" }
        };
        private static readonly List<ColorVehiculoDto> _mockColores = new()
        {
            new() { ColorVehiculoId = 1, Nombre = "Rojo" },
            new() { ColorVehiculoId = 2, Nombre = "Azul" },
            new() { ColorVehiculoId = 3, Nombre = "Blanco" },
            new() { ColorVehiculoId = 4, Nombre = "Negro" },
            new() { ColorVehiculoId = 5, Nombre = "Gris" }
        };

        private static readonly List<OrdenTrabajoListDto> _mockOrdenes = new()
        {
            new OrdenTrabajoListDto
            {
                OrdenTrabajoId = 1,
                VehiculoId = 1,
                VehiculoPlaca = "2345ABC",
                FechaIngreso = DateTime.Today.AddDays(-2),
                EstadoTrabajo = "EnReparacion",
                EstadoPago = "Pendiente",
                EstadoVehiculo = "Bueno",
                Total = 250.0
            }
        };
        private static readonly List<OrdenTrabajoDetalleDto> _mockOrdenDetalles = new()
        {
            new OrdenTrabajoDetalleDto
            {
                OrdenTrabajoId = 1,
                ClienteId = 1,
                ClienteCi = "1234567",
                ClienteNombre = "Juan Perez",
                VehiculoId = 1,
                Placa = "2345ABC",
                FechaIngreso = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                EstadoTrabajo = "EnReparacion",
                EstadoPago = "Pendiente",
                EstadoVehiculo = "Bueno",
                Total = 250.0,
                Productos = new()
                {
                    new() { ProductoId = 1, Nombre = "Filtro de Aceite", Cantidad = 1, PrecioUnitario = 50.0, Subtotal = 50.0 }
                },
                Servicios = new()
                {
                    new() { ServicioId = 1, Nombre = "Cambio de Aceite", Cantidad = 1, PrecioUnitario = 200.0, Subtotal = 200.0 }
                }
            }
        };
        private static int _nextOrdenId = 2;

        // --- Product ID Mapping Concurrent Dictionaries ---
        private static readonly ConcurrentDictionary<int, Guid> _productIntToGuid = new();
        private static readonly ConcurrentDictionary<Guid, int> _productGuidToInt = new();
        private static int _nextProductId = 0;

        private static int GetOrAddProductIntId(Guid guid)
        {
            return _productGuidToInt.GetOrAdd(guid, id =>
            {
                var newId = Interlocked.Increment(ref _nextProductId);
                _productIntToGuid[newId] = id;
                return newId;
            });
        }

        private static Guid GetProductGuid(int intId)
        {
            return _productIntToGuid.TryGetValue(intId, out var guid) ? guid : Guid.Empty;
        }

        // --- DTO models for backend APIs ---
        private class BackendProduct
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }

        private class BackendService
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public decimal PrecioBase { get; set; }
            public int CategoriaServicioId { get; set; }
        }

        public OrdenTrabajoAdapter(IHttpClientFactory httpClientFactory, IClientesAdapter clientesAdapter, IHttpContextAccessor ctx)
        {
            _httpProducts = httpClientFactory.CreateClient("ProductsApi");
            _httpServices = httpClientFactory.CreateClient("ServicesApi");
            _clientesAdapter = clientesAdapter;
            _ctx = ctx;
        }

        // ─── Órdenes de Trabajo (Mocked in-memory) ─────────────────────────────

        public async Task<List<OrdenTrabajoListDto>> GetAllOrdenesAsync()
        {
            await Task.Delay(50);
            return _mockOrdenes.Where(o => !o.IsDeleted).ToList();
        }

        public async Task<OrdenTrabajoDetalleDto?> GetOrdenDetalleAsync(int id)
        {
            await Task.Delay(50);
            return _mockOrdenDetalles.FirstOrDefault(o => o.OrdenTrabajoId == id && !o.IsDeleted);
        }

        public async Task<List<OrdenTrabajoListDto>> GetOrdenesByMecanicoAsync(int mecanicoId)
        {
            await Task.Delay(50);
            // Since it's mocked, we return all active work orders for simulation
            return _mockOrdenes.Where(o => !o.IsDeleted).ToList();
        }

        public async Task<(bool ok, string? error, int id)> RegistrarOrdenAsync(OrdenTrabajoFormDto form)
        {
            await Task.Delay(50);
            var newId = Interlocked.Increment(ref _nextOrdenId);

            var vehiculo = _mockVehiculos.FirstOrDefault(v => v.VehiculoId == form.VehiculoId);
            var vehiculoPlaca = vehiculo?.Placa ?? "Desconocido";
            var clienteNombre = vehiculo?.ClienteNombre ?? "Desconocido";

            var listDto = new OrdenTrabajoListDto
            {
                OrdenTrabajoId = newId,
                VehiculoId = form.VehiculoId,
                VehiculoPlaca = vehiculoPlaca,
                FechaIngreso = form.FechaIngreso,
                EstadoTrabajo = form.EstadoTrabajo,
                EstadoPago = form.EstadoPago,
                EstadoVehiculo = form.EstadoVehiculo,
                Total = form.Total
            };

            var detailDto = new OrdenTrabajoDetalleDto
            {
                OrdenTrabajoId = newId,
                ClienteId = form.ClienteId,
                ClienteNombre = clienteNombre,
                VehiculoId = form.VehiculoId,
                Placa = vehiculoPlaca,
                FechaIngreso = form.FechaIngreso.ToString("yyyy-MM-dd"),
                EstadoTrabajo = form.EstadoTrabajo,
                EstadoPago = form.EstadoPago,
                EstadoVehiculo = form.EstadoVehiculo,
                Total = form.Total
            };

            // Map and resolve product details
            foreach (var p in form.Productos)
            {
                var prod = await GetProductoAsync(p.ProductoId);
                detailDto.Productos.Add(new OrdenTrabajoDetalleProductoDto
                {
                    ProductoId = p.ProductoId,
                    Nombre = prod?.Nombre ?? $"Producto #{p.ProductoId}",
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario ?? prod?.Precio ?? 0.0,
                    Subtotal = p.Cantidad * (p.PrecioUnitario ?? prod?.Precio ?? 0.0)
                });
            }

            // Map and resolve service details
            foreach (var s in form.Servicios)
            {
                var svc = await GetServicioAsync(s.ServicioId);
                detailDto.Servicios.Add(new OrdenTrabajoDetalleServicioDto
                {
                    ServicioId = s.ServicioId,
                    Nombre = svc?.Nombre ?? $"Servicio #{s.ServicioId}",
                    Cantidad = s.Cantidad,
                    PrecioUnitario = s.PrecioUnitario ?? svc?.Precio ?? 0.0,
                    Subtotal = s.Cantidad * (s.PrecioUnitario ?? svc?.Precio ?? 0.0)
                });
            }

            _mockOrdenes.Add(listDto);
            _mockOrdenDetalles.Add(detailDto);

            return (true, null, newId);
        }

        public async Task<(bool ok, string? error)> ActualizarOrdenAsync(OrdenTrabajoFormDto form)
        {
            await Task.Delay(50);
            var ord = _mockOrdenes.FirstOrDefault(o => o.OrdenTrabajoId == form.OrdenTrabajoId);
            var det = _mockOrdenDetalles.FirstOrDefault(o => o.OrdenTrabajoId == form.OrdenTrabajoId);
            if (ord == null || det == null) return (false, "Orden de trabajo no encontrada.");

            ord.FechaIngreso = form.FechaIngreso;
            ord.FechaEntrega = form.FechaEntrega;
            ord.EstadoTrabajo = form.EstadoTrabajo;
            ord.EstadoPago = form.EstadoPago;
            ord.EstadoVehiculo = form.EstadoVehiculo;
            ord.Total = form.Total;

            det.FechaIngreso = form.FechaIngreso.ToString("yyyy-MM-dd");
            det.FechaEntrega = form.FechaEntrega?.ToString("yyyy-MM-dd");
            det.EstadoTrabajo = form.EstadoTrabajo;
            det.EstadoPago = form.EstadoPago;
            det.EstadoVehiculo = form.EstadoVehiculo;
            det.Total = form.Total;

            return (true, null);
        }

        public async Task<(bool ok, string? error)> AnularOrdenAsync(int id)
        {
            await Task.Delay(50);
            var ord = _mockOrdenes.FirstOrDefault(o => o.OrdenTrabajoId == id);
            var det = _mockOrdenDetalles.FirstOrDefault(o => o.OrdenTrabajoId == id);
            if (ord != null) ord.IsDeleted = true;
            if (det != null) det.IsDeleted = true;
            return (true, null);
        }

        public async Task<(bool ok, string? error)> RestaurarOrdenAsync(int id)
        {
            await Task.Delay(50);
            var ord = _mockOrdenes.FirstOrDefault(o => o.OrdenTrabajoId == id);
            var det = _mockOrdenDetalles.FirstOrDefault(o => o.OrdenTrabajoId == id);
            if (ord != null) ord.IsDeleted = false;
            if (det != null) det.IsDeleted = false;
            return (true, null);
        }

        public async Task<List<VehiculoLookupDto>> BuscarVehiculosAsync(string term, int? clienteId = null)
        {
            await Task.Delay(50);
            var query = _mockVehiculos.AsQueryable();
            if (clienteId.HasValue)
                query = query.Where(v => v.ClienteId == clienteId.Value);

            if (!string.IsNullOrWhiteSpace(term))
            {
                var norm = term.ToLowerInvariant();
                query = query.Where(v => v.Placa.ToLowerInvariant().Contains(norm) ||
                                         v.MarcaNombre.ToLowerInvariant().Contains(norm) ||
                                         v.ModeloNombre.ToLowerInvariant().Contains(norm));
            }

            return query.Select(v => new VehiculoLookupDto
            {
                Id = v.VehiculoId,
                Text = $"{v.Placa} - {v.MarcaNombre} {v.ModeloNombre} ({v.ColorNombre})"
            }).ToList();
        }

        // ─── Clientes (Delegated to ClientesAdapter) ───────────────────────────

        public Task<List<ClienteLookupDto>> GetAllClientesAsync()
            => _clientesAdapter.GetAllClientesAsync();

        public Task<List<ClienteLookupDto>> BuscarClientesAsync(string term)
            => _clientesAdapter.BuscarClientesAsync(term);

        public Task<ClienteLookupDto?> GetClienteAsync(int id)
            => _clientesAdapter.GetClienteAsync(id);

        public async Task<List<VehiculoListDto>> GetVehiculosByClienteAsync(int clienteId)
        {
            await Task.Delay(50);
            return _mockVehiculos.Where(v => v.ClienteId == clienteId).ToList();
        }

        public Task<(bool ok, string? error)> DeleteClienteAsync(int id)
            => _clientesAdapter.DeleteClienteAsync(id);

        public Task<(bool ok, string? error, ClienteLookupDto? cliente)> SaveClienteAsync(ClienteFormDto form)
            => _clientesAdapter.SaveClienteAsync(form);

        // ─── Productos (Connected to MicroServiceProduct backend) ─────────────────

        public async Task<List<ProductoDto>> GetAllProductosAsync()
        {
            try
            {
                var response = await SendAsync(_httpProducts, HttpMethod.Get, "api/products");
                if (!response.IsSuccessStatusCode) return new List<ProductoDto>();

                var backendList = await DeserializeAsync<List<BackendProduct>>(response);
                if (backendList == null) return new List<ProductoDto>();

                return backendList.Select(p => new ProductoDto
                {
                    ProductoId = GetOrAddProductIntId(p.Id),
                    Nombre = p.Name,
                    Precio = (double)p.Price,
                    Stock = p.Stock
                }).ToList();
            }
            catch
            {
                return new List<ProductoDto>();
            }
        }

        public async Task<ProductoDto?> GetProductoAsync(int id)
        {
            try
            {
                var guid = GetProductGuid(id);
                if (guid == Guid.Empty) return null;

                var response = await SendAsync(_httpProducts, HttpMethod.Get, $"api/products/{guid}");
                if (!response.IsSuccessStatusCode) return null;

                var p = await DeserializeAsync<BackendProduct>(response);
                if (p == null) return null;

                return new ProductoDto
                {
                    ProductoId = id,
                    Nombre = p.Name,
                    Precio = (double)p.Price,
                    Stock = p.Stock
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool ok, string? error)> SaveProductoAsync(ProductoFormDto form)
        {
            try
            {
                HttpResponseMessage response;
                var body = new
                {
                    name = form.Nombre,
                    description = form.Nombre,
                    price = (decimal)form.Precio,
                    stock = form.Stock
                };

                if (form.ProductoId == 0)
                {
                    response = await SendAsync(_httpProducts, HttpMethod.Post, "api/products", body);
                    if (response.IsSuccessStatusCode)
                    {
                        var createdProduct = await DeserializeAsync<BackendProduct>(response);
                        if (createdProduct != null)
                        {
                            GetOrAddProductIntId(createdProduct.Id);
                        }
                    }
                }
                else
                {
                    var guid = GetProductGuid(form.ProductoId);
                    if (guid == Guid.Empty) return (false, "ID de producto inválido.");

                    response = await SendAsync(_httpProducts, HttpMethod.Put, $"api/products/{guid}", body);
                }

                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión con servicio de productos: {ex.Message}");
            }
        }

        public async Task DeleteProductoAsync(int id)
        {
            try
            {
                var guid = GetProductGuid(id);
                if (guid == Guid.Empty) return;

                await SendAsync(_httpProducts, HttpMethod.Delete, $"api/products/{guid}");
            }
            catch {}
        }

        // ─── Servicios (Connected to MicroServiceService backend) ─────────────────

        public async Task<List<ServicioDto>> GetAllServiciosAsync()
        {
            try
            {
                var response = await SendAsync(_httpServices, HttpMethod.Get, "api/servicios?estado=true");
                if (!response.IsSuccessStatusCode) return new List<ServicioDto>();

                var backendList = await DeserializeAsync<List<BackendService>>(response);
                if (backendList == null) return new List<ServicioDto>();

                return backendList.Select(s => new ServicioDto
                {
                    ServicioId = s.Id,
                    Nombre = s.Nombre,
                    Precio = (double)s.PrecioBase
                }).ToList();
            }
            catch
            {
                return new List<ServicioDto>();
            }
        }

        public async Task<ServicioDto?> GetServicioAsync(int id)
        {
            try
            {
                var response = await SendAsync(_httpServices, HttpMethod.Get, $"api/servicios/{id}");
                if (!response.IsSuccessStatusCode) return null;

                var s = await DeserializeAsync<BackendService>(response);
                if (s == null) return null;

                return new ServicioDto
                {
                    ServicioId = s.Id,
                    Nombre = s.Nombre,
                    Precio = (double)s.PrecioBase
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool ok, string? error)> SaveServicioAsync(ServicioFormDto form)
        {
            try
            {
                HttpResponseMessage response;
                var body = new
                {
                    nombre = form.Nombre,
                    descripcion = form.Nombre,
                    precioBase = (decimal)form.Precio,
                    duracionEstimadaMinutos = 30, // Default for required field
                    categoriaServicioId = 1, // Default category
                    estado = true
                };

                if (form.ServicioId == 0)
                {
                    response = await SendAsync(_httpServices, HttpMethod.Post, "api/servicios", body);
                }
                else
                {
                    response = await SendAsync(_httpServices, HttpMethod.Put, $"api/servicios/{form.ServicioId}", body);
                }

                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión con servicio de servicios: {ex.Message}");
            }
        }

        public async Task DeleteServicioAsync(int id)
        {
            try
            {
                await SendAsync(_httpServices, HttpMethod.Delete, $"api/servicios/{id}");
            }
            catch {}
        }

        // ─── Vehículos (Mocked in-memory) ──────────────────────────────────────

        public async Task<List<VehiculoListDto>> GetAllVehiculosAsync()
        {
            await Task.Delay(50);
            return _mockVehiculos.ToList();
        }

        public async Task<List<VehiculoLookupDto>> BuscarVehiculosPorPlacaAsync(string term, int? clienteId = null)
        {
            var searchResults = await BuscarVehiculosAsync(term, clienteId);
            return searchResults;
        }

        public async Task<(bool ok, string? error, int vehiculoId)> SaveVehiculoAsync(VehiculoFormDto form)
        {
            await Task.Delay(50);
            var marca = _mockMarcas.FirstOrDefault(m => m.MarcaId == form.MarcaId)?.Nombre ?? "Toyota";
            var modelo = _mockModelos.FirstOrDefault(m => m.ModeloId == form.ModeloId)?.Nombre ?? "Corolla";
            var color = _mockColores.FirstOrDefault(c => c.ColorVehiculoId == form.ColorVehiculoId)?.Nombre ?? "Blanco";

            string clienteNombre = "Juan Perez";
            var client = await GetClienteAsync(form.ClienteId);
            if (client != null)
                clienteNombre = $"{client.Nombres} {client.PrimerApellido}";

            if (form.VehiculoId == 0)
            {
                var newId = Interlocked.Increment(ref _nextVehiculoId);
                var veh = new VehiculoListDto
                {
                    VehiculoId = newId,
                    Placa = form.Placa.Trim().ToUpperInvariant(),
                    ClienteId = form.ClienteId,
                    ClienteNombre = clienteNombre,
                    Anio = form.Anio,
                    MarcaNombre = marca,
                    ModeloNombre = modelo,
                    ColorNombre = color
                };
                _mockVehiculos.Add(veh);
                return (true, null, newId);
            }
            else
            {
                var veh = _mockVehiculos.FirstOrDefault(v => v.VehiculoId == form.VehiculoId);
                if (veh == null) return (false, "Vehículo no encontrado.", 0);

                veh.Placa = form.Placa.Trim().ToUpperInvariant();
                veh.ClienteId = form.ClienteId;
                veh.ClienteNombre = clienteNombre;
                veh.Anio = form.Anio;
                veh.MarcaNombre = marca;
                veh.ModeloNombre = modelo;
                veh.ColorNombre = color;

                return (true, null, form.VehiculoId);
            }
        }

        public async Task DeleteVehiculoAsync(int id)
        {
            await Task.Delay(50);
            var veh = _mockVehiculos.FirstOrDefault(v => v.VehiculoId == id);
            if (veh != null) _mockVehiculos.Remove(veh);
        }

        // ─── Catálogos (Mocked in-memory) ──────────────────────────────────────

        public async Task<List<MarcaDto>> GetAllMarcasAsync()
        {
            await Task.Delay(10);
            return _mockMarcas.ToList();
        }

        public async Task<List<ModeloDto>> GetAllModelosAsync()
        {
            await Task.Delay(10);
            return _mockModelos.ToList();
        }

        public async Task<List<ColorVehiculoDto>> GetAllColoresAsync()
        {
            await Task.Delay(10);
            return _mockColores.ToList();
        }

        // ─── Private Helpers ─────────────────────────────────────────────────

        private async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string url, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);

            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (string.IsNullOrWhiteSpace(token))
                token = _ctx.HttpContext?.User.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body is not null)
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json");

            return await client.SendAsync(request);
        }

        private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
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
                    if (root.TryGetProperty("message", out var msgProp))
                        return msgProp.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                    if (root.TryGetProperty("error", out var errProp))
                        return errProp.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                }
            }
            catch { }
            return $"Error HTTP {(int)response.StatusCode}.";
        }
    }
}
