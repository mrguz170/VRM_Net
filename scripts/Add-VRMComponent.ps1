<#
.SYNOPSIS
    Agrega un nuevo componente Blazor a un módulo VRM existente.

.DESCRIPTION
    Crea componente .razor, entidad (opcional), service (por defecto), repository + DTO (opcional).
    Convierte automáticamente el nombre plural a singular para DTO/Entity/Repository/Service y pregunta solo si la conversión es ambigua.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName,

    [Parameter(Mandatory=$true)]
    [string]$ComponentName,

    [Parameter(Mandatory=$false)]
    [bool]$CreateEntity = $false,

    # Crear servicio por defecto (true). Pasar -CreateService:$false para omitir.
    [Parameter(Mandatory=$false)]
    [bool]$CreateService = $true,

    # Crear repository y DTO (opcional)
    [Parameter(Mandatory=$false)]
    [bool]$CreateRepository = $false,

    # Forzar nombre singular si se desea (opcional)
    [Parameter(Mandatory=$false)]
    [string]$SingularName
)

# ==================== Helpers ====================
function To-Singular([string]$name) {
    if ([string]::IsNullOrWhiteSpace($name)) { return $name }
    $n = $name.Trim()

    # Reglas simples (ES + EN heurísticas)
    if ($n.Length -gt 3 -and $n.ToLower().EndsWith("ces")) {
        return $n.Substring(0, $n.Length - 3) + "z"
    }
    if ($n.Length -gt 3 -and $n.ToLower().EndsWith("ies")) {
        return $n.Substring(0, $n.Length - 3) + "y"
    }
    if ($n.Length -gt 2 -and $n.ToLower().EndsWith("es")) {
        return $n.Substring(0, $n.Length - 2)
    }
    if ($n.Length -gt 1 -and $n.ToLower().EndsWith("s")) {
        return $n.Substring(0, $n.Length - 1)
    }
    return $n
}

# ==================== Preparación ====================
$RootPath = Get-Location
$ModulesPath = Join-Path $RootPath "src\Modules"

# Buscar el módulo en todas las subcarpetas
$ModuleProjectName = "VRM_Plugin.Module.$ModuleName"
$ModuleFullPath = Get-ChildItem -Path $ModulesPath -Recurse -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -eq $ModuleProjectName } |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $ModuleFullPath) {
    Write-Host "? ERROR: No se encontró el módulo '$ModuleName' en $ModulesPath" -ForegroundColor Red
    exit 1
}

# Determinar nombre base en singular (para DTO/Entity/Repo/Service)
if ($SingularName) {
    $BaseName = $SingularName.Trim()
} else {
    $BaseName = To-Singular $ComponentName
    # Si no cambió mucho, pedir confirmación
    if ($BaseName -eq $ComponentName -or ($ComponentName.Length - $BaseName.Length) -lt 1) {
        $resp = Read-Host "Usar '$BaseName' como nombre singular para DTO/Entity/Repo/Service? (S/n)"
        if ($resp -match '^[Nn]') {
            $entered = Read-Host "Introduce el nombre en singular (ej: Factura)"
            if (-not [string]::IsNullOrWhiteSpace($entered)) { $BaseName = $entered.Trim() }
        }
    }
}

Write-Host "`n???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? AGREGAR COMPONENTE A MÓDULO VRM" -ForegroundColor Cyan
Write-Host "  Módulo: $ModuleName" -ForegroundColor Cyan
Write-Host "  Componente UI: $ComponentName" -ForegroundColor Cyan
Write-Host "  Base (singular): $BaseName" -ForegroundColor Cyan
Write-Host "  CrearEntity: $CreateEntity, CreateService: $CreateService, CreateRepository: $CreateRepository" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

