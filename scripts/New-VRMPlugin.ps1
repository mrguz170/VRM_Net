<#
.SYNOPSIS
    Crea la estructura completa de un módulo VRM con arquitectura homologada v3.0

.PARAMETER ModuleName
    Nombre del módulo (ej: Inventario, Ventas, RRHH)

.PARAMETER ModuleId
    ID numérico único del módulo (debe coincidir con el ID en BD)

.PARAMETER FolderOrganization
    Carpeta de organización (ej: Finanzas, Operaciones, Onboarding)

.EXAMPLE
    .\scripts\New-VRMPlugin.ps1 -ModuleName Ventas -ModuleId 3 -FolderOrganization Comercial
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName,
    
    [Parameter(Mandatory=$true)]
    [int]$ModuleId,
    
    [Parameter(Mandatory=$true)]
    [string]$FolderOrganization
)

# ==================== CONFIGURACIÓN ====================

$RootPath = Get-Location
$ModulesPath = Join-Path $RootPath "src\Modules\$FolderOrganization"

# ? SIN "Demo" ni "Modules" en nombre de carpeta
$ModuleProjectName = "VRM_Plugin.Module.$ModuleName"
$ModuleFullPath = Join-Path $ModulesPath $ModuleProjectName
$AssemblyName = "VRM_Plugin.Module.$ModuleName"
$RootNamespace = "VRM_Plugin.Module.$ModuleName"

Write-Host "`n??????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  GENERADOR DE PLUGINS VRM v3.0" -ForegroundColor Cyan
Write-Host "  Módulo: $ModuleName (ID: $ModuleId)" -ForegroundColor Cyan
Write-Host "  Proyecto: $ModuleProjectName" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????`n" -ForegroundColor Cyan

# ==================== 1. CREAR PROYECTO ====================

Write-Host "?? [1/5] Creando proyecto..." -ForegroundColor Yellow

if (!(Test-Path $ModulesPath)) {
    New-Item -ItemType Directory -Path $ModulesPath -Force | Out-Null
}

Set-Location $ModulesPath
dotnet new razorclasslib -n $ModuleProjectName -o $ModuleProjectName --framework net8.0

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al crear proyecto" -ForegroundColor Red
    Set-Location $RootPath
    exit 1
}

Write-Host "? Proyecto creado" -ForegroundColor Green

# ==================== 2. CREAR .CSPROJ ====================

Write-Host "`n?? [2/5] Configurando proyecto..." -ForegroundColor Yellow

$csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <RootNamespace>$RootNamespace</RootNamespace>
    <AssemblyName>$AssemblyName</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.*" />
    <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.0.*" />
    <PackageReference Include="MySqlConnector" Version="2.5.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\Core\VRM_Plugin.Core.Abstractions\VRM_Plugin.Core.Abstractions.csproj" />
    <ProjectReference Include="..\..\..\Core\VRM_Plugin.Core.Shared\VRM_Plugin.Core.Shared.csproj" />
  </ItemGroup>
</Project>
"@

Set-Content -Path (Join-Path $ModuleFullPath "$ModuleProjectName.csproj") -Value $csprojContent -Force
Write-Host "? Proyecto configurado" -ForegroundColor Green

# =============================================================
#2.1 Agregar referencia al proyecto Core.Shared en el archivo .sln principal
Set-Location $RootPath

$solution = Get-ChildItem -Path $RootPath -Filter '*.sln' -File -Recurse | Select-Object -First 1
if ($null -eq $solution) {
    Write-Host "? No se encontró archivo .sln en $RootPath" -ForegroundColor Yellow
} else {
    Write-Host "?? Agregando proyecto y dependencias a la solución: $($solution.FullName)" -ForegroundColor Yellow
        
    # Agregar proyecto a la solución (SIN crear solution-folder => no se verá la carpeta en Solution Explorer)
    $moduleCsproj = Join-Path $ModuleFullPath "$ModuleProjectName.csproj"
    if (Test-Path $moduleCsproj) {
        
        dotnet sln $solution.FullName add $moduleCsproj --solution-folder "src/Modules" | Out-Null

    }

    Write-Host "? Proyectos agregados a la solución" -ForegroundColor Green
}

# ==================== 3. CREAR CARPETAS ====================

Write-Host "`n?? [3/5] Creando estructura..." -ForegroundColor Yellow

# Asegurar que exista la carpeta raíz del módulo
if (!(Test-Path $ModuleFullPath)) {
    Write-Host "?? Carpeta del módulo no encontrada. Creando: $ModuleFullPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $ModuleFullPath -Force | Out-Null
}

