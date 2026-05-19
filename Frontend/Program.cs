using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure authentication: use OpenID Connect only if AzureAd:ClientId is set.
var azureClientId = builder.Configuration["AzureAd:ClientId"];
if (!string.IsNullOrEmpty(azureClientId))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // apunta a Pages/Login.cshtml
    })
    .AddOpenIdConnect("Azure", options =>
    {
        options.Authority = builder.Configuration["AzureAd:Authority"] ?? "";
        options.ClientId = builder.Configuration["AzureAd:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
        options.ResponseType = "code";
        options.SaveTokens = true;
    });
}
else
{
    // Development fallback: use cookie auth so app doesn't throw on startup.
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Login";
        });
}

builder.Services.AddAuthorization();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Typed HttpClient for Users service (backend). Default to localhost:5004 for dev.
builder.Services.AddHttpClient<WebService.Adapters.UsersServiceAdapter>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UsersService:BaseUrl"] ?? "http://localhost:5004/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    // In Development we avoid forcing HTTPS so local testing is easier (dev certs can be problematic).
}

// Compatibility redirect for links that still point to MVC-style login path.
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect("/Login" + context.Request.QueryString);
        return;
    }

    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