# ==================== 1. CREAR ENTIDAD (OPCIONAL) ====================
if ($CreateEntity) {
    Write-Host "?? [1] Creando entidad de dominio..." -ForegroundColor Yellow

    $entityFolder = Join-Path $ModuleFullPath "Domain"
    if (!(Test-Path $entityFolder)) { New-Item -ItemType Directory -Path $entityFolder -Force | Out-Null }

    $entityPath = Join-Path $entityFolder "${BaseName}.cs"
    if (Test-Path $entityPath) {
        Write-Host "??  La entidad ya existe: $($entityPath.Substring($ModuleFullPath.Length + 1))" -ForegroundColor Yellow
    } else {
        $entityContent = @"
namespace VRM_Plugin.Module.$ModuleName.Domain;

/// <summary>
/// Entidad $BaseName del módulo $ModuleName
/// </summary>
public class $BaseName
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
"@
        Set-Content -Path $entityPath -Value $entityContent -Encoding UTF8 -Force
        Write-Host "? Entidad creada: Domain/$BaseName.cs" -ForegroundColor Green
    }
}

# ==================== 2. CREAR SERVICIO (RECOMENDADO) ====================
if ($CreateService) {
    Write-Host "`n??  [2] Creando service e interfaz..." -ForegroundColor Yellow

    $servicesFolder = Join-Path $ModuleFullPath "Services"
    if (!(Test-Path $servicesFolder)) { New-Item -ItemType Directory -Path $servicesFolder -Force | Out-Null }

    $interfacePath = Join-Path $servicesFolder "I${BaseName}Service.cs"
    if (Test-Path $interfacePath) {
        Write-Host "??  La interfaz ya existe: Services/I${BaseName}Service.cs" -ForegroundColor Yellow
    } else {
        $interfaceContent = @"
using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.$ModuleName.Domain;
using VRM_Plugin.Module.$ModuleName.Data.DTOs;

namespace VRM_Plugin.Module.$ModuleName.Services;

/// <summary>
/// Servicio base para gestionar $BaseName
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public interface I${BaseName}Service
{
    Task<List<${BaseName}Dto>> GetAllAsync();
    Task<${BaseName}Dto?> GetByIdAsync(int id);
    Task<${BaseName}Dto> CreateAsync(${BaseName}Dto dto);
    Task<${BaseName}Dto> UpdateAsync(${BaseName}Dto dto);
    Task<bool> DeleteAsync(int id);
}
"@
        Set-Content -Path $interfacePath -Value $interfaceContent -Encoding UTF8 -Force
        Write-Host "? Interfaz creada: Services/I${BaseName}Service.cs" -ForegroundColor Green
    }

    $servicePath = Join-Path $servicesFolder "${BaseName}Service.cs"
    if (Test-Path $servicePath) {
        Write-Host "??  La implementación ya existe: Services/${BaseName}Service.cs" -ForegroundColor Yellow
    } else {
        $serviceContent = @"
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRM_Plugin.Module.$ModuleName.Data.DTOs;
using VRM_Plugin.Module.$ModuleName.Data.Repositories;
using VRM_Plugin.Module.$ModuleName.Domain;

namespace VRM_Plugin.Module.$ModuleName.Services;

/// <summary>
/// Implementación base del servicio de $BaseName
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public class ${BaseName}Service : I${BaseName}Service
{
    private readonly I${BaseName}Repository? _repo;

    public ${BaseName}Service(I${BaseName}Repository? repo = null)
    {
        _repo = repo;
    }

    public async Task<List<${BaseName}Dto>> GetAllAsync()
    {
        if (_repo != null) return await _repo.GetSampleAsync();
        return await Task.FromResult(new List<${BaseName}Dto>());
    }

    public async Task<${BaseName}Dto?> GetByIdAsync(int id)
    {
        var list = await GetAllAsync();
        return list.FirstOrDefault(x => x.Id == id);
    }

    public async Task<${BaseName}Dto> CreateAsync(${BaseName}Dto dto)
    {
        
        return await Task.FromResult(dto);
    }

    public async Task<${BaseName}Dto> UpdateAsync(${BaseName}Dto dto)
    {
        
        return await Task.FromResult(dto);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        
        return await Task.FromResult(false);
    }
}
"@
        Set-Content -Path $servicePath -Value $serviceContent -Encoding UTF8 -Force
        Write-Host "? Servicio creado: Services/${BaseName}Service.cs" -ForegroundColor Green
    }
}

