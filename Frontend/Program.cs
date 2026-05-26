using Microsoft.AspNetCore.Authentication.Cookies;
using WebService.Adapters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccesoDenegado";
    });

builder.Services.AddHttpContextAccessor();

// Configure Named HttpClients for Microservices
builder.Services.AddHttpClient("UsersApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendUrls:UsersApi"] ?? "http://localhost:5122/");
});
builder.Services.AddHttpClient("ClientsApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendUrls:ClientsApi"] ?? "http://localhost:5178/");
});
builder.Services.AddHttpClient("ProductsApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendUrls:ProductsApi"] ?? "http://localhost:5177/");
});
builder.Services.AddHttpClient("ServicesApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendUrls:ServicesApi"] ?? "http://localhost:5179/");
});

// Register Adapters (interface → implementation, with concrete forward for backwards compatibility)
builder.Services.AddScoped<IUsersServiceAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new UsersServiceAdapter(factory.CreateClient("UsersApi"), ctx);
});
builder.Services.AddScoped<UsersServiceAdapter>(sp =>
    (UsersServiceAdapter)sp.GetRequiredService<IUsersServiceAdapter>());

builder.Services.AddScoped<IEmpleadosAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new EmpleadosAdapter(factory.CreateClient("UsersApi"), ctx);
});
builder.Services.AddScoped<EmpleadosAdapter>(sp =>
    (EmpleadosAdapter)sp.GetRequiredService<IEmpleadosAdapter>());

builder.Services.AddScoped<IClientesAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new ClientesAdapter(factory.CreateClient("ClientsApi"), ctx);
});
builder.Services.AddScoped<ClientesAdapter>(sp =>
    (ClientesAdapter)sp.GetRequiredService<IClientesAdapter>());

builder.Services.AddScoped<IOrdenTrabajoAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var clientes = sp.GetRequiredService<IClientesAdapter>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new OrdenTrabajoAdapter(factory, clientes, ctx);
});
builder.Services.AddScoped<OrdenTrabajoAdapter>(sp =>
    (OrdenTrabajoAdapter)sp.GetRequiredService<IOrdenTrabajoAdapter>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Authentication and Session middlewares must be executed before Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