# Normalizar a ruta absoluta para evitar problemas de contexto
try {
    $ModuleFullPath = (Resolve-Path -Path $ModuleFullPath).ProviderPath
} catch {
    Write-Host "⚠ No se pudo resolver ruta absoluta: $ModuleFullPath" -ForegroundColor Yellow
}

$Folders = @("Components", "Domain", "Services", "Data", "Data\DTOs", "Data\Repositories", "wwwroot\assets", "wwwroot\assets\images", "wwwroot\assets\css", "wwwroot\assets\js")

foreach ($folder in $Folders) {
    $target = Join-Path $ModuleFullPath $folder
    if (!(Test-Path $target)) {
        New-Item -ItemType Directory -Path $target -Force | Out-Null
    }
}

# Crear README.md informativos en cada carpeta para que Visual Studio y Git los muestren
$folderDescriptions = @{
    "Components"               = "Componentes Blazor del módulo."
    "Domain"                   = "Modelos y entidades del dominio del módulo."
    "Services"                 = "Servicios de negocio y lógica de aplicación."
    "Data\DTOs"                = "DTOs (Data Transfer Objects) usados por el módulo."
    "Data\Repositories"        = "Implementaciones y contratos de repositorios."   
    "wwwroot\assets\images"    = "Imágenes del módulo."
    "wwwroot\assets\css"       = "Hojas de estilo (CSS) del módulo."
    "wwwroot\assets\js"        = "Scripts JavaScript del módulo."
}

# Crear README.md informativos sólo en carpetas listadas en $folderDescriptions
foreach ($folder in $folderDescriptions.Keys) {
    $target = Join-Path $ModuleFullPath $folder
    if (!(Test-Path $target)) {
        New-Item -ItemType Directory -Path $target -Force | Out-Null
    }

    $readmePath = Join-Path $target "README.md"
    if (!(Test-Path $readmePath)) {
        $desc = $folderDescriptions[$folder]
        if ([string]::IsNullOrWhiteSpace($desc)) {
            $desc = "Carpeta para $folder."
        }
        Set-Content -Path $readmePath -Value $desc -Encoding UTF8 -Force
    }
}

# Crear archivos placeholder .cs para asegurar namespaces en IDE
$placeholders = @{
    "Domain" = @"
// REUTILIZAR Namespace para Domain en futuras clases
// ELIMINAR este archivo cuando se agreguen nuevas clases en este namespace
namespace $RootNamespace.Domain
{
    internal static class NamespacePlaceholder
    {
        private const string Purpose = "Placeholder para $RootNamespace.Domain";
    }
}
"@
    "Services" = @"
// REUTILIZAR Namespace para Domain en futuras clases
// ELIMINAR este archivo cuando se agreguen nuevas clases en este namespace
namespace $RootNamespace.Services
{
    internal static class NamespacePlaceholder
    {
        private const string Purpose = "Placeholder para $RootNamespace.Services";
    }
}
"@
}

foreach ($kv in $placeholders.GetEnumerator()) {
    $relFolder = $kv.Key
    $content = $kv.Value
    $folderPath = Join-Path $ModuleFullPath $relFolder
    if (!(Test-Path $folderPath)) {
        New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
    }
    $filePath = Join-Path $folderPath "NamespacePlaceholder.cs"
    if (!(Test-Path $filePath)) {
        Set-Content -Path $filePath -Value $content -Encoding UTF8 -Force
    }
}

# Limpiar archivos de plantilla
Remove-Item (Join-Path $ModuleFullPath "Component1.razor.css") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $ModuleFullPath "Component1.razor") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $ModuleFullPath "ExampleJsInterop.cs") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $ModuleFullPath "_Imports.razor") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $ModuleFullPath "wwwroot/background.png") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $ModuleFullPath "wwwroot/exampleJsInterop.js") -ErrorAction SilentlyContinue


Write-Host "? Estructura creada" -ForegroundColor Green


# ==================== 4. CREAR CLASE MÓDULO ====================

Write-Host "`n?? [4/5] Generando clase módulo..." -ForegroundColor Yellow

$moduleContent = @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Data.DTOs;

namespace VRM_Plugin.Module.$ModuleName;

public class ${ModuleName}Module : IModule
{
    // ==================== CAMPOS PRIVADOS ====================
    
    // Datos inyectados por el Host (desde BD)
    private List<ModuleComponentDto> _components = new();
    private List<ModuleActionDto> _actions = new();
    