# ==================== 2.1 CREAR REPOSITORY + DTO (OPCIONAL) ====================
if ($CreateRepository) {
    Write-Host "`n??  [2.1] Creando repository y DTO..." -ForegroundColor Yellow

    # DTO
    $dtoFolder = Join-Path $ModuleFullPath "Data\DTOs"
    if (!(Test-Path $dtoFolder)) { New-Item -ItemType Directory -Path $dtoFolder -Force | Out-Null }
    $dtoPath = Join-Path $dtoFolder "${BaseName}Dto.cs"
    if (Test-Path $dtoPath) {
        Write-Host "?? DTO ya existe: Data/DTOs/${BaseName}Dto.cs" -ForegroundColor Yellow
    } else {
        $dtoContent = @"
namespace VRM_Plugin.Module.$ModuleName.Data.DTOs;

/// <summary>
/// DTO básico para $BaseName
/// </summary>
public class ${BaseName}Dto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
"@
        Set-Content -Path $dtoPath -Value $dtoContent -Encoding UTF8 -Force
        Write-Host "? DTO creado: Data/DTOs/${BaseName}Dto.cs" -ForegroundColor Green
    }

    # Repository interface
    $repoFolder = Join-Path $ModuleFullPath "Data\Repositories"
    if (!(Test-Path $repoFolder)) { New-Item -ItemType Directory -Path $repoFolder -Force | Out-Null }
    $repoInterfacePath = Join-Path $repoFolder "I${BaseName}Repository.cs"
    if (Test-Path $repoInterfacePath) {
        Write-Host "?? Interface repository ya existe: Data/Repositories/I${BaseName}Repository.cs" -ForegroundColor Yellow
    } else {
        $repoInterfaceContent = @"
using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.$ModuleName.Data.DTOs;

namespace VRM_Plugin.Module.$ModuleName.Data.Repositories;

/// <summary>
/// Acceso a datos de $BaseName (contrato)
/// </summary>
public interface I${BaseName}Repository
{
    /// <summary>
    /// Método de ejemplo que devuelve una lista de DTOs
    /// </summary>
    Task<List<${BaseName}Dto>> GetSampleAsync();
}
"@
        Set-Content -Path $repoInterfacePath -Value $repoInterfaceContent -Encoding UTF8 -Force
        Write-Host "? Interface repository creada: Data/Repositories/I${BaseName}Repository.cs" -ForegroundColor Green
    }

    # Repository implementación (base, en memoria)
    $repoImplPath = Join-Path $repoFolder "${BaseName}Repository.cs"
    if (Test-Path $repoImplPath) {
        Write-Host "?? Repository ya existe: Data/Repositories/${BaseName}Repository.cs" -ForegroundColor Yellow
    } else {
        $repoImplContent = @"
using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.$ModuleName.Data.DTOs;

namespace VRM_Plugin.Module.$ModuleName.Data.Repositories;

/// <summary>
/// Implementación base del repository de $BaseName (solo ejemplo)
/// </summary>
public class ${BaseName}Repository : I${BaseName}Repository
{
    private readonly List<${BaseName}Dto> _items = new();

    public Task<List<${BaseName}Dto>> GetSampleAsync()
    {
        return Task.FromResult(_items);
    }
}
"@
        Set-Content -Path $repoImplPath -Value $repoImplContent -Encoding UTF8 -Force
        Write-Host "? Repository creado: Data/Repositories/${BaseName}Repository.cs" -ForegroundColor Green
    }

    Write-Host "? Nota: registra I${BaseName}Repository/${BaseName}Repository y el service en ConfigureServices() del módulo." -ForegroundColor Cyan
}

# ==================== 3. CREAR COMPONENTE BLAZOR ====================
Write-Host "`n?? [3] Creando componente Blazor..." -ForegroundColor Yellow

$componentsFolder = Join-Path $ModuleFullPath "Components"
if (!(Test-Path $componentsFolder)) { New-Item -ItemType Directory -Path $componentsFolder -Force | Out-Null }

