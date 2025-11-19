<#
.SYNOPSIS
    Crea la estructura completa de un módulo VRM sin servicio base (solo contenedor).

.DESCRIPTION
    Crea la estructura completa de un módulo VRM:
    - Proyecto Razor Class Library
    - Carpetas: Domain, Services, Components
    - Archivos base con IDs únicos
    - NUEVO: NO crea servicio base del módulo (es solo contenedor)
    - NUEVO: Opción -FirstComponent para crear primer componente con servicio
    - Compila y copia al Host automáticamente

.PARAMETER ModuleName
    Nombre del módulo (ej: Inventario, Ventas, RRHH)

.PARAMETER IdModule
    ID numérico único del módulo (debe ser diferente a los existentes)

.PARAMETER Category
    Categoría del módulo (ej: Finanzas, Operaciones, Administración)

.PARAMETER StartIdComponent
    ID inicial para componentes (ej: 100, 200, 300)

.PARAMETER StartIdAction
    ID inicial para acciones (ej: 100, 200, 300)

.PARAMETER IconRoot
    Icono Remix Icon para la categoría raíz (ej: ri-box-line)

.PARAMETER FirstComponent
    [OPCIONAL] Nombre del primer componente a crear (ej: Facturas, Productos)
    Si se especifica, se creará el componente con entidad y servicio automáticamente

.EXAMPLE
    # Crear módulo Finanzas SOLO como contenedor (sin componentes)
    .\New-VRMPlugin.ps1 -ModuleName Finanzas -IdModule 1 -Category Finanzas `
        -StartIdComponent 1 -StartIdAction 1 -IconRoot "ri-money-dollar-circle-line"

.EXAMPLE
    # Crear módulo Finanzas CON primer componente "Facturas"
    .\New-VRMPlugin.ps1 -ModuleName Finanzas -IdModule 1 -Category Finanzas `
        -StartIdComponent 1 -StartIdAction 1 -IconRoot "ri-money-dollar-circle-line" `
        -FirstComponent "Facturas"

.EXAMPLE
    # Crear módulo Inventario con componente "Productos"
    .\New-VRMPlugin.ps1 -ModuleName Inventario -IdModule 3 -Category Operaciones `
        -StartIdComponent 100 -StartIdAction 100 -IconRoot "ri-box-line" `
        -FirstComponent "Productos"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName,
    
    [Parameter(Mandatory=$true)]
    [int]$IdModule,
    
    [Parameter(Mandatory=$true)]
    [string]$Category,
    
    [Parameter(Mandatory=$true)]
    [int]$StartIdComponent,
    
    [Parameter(Mandatory=$true)]
    [int]$StartIdAction,
    
    [Parameter(Mandatory=$true)]
    [string]$IconRoot,
    
    # ?? NUEVO: Componente inicial opcional
    [Parameter(Mandatory=$false)]
    [string]$FirstComponent = ""
)

# ==================== CONFIGURACIÓN ====================

$RootPath = Get-Location
$ModulesPath = Join-Path $RootPath "src\Modules\$Category"
$ModuleProjectName = "VRM_Plugin.Modules.$ModuleName"
$ModuleFullPath = Join-Path $ModulesPath $ModuleProjectName

Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?  ?? GENERADOR DE PLUGINS VRM                            ?" -ForegroundColor Cyan
Write-Host "?  Módulo: $ModuleName".PadRight(60) + "?" -ForegroundColor Cyan
if ($FirstComponent) {
    Write-Host "?  Primer Componente: $FirstComponent".PadRight(60) + "?" -ForegroundColor Cyan
}
Write-Host "??????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

# ==================== 1. CREAR PROYECTO ====================

Write-Host "?? [1/7] Creando proyecto Razor Class Library..." -ForegroundColor Yellow

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

Write-Host "? Proyecto creado: $ModuleProjectName" -ForegroundColor Green

# ==================== 2. ESTRUCTURA DE CARPETAS ====================

Write-Host "`n?? [2/7] Creando estructura de carpetas..." -ForegroundColor Yellow

$Folders = @(
    "Domain",
    "Services",
    "Components"
)

