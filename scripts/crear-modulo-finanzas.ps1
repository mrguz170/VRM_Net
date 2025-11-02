# Script para crear el módulo de Finanzas
# Ejecutar desde la raíz del proyecto: .\crear-modulo-finanzas.ps1

Write-Host "?? Creando módulo de Finanzas..." -ForegroundColor Green

# Paso 1: Crear carpeta de categoría Finanzas
Write-Host "`n?? Paso 1: Creando carpeta Finanzas..." -ForegroundColor Cyan
if (!(Test-Path "src\Modules\Finanzas")) {
    New-Item -ItemType Directory -Path "src\Modules\Finanzas" -Force | Out-Null
    Write-Host "? Carpeta src\Modules\Finanzas creada" -ForegroundColor Green
} else {
    Write-Host "??  Carpeta src\Modules\Finanzas ya existe" -ForegroundColor Yellow
}

# Paso 2: Crear proyecto Razor Class Library
Write-Host "`n?? Paso 2: Creando proyecto Razor Class Library..." -ForegroundColor Cyan
$modulePath = "src\Modules\Finanzas\VRM_PluginDemo.Modules.Finanzas"

dotnet new razorclasslib -n "VRM_PluginDemo.Modules.Finanzas" -o $modulePath

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Proyecto creado en $modulePath" -ForegroundColor Green
} else {
    Write-Host "? Error al crear el proyecto" -ForegroundColor Red
    exit 1
}

# Paso 3: Crear estructura de carpetas
Write-Host "`n?? Paso 3: Creando estructura de carpetas..." -ForegroundColor Cyan
New-Item -ItemType Directory -Path "$modulePath\Components" -Force | Out-Null
New-Item -ItemType Directory -Path "$modulePath\Services" -Force | Out-Null
New-Item -ItemType Directory -Path "$modulePath\Domain" -Force | Out-Null
Write-Host "? Carpetas Components, Services, Domain creadas" -ForegroundColor Green

# Paso 4: Actualizar .csproj con referencias correctas
Write-Host "`n?? Paso 4: Actualizando referencias del proyecto..." -ForegroundColor Cyan

$csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.20" />
  </ItemGroup>

  <ItemGroup>
    <!-- Ruta: Finanzas -> Modules -> src -> Core (3 niveles arriba) -->
    <ProjectReference Include="..\..\..\Core\VRM_PluginDemo.Core.Abstractions\VRM_PluginDemo.Core.Abstractions.csproj" />
  </ItemGroup>

</Project>
"@

$csprojContent | Out-File -FilePath "$modulePath\VRM_PluginDemo.Modules.Finanzas.csproj" -Encoding UTF8 -Force
Write-Host "? Archivo .csproj actualizado con referencias correctas" -ForegroundColor Green

# Paso 5: Crear _Imports.razor en Components
Write-Host "`n?? Paso 5: Creando _Imports.razor..." -ForegroundColor Cyan

$importsContent = @"
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using VRM_PluginDemo.Modules.Finanzas.Services
@using VRM_PluginDemo.Modules.Finanzas.Domain

@namespace VRM_PluginDemo.Modules.Finanzas.Components
"@

$importsContent | Out-File -FilePath "$modulePath\Components\_Imports.razor" -Encoding UTF8 -Force
Write-Host "? Archivo _Imports.razor creado" -ForegroundColor Green

# Paso 6: Crear componente Finanzas.razor
Write-Host "`n?? Paso 6: Creando componente Finanzas.razor..." -ForegroundColor Cyan

$finanzasRazorContent = @"
@page "/finanzas"
@rendermode InteractiveServer

<PageTitle>Gestión de Finanzas</PageTitle>

<h1>?? Gestión de Finanzas</h1>

<div class="alert alert-info mt-4">
    <h4>Módulo de Finanzas</h4>
    <p>Este módulo permite gestionar:</p>
    <ul>
        <li>Facturas</li>
        <li>Pagos</li>
        <li>Conciliaciones bancarias</li>
        <li>Cuentas por pagar</li>
    </ul>
</div>

