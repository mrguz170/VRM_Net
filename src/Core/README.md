# Core - Abstracciones y Domain

Esta carpeta contiene los proyectos compartidos que definen las interfaces y modelos del dominio.

## Proyectos

### VRM_Plugin.Core.Abstractions

Define las interfaces base del sistema de plugins.

**Archivo clave:** `IModule.cs`

```csharp
public interface IModule
{
    string ModuleId { get; }
    string DisplayName { get; }
    string Version { get; }
    
    // Componentes Blazor del modulo
    List<ComponentInfo> GetComponents();
    
    // Registrar servicios en DI
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    
    // Permisos granulares por accion
    Dictionary<string, string[]> GetActionPermissions();
}
```

**Otros archivos:**
- `ComponentInfo.cs` - Metadata de componentes Blazor
- `DependencyInfo.cs` - Informacion de dependencias entre modulos

### VRM_Plugin.Core.Domain

Modelos del dominio compartidos entre modulos.

**Archivos:**
- `ConfiguracionFiscal.cs` - Configuracion fiscal del cliente
- `ConfiguracionNegocio.cs` - Configuracion de negocio
- `ModuloHabilitado.cs` - Modulos habilitados por cliente

---

## Como Usar IModule

Cada modulo debe implementar `IModule`:

```csharp
public class FinanzasModule : IModule
{
    public string ModuleId => "Finanzas";
    public string DisplayName => "Modulo de Finanzas";
    public string Version => "1.0.0";
    
    public List<ComponentInfo> GetComponents()
    {
        return new List<ComponentInfo>
        {
            new ComponentInfo
            {
                Name = "Finanzas",
                Route = "/finanzas",
                ComponentType = typeof(Components.Finanzas),
                ShowInMenu = true
            }
        };
    }
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFacturaService, FacturaService>();
    }
    
    public Dictionary<string, string[]> GetActionPermissions()
    {
        return new Dictionary<string, string[]>
        {
            ["Finanzas.Facturas.Ver"] = new[] { "Admin", "GerenteFinanzas" },
            ["Finanzas.Facturas.TimbrarSAT"] = new[] { "Admin", "GerenteFinanzas" }
        };
    }
}
```

---

Volver a [README principal](../../README.md)