foreach ($folder in $Folders) {
    $folderPath = Join-Path $ModuleFullPath $folder
    New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
    Write-Host "  ? Creada carpeta: $folder" -ForegroundColor Gray
}

# Eliminar archivos de plantilla innecesarios
$filesToRemove = @(
    "Component1.razor",
    "ExampleJsInterop.cs"
)

foreach ($file in $filesToRemove) {
    $filePath = Join-Path $ModuleFullPath $file
    if (Test-Path $filePath) {
        Remove-Item $filePath -Force
        Write-Host "  ? Eliminado: $file" -ForegroundColor Gray
    }
}

# Eliminar carpeta wwwroot si existe
$wwwrootPath = Join-Path $ModuleFullPath "wwwroot"
if (Test-Path $wwwrootPath) {
    Remove-Item $wwwrootPath -Recurse -Force
    Write-Host "  ? Eliminada carpeta: wwwroot" -ForegroundColor Gray
}

# ==================== 3. MODIFICAR .CSPROJ ====================

Write-Host "`n?? [3/7] Configurando referencias del proyecto..." -ForegroundColor Yellow

$csprojPath = Join-Path $ModuleFullPath "$ModuleProjectName.csproj"

$csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.*" />
    <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.0.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\Core\VRM_Plugin.Core.Abstractions\VRM_Plugin.Core.Abstractions.csproj" />
  </ItemGroup>

</Project>
"@

Set-Content -Path $csprojPath -Value $csprojContent -Force
Write-Host "? Archivo .csproj actualizado" -ForegroundColor Green

# ==================== 4. GENERAR ARCHIVOS BASE ====================

Write-Host "`n?? [4/7] Generando archivos base del módulo..." -ForegroundColor Yellow

# --- 4.1 ModuleClass.cs (SIN SERVICIO BASE) ---

$moduleClassPath = Join-Path $ModuleFullPath "${ModuleName}Module.cs"

