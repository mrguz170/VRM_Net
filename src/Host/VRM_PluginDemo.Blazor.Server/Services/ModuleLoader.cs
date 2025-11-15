using System.Reflection;
using System.Runtime.Loader;
using VRM_Plugin.Core.Abstractions;

namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Carga dinámicamente módulos desde ensamblados
/// ✅ Actualizado para nueva arquitectura sin Author, Category, Dependencies
/// ✅ Sin palabra "Demo" en namespaces ni DLLs
/// </summary>
public class ModuleLoader : IModuleManager
{
    private readonly List<IModule> _loadedModules = new();
    private readonly ILogger<ModuleLoader> _logger;

    public IEnumerable<IModule> LoadedModules { get; internal set; }

    public ModuleLoader(ILogger<ModuleLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Descubre y carga módulos desde una ruta específica
    /// Busca DLLs con patrón: VRM_Plugin.Modules.*.dll
    /// </summary>
    public async Task<int> DiscoverAndLoadModulesAsync(string modulesPath)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation(
            "[ModuleLoader] Iniciando descubrimiento de módulos en: {ModulesPath}",
            modulesPath);

        if (!Directory.Exists(modulesPath))
        {
            _logger.LogWarning(
                "[ModuleLoader] Carpeta de módulos no encontrada: {ModulesPath}. Creando carpeta...",
                modulesPath);
            Directory.CreateDirectory(modulesPath);
            return 0;
        }

        // ✅ Buscar: VRM_Plugin.Modules.*.dll
        var dllFiles = Directory.GetFiles(modulesPath, "VRM_Plugin.Modules.*.dll", SearchOption.AllDirectories);
        
        _logger.LogInformation(
            "[ModuleLoader] Encontrados {DllCount} archivos DLL potenciales",
            dllFiles.Length);

        foreach (var dllPath in dllFiles)
        {
            try
            {
                _logger.LogDebug(
                    "[ModuleLoader] Intentando cargar ensamblado: {DllPath}",
                    dllPath);

                var assembly = Assembly.LoadFrom(dllPath);
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                if (!moduleTypes.Any())
                {
                    _logger.LogDebug(
                        "[ModuleLoader] No se encontraron tipos IModule en: {AssemblyName}",
                        assembly.GetName().Name);
                    continue;
                }

                foreach (var moduleType in moduleTypes)
                {
                    try
                    {
                        var module = (IModule)Activator.CreateInstance(moduleType)!;

                       // var metadata = await _metadataService.GetMetadataAsync(module.IdModule);

                        _logger.LogInformation(
                            "[ModuleLoader] Módulo descubierto: {ModuleName} (ID: {IdModule}, Versión: {Version})",
                            module.ModuleName,
                            module.IdModule,
                            module.Version);

                        _loadedModules.Add(module);

                        await module.OnModuleLoadedAsync();
                        
                        _logger.LogDebug(
                            "[ModuleLoader] OnModuleLoadedAsync ejecutado para: {ModuleName}",
                            module.ModuleName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "[ModuleLoader] Error al instanciar módulo: {ModuleType}",
                            moduleType.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[ModuleLoader] Error al cargar ensamblado: {DllPath}",
                    dllPath);
            }
        }

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        
        _logger.LogInformation(
            "[ModuleLoader] ✅ Descubrimiento completado: {ModuleCount} módulos cargados en {ElapsedMs}ms",
            _loadedModules.Count,
            elapsedMs);

        return _loadedModules.Count;
    }

    // ==================== IMPLEMENTACIÓN DE IModuleManager ====================

    public IReadOnlyList<IModule> GetAllModules()
    {
        return _loadedModules.AsReadOnly();
    }

    public IModule? GetModuleById(string moduleId)
    {
        // Buscar por IdModule (int convertido a string) o por ModuleName
        if (int.TryParse(moduleId, out var id))
        {
            return _loadedModules.FirstOrDefault(m => m.IdModule == id);
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
        // ❌ ELIMINADO: Category ya no existe
        // Ahora se usa jerarquía de componentes (IdParent = null como categorías)
        _logger.LogWarning("GetModulesByCategory está obsoleto. Use jerarquía de componentes con IdParent.");
        return Enumerable.Empty<IModule>();
    }

    public bool IsModuleLoaded(string moduleId)
    {
        if (int.TryParse(moduleId, out var id))
        {
            return _loadedModules.Any(m => m.IdModule == id);
        }
        
        return _loadedModules.Any(m => m.ModuleName.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
    }

    public IModule? GetModule(string moduleId)
    {
        return GetModuleById(moduleId);
    }
}