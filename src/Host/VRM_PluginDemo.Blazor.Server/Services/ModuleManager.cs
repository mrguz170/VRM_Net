using Microsoft.Extensions.Logging;
using System.Reflection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Services;
using VRM_Plugin.Core.Abstractions.Data.DTOs;

namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Gestor de módulos del sistema de plugins.
/// Descubre, carga y administra módulos dinámicamente desde ensamblados.
/// ? ACTUALIZADO: Usa IModuleMetadataService para inyectar datos desde BD
/// </summary>
public class ModuleManager : IModuleManager
{
    private readonly List<IModule> _loadedModules = new();
    private readonly ILogger<ModuleManager> _logger;
    private readonly IModuleMetadataService? _metadataService;

    public IEnumerable<IModule> LoadedModules { get; internal set; }

    // Constructor original (sin metadata service) - para compatibilidad
    public ModuleManager(ILogger<ModuleManager> logger)
    {
        _logger = logger;
        _metadataService = null;
    }
    
    // ? NUEVO: Constructor con metadata service
    public ModuleManager(ILogger<ModuleManager> logger, IModuleMetadataService metadataService)
    {
        _logger = logger;
        _metadataService = metadataService;
    }

    /// <summary>
    /// Descubre y carga módulos desde una ruta específica
    /// Busca DLLs con patrón: VRM_Plugin.Modules.*.dll
    /// </summary>
    public async Task<int> DiscoverAndLoadModulesAsync(string modulesPath)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation(
            "[ModuleManager] Iniciando descubrimiento de módulos en: {ModulesPath}",
            modulesPath);

        if (!Directory.Exists(modulesPath))
        {
            _logger.LogWarning(
                "[ModuleManager] Carpeta de módulos no encontrada: {ModulesPath}. Creando carpeta...",
                modulesPath);
            Directory.CreateDirectory(modulesPath);
            return 0;
        }

        // ? Buscar: VRM_Plugin.Modules.*.dll
        var dllFiles = Directory.GetFiles(modulesPath, "VRM_Plugin.Modules.*.dll", SearchOption.AllDirectories);
        
        _logger.LogInformation(
            "[ModuleManager] Encontrados {DllCount} archivos DLL potenciales",
            dllFiles.Length);

