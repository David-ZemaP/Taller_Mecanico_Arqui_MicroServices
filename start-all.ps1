<#
.SYNOPSIS
    Levanta todo el entorno del Taller Mecánico: Docker (DBs + users-api) y
    microservicios restantes con dotnet run en ventanas separadas.

.DESCRIPTION
    1. Verifica Docker y dotnet SDK
    2. docker compose up -d (bases de datos + users-api)
    3. Espera a que las DBs estén listas
    4. Abre ventanas de terminal para cada microservicio faltante
    5. Abre el Frontend
    6. Muestra URLs de todos los servicios

.NOTES
    Ejecutar desde la raíz del repositorio.
    Si algún proceso falla, revisá la terminal correspondiente.
#>

$ErrorActionPreference = "Stop"
$rootDir = $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Taller Mecánico — Entorno Completo" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ─── 1. PREREQUISITOS ─────────────────────────────────────────────────────────

# Verificar Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker no está instalado o no está en el PATH."
    exit 1
}

# Verificar dotnet
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet SDK no está instalado o no está en el PATH."
    exit 1
}

# Verificar que Docker Desktop esté corriendo
try {
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker Desktop no está corriendo. Inicialo primero."
        exit 1
    }
} catch {
    Write-Error "No se pudo conectar con Docker Desktop."
    exit 1
}

Write-Host "[✓] Docker Desktop corriendo" -ForegroundColor Green
Write-Host "[✓] dotnet SDK disponible" -ForegroundColor Green
Write-Host ""

# ─── 2. DOCKER COMPOSE ────────────────────────────────────────────────────────

Write-Host ">>> Levantando contenedores Docker (bases de datos + users-api)..." -ForegroundColor Yellow
Push-Location -LiteralPath "$rootDir\Backend"
try {
    docker compose up -d
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Fallo al levantar contenedores. Revisá Docker Desktop."
        exit 1
    }
} finally {
    Pop-Location
}
Write-Host "[✓] Contenedores levantados" -ForegroundColor Green

# ─── 3. ESPERAR HEALTHCHECKS ──────────────────────────────────────────────────

Write-Host ">>> Esperando que las bases de datos estén saludables..." -ForegroundColor Yellow

$containers = @{
    "taller-mysql-productos"     = "MySQL (productos)"
    "taller-postgresql-servicios" = "PostgreSQL (servicios)"
    "taller-postgresql-usuarios"  = "PostgreSQL (usuarios)"
    "taller-users-api"            = "Users API"
}

$maxWait = 90  # segundos totales
$interval = 5  # segundos entre chequeos
$elapsed = 0

while ($elapsed -lt $maxWait) {
    $allHealthy = $true
    foreach ($name in $containers.Keys) {
        $status = docker inspect --format='{{.State.Health.Status}}' $name 2>$null
        if ($status -ne "healthy") {
            $allHealthy = $false
        }
    }
    if ($allHealthy) {
        Write-Host "[✓] Todos los contenedores están saludables" -ForegroundColor Green
        break
    }
    Write-Host "    Esperando... ($elapsed s)" -ForegroundColor DarkYellow
    Start-Sleep -Seconds $interval
    $elapsed += $interval
}

if ($elapsed -ge $maxWait) {
    Write-Warning "Algunos contenedores no reportan healthy. Igual vamos a intentar levantar los servicios."
}

# ─── 4. MICROSERVICIOS (ventanas separadas) ───────────────────────────────────

Write-Host ""
Write-Host ">>> Abriendo microservicios en ventanas separadas..." -ForegroundColor Yellow

# PRODUCT API — requiere MySQL (puerto 3308 mapeado en docker)
Write-Host "  · Products API      → http://localhost:5177" -ForegroundColor Magenta
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command", "cd '$rootDir\Backend\MicroServiceProduct'; dotnet run --launch-profile http"
) -WindowStyle Normal

Start-Sleep -Seconds 2

# SERVICE API — requiere PostgreSQL (puerto 5432 mapeado en docker)
Write-Host "  · Services API      → http://localhost:5179" -ForegroundColor Magenta
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command", "cd '$rootDir\Backend\MicroServiceService\src\Services.API'; dotnet run --launch-profile http"
) -WindowStyle Normal

Start-Sleep -Seconds 2

# CLIENT API — Firebase (no necesita DB local)
Write-Host "  · Clients API       → http://localhost:5178" -ForegroundColor Magenta
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command", "cd '$rootDir\Backend\MicroServiceClient\src\Clients.API'; dotnet run --launch-profile http"
) -WindowStyle Normal

Start-Sleep -Seconds 2

# FRONTEND
Write-Host "  · Frontend          → http://localhost:5113" -ForegroundColor Magenta
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command", "cd '$rootDir\Frontend'; dotnet run --launch-profile http"
) -WindowStyle Normal

# ─── 5. RESUMEN ───────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  TODO LEVANTADO" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Servicios Docker:" -ForegroundColor White
Write-Host "    MySQL (productos)     → localhost:3308" -ForegroundColor Gray
Write-Host "    PostgreSQL (servicios) → localhost:5432" -ForegroundColor Gray
Write-Host "    PostgreSQL (usuarios)  → localhost:5433" -ForegroundColor Gray
Write-Host "    Users API (docker)    → http://localhost:5004/swagger" -ForegroundColor Gray
Write-Host ""
Write-Host "  Microservicios (dotnet run):" -ForegroundColor White
Write-Host "    Products API          → http://localhost:5177" -ForegroundColor Gray
Write-Host "    Services API          → http://localhost:5179" -ForegroundColor Gray
Write-Host "    Clients API           → http://localhost:5178" -ForegroundColor Gray
Write-Host ""
Write-Host "  Frontend:" -ForegroundColor White
Write-Host "    Web UI                → http://localhost:5113" -ForegroundColor Gray
Write-Host ""
Write-Host "  Para ver logs, revisá cada ventana de terminal." -ForegroundColor DarkYellow
Write-Host "  Para detener Docker:     docker compose down (en Backend/)" -ForegroundColor DarkYellow
Write-Host "  Para detener servicios:  cerrar las ventanas de terminal" -ForegroundColor DarkYellow
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Presioná cualquier tecla para cerrar esta ventana (los servicios siguen corriendo)..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
