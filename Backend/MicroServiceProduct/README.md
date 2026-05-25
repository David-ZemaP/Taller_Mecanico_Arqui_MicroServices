# MicroServiceProduct — Guía rápida

Resumen: microservicio de Productos con arquitectura hexagonal (Domain/Application/Infrastructure) usando EF Core y MySQL.

Instrucciones locales rápidas:

1. Levantar MySQL + servicio con Docker Compose:

```bash
cd Backend/MicroServiceProduct
docker-compose up --build
```

El servicio quedará expuesto en `http://localhost:5002`.

2. Aplicar migraciones (desde host o dentro del contenedor):

- Instalar `dotnet-ef` si no lo tienes:

```bash
dotnet tool install --global dotnet-ef
```

- Crear migración y aplicar (requiere que la DB esté accesible):

```bash
cd Backend/MicroServiceProduct
dotnet ef migrations add InitialCreate
dotnet ef database update
```

3. Configuración de conexión: `appsettings.json` contiene una `ConnectionStrings:DefaultConnection` por defecto. Puedes sobreescribirla con la variable de entorno `ConnectionStrings__DefaultConnection` (usada en `docker-compose.yml`).

Endpoints principales:
- `GET /api/Products`
- `GET /api/Products/{id}`
- `POST /api/Products`
- `PUT /api/Products/{id}`
- `DELETE /api/Products/{id}`
