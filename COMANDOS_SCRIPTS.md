# COMANDOS Y SCRIPTS - Guia de Referencia Rapida

Comandos y scripts listos para copiar y ejecutar en el desarrollo del proyecto VRM Plugin Demo.

---

## Tabla de Contenido

1. [Setup Inicial](#1-setup-inicial)
2. [Fase 1: Fundamentos](#2-fase-1-fundamentos)
3. [Fase 2: Base de Datos](#3-fase-2-base-de-datos)
4. [Fase 3: Docker](#4-fase-3-docker)
5. [Scripts de Mantenimiento](#5-scripts-de-mantenimiento)
6. [Comandos de Uso Diario](#6-comandos-de-uso-diario)
7. [GitHub Actions](#7-github-actions)

---

## 1. Setup Inicial

### Clonar Repositorio

```bash
git clone https://github.com/mrguz170/VRM_Plugin.git
cd VRM_Plugin
```

### Verificar .NET SDK

```bash
dotnet --version
# Debe ser 8.0.x
```

### Restaurar y Compilar

```bash
dotnet restore
dotnet build
```

### Ejecutar Aplicacion

```bash
cd src/Host/VRM_Plugin.Blazor.Server
dotnet run
```

Abrir navegador en: `https://localhost:XXXX/login`

---

## 2. Fase 1: Fundamentos

### Crear Nuevos Proyectos

```bash
# Asegurate de estar en la raiz del proyecto
cd C:\Users\lobo_\Documents\VRM\Criteria\VRM_Plugin

# Crear carpeta Shared
mkdir src\Shared

# Crear proyectos Core
dotnet new classlib -n VRM_Plugin.Core.Application -o src\Core\VRM_Plugin.Core.Application --framework net8.0
dotnet new classlib -n VRM_Plugin.Core.Infrastructure -o src\Core\VRM_Plugin.Core.Infrastructure --framework net8.0

# Crear proyectos Shared
dotnet new classlib -n VRM_Plugin.Shared.DTOs -o src\Shared\VRM_Plugin.Shared.DTOs --framework net8.0
dotnet new classlib -n VRM_Plugin.Shared.Contracts -o src\Shared\VRM_Plugin.Shared.Contracts --framework net8.0
dotnet new classlib -n VRM_Plugin.Shared.Common -o src\Shared\VRM_Plugin.Shared.Common --framework net8.0

# Crear proyectos de pruebas
mkdir tests
dotnet new xunit -n VRM_Plugin.UnitTests -o tests\VRM_Plugin.UnitTests --framework net8.0
dotnet new xunit -n VRM_Plugin.IntegrationTests -o tests\VRM_Plugin.IntegrationTests --framework net8.0

# Agregar proyectos a la solucion
dotnet sln add src\Core\VRM_Plugin.Core.Application\VRM_Plugin.Core.Application.csproj
dotnet sln add src\Core\VRM_Plugin.Core.Infrastructure\VRM_Plugin.Core.Infrastructure.csproj
dotnet sln add src\Shared\VRM_Plugin.Shared.DTOs\VRM_Plugin.Shared.DTOs.csproj
dotnet sln add src\Shared\VRM_Plugin.Shared.Contracts\VRM_Plugin.Shared.Contracts.csproj
dotnet sln add src\Shared\VRM_Plugin.Shared.Common\VRM_Plugin.Shared.Common.csproj
dotnet sln add tests\VRM_Plugin.UnitTests\VRM_Plugin.UnitTests.csproj
dotnet sln add tests\VRM_Plugin.IntegrationTests\VRM_Plugin.IntegrationTests.csproj
```

### Instalar Paquetes de Logging (Serilog)

```bash
# Navegar al proyecto Host
cd src\Host\VRM_Plugin.Blazor.Server

# Instalar Serilog
dotnet add package Serilog.AspNetCore --version 8.0.3
dotnet add package Serilog.Sinks.File --version 6.0.0
dotnet add package Serilog.Sinks.Seq --version 8.0.0
dotnet add package Serilog.Enrichers.Environment --version 3.0.1
dotnet add package Serilog.Enrichers.Thread --version 4.0.0

# Regresar a la raiz
cd ..\..\..
```

### Crear Carpetas de Documentacion

```bash
mkdir docs
mkdir docs\architecture
mkdir docs\api
mkdir docs\deployment

mkdir scripts
mkdir scripts\db
mkdir scripts\deployment
mkdir scripts\maintenance
```

---

## 3. Fase 2: Base de Datos

### Instalar Paquetes de Entity Framework

```bash
# Navegar al proyecto Infrastructure
cd src\Core\VRM_Plugin.Core.Infrastructure

# Instalar paquetes de EF Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.11
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.11

# Regresar a la raiz
cd ..\..\..
```

### Instalar EF Core Tools (Global)

```bash
# Solo una vez por maquina
dotnet tool install --global dotnet-ef

# O actualizar si ya esta instalado
dotnet tool update --global dotnet-ef
```

### Crear Connection String

Agregar a `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VRM_Plugin_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Crear Primera Migracion

```bash
# Desde la raiz del proyecto
dotnet ef migrations add InitialCreate ^
  --project src\Core\VRM_Plugin.Core.Infrastructure ^
  --startup-project src\Host\VRM_Plugin.Blazor.Server ^
  --context ApplicationDbContext ^
  --output-dir Migrations
```

### Aplicar Migracion

```bash
dotnet ef database update ^
  --project src\Core\VRM_Plugin.Core.Infrastructure ^
  --startup-project src\Host\VRM_Plugin.Blazor.Server ^
  --context ApplicationDbContext
```

### Ver Lista de Migraciones

```bash
dotnet ef migrations list ^
  --project src\Core\VRM_Plugin.Core.Infrastructure ^
  --startup-project src\Host\VRM_Plugin.Blazor.Server ^
  --context ApplicationDbContext
```

### Generar Script SQL

```bash
# Generar script SQL de todas las migraciones
dotnet ef migrations script ^
  --project src\Core\VRM_Plugin.Core.Infrastructure ^
  --startup-project src\Host\VRM_Plugin.Blazor.Server ^
  --context ApplicationDbContext ^
  --output scripts\db\migration.sql
```

### Revertir Ultima Migracion

```bash
dotnet ef migrations remove ^
  --project src\Core\VRM_Plugin.Core.Infrastructure ^
  --startup-project src\Host\VRM_Plugin.Blazor.Server ^
  --context ApplicationDbContext
```

### Rollback a Migracion Especifica

```bash
# Ver lista de migraciones primero
dotnet ef migrations list

# Rollback a una migracion especifica
dotnet ef database update NombreDeLaMigracion ^
  --project src\Core\VRM_Plugin.Core.Infrastructure ^
  --startup-project src\Host\VRM_Plugin.Blazor.Server ^
  --context ApplicationDbContext
```

---

## 4. Fase 3: Docker

### Instalar Docker Desktop

Descargar de: https://www.docker.com/products/docker-desktop

### Crear Dockerfile

Crear archivo `Dockerfile` en la raiz:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/Host/VRM_Plugin.Blazor.Server/VRM_Plugin.Blazor.Server.csproj", "Host/"]
COPY ["src/Core/VRM_Plugin.Core.Abstractions/VRM_Plugin.Core.Abstractions.csproj", "Core/Abstractions/"]
COPY ["src/Core/VRM_Plugin.Core.Domain/VRM_Plugin.Core.Domain.csproj", "Core/Domain/"]
COPY ["src/Core/VRM_Plugin.Core.Infrastructure/VRM_Plugin.Core.Infrastructure.csproj", "Core/Infrastructure/"]
COPY ["src/Core/VRM_Plugin.Core.Application/VRM_Plugin.Core.Application.csproj", "Core/Application/"]

# Restaurar dependencias
RUN dotnet restore "Host/VRM_Plugin.Blazor.Server.csproj"

# Copiar todo el codigo
COPY src/ .

# Build
WORKDIR "/src/Host"
RUN dotnet build "VRM_Plugin.Blazor.Server.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "VRM_Plugin.Blazor.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/Modules

ENTRYPOINT ["dotnet", "VRM_Plugin.Blazor.Server.dll"]
```

### Crear docker-compose.yml

Crear archivo `docker-compose.yml` en la raiz:

```yaml
version: '3.8'

services:
  web:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: vrm-plugin-demo-web
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=db;Database=VRM_Plugin;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;
      - Serilog__WriteTo__1__Name=Seq
      - Serilog__WriteTo__1__Args__serverUrl=http://seq:5341
    depends_on:
      - db
      - seq
    volumes:
      - ./Modules:/app/Modules
    restart: unless-stopped
    networks:
      - vrm-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: vrm-plugin-demo-db
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd123
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    restart: unless-stopped
    networks:
      - vrm-network

  seq:
    image: datalust/seq:latest
    container_name: vrm-plugin-demo-seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80"
    volumes:
      - seqdata:/data
    restart: unless-stopped
    networks:
      - vrm-network

volumes:
  sqldata:
    driver: local
  seqdata:
    driver: local

networks:
  vrm-network:
    driver: bridge
```

### Comandos Docker

```bash
# Construir imagen
docker-compose build

# Iniciar servicios en background
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f web

# Ver logs de todos los servicios
docker-compose logs -f

# Detener servicios
docker-compose down

# Detener y eliminar volumenes
docker-compose down -v

# Ver contenedores corriendo
docker ps

# Ver todas las imagenes
docker images

# Acceder a shell del contenedor web
docker exec -it vrm-plugin-demo-web /bin/bash

# Acceder a SQL Server
docker exec -it vrm-plugin-demo-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd123" -C

# Ver uso de recursos
docker stats

# Limpiar imagenes no usadas
docker image prune -a

# Limpiar volumenes no usados
docker volume prune
```

---

## 5. Scripts de Mantenimiento

### Script de Backup (PowerShell)

Guardar como: `scripts/maintenance/backup-database.ps1`

```powershell
param(
    [string]$ConnectionString = "Server=localhost;Database=VRM_Plugin;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True",
    [string]$BackupPath = "C:\Backups\VRM_Plugin"
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = Join-Path $BackupPath "VRM_Plugin_$timestamp.bak"

if (!(Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath | Out-Null
}

$query = @"
BACKUP DATABASE [VRM_Plugin]
TO DISK = '$backupFile'
WITH FORMAT, COMPRESSION, STATS = 10;
"@

Write-Host "Creando backup en: $backupFile" -ForegroundColor Cyan

try {
    Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $query
    Write-Host "Backup completado exitosamente" -ForegroundColor Green
    
    # Limpiar backups viejos (mantener ultimos 7 dias)
    Get-ChildItem $BackupPath -Filter "VRM_Plugin_*.bak" |
        Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-7) } |
        Remove-Item -Force
        
    Write-Host "Backups antiguos eliminados" -ForegroundColor Yellow
}
catch {
    Write-Host "Error al crear backup: $_" -ForegroundColor Red
    exit 1
}
```

**Uso:**

```powershell
# Backup con parametros por defecto
.\scripts\maintenance\backup-database.ps1

# Backup con parametros personalizados
.\scripts\maintenance\backup-database.ps1 -ConnectionString "Server=prod-server;..." -BackupPath "D:\Backups"
```

### Script de Deploy (PowerShell)

Guardar como: `scripts/deployment/deploy.ps1`

```powershell
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment
)

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  VRM Plugin Demo - Deploy Script" -ForegroundColor Cyan
Write-Host "  Ambiente: $Environment" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# 1. Limpiar
Write-Host "[1/7] Limpiando solucion..." -ForegroundColor Yellow
dotnet clean --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al limpiar. Deploy cancelado." -ForegroundColor Red
    exit 1
}

# 2. Restaurar
Write-Host "[2/7] Restaurando paquetes..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al restaurar. Deploy cancelado." -ForegroundColor Red
    exit 1
}

# 3. Build
Write-Host "[3/7] Compilando..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al compilar. Deploy cancelado." -ForegroundColor Red
    exit 1
}

# 4. Ejecutar pruebas
Write-Host "[4/7] Ejecutando pruebas..." -ForegroundColor Yellow
dotnet test --configuration Release --no-build --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Pruebas fallidas. Deploy cancelado." -ForegroundColor Red
    exit 1
}

# 5. Publish
Write-Host "[5/7] Publicando..." -ForegroundColor Yellow
$publishPath = ".\publish\$Environment"

if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}

dotnet publish src\Host\VRM_Plugin.Blazor.Server\VRM_Plugin.Blazor.Server.csproj `
    --configuration Release `
    --output $publishPath `
    --no-build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al publicar. Deploy cancelado." -ForegroundColor Red
    exit 1
}

# 6. Copiar modulos
Write-Host "[6/7] Copiando modulos..." -ForegroundColor Yellow
$modulesSource = "src\Modules"
$modulesDestination = "$publishPath\Modules"

if (!(Test-Path $modulesDestination)) {
    New-Item -ItemType Directory -Path $modulesDestination | Out-Null
}

$moduleCount = 0
Get-ChildItem -Path $modulesSource -Recurse -Filter "*.dll" |
    Where-Object { $_.FullName -like "*\bin\Release\net8.0\VRM_Plugin.Modules.*.dll" } |
    ForEach-Object {
        Copy-Item $_.FullName -Destination $modulesDestination -Force
        Write-Host "  Copiado: $($_.Name)" -ForegroundColor Gray
        $moduleCount++
    }

Write-Host "  Total modulos copiados: $moduleCount" -ForegroundColor Green

# 7. Copiar archivo de configuracion
Write-Host "[7/7] Copiando configuracion..." -ForegroundColor Yellow
$configSource = "src\Host\VRM_Plugin.Blazor.Server\appsettings.$Environment.json"

if (Test-Path $configSource) {
    Copy-Item $configSource -Destination "$publishPath\appsettings.json" -Force
    Write-Host "  Configuracion copiada: appsettings.$Environment.json" -ForegroundColor Gray
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host "  Deploy completado exitosamente!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""
Write-Host "Ubicacion: $publishPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Siguientes pasos:" -ForegroundColor Yellow
Write-Host "  1. Copiar contenido de '$publishPath' al servidor" -ForegroundColor Gray
Write-Host "  2. Ejecutar migraciones de BD si es necesario" -ForegroundColor Gray
Write-Host "  3. Reiniciar IIS/servicio" -ForegroundColor Gray
Write-Host ""
```

**Uso:**

```powershell
# Deploy a Development
.\scripts\deployment\deploy.ps1 -Environment Development

# Deploy a Staging
.\scripts\deployment\deploy.ps1 -Environment Staging

# Deploy a Production
.\scripts\deployment\deploy.ps1 -Environment Production
```

### Script para Compilar Modulos

Guardar como: `scripts/build-modules.ps1`

```powershell
Write-Host "Compilando modulos..." -ForegroundColor Cyan

$modulesPath = "src\Modules"
$outputPath = "src\Host\VRM_Plugin.Blazor.Server\Modules"

# Crear carpeta de salida si no existe
if (!(Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath | Out-Null
}

# Buscar todos los .csproj de modulos
Get-ChildItem -Path $modulesPath -Recurse -Filter "VRM_Plugin.Modules.*.csproj" | ForEach-Object {
    $projectPath = $_.FullName
    $projectName = $_.BaseName
    
    Write-Host "Compilando: $projectName" -ForegroundColor Yellow
    
    # Compilar
    dotnet build $projectPath --configuration Release
    
    if ($LASTEXITCODE -eq 0) {
        # Buscar DLL compilada
        $dllPath = Join-Path (Split-Path $projectPath) "bin\Release\net8.0\$projectName.dll"
        
        if (Test-Path $dllPath) {
            # Copiar a Modules
            Copy-Item $dllPath -Destination $outputPath -Force
            Write-Host "  Copiado a: $outputPath" -ForegroundColor Green
        }
    }
    else {
        Write-Host "  Error al compilar $projectName" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host "Compilacion de modulos completada!" -ForegroundColor Green
```

**Uso:**

```powershell
.\scripts\build-modules.ps1
```

---

## 6. Comandos de Uso Diario

### Compilacion

```bash
# Compilar todo
dotnet build

# Compilar en Release
dotnet build -c Release

# Limpiar
dotnet clean

# Limpiar y compilar
dotnet clean && dotnet build
```

### Ejecucion

```bash
# Ejecutar aplicacion
dotnet run --project src\Host\VRM_Plugin.Blazor.Server

# Ejecutar con ambiente especifico
dotnet run --project src\Host\VRM_Plugin.Blazor.Server --environment Production

# Watch mode (recompila automaticamente)
dotnet watch run --project src\Host\VRM_Plugin.Blazor.Server
```

### Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con verbosidad
dotnet test --verbosity normal

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar pruebas de un proyecto especifico
dotnet test tests\VRM_Plugin.UnitTests\VRM_Plugin.UnitTests.csproj
```

### NuGet

```bash
# Ver paquetes desactualizados
dotnet list package --outdated

# Actualizar paquete especifico
dotnet add package NombrePaquete

# Remover paquete
dotnet remove package NombrePaquete

# Restaurar paquetes
dotnet restore
```

### Formateo de Codigo

```bash
# Formatear todo el codigo
dotnet format

# Verificar formato sin cambiar
dotnet format --verify-no-changes

# Formatear solo archivos modificados
dotnet format --include src/
```

### Git

```bash
# Ver estado
git status

# Agregar cambios
git add .

# Commit
git commit -m "Descripcion del cambio"

# Push
git push origin main

# Crear rama
git checkout -b feature/nueva-funcionalidad

# Ver ramas
git branch -a

# Cambiar de rama
git checkout main

# Merge
git merge feature/nueva-funcionalidad

# Ver log
git log --oneline --graph --all

# Deshacer ultimo commit (mantener cambios)
git reset --soft HEAD~1

# Deshacer ultimo commit (eliminar cambios)
git reset --hard HEAD~1
```

---

## 7. GitHub Actions

### Workflow CI/CD

Crear archivo: `.github/workflows/ci-cd.yml`

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'
    
    - name: Publish artifacts
      if: github.ref == 'refs/heads/main'
      run: dotnet publish src/Host/VRM_Plugin.Blazor.Server/VRM_Plugin.Blazor.Server.csproj -c Release -o ./publish
    
    - name: Upload artifacts
      if: github.ref == 'refs/heads/main'
      uses: actions/upload-artifact@v4
      with:
        name: published-app
        path: ./publish

  deploy-staging:
    needs: build-and-test
    if: github.ref == 'refs/heads/develop'
    runs-on: ubuntu-latest
    environment: staging
    
    steps:
    - name: Download artifacts
      uses: actions/download-artifact@v4
      with:
        name: published-app
        path: ./publish
    
    - name: Deploy to Staging
      run: |
        echo "Deploying to staging server..."
        # Agregar comandos de deploy real aqui

  deploy-production:
    needs: build-and-test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    
    steps:
    - name: Download artifacts
      uses: actions/download-artifact@v4
      with:
        name: published-app
        path: ./publish
    
    - name: Deploy to Production
      run: |
        echo "Deploying to production server..."
        # Agregar comandos de deploy real aqui
```

### Comandos GitHub Actions

```bash
# Ver workflows
gh workflow list

# Ver runs de un workflow
gh run list

# Ver detalles de un run
gh run view RUN_ID

# Ejecutar workflow manualmente
gh workflow run ci-cd.yml

# Ver logs de un run
gh run view RUN_ID --log
```

---

## Atajos y Tips

### Aliases de PowerShell

Agregar a tu perfil de PowerShell (`$PROFILE`):

```powershell
# VRM Plugin Demo aliases
function vrm-build { dotnet build }
function vrm-run { dotnet run --project src\Host\VRM_Plugin.Blazor.Server }
function vrm-test { dotnet test }
function vrm-clean { dotnet clean }
function vrm-modules { .\scripts\build-modules.ps1 }
function vrm-up { docker-compose up -d }
function vrm-down { docker-compose down }
function vrm-logs { docker-compose logs -f web }
```

### Variables de Entorno Utiles

```powershell
# Development
$env:ASPNETCORE_ENVIRONMENT="Development"

# Staging
$env:ASPNETCORE_ENVIRONMENT="Staging"

# Production
$env:ASPNETCORE_ENVIRONMENT="Production"

# Ver variable actual
$env:ASPNETCORE_ENVIRONMENT
```

---

## Troubleshooting

### Problema: No se encuentran las DLLs de modulos

```bash
# Recompilar modulos
.\scripts\build-modules.ps1

# Verificar que existan
dir src\Host\VRM_Plugin.Blazor.Server\Modules
```

### Problema: Error al aplicar migraciones

```bash
# Verificar connection string
# Ver migraciones pendientes
dotnet ef migrations list

# Drop database y recrear
dotnet ef database drop -f
dotnet ef database update
```

### Problema: Puerto en uso

```powershell
# Ver que esta usando el puerto 5000
netstat -ano | findstr :5000

# Matar proceso
taskkill /PID XXXX /F
```

### Problema: Docker no inicia

```bash
# Verificar Docker esta corriendo
docker version

# Reiniciar Docker Desktop
# O desde PowerShell:
Restart-Service docker
```

---

Todos estos comandos estan probados y listos para usar. Ajusta rutas y credenciales segun tu ambiente.

**Ultima actualizacion:** Enero 2025