<div class="card mt-3">
    <div class="card-header bg-primary text-white">
        <h5>?? Estadísticas</h5>
    </div>
    <div class="card-body">
        <p>Contenido de finanzas aquí...</p>
    </div>
</div>

@code {
    protected override async Task OnInitializedAsync()
    {
        // Lógica de inicialización
        await Task.CompletedTask;
    }
}
"@

$finanzasRazorContent | Out-File -FilePath "$modulePath\Components\Finanzas.razor" -Encoding UTF8 -Force
Write-Host "? Componente Finanzas.razor creado" -ForegroundColor Green

# Paso 7: Crear FinanzasModule.cs
Write-Host "`n??  Paso 7: Creando FinanzasModule.cs..." -ForegroundColor Cyan

$finanzasModuleContent = @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_PluginDemo.Core.Abstractions;

namespace VRM_PluginDemo.Modules.Finanzas;

/// <summary>
/// Módulo de gestión financiera.
/// Implementa IModule para integrarse en el sistema de plugins.
/// </summary>
public class FinanzasModule : IModule
{
    // ==================== IDENTIFICACIÓN ====================

    public string ModuleId => "Finanzas";

    public string DisplayName => "Gestión de Finanzas";

    public string Description =>
        "Módulo para gestionar operaciones financieras. " +
        "Incluye facturas, pagos, conciliaciones y cuentas por pagar.";

    public string Version => "1.0.0";

    public string Author => "Equipo de Desarrollo VRM";

    // ==================== CATEGORIZACIÓN ====================

    public string Category => "Finanzas";

    // ==================== DEPENDENCIAS ====================

    public List<string> Dependencies => new()
    {
        // Este módulo no tiene dependencias de otros módulos
    };

    // ==================== PERMISOS ====================

    public List<string> RequiredPermissions => new()
    {
        "Admin",
        "GestorFinanzas",
        "Contador"
    };

    // ==================== COMPONENTES BLAZOR ====================

    public List<ModuleComponentInfo> GetComponents()
    {
        return new List<ModuleComponentInfo>
        {
            new ModuleComponentInfo
            {
                Name = "Finanzas",
                Route = "/finanzas",
                ComponentType = typeof(VRM_PluginDemo.Modules.Finanzas.Components.Finanzas),
                Icon = "bi-currency-dollar",
                ShowInMenu = true,
                MenuOrder = 20
            }
        };
    }

    // ==================== CONFIGURACIÓN ====================

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Registrar servicios del módulo
        // services.AddScoped<IFacturaService, FacturaService>();
    }

    // ==================== HABILITACIÓN POR CLIENTE ====================

    public bool IsEnabledForClient(string clienteId)
    {
        // TODO: Implementar consulta a ConfiguracionNegocio del cliente
        return true;
    }

    // ==================== CICLO DE VIDA ====================

    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleId}] Módulo cargado exitosamente - Versión {Version}");
        Console.WriteLine($"[{ModuleId}] Componentes registrados: {GetComponents().Count}");

        await Task.CompletedTask;
    }
}
"@

$finanzasModuleContent | Out-File -FilePath "$modulePath\FinanzasModule.cs" -Encoding UTF8 -Force
Write-Host "? Archivo FinanzasModule.cs creado" -ForegroundColor Green

# Paso 8: Compilar para verificar
Write-Host "`n?? Paso 8: Compilando proyecto..." -ForegroundColor Cyan
dotnet build "src\Host\VRM_PluginDemo.Blazor.Server\VRM_PluginDemo.Blazor.Server.csproj"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? ¡Módulo de Finanzas creado exitosamente!" -ForegroundColor Green
    Write-Host "`n?? Resumen:" -ForegroundColor Cyan
    Write-Host "  ?? Ubicación: src\Modules\Finanzas\VRM_PluginDemo.Modules.Finanzas\" -ForegroundColor White
    Write-Host "  ?? Ruta web: https://localhost:XXXX/finanzas" -ForegroundColor White
    Write-Host "  ?? Componentes: Finanzas.razor" -ForegroundColor White
    Write-Host "  ??  Entry point: FinanzasModule.cs" -ForegroundColor White
} else {
    Write-Host "`n? Error al compilar el proyecto" -ForegroundColor Red
}
