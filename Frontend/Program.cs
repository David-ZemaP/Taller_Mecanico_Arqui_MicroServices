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

// Register Adapters
builder.Services.AddScoped<UsersServiceAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new UsersServiceAdapter(factory.CreateClient("UsersApi"), ctx);
});

builder.Services.AddScoped<EmpleadosAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new EmpleadosAdapter(factory.CreateClient("UsersApi"), ctx);
});

builder.Services.AddScoped<ClientesAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new ClientesAdapter(factory.CreateClient("ClientsApi"), ctx);
});

builder.Services.AddScoped<OrdenTrabajoAdapter>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var clientes = sp.GetRequiredService<ClientesAdapter>();
    var ctx = sp.GetRequiredService<IHttpContextAccessor>();
    return new OrdenTrabajoAdapter(factory, clientes, ctx);
});

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
