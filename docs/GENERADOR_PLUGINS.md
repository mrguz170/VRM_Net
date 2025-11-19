# ?? Script de Generación de Nuevos Plugins VRM

## ?? Tabla de Contenidos
1. [Introducción](#introducción)
2. [PowerShell Script Completo](#powershell-script-completo)
3. [Guía de Uso](#guía-de-uso)
4. [Ejemplos de Uso](#ejemplos-de-uso)
5. [Estructura Generada](#estructura-generada)

---

## ?? Introducción

Este script automatiza completamente la creación de nuevos módulos VRM sin incluir la palabra "Demo" en ningún namespace o nombre de archivo.

### **Características**

? **Sin palabra "Demo"**: Nombres limpios (VRM_Plugin.Modules.XXX)  
? **Estructura completa**: Domain, Services, Components  
? **IDs únicos**: Genera IDs para módulo, componentes y acciones  
? **Plantillas predefinidas**: Código funcional desde el inicio  
? **Build automático**: Compila y copia DLL al Host  
? **Documentación**: Genera README.md del módulo  

---

## ?? PowerShell Script Completo

Guarda este archivo como `New-VRMPlugin.ps1` en la raíz del proyecto:

```powershell
<#
.SYNOPSIS
    Genera un nuevo módulo de plugin VRM sin la palabra "Demo"

.DESCRIPTION
    Crea la estructura completa de un módulo VRM:
    - Proyecto Razor Class Library
    - Carpetas: Domain, Services, Components
    - Archivos base con IDs únicos
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

.EXAMPLE
    .\New-VRMPlugin.ps1 -ModuleName Inventario -IdModule 3 -Category Operaciones -StartIdComponent 100 -StartIdAction 100 -IconRoot "ri-box-line"

.EXAMPLE
    .\New-VRMPlugin.ps1 -ModuleName Ventas -IdModule 4 -Category Comercial -StartIdComponent 200 -StartIdAction 200 -IconRoot "ri-shopping-cart-line"
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
    [string]$IconRoot
)

# ==================== CONFIGURACIÓN ====================

$RootPath = Get-Location
$ModulesPath = Join-Path $RootPath "src\Modules\$Category"
$ModuleProjectName = "VRM_Plugin.Modules.$ModuleName"
$ModuleFullPath = Join-Path $ModulesPath $ModuleProjectName

Write-Host "`n????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?  ?? GENERADOR DE PLUGINS VRM                            ?" -ForegroundColor Cyan
Write-Host "?  Módulo: $ModuleName".PadRight(60) + "?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

# ==================== 1. CREAR PROYECTO ====================

Write-Host "?? [1/7] Creando proyecto Razor Class Library..." -ForegroundColor Yellow

if (!(Test-Path $ModulesPath)) {
    New-Item -ItemType Directory -Path $ModulesPath -Force | Out-Null
}

Set-Location $ModulesPath

dotnet new razorclasslib -n $ModuleProjectName -o $ModuleProjectName --framework net8.0

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al crear proyecto" -ForegroundColor Red
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
    "ExampleJsInterop.cs",
    "wwwroot\background.png",
    "wwwroot\exampleJsInterop.js"
)

foreach ($file in $filesToRemove) {
    $filePath = Join-Path $ModuleFullPath $file
    if (Test-Path $filePath) {
        Remove-Item $filePath -Force
        Write-Host "  ? Eliminado: $file" -ForegroundColor Gray
    }
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

Write-Host "`n?? [4/7] Generando archivos base..." -ForegroundColor Yellow

# --- 4.1 ModuleClass.cs ---

$moduleClassPath = Join-Path $ModuleFullPath "${ModuleName}Module.cs"
$moduleClassContent = @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Entities;
using VRM_Plugin.Modules.$ModuleName.Services;

namespace VRM_Plugin.Modules.$ModuleName;

/// <summary>
/// Módulo de $ModuleName
/// Implementa IModule para integrarse en el sistema de plugins.
/// ? Arquitectura con IDs numéricos y permisos separados.
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
            
            // ===== SUBMENÚ: PRINCIPAL =====
            new ModuleComponent 
            { 
                IdComponent = $($StartIdComponent + 1), 
                IdModule = $IdModule, 
                IdParent = $StartIdComponent,  // Hijo de la categoría raíz
                ComponentCode = "$ModuleName.Principal", 
                Name = "$ModuleName", 
                Description = "Vista principal de $ModuleName", 
                Route = "/$($ModuleName.ToLower())", 
                Icon = "$IconRoot", 
                MenuOrder = 1, 
                ShowInMenu = true, 
                ComponentType = typeof(Components.$ModuleName), 
                ComponentTypeName = "$ModuleName.Components.$ModuleName",
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            }
        };
    }

    public List<ModuleAction> GetActions()
    {
        return new List<ModuleAction>
        {
            new ModuleAction 
            { 
                IdAction = $StartIdAction, 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.Ver", 
                Name = "Ver $ModuleName", 
                Description = "Permite visualizar $ModuleName", 
                IdActionType = 1,  // Lectura
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            },
            
            new ModuleAction 
            { 
                IdAction = $($StartIdAction + 1), 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.Crear", 
                Name = "Crear $ModuleName", 
                Description = "Permite crear nuevos registros", 
                IdActionType = 2,  // Escritura
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            },
            
            new ModuleAction 
            { 
                IdAction = $($StartIdAction + 2), 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.Editar", 
                Name = "Editar $ModuleName", 
                Description = "Permite modificar registros existentes", 
                IdActionType = 2,  // Escritura
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            },
            
            new ModuleAction 
            { 
                IdAction = $($StartIdAction + 3), 
                IdComponent = $($StartIdComponent + 1), 
                ActionKey = "$ModuleName.Eliminar", 
                Name = "Eliminar $ModuleName", 
                Description = "Permite eliminar registros", 
                IdActionType = 3,  // Crítica
                RequiredPermissionIds = new List<int> { 1 }, 
                IsActive = true 
            }
        };
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<I${ModuleName}Service, ${ModuleName}Service>();
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

# --- 4.2 Domain Model ---

$domainModelPath = Join-Path $ModuleFullPath "Domain\${ModuleName}Item.cs"
$domainModelContent = @"
namespace VRM_Plugin.Modules.$ModuleName.Domain;

/// <summary>
/// Entidad principal del módulo $ModuleName
/// </summary>
public class ${ModuleName}Item
{
    public int Id { get; set; }
    
    public string Nombre { get; set; } = string.Empty;
    
    public string Descripcion { get; set; } = string.Empty;
    
    public bool Activo { get; set; } = true;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public string CreadoPor { get; set; } = string.Empty;
}
"@

Set-Content -Path $domainModelPath -Value $domainModelContent -Force
Write-Host "  ? Generado: Domain/${ModuleName}Item.cs" -ForegroundColor Gray

# --- 4.3 Service Interface ---

$serviceInterfacePath = Join-Path $ModuleFullPath "Services\I${ModuleName}Service.cs"
$serviceInterfaceContent = @"
using VRM_Plugin.Modules.$ModuleName.Domain;

namespace VRM_Plugin.Modules.$ModuleName.Services;

/// <summary>
/// Interfaz del servicio de $ModuleName
/// </summary>
public interface I${ModuleName}Service
{
    Task<List<${ModuleName}Item>> GetAllAsync();
    
    Task<${ModuleName}Item?> GetByIdAsync(int id);
    
    Task<${ModuleName}Item> CreateAsync(${ModuleName}Item item);
    
    Task<${ModuleName}Item> UpdateAsync(${ModuleName}Item item);
    
    Task<bool> DeleteAsync(int id);
}
"@

Set-Content -Path $serviceInterfacePath -Value $serviceInterfaceContent -Force
Write-Host "  ? Generado: Services/I${ModuleName}Service.cs" -ForegroundColor Gray

# --- 4.4 Service Implementation ---

$serviceImplPath = Join-Path $ModuleFullPath "Services\${ModuleName}Service.cs"
$serviceImplContent = @"
using VRM_Plugin.Modules.$ModuleName.Domain;

namespace VRM_Plugin.Modules.$ModuleName.Services;

/// <summary>
/// Implementación del servicio de $ModuleName
/// TODO: Conectar con base de datos real
/// </summary>
public class ${ModuleName}Service : I${ModuleName}Service
{
    private readonly List<${ModuleName}Item> _items = new();
    
    public Task<List<${ModuleName}Item>> GetAllAsync()
    {
        return Task.FromResult(_items);
    }
    
    public Task<${ModuleName}Item?> GetByIdAsync(int id)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item);
    }
    
    public Task<${ModuleName}Item> CreateAsync(${ModuleName}Item item)
    {
        item.Id = _items.Count > 0 ? _items.Max(x => x.Id) + 1 : 1;
        item.FechaCreacion = DateTime.UtcNow;
        _items.Add(item);
        return Task.FromResult(item);
    }
    
    public Task<${ModuleName}Item> UpdateAsync(${ModuleName}Item item)
    {
        var existing = _items.FirstOrDefault(x => x.Id == item.Id);
        if (existing != null)
        {
            var index = _items.IndexOf(existing);
            _items[index] = item;
        }
        return Task.FromResult(item);
    }
    
    public Task<bool> DeleteAsync(int id)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            _items.Remove(item);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
"@

Set-Content -Path $serviceImplPath -Value $serviceImplContent -Force
Write-Host "  ? Generado: Services/${ModuleName}Service.cs" -ForegroundColor Gray

# --- 4.5 Razor Component ---

$componentPath = Join-Path $ModuleFullPath "Components\$ModuleName.razor"
$componentContent = @"
@page "/$($ModuleName.ToLower())"
@attribute [Authorize]
@rendermode InteractiveServer
@inject I${ModuleName}Service ${ModuleName}Service

<PageTitle>$ModuleName</PageTitle>

<h1>?? Gestión de $ModuleName</h1>

<div class="alert alert-info mt-4">
    <h4>Módulo de $ModuleName</h4>
    <p>Este módulo permite gestionar operaciones relacionadas con $ModuleName.</p>
</div>

<div class="row mt-4">
    <div class="col-md-4">
        <div class="card">
            <div class="card-header bg-success text-white">
                <h5>? Crear Nuevo</h5>
            </div>
            <div class="card-body">
                <EditForm Model="nuevoItem" OnValidSubmit="CrearItem">
                    <div class="mb-3">
                        <label class="form-label">Nombre</label>
                        <InputText @bind-Value="nuevoItem.Nombre" class="form-control" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Descripción</label>
                        <InputTextArea @bind-Value="nuevoItem.Descripcion" class="form-control" rows="3" />
                    </div>
                    <button type="submit" class="btn btn-success w-100">Crear</button>
                </EditForm>
            </div>
        </div>
    </div>

    <div class="col-md-8">
        <div class="card">
            <div class="card-header bg-primary text-white">
                <h5>?? Lista de $ModuleName</h5>
            </div>
            <div class="card-body">
                @if (items == null)
                {
                    <p>Cargando...</p>
                }
                else if (!items.Any())
                {
                    <div class="alert alert-info">
                        No hay registros. Crea uno usando el formulario.
                    </div>
                }
                else
                {
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Nombre</th>
                                <th>Descripción</th>
                                <th>Fecha Creación</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            @foreach (var item in items)
                            {
                                <tr>
                                    <td>@item.Id</td>
                                    <td>@item.Nombre</td>
                                    <td>@item.Descripcion</td>
                                    <td>@item.FechaCreacion.ToShortDateString()</td>
                                    <td>
                                        <button class="btn btn-sm btn-danger" @onclick="() => EliminarItem(item.Id)">
                                            Eliminar
                                        </button>
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>
                }
            </div>
        </div>
    </div>
</div>

@code {
    private List<${ModuleName}Item>? items;
    private ${ModuleName}Item nuevoItem = new();

    protected override async Task OnInitializedAsync()
    {
        await CargarItems();
    }

    private async Task CargarItems()
    {
        items = await ${ModuleName}Service.GetAllAsync();
    }

    private async Task CrearItem()
    {
        nuevoItem.CreadoPor = "Sistema";
        await ${ModuleName}Service.CreateAsync(nuevoItem);
        nuevoItem = new ${ModuleName}Item();
        await CargarItems();
    }

    private async Task EliminarItem(int id)
    {
        await ${ModuleName}Service.DeleteAsync(id);
        await CargarItems();
    }
}
"@

Set-Content -Path $componentPath -Value $componentContent -Force
Write-Host "  ? Generado: Components/$ModuleName.razor" -ForegroundColor Gray

# --- 4.6 _Imports.razor ---

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

# ==================== 5. COMPILAR PROYECTO ====================

Write-Host "`n?? [5/7] Compilando proyecto..." -ForegroundColor Yellow

Set-Location $ModuleFullPath

dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al compilar proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "? Proyecto compilado correctamente" -ForegroundColor Green

# ==================== 6. COPIAR DLL AL HOST ====================

Write-Host "`n?? [6/7] Copiando DLL al Host..." -ForegroundColor Yellow

$sourceDll = Join-Path $ModuleFullPath "bin\Release\net8.0\$ModuleProjectName.dll"
$hostModulesPath = Join-Path $RootPath "src\Host\VRM_Plugin.Blazor.Server\bin\Debug\net8.0\Modules"

if (!(Test-Path $hostModulesPath)) {
    New-Item -ItemType Directory -Path $hostModulesPath -Force | Out-Null
}

Copy-Item $sourceDll $hostModulesPath -Force

Write-Host "? DLL copiada a: $hostModulesPath" -ForegroundColor Green

# ==================== 7. GENERAR DOCUMENTACIÓN ====================

Write-Host "`n?? [7/7] Generando documentación..." -ForegroundColor Yellow

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
| **Ruta Principal** | /$($ModuleName.ToLower()) |

---

## ??? Estructura del Proyecto

``````
VRM_Plugin.Modules.$ModuleName/
??? ${ModuleName}Module.cs          # Implementación de IModule
??? Domain/
?   ??? ${ModuleName}Item.cs        # Entidad principal
??? Services/
?   ??? I${ModuleName}Service.cs    # Interfaz del servicio
?   ??? ${ModuleName}Service.cs     # Implementación del servicio
??? Components/
    ??? _Imports.razor              # Importaciones comunes
    ??? $ModuleName.razor           # Componente principal Blazor
``````

---

## ?? IDs Asignados

### Componentes
- **$StartIdComponent**: Categoría raíz ($ModuleName)
- **$($StartIdComponent + 1)**: Vista principal

### Acciones
- **$StartIdAction**: Ver $ModuleName
- **$($StartIdAction + 1)**: Crear $ModuleName
- **$($StartIdAction + 2)**: Editar $ModuleName
- **$($StartIdAction + 3)**: Eliminar $ModuleName

---

## ?? Cómo Usar

### 1. Compilar Módulo

``````bash
dotnet build src/Modules/$Category/$ModuleProjectName/ --configuration Release
``````

### 2. Copiar DLL al Host

``````bash
copy "src/Modules/$Category/$ModuleProjectName/bin/Release/net8.0/$ModuleProjectName.dll" "src/Host/VRM_Plugin.Blazor.Server/bin/Debug/net8.0/Modules/"
``````

### 3. Reiniciar Aplicación

``````bash
cd src/Host/VRM_Plugin.Blazor.Server
dotnet run
``````

---

## ?? Personalización

### Agregar Nuevo Componente

1. Crear archivo Razor en `Components/`
2. Agregar entrada en `${ModuleName}Module.GetComponents()`
3. Asignar ID único de componente

### Agregar Nueva Acción

1. Crear acción en `${ModuleName}Module.GetActions()`
2. Asignar ID único de acción
3. Definir permisos requeridos

### Conectar con Base de Datos

Modificar `${ModuleName}Service.cs` para usar Entity Framework:

``````csharp
public class ${ModuleName}Service : I${ModuleName}Service
{
    private readonly ApplicationDbContext _context;
    
    public ${ModuleName}Service(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<${ModuleName}Item>> GetAllAsync()
    {
        return await _context.${ModuleName}Items.ToListAsync();
    }
    
    // ... resto de métodos
}
``````

---

## ? Checklist de Desarrollo

- [ ] Definir entidades de dominio adicionales
- [ ] Implementar lógica de negocio en servicios
- [ ] Agregar validaciones a componentes Blazor
- [ ] Configurar permisos granulares
- [ ] Crear tests unitarios
- [ ] Documentar endpoints o APIs
- [ ] Configurar migraciones de base de datos

---

**Generado automáticamente por VRM Plugin Generator**
"@

Set-Content -Path $readmePath -Value $readmeContent -Force
Write-Host "? Documentación generada: README.md" -ForegroundColor Green

# ==================== RESUMEN ====================

Write-Host "`n????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  ? MÓDULO CREADO EXITOSAMENTE                          ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Green

Write-Host "`n?? RESUMEN:" -ForegroundColor Cyan
Write-Host "  • Nombre: $ModuleName" -ForegroundColor White
Write-Host "  • ID Módulo: $IdModule" -ForegroundColor White
Write-Host "  • Categoría: $Category" -ForegroundColor White
Write-Host "  • Ruta: /$($ModuleName.ToLower())" -ForegroundColor White
Write-Host "  • Ubicación: $ModuleFullPath" -ForegroundColor White

Write-Host "`n?? ARCHIVOS GENERADOS:" -ForegroundColor Cyan
Write-Host "  ? ${ModuleName}Module.cs" -ForegroundColor Gray
Write-Host "  ? Domain/${ModuleName}Item.cs" -ForegroundColor Gray
Write-Host "  ? Services/I${ModuleName}Service.cs" -ForegroundColor Gray
Write-Host "  ? Services/${ModuleName}Service.cs" -ForegroundColor Gray
Write-Host "  ? Components/$ModuleName.razor" -ForegroundColor Gray
Write-Host "  ? Components/_Imports.razor" -ForegroundColor Gray
Write-Host "  ? README.md" -ForegroundColor Gray

Write-Host "`n?? PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "  1. Reiniciar la aplicación VRM:" -ForegroundColor Yellow
Write-Host "     cd src\Host\VRM_Plugin.Blazor.Server" -ForegroundColor White
Write-Host "     dotnet run" -ForegroundColor White
Write-Host "`n  2. Navegar a: http://localhost:5000/$($ModuleName.ToLower())" -ForegroundColor Yellow
Write-Host "`n  3. Personalizar el módulo según tus necesidades" -ForegroundColor Yellow

Write-Host "`n? ¡Módulo listo para usar!" -ForegroundColor Green

Set-Location $RootPath
```

---

## ?? Guía de Uso

### **Requisitos Previos**

1. PowerShell 5.1 o superior
2. .NET 8 SDK instalado
3. Estar en la raíz del proyecto VRM

### **Sintaxis**

```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName <NombreDelModulo> `
    -IdModule <IDNumerico> `
    -Category <Categoria> `
    -StartIdComponent <IDInicialComponentes> `
    -StartIdAction <IDInicialAcciones> `
    -IconRoot <IconoRemixIcon>
```

### **Parámetros**

| Parámetro | Descripción | Ejemplo |
|-----------|-------------|---------|
| `ModuleName` | Nombre del módulo (sin espacios) | `Inventario` |
| `IdModule` | ID numérico único (1-999) | `3` |
| `Category` | Categoría organizacional | `Operaciones` |
| `StartIdComponent` | ID inicial para componentes | `100` |
| `StartIdAction` | ID inicial para acciones | `100` |
| `IconRoot` | Icono Remix Icon | `ri-box-line` |

---

## ?? Ejemplos de Uso

### **Ejemplo 1: Módulo de Inventario**

```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName "Inventario" `
    -IdModule 3 `
    -Category "Operaciones" `
    -StartIdComponent 100 `
    -StartIdAction 100 `
    -IconRoot "ri-box-line"
```

**Resultado:**
- ? `VRM_Plugin.Modules.Inventario`
- ? Ruta: `/inventario`
- ? ID Módulo: 3
- ? Componentes: 100, 101
- ? Acciones: 100, 101, 102, 103

---

### **Ejemplo 2: Módulo de Ventas**

```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName "Ventas" `
    -IdModule 4 `
    -Category "Comercial" `
    -StartIdComponent 200 `
    -StartIdAction 200 `
    -IconRoot "ri-shopping-cart-line"
```

**Resultado:**
- ? `VRM_Plugin.Modules.Ventas`
- ? Ruta: `/ventas`
- ? ID Módulo: 4
- ? Componentes: 200, 201
- ? Acciones: 200, 201, 202, 203

---

### **Ejemplo 3: Módulo de RRHH**

```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName "RRHH" `
    -IdModule 5 `
    -Category "Administracion" `
    -StartIdComponent 300 `
    -StartIdAction 300 `
    -IconRoot "ri-team-line"
```

**Resultado:**
- ? `VRM_Plugin.Modules.RRHH`
- ? Ruta: `/rrhh`
- ? ID Módulo: 5
- ? Componentes: 300, 301
- ? Acciones: 300, 301, 302, 303

---

## ?? Estructura Generada

```
src/Modules/{Category}/VRM_Plugin.Modules.{ModuleName}/
?
??? VRM_Plugin.Modules.{ModuleName}.csproj    # Proyecto Razor Class Library
?
??? {ModuleName}Module.cs                     # ? Implementa IModule
?   ??? GetComponents()                       # Define componentes UI
?   ??? GetActions()                          # Define acciones granulares
?   ??? ConfigureServices()                   # Registra servicios DI
?
??? Domain/
?   ??? {ModuleName}Item.cs                   # Entidad principal
?
??? Services/
?   ??? I{ModuleName}Service.cs               # Interfaz del servicio
?   ??? {ModuleName}Service.cs                # Implementación (mock)
?
??? Components/
?   ??? _Imports.razor                        # Importaciones comunes
?   ??? {ModuleName}.razor                    # Componente Blazor principal
?       ??? @page "/{modulename}"            # Ruta dinámica
?       ??? Formulario de creación
?       ??? Tabla de listado
?
??? README.md                                  # Documentación del módulo
```

---

## ?? Características del Script

### **1. Nombres Limpios (Sin "Demo")**

? **Antes**: `VRM_PluginDemo.Modules.Inventario`  
? **Después**: `VRM_Plugin.Modules.Inventario`

### **2. IDs Únicos Automáticos**

El script genera IDs secuenciales:

- **Componente Raíz**: `StartIdComponent`
- **Componente Principal**: `StartIdComponent + 1`
- **Acción Ver**: `StartIdAction`
- **Acción Crear**: `StartIdAction + 1`
- **Acción Editar**: `StartIdAction + 2`
- **Acción Eliminar**: `StartIdAction + 3`

### **3. Código Funcional Desde el Inicio**

? Servicio mock implementado  
? Componente Blazor con CRUD básico  
? Formulario de creación funcional  
? Tabla de listado responsive  

### **4. Compilación y Despliegue Automático**

? Compila el proyecto en Release  
? Copia DLL al Host automáticamente  
? Listo para usar después de reiniciar

### **5. Documentación Incluida**

? Genera `README.md` completo  
? Lista IDs asignados  
? Instrucciones de personalización  
? Checklist de desarrollo

---

## ?? Flujo del Script

```
???????????????????????????????????????
?  EJECUTAR SCRIPT                    ?
?  New-VRMPlugin.ps1                  ?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  1. CREAR PROYECTO                  ?
?  dotnet new razorclasslib           ?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  2. ESTRUCTURA DE CARPETAS          ?
?  Domain/, Services/, Components/    ?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  3. CONFIGURAR REFERENCIAS          ?
?  Modificar .csproj                  ?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  4. GENERAR ARCHIVOS BASE           ?
?  - ModuleClass.cs                   ?
?  - Domain models                    ?
?  - Services                         ?
?  - Razor components                 ?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  5. COMPILAR PROYECTO               ?
?  dotnet build --configuration Release?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  6. COPIAR DLL AL HOST              ?
?  Copy-Item *.dll ? Host/Modules/    ?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  7. GENERAR DOCUMENTACIÓN           ?
?  README.md con IDs e instrucciones  ?
???????????????????????????????????????
             ?
             ?
???????????????????????????????????????
?  ? MÓDULO LISTO                    ?
?  Reiniciar app y navegar a ruta     ?
???????????????????????????????????????
```

---

## ? Comandos Rápidos

### **Crear Múltiples Módulos**

```powershell
# Inventario
.\New-VRMPlugin.ps1 -ModuleName "Inventario" -IdModule 3 -Category "Operaciones" -StartIdComponent 100 -StartIdAction 100 -IconRoot "ri-box-line"

# Ventas
.\New-VRMPlugin.ps1 -ModuleName "Ventas" -IdModule 4 -Category "Comercial" -StartIdComponent 200 -StartIdAction 200 -IconRoot "ri-shopping-cart-line"

# RRHH
.\New-VRMPlugin.ps1 -ModuleName "RRHH" -IdModule 5 -Category "Administracion" -StartIdComponent 300 -StartIdAction 300 -IconRoot "ri-team-line"

# Compras
.\New-VRMPlugin.ps1 -ModuleName "Compras" -IdModule 6 -Category "Operaciones" -StartIdComponent 400 -StartIdAction 400 -IconRoot "ri-shopping-bag-line"
```

### **Compilar Todos los Módulos**

```powershell
Get-ChildItem -Path "src\Modules" -Filter "*.csproj" -Recurse | ForEach-Object {
    dotnet build $_.FullName --configuration Release
}
```

### **Copiar Todas las DLLs al Host**

```powershell
$targetPath = "src\Host\VRM_Plugin.Blazor.Server\bin\Debug\net8.0\Modules"
Get-ChildItem -Path "src\Modules" -Filter "VRM_Plugin.Modules.*.dll" -Recurse | 
    Where-Object { $_.Directory.Name -eq "net8.0" -and $_.Directory.Parent.Name -eq "Release" } |
    Copy-Item -Destination $targetPath -Force
```

---

## ?? Tips y Mejores Prácticas

### **1. Nomenclatura de IDs**

```
Módulo 1 (Finanzas):      IdModule=1,   Components: 1-99,    Actions: 1-99
Módulo 2 (Prospectos):    IdModule=2,   Components: 100-199, Actions: 100-199
Módulo 3 (Inventario):    IdModule=3,   Components: 200-299, Actions: 200-299
Módulo 4 (Ventas):        IdModule=4,   Components: 300-399, Actions: 300-399
```

### **2. Categorías Recomendadas**

- `Finanzas` - Módulos financieros
- `Operaciones` - Logística, inventario
- `Comercial` - Ventas, CRM
- `Administracion` - RRHH, configuración
- `Onboarding` - Proveedores, clientes

### **3. Iconos Remix Útiles**

- `ri-box-line` - Inventario
- `ri-shopping-cart-line` - Ventas
- `ri-team-line` - RRHH
- `ri-money-dollar-circle-line` - Finanzas
- `ri-truck-line` - Logística
- `ri-file-list-3-line` - Documentos

---

## ? Checklist Post-Generación

Después de generar un módulo:

- [ ] Reiniciar aplicación VRM
- [ ] Verificar que el módulo aparece en el menú
- [ ] Probar navegación a la ruta principal
- [ ] Personalizar entidades de dominio
- [ ] Implementar lógica de negocio en servicios
- [ ] Agregar validaciones a formularios
- [ ] Configurar permisos granulares
- [ ] Crear componentes adicionales si es necesario
- [ ] Conectar con base de datos real
- [ ] Agregar tests unitarios

---

**¡Ahora puedes generar nuevos plugins VRM en segundos!** ??
