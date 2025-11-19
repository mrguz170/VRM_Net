<#
.SYNOPSIS
    Agrega un nuevo componente Blazor a un módulo VRM existente.

.DESCRIPTION
    Este script facilita agregar:
    - Componente Blazor (.razor)
    - Entidad de dominio (opcional)
    - Servicio e interfaz (opcional)
    
    Los IDs deben obtenerse de la BD antes de actualizar el módulo.

.PARAMETER ModuleName
    Nombre del módulo existente (ej: Inventario, Finanzas)

.PARAMETER ComponentName
    Nombre del nuevo componente (ej: Productos, Clientes)

.PARAMETER CreateEntity
    Si es $true, crea una entidad de dominio para el componente

.PARAMETER CreateService
    Si es $true, crea interfaz y servicio para el componente

.EXAMPLE
    .\Add-VRMComponent.ps1 -ModuleName Inventario -ComponentName Productos

.EXAMPLE
    .\Add-VRMComponent.ps1 -ModuleName Inventario -ComponentName Productos -CreateEntity -CreateService
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName,
    
    [Parameter(Mandatory=$true)]
    [string]$ComponentName,
    
    [Parameter(Mandatory=$false)]
    [switch]$CreateEntity,
    
    [Parameter(Mandatory=$false)]
    [switch]$CreateService
)

# ==================== CONFIGURACIÓN ====================

$RootPath = Get-Location
$ModulesPath = Join-Path $RootPath "src\Modules"

# Buscar el módulo en todas las subcarpetas
$ModuleProjectName = "VRM_Plugin.Modules.$ModuleName"
$ModuleFullPath = Get-ChildItem -Path $ModulesPath -Recurse -Directory | 
                  Where-Object { $_.Name -eq $ModuleProjectName } | 
                  Select-Object -First 1 -ExpandProperty FullName

if (-not $ModuleFullPath) {
    Write-Host "? ERROR: No se encontró el módulo '$ModuleName'" -ForegroundColor Red
    Write-Host "Buscar en: $ModulesPath" -ForegroundColor Gray
    exit 1
}

Write-Host "`n???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? AGREGAR COMPONENTE A MÓDULO VRM" -ForegroundColor Cyan
Write-Host "  Módulo: $ModuleName" -ForegroundColor Cyan
Write-Host "  Componente: $ComponentName" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

# ==================== 1. CREAR ENTIDAD (OPCIONAL) ====================

if ($CreateEntity) {
    Write-Host "?? [1] Creando entidad de dominio..." -ForegroundColor Yellow
    
    $entityPath = Join-Path $ModuleFullPath "Domain\$ComponentName.cs"
    
    if (Test-Path $entityPath) {
        Write-Host "??  La entidad ya existe: $ComponentName.cs" -ForegroundColor Yellow
    } else {
        $entityContent = @"
namespace VRM_Plugin.Modules.$ModuleName.Domain;

/// <summary>
/// Entidad $ComponentName del módulo $ModuleName
/// </summary>
public class $ComponentName
{
    public int Id { get; set; }
    
    public string Nombre { get; set; } = string.Empty;
    
    public string Descripcion { get; set; } = string.Empty;
    
    public bool Activo { get; set; } = true;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public DateTime? FechaModificacion { get; set; }
    
    public string CreadoPor { get; set; } = string.Empty;
    
    public string? ModificadoPor { get; set; }
}
"@
        
        Set-Content -Path $entityPath -Value $entityContent -Force
        Write-Host "? Entidad creada: Domain/$ComponentName.cs" -ForegroundColor Green
    }
}

# ==================== 2. CREAR SERVICIO (OPCIONAL) ====================