$componentPath = Join-Path $componentsFolder "$ComponentName.razor"
if (Test-Path $componentPath) {
    Write-Host "??  El componente ya existe: Components/$ComponentName.razor" -ForegroundColor Yellow
} else {
    $routeName = $ComponentName.ToLower()
    $moduleLower = $ModuleName.ToLower()
    $componentContent = @"
@page "/$moduleLower/$routeName"
@attribute [Authorize]
@rendermode InteractiveServer
@inject ${ModuleName}Module CurrentModule

<CascadingValue Value="@CurrentModule" Name="CurrentModule">
    <div>
        <!-- Contenido base mínimo; reemplaza según la UI requerida -->
    </div>
</CascadingValue>

@code {
    // Servicio inyectable (descomenta si lo registras en ConfigureServices)
    // @inject I${BaseName}Service ${BaseName}Service

    protected override async Task OnInitializedAsync()
    {
        await Task.CompletedTask;
    }
}
"@
    Set-Content -Path $componentPath -Value $componentContent -Encoding UTF8 -Force
    Write-Host "? Componente creado: Components/$ComponentName.razor" -ForegroundColor Green
}

# ==================== 4. ACTUALIZAR _IMPORTS (SI ES NECESARIO) ====================
Write-Host "`n?? [4] Verificando _Imports.razor..." -ForegroundColor Yellow
$importsPath = Join-Path $ModuleFullPath "Components\_Imports.razor"
if (Test-Path $importsPath) {
    $importsContent = Get-Content -Path $importsPath -Raw

    $needsUpdate = $false
    $newImports = @()

    if ($CreateEntity -and $importsContent -notmatch "VRM_Plugin\.Module\.$ModuleName\.Domain") {
        $newImports += "@using VRM_Plugin.Module.$ModuleName.Domain"
        $needsUpdate = $true
    }

    if ($CreateService -and $importsContent -notmatch "VRM_Plugin\.Module\.$ModuleName\.Services") {
        $newImports += "@using VRM_Plugin.Module.$ModuleName.Services"
        $needsUpdate = $true
    }

    if ($CreateRepository -and $importsContent -notmatch "VRM_Plugin\.Module\.$ModuleName\.Data\.DTOs") {
        $newImports += "@using VRM_Plugin.Module.$ModuleName.Data.DTOs"
        $needsUpdate = $true
    }

    if ($needsUpdate) {
        $importsContent += "`n" + ($newImports -join "`n")
        Set-Content -Path $importsPath -Value $importsContent -Encoding UTF8 -Force
        Write-Host "? _Imports.razor actualizado" -ForegroundColor Green
    } else {
        Write-Host "? _Imports.razor ya está actualizado" -ForegroundColor Gray
    }
} else {
    Write-Host "?? No existe Components\_Imports.razor en el módulo (se puede crear manualmente)." -ForegroundColor Yellow
}

# ==================== 5. INSTRUCCIONES FINALES ====================
Write-Host "`n???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  ? COMPONENTE AGREGADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green

Write-Host "`n?? ARCHIVOS CREADOS:" -ForegroundColor Cyan
if ($CreateEntity) { Write-Host "  ? Domain/$BaseName.cs" -ForegroundColor White }
if ($CreateRepository) {
    Write-Host "  ? Data/DTOs/${BaseName}Dto.cs" -ForegroundColor White
    Write-Host "  ? Data/Repositories/I${BaseName}Repository.cs" -ForegroundColor White
    Write-Host "  ? Data/Repositories/${BaseName}Repository.cs" -ForegroundColor White
}
if ($CreateService) {
    Write-Host "  ? Services/I${BaseName}Service.cs" -ForegroundColor White
    Write-Host "  ? Services/${BaseName}Service.cs" -ForegroundColor White
}
Write-Host "  ? Components/$ComponentName.razor" -ForegroundColor White

Write-Host "`n??  PASOS SIGUIENTES (manuales):" -ForegroundColor Yellow
Write-Host "  1. Registrar el componente en BD y obtener IdComponent." -ForegroundColor White
Write-Host "  2. Actualizar ${ModuleName}Module.cs: agregar entry en GetComponents() y GetActions()." -ForegroundColor White
Write-Host "  3. Registrar servicios/repos en ConfigureServices() del módulo:" -ForegroundColor White
Write-Host "     services.AddScoped<I${BaseName}Service, ${BaseName}Service>();" -ForegroundColor Gray
if ($CreateRepository) {
    Write-Host "     services.AddScoped<I${BaseName}Repository, ${BaseName}Repository>();" -ForegroundColor Gray
}
Write-Host "  4. Compilar módulo: cd `"$ModuleFullPath`" && dotnet build" -ForegroundColor White

Set-Location $RootPath