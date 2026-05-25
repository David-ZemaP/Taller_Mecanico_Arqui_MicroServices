# MicroServiceProduct — Guía rápida

Resumen: microservicio de Productos con arquitectura hexagonal (Domain/Application/Infrastructure) usando EF Core y MySQL.

Instrucciones locales rápidas:

1. Levantar solo la base de datos de Productos desde el backend:

```bash
cd Backend
docker compose up -d mysql-productos
```

2. Aplicar las migraciones de EF Core:

```bash
cd Backend/MicroServiceProduct
dotnet tool install --global dotnet-ef
dotnet ef database update
```

3. Arrancar la API del microservicio:

```bash
cd Backend/MicroServiceProduct
dotnet run
```

El servicio quedará expuesto según el perfil local configurado en `launchSettings.json`.

Si vas a usar Docker, el compose genérico del backend solo levanta las bases de datos y el servicio de usuarios; este microservicio de Productos no está incluido ahí.

4. Crear una migración nueva solo si cambias el modelo:

```bash
cd Backend/MicroServiceProduct
dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update
```

5. Configuración de conexión: `appsettings.json` contiene una `ConnectionStrings:DefaultConnection` por defecto. Puedes sobreescribirla con la variable de entorno `ConnectionStrings__DefaultConnection`.

Endpoints principales:
- `GET /api/Products`
- `GET /api/Products/{id}`
- `POST /api/Products`
- `PUT /api/Products/{id}`
- `DELETE /api/Products/{id}`