# ?? Determinar qué componentes incluir en GetComponents()
if ($FirstComponent) {
    $firstComponentLower = $FirstComponent.ToLower()
    $getComponentsCode = @"
        return new List<ModuleComponent>
        {
            // ===== COMPONENTE RAÍZ (ACTÚA COMO CATEGORÍA) =====
            new ModuleComponent 
            { 
                IdComponent = $StartIdComponent, 
                IdModule = $IdModule, 
                IdParent = null,  // NULL = Categoría raíz en menú
                ComponentCode = "$ModuleName.Root", 
                Name = "$ModuleName", 
                Description = "Módulo principal de $ModuleName", 
                Route = "",  // Sin ruta, solo contenedor
                Icon = "$IconRoot", 
                MenuOrder = 100, 
                ShowInMenu = true, 
                ComponentType = null,  // Sin componente Blazor, solo contenedor
                ComponentTypeName = null,
                RequiredPermissionIds = new List<int> { 1 },  // Solo Admin por defecto
                IsActive = true 
            },
            
            // ===== PRIMER COMPONENTE: $FirstComponent =====
            new ModuleComponent 
            { 
                IdComponent = $($StartIdComponent + 1), 
                IdModule = $IdModule, 
                IdParent = $StartIdComponent,  // Hijo de la categoría raíz
                ComponentCode = "$ModuleName.$FirstComponent", 
                Name = "$FirstComponent", 
                Description = "Gestión de $FirstComponent", 
                Route = "/$($ModuleName.ToLower())/$firstComponentLower", 
                Icon = "$IconRoot", 
                MenuOrder = 1, 
                ShowInMenu = true, 
                ComponentType = typeof(Components.$FirstComponent), 
                ComponentTypeName = "VRM_Plugin.Modules.$ModuleName.Components.$FirstComponent",
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            }
        };
"@

    $getActionsCode = @"
        return new List<ModuleAction>
        {
            // Acciones del componente: $FirstComponent
            new ModuleAction 
            { 
                IdAction = $StartIdAction, 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.$FirstComponent.Ver", 
                Name = "Ver $FirstComponent", 
                Description = "Permite visualizar $FirstComponent", 
                IdActionType = 1,  // Lectura
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            },
            
            new ModuleAction 
            { 
                IdAction = $($StartIdAction + 1), 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.$FirstComponent.Crear", 
                Name = "Crear $FirstComponent", 
                Description = "Permite crear nuevos registros de $FirstComponent", 
                IdActionType = 2,  // Escritura
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            },
            
            new ModuleAction 
            { 
                IdAction = $($StartIdAction + 2), 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.$FirstComponent.Editar", 
                Name = "Editar $FirstComponent", 
                Description = "Permite modificar registros de $FirstComponent", 
                IdActionType = 2,  // Escritura
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            },
            
            new ModuleAction 
            { 
                IdAction = $($StartIdAction + 3), 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.$FirstComponent.Eliminar", 
                Name = "Eliminar $FirstComponent", 
                Description = "Permite eliminar registros de $FirstComponent", 
                IdActionType = 3,  // Crítica
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            }
        };
"@

    $configureServicesCode = @"
        // ? Registrar servicios de componentes hijos
        services.AddScoped<I${FirstComponent}Service, ${FirstComponent}Service>();
        
        // ?? NO hay servicio base del módulo raíz (es solo contenedor)
"@
} else {
    $getComponentsCode = @"
        return new List<ModuleComponent>
        {
            // ===== COMPONENTE RAÍZ (ACTÚA COMO CATEGORÍA) =====
            new ModuleComponent 
            { 
                IdComponent = $StartIdComponent, 
                IdModule = $IdModule, 
                IdParent = null,  // NULL = Categoría raíz en menú
                ComponentCode = "$ModuleName.Root", 
                Name = "$ModuleName", 
                Description = "Módulo principal de $ModuleName", 
                Route = "",  // Sin ruta, solo contenedor
                Icon = "$IconRoot", 
                MenuOrder = 100, 
                ShowInMenu = true, 
                ComponentType = null,  // Sin componente Blazor, solo contenedor
                ComponentTypeName = null,
                RequiredPermissionIds = new List<int> { 1 },  // Solo Admin por defecto
                IsActive = true 
            }
            
            // TODO: Agregar componentes hijos usando Add-VRMComponent.ps1
        };
"@

    $getActionsCode = @"
        return new List<ModuleAction>
        {
            // TODO: Agregar acciones cuando se agreguen componentes
        };
"@

    $configureServicesCode = @"
        // ?? NO hay servicio base del módulo raíz (es solo contenedor)
        // Los servicios se registrarán al agregar componentes con Add-VRMComponent.ps1
        
        // Ejemplo al agregar componente "Facturas":
        // services.AddScoped<IFacturaService, FacturaService>();
"@
}

$moduleClassContent = @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Entities;
$(if ($FirstComponent) { "using VRM_Plugin.Modules.$ModuleName.Services;" } else { "// using VRM_Plugin.Modules.$ModuleName.Services;  // Se descomentará al agregar servicios" })

namespace VRM_Plugin.Modules.$ModuleName;

/// <summary>
/// Módulo de $ModuleName
/// Implementa IModule para integrarse en el sistema de plugins.
/// 
/// ?? ARQUITECTURA:
/// - Este módulo es un CONTENEDOR ORGANIZACIONAL (no tiene lógica propia)
/// - Los servicios se crean SOLO para componentes hijos
/// - Ejemplo: FacturaService, PagoService (no FinanzasService)
/// </summary>
public class ${ModuleName}Module : IModule
{
    public int IdModule { get; set; } = $IdModule;
    public string ModuleName => "$ModuleName";
    public string DisplayName => "Gestión de $ModuleName";
    public string Description => "Módulo para gestionar operaciones de $ModuleName.";
    public string Version => "1.0.0";

    public List<ModuleComponent> GetComponents()
    {
$getComponentsCode
    }

    public List<ModuleAction> GetActions()
    {
$getActionsCode
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
$configureServicesCode
    }

    public bool IsEnabledForClient(string clienteId) => true;

    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine(`$"[{ModuleName}] Módulo cargado - IdModule: {IdModule}");
        Console.WriteLine(`$"[{ModuleName}] Componentes: {GetComponents().Count}, Acciones: {GetActions().Count}");
        await Task.CompletedTask;
    }
}
"@