    // Metadata del módulo (se inyectan desde BD, valores vacíos por defecto)
    private string _moduleName = string.Empty;
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private string _version = string.Empty;
        
// ==================== PROPIEDADES PÚBLICAS ====================
    
    /// <summary>
    /// ID numérico del módulo
    /// </summary>
    public int ModuleId { get; set; } = $ModuleId;
    
    public string ModuleName => _moduleName;
    public string DisplayName => _displayName;
    public string Description => _description;
    public string Version => _version;

// ==================== MÉTODOS PÚBLICOS ====================

    /// <summary>
    /// Devuelve los componentes inyectados por el Host
    /// </summary>
    public List<ModuleComponentDto> GetComponents()
    {
        if (_components.Count > 0)
        {
            return _components;
        }

        // Fallback: valores por defecto si no se cargaron desde BD
        return new List<ModuleComponentDto>();
    }
    
    /// <summary>
    /// Devuelve las acciones inyectadas por el Host
    /// </summary>
    public List<ModuleActionDto> GetActions()
    {
        if (_actions.Count > 0)
        {
            return _actions;
        }

        return new List<ModuleActionDto>();
    }
        
    // ==================== MÉTODOS DE INYECCIÓN (Llamados por el Host) ====================
    
    /// <summary>
    /// El Host llama este método para inyectar componentes desde BD
    /// </summary>
    public void SetComponents(List<ModuleComponentDto> components)
    {
        _components = components ?? new List<ModuleComponentDto>();
    }
    
    /// <summary>
    /// El Host llama este método para inyectar acciones desde BD
    /// </summary>
    public void SetActions(List<ModuleActionDto> actions)
    {
        _actions = actions ?? new List<ModuleActionDto>();
    }
    
    /// <summary>
    /// El Host llama este método para inyectar metadata desde BD
    /// </summary>    
    public void SetMetadata(string moduleName, string displayName, string description, string version)
    {
        _moduleName = moduleName ?? _moduleName;
        _displayName = displayName ?? _displayName;
        _description = description ?? _description;
        _version = version ?? _version;
    }

// ==================== CONFIGURACIÓN DE SERVICIOS ====================
    
    /// <summary>
    /// Registra servicios de NEGOCIO y REPOSITORIOS del módulo
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //  Registrar repositorios del módulo (capa de datos)
        //services.AddScoped<IRepositoryInterface, RepositoryInterfaceImplemented>();
                
        //  Registrar servicios de negocio del módulo
        //services.AddScoped<IServiceInterface,ServiceInterfaceImplemented>();
        
        //  Registrar el módulo como IModule para inyección en layouts/componentes
        services.AddSingleton<IModule>(this);
        services.AddSingleton(this); 
    }
        
    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - ModuleId: {ModuleId}");
        Console.WriteLine($"[{ModuleName}] DisplayName: {DisplayName}");
        Console.WriteLine($"[{ModuleName}] Componentes: {_components.Count}, Acciones: {_actions.Count}");
        await Task.CompletedTask;
    }

}
"@

Set-Content -Path (Join-Path $ModuleFullPath "${ModuleName}Module.cs") -Value $moduleContent -Force

# Crear _Imports.razor
$importsContent = @"
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Authorization
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using VRM_Plugin.Core.Shared.Components.Auth
@using VRM_Plugin.Module.$ModuleName.Domain
@using VRM_Plugin.Module.$ModuleName.Services
@namespace VRM_Plugin.Module.$ModuleName.Components
"@

Set-Content -Path (Join-Path $ModuleFullPath "Components\_Imports.razor") -Value $importsContent -Force

Write-Host "? Clase módulo generada" -ForegroundColor Green

# ==================== 5. COMPILAR ====================

Write-Host "`n?? [5/5] Compilando..." -ForegroundColor Yellow

Set-Location $ModuleFullPath
dotnet build --configuration Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al compilar" -ForegroundColor Red
    Set-Location $RootPath
    exit 1
}

# ==================== RESUMEN ====================

Write-Host "`n??????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  ? MÓDULO CREADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "`n?? Carpeta: $ModuleProjectName" -ForegroundColor Cyan
Write-Host "?? DLL: $AssemblyName.dll" -ForegroundColor Cyan
Write-Host "?? Namespace: $RootNamespace" -ForegroundColor Cyan
Write-Host "`n? Módulo listo para desarrollo`n" -ForegroundColor Green

Set-Location $RootPath