        foreach (var dllPath in dllFiles)
        {
            try
            {
                _logger.LogDebug(
                    "[ModuleManager] Intentando cargar ensamblado: {DllPath}",
                    dllPath);

                var assembly = Assembly.LoadFrom(dllPath);
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                if (!moduleTypes.Any())
                {
                    _logger.LogDebug(
                        "[ModuleManager] No se encontraron tipos IModule en: {AssemblyName}",
                        assembly.GetName().Name);
                    continue;
                }

                foreach (var moduleType in moduleTypes)
                {
                    try
                    {
                        var module = (IModule)Activator.CreateInstance(moduleType)!;

                        // ? NUEVO: Inyectar metadata desde BD
                        if (_metadataService != null)
                        {
                            await LoadModuleMetadataFromDatabase(module);
                        }
                        
                        _logger.LogInformation(
                            "[ModuleManager] Módulo descubierto: {ModuleName} (ID: {ModuleId}, Versión: {Version})",
                            module.ModuleName,
                            module.ModuleId,
                            module.Version);

                        _loadedModules.Add(module);

                        await module.OnModuleLoadedAsync();
                        
                        _logger.LogDebug(
                            "[ModuleManager] OnModuleLoadedAsync ejecutado para: {ModuleName}",
                            module.ModuleName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "[ModuleManager] Error al instanciar módulo: {ModuleType}",
                            moduleType.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[ModuleManager] Error al cargar ensamblado: {DllPath}",
                    dllPath);
            }
        }

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        
        _logger.LogInformation(
            "[ModuleManager] ? Descubrimiento completado: {ModuleCount} módulos cargados en {ElapsedMs}ms",
            _loadedModules.Count,
            elapsedMs);

        return _loadedModules.Count;
    }
    
    /// <summary>
    /// ? NUEVO: Carga metadata desde BD e inyecta al módulo
    /// </summary>
    private async Task LoadModuleMetadataFromDatabase(IModule module)
    {
        if (_metadataService == null)
        {
            _logger.LogWarning(
                "[ModuleManager] IModuleMetadataService no está configurado. Usando valores por defecto para {ModuleName}",
                module.ModuleName);
            return;
        }
        
        try
        {
            _logger.LogDebug(
                "[ModuleManager] Cargando metadata desde BD para módulo {ModuleName} (ID: {ModuleId})",
                module.ModuleName,
                module.ModuleId);
            
            // 1. Obtener metadata básica (ahora es ModuleMetadataDto)
            var metadata = _metadataService.GetModuleMetadata(module.ModuleId);
            
            // 2. Obtener componentes
            var components = _metadataService.GetComponentsByModuleId(module.ModuleId);
            
            // 3. Obtener acciones
            var actions = _metadataService.GetActionsByModuleId(module.ModuleId);
            
            // 4. Obtener permisos por acción
            var permissions = _metadataService.GetActionPermissions(module.ModuleName);
            
            // 5. Inyectar al módulo usando reflexión
            var moduleType = module.GetType();
            
            // Inyectar metadata
            var setMetadataMethod = moduleType.GetMethod("SetMetadata");
            if (setMetadataMethod != null)
            {
                setMetadataMethod.Invoke(module, new object[] { metadata.ModuleName , metadata.DisplayName, metadata.Description, metadata.Version });
                
                _logger.LogDebug(
                    "[ModuleManager] Metadata inyectada para {ModuleName}: {DisplayName} v{Version}",
                    module.ModuleName,
                    metadata.DisplayName,
                    metadata.Version);
            }
            
            // Inyectar componentes
            var setComponentsMethod = moduleType.GetMethod("SetComponents");
            if (setComponentsMethod != null)
            {
                setComponentsMethod.Invoke(module, new object[] { components });
                _logger.LogDebug(
                    "[ModuleManager] {Count} componentes inyectados para {ModuleName}",
                    components.Count,
                    module.ModuleName);
            }
            
            // Inyectar acciones
            var setActionsMethod = moduleType.GetMethod("SetActions");
            if (setActionsMethod != null)
            {
                setActionsMethod.Invoke(module, new object[] { actions });
                _logger.LogDebug(
                    "[ModuleManager] {Count} acciones inyectadas para {ModuleName}",
                    actions.Count,
                    module.ModuleName);
            }
            
            // Inyectar permisos
            var setPermissionsMethod = moduleType.GetMethod("SetActionPermissions");
            if (setPermissionsMethod != null)
            {
                setPermissionsMethod.Invoke(module, new object[] { permissions });
                _logger.LogDebug(
                    "[ModuleManager] {Count} permisos inyectados para {ModuleName}",
                    permissions.Count,
                    module.ModuleName);
            }
            
            _logger.LogInformation(
                "[ModuleManager] ? Metadata completa cargada desde BD para {ModuleName}",
                module.ModuleName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[ModuleManager] Error al cargar metadata desde BD para {ModuleName}. Usando valores por defecto.",
                module.ModuleName);
        }
        
        await Task.CompletedTask;
    }

    // ==================== IMPLEMENTACIÓN DE IModuleManager ====================

    public IReadOnlyList<IModule> GetAllModules()
    {
        return _loadedModules.AsReadOnly();
    }

    public IModule? GetModuleById(string moduleId)
    {
        // Buscar por ModuleId (int convertido a string) o por ModuleName
        if (int.TryParse(moduleId, out var id))
        {
            return _loadedModules.FirstOrDefault(m => m.ModuleId == id);
        }
        
        return _loadedModules.FirstOrDefault(m =>
            m.ModuleName.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<IModule> GetModulesForClient(string clienteId)
    {
        return _loadedModules.Where(m => m.IsEnabledForClient(clienteId));
    }

    public IEnumerable<IModule> GetModulesByCategory(string category)
    {
        // ? ELIMINADO: Category ya no existe
        // Ahora se usa jerarquía de componentes (IdParent = null como categorías)
        _logger.LogWarning("GetModulesByCategory está obsoleto. Use jerarquía de componentes con IdParent.");
        return Enumerable.Empty<IModule>();
    }

    public bool IsModuleLoaded(string moduleId)
    {
        if (int.TryParse(moduleId, out var id))
        {
            return _loadedModules.Any(m => m.ModuleId == id);
        }
        
        return _loadedModules.Any(m => m.ModuleName.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
    }

    public IModule? GetModule(string moduleId)
    {
        return GetModuleById(moduleId);
    }
}