Set-Content -Path $moduleClassPath -Value $moduleClassContent -Force
Write-Host "  ? Generado: ${ModuleName}Module.cs" -ForegroundColor Gray
if (!$FirstComponent) {
    Write-Host "    ??  Sin servicio base (módulo es solo contenedor)" -ForegroundColor DarkGray
}

# --- 4.2 _Imports.razor ---

$importsPath = Join-Path $ModuleFullPath "Components\_Imports.razor"
$importsContent = @"
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Authorization
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using VRM_Plugin.Modules.$ModuleName.Domain
@using VRM_Plugin.Modules.$ModuleName.Services

@namespace VRM_Plugin.Modules.$ModuleName.Components
"@

Set-Content -Path $importsPath -Value $importsContent -Force
Write-Host "  ? Generado: Components/_Imports.razor" -ForegroundColor Gray

# ==================== 5. CREAR PRIMER COMPONENTE (OPCIONAL) ====================

if ($FirstComponent) {
    Write-Host "`n?? [5/7] Creando primer componente: $FirstComponent..." -ForegroundColor Yellow
    
    # Llamar a Add-VRMComponent.ps1 para crear el componente con servicio
    $addComponentScriptPath = Join-Path $RootPath "Add-VRMComponent.ps1"
    
    if (Test-Path $addComponentScriptPath) {
        # Ejecutar Add-VRMComponent internamente
        & $addComponentScriptPath -ModuleName $ModuleName -ComponentName $FirstComponent -CreateEntity -CreateService
        
        Write-Host "? Primer componente '$FirstComponent' creado con éxito" -ForegroundColor Green
    } else {
        Write-Host "??  No se encontró Add-VRMComponent.ps1" -ForegroundColor Yellow
        Write-Host "    Crea el componente manualmente después" -ForegroundColor Gray
    }
} else {
    Write-Host "`n?? [5/7] Sin primer componente (módulo vacío)" -ForegroundColor Yellow
    Write-Host "    Usa Add-VRMComponent.ps1 para agregar componentes" -ForegroundColor Gray
}

# ==================== 6. COMPILAR PROYECTO ====================

Write-Host "`n?? [6/7] Compilando proyecto..." -ForegroundColor Yellow

Set-Location $ModuleFullPath

dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al compilar proyecto" -ForegroundColor Red
    Set-Location $RootPath
    exit 1
}

Write-Host "? Proyecto compilado correctamente" -ForegroundColor Green

# ==================== 7. COPIAR DLL AL HOST ====================

Write-Host "`n?? [7/7] Copiando DLL al Host..." -ForegroundColor Yellow

$sourceDll = Join-Path $ModuleFullPath "bin\Release\net8.0\$ModuleProjectName.dll"
$hostModulesPath = Join-Path $RootPath "src\Host\VRM_Plugin.Blazor.Server\bin\Debug\net8.0\Modules"

if (!(Test-Path $hostModulesPath)) {
    New-Item -ItemType Directory -Path $hostModulesPath -Force | Out-Null
}

Copy-Item $sourceDll $hostModulesPath -Force

Write-Host "? DLL copiada a: $hostModulesPath" -ForegroundColor Green

# ==================== 8. GENERAR DOCUMENTACIÓN ====================

$readmePath = Join-Path $ModuleFullPath "README.md"
$readmeContent = @"
# $ModuleName Module - VRM System

## ?? Información del Módulo

| Propiedad | Valor |
|-----------|-------|
| **Nombre** | $ModuleName |
| **ID Módulo** | $IdModule |
| **Categoría** | $Category |
| **Versión** | 1.0.0 |
$(if ($FirstComponent) { "| **Primer Componente** | $FirstComponent |" })

---

## ?? IDs Asignados

### Componentes
- **$StartIdComponent**: Categoría raíz ($ModuleName) - Solo contenedor
$(if ($FirstComponent) { "- **$($StartIdComponent + 1)**: $FirstComponent" })

### Acciones
$(if ($FirstComponent) { 
"- **$StartIdAction**: Ver $FirstComponent
- **$($StartIdAction + 1)**: Crear $FirstComponent
- **$($StartIdAction + 2)**: Editar $FirstComponent
- **$($StartIdAction + 3)**: Eliminar $FirstComponent"
} else {
"- *Pendiente: Agregar componentes primero*"
})

