using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MicroServiceClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Firebase
var firebaseConfig = builder.Configuration.GetSection("Firebase");
var projectId = firebaseConfig["ProjectId"];
var credentialsPath = firebaseConfig["CredentialsPath"];

// Inicializar Firebase Admin SDK
if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
{
    var credential = GoogleCredential.FromFile(credentialsPath);
    FirebaseApp.Create(new AppOptions
    {
        Credential = credential,
        ProjectId = projectId
    });
}
else
{
    // Usar variable de entorno GOOGLE_APPLICATION_CREDENTIALS
    FirebaseApp.Create();
}

// Registrar servicios
builder.Services.AddSingleton<IFirebaseService, FirebaseService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();