if ($CreateService) {
    Write-Host "`n??  [2] Creando servicio..." -ForegroundColor Yellow
    
    # Interfaz
    $interfacePath = Join-Path $ModuleFullPath "Services\I${ComponentName}Service.cs"
    
    if (Test-Path $interfacePath) {
        Write-Host "??  La interfaz ya existe: I${ComponentName}Service.cs" -ForegroundColor Yellow
    } else {
        $interfaceContent = @"
using VRM_Plugin.Modules.$ModuleName.Domain;

namespace VRM_Plugin.Modules.$ModuleName.Services;

/// <summary>
/// Servicio para gestionar $ComponentName
/// </summary>
public interface I${ComponentName}Service
{
    /// <summary>
    /// Obtiene todos los registros de $ComponentName
    /// </summary>
    Task<List<$ComponentName>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un registro de $ComponentName por ID
    /// </summary>
    Task<$ComponentName?> GetByIdAsync(int id);
    
    /// <summary>
    /// Crea un nuevo registro de $ComponentName
    /// </summary>
    Task<$ComponentName> CreateAsync($ComponentName entity);
    
    /// <summary>
    /// Actualiza un registro existente de $ComponentName
    /// </summary>
    Task<$ComponentName> UpdateAsync($ComponentName entity);
    
    /// <summary>
    /// Elimina un registro de $ComponentName
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
"@
        
        Set-Content -Path $interfacePath -Value $interfaceContent -Force
        Write-Host "? Interfaz creada: Services/I${ComponentName}Service.cs" -ForegroundColor Green
    }
    
    # Implementación
    $servicePath = Join-Path $ModuleFullPath "Services\${ComponentName}Service.cs"
    
    if (Test-Path $servicePath) {
        Write-Host "??  La implementación ya existe: ${ComponentName}Service.cs" -ForegroundColor Yellow
    } else {
        $serviceContent = @"
using VRM_Plugin.Modules.$ModuleName.Domain;

namespace VRM_Plugin.Modules.$ModuleName.Services;

/// <summary>
/// Implementación del servicio de $ComponentName
/// TODO: Conectar con base de datos real mediante DbContext
/// </summary>
public class ${ComponentName}Service : I${ComponentName}Service
{
    // TODO: Inyectar DbContext cuando esté disponible
    // private readonly ApplicationDbContext _context;
    
    // Lista temporal en memoria (SOLO PARA DESARROLLO)
    private readonly List<$ComponentName> _items = new();
    
    public Task<List<$ComponentName>> GetAllAsync()
    {
        // TODO: Reemplazar con: await _context.$ComponentName.ToListAsync();
        return Task.FromResult(_items);
    }
    
    public Task<$ComponentName?> GetByIdAsync(int id)
    {
        // TODO: Reemplazar con: await _context.$ComponentName.FindAsync(id);
        var item = _items.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item);
    }
    
    public Task<$ComponentName> CreateAsync($ComponentName entity)
    {
        // TODO: Reemplazar con:
        // _context.$ComponentName.Add(entity);
        // await _context.SaveChangesAsync();
        
        entity.Id = _items.Count > 0 ? _items.Max(x => x.Id) + 1 : 1;
        entity.FechaCreacion = DateTime.UtcNow;
        _items.Add(entity);
        return Task.FromResult(entity);
    }
    
    public Task<$ComponentName> UpdateAsync($ComponentName entity)
    {
        // TODO: Reemplazar con:
        // _context.$ComponentName.Update(entity);
        // await _context.SaveChangesAsync();
        
        var existing = _items.FirstOrDefault(x => x.Id == entity.Id);
        if (existing != null)
        {
            var index = _items.IndexOf(existing);
            entity.FechaModificacion = DateTime.UtcNow;
            _items[index] = entity;
        }
        return Task.FromResult(entity);
    }
    
    public Task<bool> DeleteAsync(int id)
    {
        // TODO: Reemplazar con:
        // var entity = await _context.$ComponentName.FindAsync(id);
        // if (entity != null) { _context.$ComponentName.Remove(entity); await _context.SaveChangesAsync(); }
        
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
        
        Set-Content -Path $servicePath -Value $serviceContent -Force
        Write-Host "? Servicio creado: Services/${ComponentName}Service.cs" -ForegroundColor Green
    }
}

# ==================== 3. CREAR COMPONENTE BLAZOR ====================

Write-Host "`n?? [3] Creando componente Blazor..." -ForegroundColor Yellow

$componentPath = Join-Path $ModuleFullPath "Components\$ComponentName.razor"

if (Test-Path $componentPath) {
    Write-Host "??  El componente ya existe: $ComponentName.razor" -ForegroundColor Yellow
} else {
    $routeName = $ComponentName.ToLower()
    $moduleLower = $ModuleName.ToLower()
    
    $componentContent = @"
@page "/$moduleLower/$routeName"
@attribute [Authorize]
@rendermode InteractiveServer

<PageTitle>$ComponentName - $ModuleName</PageTitle>

<div class="container-fluid py-4">
    <div class="row">
        <div class="col-12">
            <div class="card shadow-sm">
                <div class="card-header bg-primary text-white">
                    <h3 class="mb-0">
                        <i class="ri-file-list-line me-2"></i>
                        $ComponentName
                    </h3>
                </div>
                <div class="card-body">
                    <div class="alert alert-info">
                        <h5>?? Componente: $ComponentName</h5>
                        <p class="mb-0">Este componente pertenece al módulo <strong>$ModuleName</strong>.</p>
                    </div>
                    
                    <div class="row mt-4">
                        <div class="col-md-12">
                            <h5>?? Próximos pasos:</h5>
                            <ol>
                                <li>Registrar este componente en la BD</li>
                                <li>Obtener el <code>IdComponent</code> asignado</li>
                                <li>Actualizar <code>GetComponents()</code> en <code>${ModuleName}Module.cs</code></li>
                                <li>Implementar la lógica de negocio</li>
                            </ol>
                        </div>
                    </div>
                    
                    <!-- TODO: Implementar UI del componente -->
                    
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    // TODO: Inyectar servicios necesarios
    // @inject I${ComponentName}Service ${ComponentName}Service
    
    protected override async Task OnInitializedAsync()
    {
        // TODO: Cargar datos
        await Task.CompletedTask;
    }
}
"@
    
    Set-Content -Path $componentPath -Value $componentContent -Force
    Write-Host "? Componente creado: Components/$ComponentName.razor" -ForegroundColor Green
}

# ==================== 4. ACTUALIZAR _IMPORTS (SI ES NECESARIO) ====================

if ($CreateEntity -or $CreateService) {
    Write-Host "`n?? [4] Verificando _Imports.razor..." -ForegroundColor Yellow
    
    $importsPath = Join-Path $ModuleFullPath "Components\_Imports.razor"
    
    if (Test-Path $importsPath) {
        $importsContent = Get-Content -Path $importsPath -Raw
        
        $needsUpdate = $false
        $newImports = @()
        
        if ($CreateEntity -and $importsContent -notmatch "VRM_Plugin\.Modules\.$ModuleName\.Domain") {
            $newImports += "@using VRM_Plugin.Modules.$ModuleName.Domain"
            $needsUpdate = $true
        }
        
        if ($CreateService -and $importsContent -notmatch "VRM_Plugin\.Modules\.$ModuleName\.Services") {
            $newImports += "@using VRM_Plugin.Modules.$ModuleName.Services"
            $needsUpdate = $true
        }
        
        if ($needsUpdate) {
            $importsContent += "`n" + ($newImports -join "`n")
            Set-Content -Path $importsPath -Value $importsContent -Force
            Write-Host "? _Imports.razor actualizado" -ForegroundColor Green
        } else {
            Write-Host "? _Imports.razor ya está actualizado" -ForegroundColor Gray
        }
    }
}

# ==================== 5. INSTRUCCIONES FINALES ====================

Write-Host "`n???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  ? COMPONENTE AGREGADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green

Write-Host "`n?? ARCHIVOS CREADOS:" -ForegroundColor Cyan

if ($CreateEntity) {
    Write-Host "  ? Domain/$ComponentName.cs" -ForegroundColor White
}

if ($CreateService) {
    Write-Host "  ? Services/I${ComponentName}Service.cs" -ForegroundColor White
    Write-Host "  ? Services/${ComponentName}Service.cs" -ForegroundColor White
}

Write-Host "  ? Components/$ComponentName.razor" -ForegroundColor White

Write-Host "`n??  PASOS SIGUIENTES:" -ForegroundColor Yellow
Write-Host "  1. Registrar componente en BD y obtener IDs:" -ForegroundColor White
Write-Host "     • IdComponent (para el componente UI)" -ForegroundColor Gray
Write-Host "     • IdAction (para cada acción que definas)" -ForegroundColor Gray

Write-Host "`n  2. Actualizar ${ModuleName}Module.cs:" -ForegroundColor White
Write-Host "     • Agregar componente en GetComponents():" -ForegroundColor Gray
Write-Host "       new ModuleComponent {" -ForegroundColor DarkGray
Write-Host "           IdComponent = [ID_DE_BD]," -ForegroundColor DarkGray
Write-Host "           IdModule = this.IdModule," -ForegroundColor DarkGray
Write-Host "           Name = `"$ComponentName`"," -ForegroundColor DarkGray
Write-Host "           Route = `"/$moduleLower/$routeName`"," -ForegroundColor DarkGray
Write-Host "           ComponentType = typeof(Components.$ComponentName)" -ForegroundColor DarkGray
Write-Host "       }" -ForegroundColor DarkGray

Write-Host "`n     • Agregar acciones en GetActions():" -ForegroundColor Gray
Write-Host "       new ModuleAction {" -ForegroundColor DarkGray
Write-Host "           IdAction = [ID_DE_BD]," -ForegroundColor DarkGray
Write-Host "           IdComponent = [ID_COMPONENTE_DE_BD]," -ForegroundColor DarkGray
Write-Host "           ActionKey = `"$ModuleName.$ComponentName.Ver`"," -ForegroundColor DarkGray
Write-Host "           IdActionType = 1  // Lectura" -ForegroundColor DarkGray
Write-Host "       }" -ForegroundColor DarkGray

if ($CreateService) {
    Write-Host "`n  3. Registrar servicio en ConfigureServices():" -ForegroundColor White
    Write-Host "     services.AddScoped<I${ComponentName}Service, ${ComponentName}Service>();" -ForegroundColor Gray
}

Write-Host "`n  4. Compilar módulo:" -ForegroundColor White
Write-Host "     cd `"$ModuleFullPath`"" -ForegroundColor Gray
Write-Host "     dotnet build" -ForegroundColor Gray

Write-Host "`n  5. Copiar DLL al Host y reiniciar aplicación" -ForegroundColor White

Write-Host "`n?? Ruta: /$moduleLower/$routeName" -ForegroundColor Cyan
Write-Host "`n? ¡Componente listo para desarrollo!" -ForegroundColor Green

Set-Location $RootPath