---

## ?? Arquitectura

?? **Este módulo NO tiene servicio base** porque es solo un contenedor organizacional.

Los servicios se crean para cada componente hijo:
$(if ($FirstComponent) {
"- ? ``I${FirstComponent}Service`` ? Lógica de $FirstComponent"
} else {
"- Ejemplo: ``IFacturaService`` ? Lógica de Facturas
- Ejemplo: ``IPagoService`` ? Lógica de Pagos"
})

---

## ?? Próximos Pasos

$(if ($FirstComponent) {
"1. Revisar y personalizar el componente ``$FirstComponent.razor``
2. Agregar más componentes:
   ``````powershell
   .\Add-VRMComponent.ps1 -ModuleName $ModuleName -ComponentName NuevoComponente -CreateEntity -CreateService
   ``````"
} else {
"1. Agregar componentes al módulo:
   ``````powershell
   .\Add-VRMComponent.ps1 -ModuleName $ModuleName -ComponentName MiComponente -CreateEntity -CreateService
   ``````"
})

3. Registrar servicios en ``ConfigureServices()`` de ``${ModuleName}Module.cs``
4. Compilar y probar

---

**Generado automáticamente por VRM Plugin Generator v2.0**
"@

Set-Content -Path $readmePath -Value $readmeContent -Force

# ==================== RESUMEN ====================

Write-Host "`n??????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  ? MÓDULO CREADO EXITOSAMENTE                          ?" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Green

Write-Host "`n?? RESUMEN:" -ForegroundColor Cyan
Write-Host "  • Nombre: $ModuleName" -ForegroundColor White
Write-Host "  • ID Módulo: $IdModule" -ForegroundColor White
Write-Host "  • Categoría: $Category" -ForegroundColor White
if ($FirstComponent) {
    Write-Host "  • Primer Componente: $FirstComponent" -ForegroundColor White
    Write-Host "  • Ruta: /$($ModuleName.ToLower())/$($FirstComponent.ToLower())" -ForegroundColor White
} else {
    Write-Host "  • Estado: Módulo vacío (solo contenedor)" -ForegroundColor Yellow
}
Write-Host "  • Ubicación: $ModuleFullPath" -ForegroundColor White

Write-Host "`n?? ARQUITECTURA:" -ForegroundColor Cyan
if ($FirstComponent) {
    Write-Host "  ? Módulo creado CON primer componente '$FirstComponent'" -ForegroundColor Green
    Write-Host "  ? Servicio I${FirstComponent}Service registrado" -ForegroundColor Green
} else {
    Write-Host "  ??  Módulo creado SIN componentes (solo contenedor)" -ForegroundColor Yellow
    Write-Host "  ??  Sin servicio base (no es necesario)" -ForegroundColor Gray
}

Write-Host "`n?? PRÓXIMOS PASOS:" -ForegroundColor Cyan

if ($FirstComponent) {
    Write-Host "  1. Personalizar componente $FirstComponent.razor" -ForegroundColor Yellow
    Write-Host "`n  2. Agregar más componentes (opcional):" -ForegroundColor Yellow
    Write-Host "     .\Add-VRMComponent.ps1 -ModuleName $ModuleName -ComponentName NuevoComponente -CreateEntity -CreateService" -ForegroundColor White
} else {
    Write-Host "  1. Agregar componentes al módulo:" -ForegroundColor Yellow
    Write-Host "     .\Add-VRMComponent.ps1 -ModuleName $ModuleName -ComponentName MiComponente -CreateEntity -CreateService" -ForegroundColor White
}

Write-Host "`n  $(if ($FirstComponent) { '3' } else { '2' }). Reiniciar la aplicación VRM:" -ForegroundColor Yellow
Write-Host "     cd src\Host\VRM_Plugin.Blazor.Server" -ForegroundColor White
Write-Host "     dotnet run" -ForegroundColor White

if ($FirstComponent) {
    Write-Host "`n  4. Navegar a: http://localhost:5000/$($ModuleName.ToLower())/$($FirstComponent.ToLower())" -ForegroundColor Yellow
}

Write-Host "`n? ¡Módulo listo para desarrollo!" -ForegroundColor Green

Set-Location $RootPath
