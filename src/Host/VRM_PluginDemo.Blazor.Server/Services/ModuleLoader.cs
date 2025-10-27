using System.Reflection;
using System.Runtime.Loader;
using VRM_Plugin.Core.Abstractions;

namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Carga dinámicamente módulos desde ensamblados
/// </summary>
public class ModuleLoader : IModuleManager
{
    private readonly List<IModule> _loadedModules = new();
    private readonly ILogger<ModuleLoader> _logger;

    public ModuleLoader(ILogger<ModuleLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Descubre y carga módulos desde una ruta específica
    /// </summary>
    public async Task<int> DiscoverAndLoadModulesAsync(string modulesPath)
    {
        _logger.LogInformation("🔍 Iniciando descubrimiento de módulos en: {Path}", modulesPath);

        var baseDirectory = AppContext.BaseDirectory;
        var fullPath = Path.Combine(baseDirectory, modulesPath);

        if (!Directory.Exists(fullPath))
        {
            _logger.LogWarning("⚠️ La ruta de módulos no existe: {Path}", fullPath);
            Directory.CreateDirectory(fullPath);
            _logger.LogInformation("✅ Carpeta de módulos creada: {Path}", fullPath);
            return 0;
        }

        // Buscar todos los DLLs que coincidan con el patrón de módulos
        var moduleFiles = Directory.GetFiles(fullPath, "VRM_Plugin.*.dll", SearchOption.AllDirectories);

        _logger.LogInformation("📦 Encontrados {Count} archivos de módulos potenciales", moduleFiles.Length);

        foreach (var file in moduleFiles)
        {
            try
            {
                await LoadModuleFromAssemblyAsync(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar módulo desde: {File}", file);
            }
        }

        _logger.LogInformation("✅ Carga de módulos completada. Total cargados: {Count}", _loadedModules.Count);
        return _loadedModules.Count;
    }

    /// <summary>
    /// Carga un módulo desde un archivo de ensamblado
    /// </summary>
    private async Task LoadModuleFromAssemblyAsync(string assemblyPath)
    {
        _logger.LogDebug("🔄 Cargando ensamblado: {Path}", assemblyPath);

        // Evitar cargar dos veces el mismo ensamblado
        var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.Location, assemblyPath, StringComparison.OrdinalIgnoreCase));
        var assembly = alreadyLoaded ?? AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

        // Buscar tipos que implementen IModule
        var moduleTypes = assembly.GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t) &&
                       !t.IsInterface &&
                       !t.IsAbstract)
            .ToList();

        if (!moduleTypes.Any())
        {
            _logger.LogDebug("⚠️ No se encontraron implementaciones de IModule en: {Assembly}", assembly.FullName);
            return;
        }

        foreach (var moduleType in moduleTypes)
        {
            try
            {
                // Crear instancia del módulo
                if (Activator.CreateInstance(moduleType) is not IModule module)
                {
                    _logger.LogWarning("⚠️ No se pudo crear instancia de: {Type}", moduleType.FullName);
                    continue;
                }

                // Verificar dependencias
                if (!ValidateDependencies(module))
                {
                    _logger.LogError("❌ Faltan dependencias para el módulo: {ModuleId}", module.ModuleId);
                    continue;
                }

                // Ejecutar inicialización del módulo
                await module.OnModuleLoadedAsync();

                // Agregar a la lista de módulos cargados
                _loadedModules.Add(module);

                _logger.LogInformation(
                    "✅ Módulo cargado: {ModuleId} v{Version} - {DisplayName} (Autor: {Author})",
                    module.ModuleId,
                    module.Version,
                    module.DisplayName,
                    module.Author
                );

                if (module.Dependencies.Any())
                {
                    _logger.LogInformation("   📌 Dependencias: {Dependencies}",
                        string.Join(", ", module.Dependencies));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al instanciar módulo: {Type}", moduleType.FullName);
            }
        }
    }

    /// <summary>
    /// Valida que las dependencias de un módulo estén satisfechas
    /// </summary>
    private bool ValidateDependencies(IModule module)
    {
        if (!module.Dependencies.Any())
            return true;

        var missingDependencies = module.Dependencies
            .Where(dep => !_loadedModules.Any(m => m.ModuleId == dep))
            .ToList();

        if (missingDependencies.Any())
        {
            _logger.LogWarning(
                "⚠️ Módulo {ModuleId} tiene dependencias faltantes: {Missing}",
                module.ModuleId,
                string.Join(", ", missingDependencies)
            );
            return false;
        }

        return true;
    }

    // ==================== IMPLEMENTACIÓN DE IModuleManager ====================

    public IReadOnlyList<IModule> GetAllModules()
    {
        return _loadedModules.AsReadOnly();
    }

    public IModule? GetModuleById(string moduleId)
    {
        return _loadedModules.FirstOrDefault(m =>
            m.ModuleId.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<IModule> GetModulesForClient(string clienteId)
    {
        return _loadedModules.Where(m => m.IsEnabledForClient(clienteId));
    }

    public IEnumerable<IModule> GetModulesByCategory(string category)
    {
        return _loadedModules.Where(m =>
            m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsModuleLoaded(string moduleId)
    {
        return _loadedModules.Any(m => m.ModuleId.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
    }

    public IModule? GetModule(string moduleId)
    {
        return _loadedModules.FirstOrDefault(m => 
            m.ModuleId.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
    }

    // ==================== PRIVATE METHODS ====================
}