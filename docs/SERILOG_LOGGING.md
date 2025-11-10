# ?? Serilog - Sistema de Logging Estructurado

## ?? **¿Qué es Serilog?**

**Serilog** es un sistema de logging estructurado para .NET que permite:
- ? **Logs con propiedades** (no solo texto plano)
- ? **Múltiples destinos** (Console, File, Database, Cloud)
- ? **Niveles de log** configurables por namespace
- ? **Enriquecedores** automáticos (Machine, Environment, User)
- ? **Búsqueda eficiente** en logs (filtro por propiedades)

---

## ?? **Paquetes Instalados**

```xml
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
```

---

## ?? **Configuración Actual**

### **Development** (más detalle):
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "VRM_Plugin.Blazor.Server": "Debug",
        "VRM_Plugin.Modules": "Debug"
      }
    }
  }
}
```

### **Production** (menos ruido):
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "VRM_Plugin.Blazor.Server": "Information"
      }
    }
  }
}
```

---

## ?? **Niveles de Log**

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| **Verbose** | Debugging detallado | `_logger.LogTrace("Variable X = {Value}", x)` |
| **Debug** | Información de desarrollo | `_logger.LogDebug("Ejecutando método {Method}", nameof(Load))` |
| **Information** | Operaciones normales | `_logger.LogInformation("Usuario {User} autenticado", username)` |
| **Warning** | Situaciones anómalas | `_logger.LogWarning("Cache miss para {Key}", key)` |
| **Error** | Errores recuperables | `_logger.LogError(ex, "Error al guardar {Entity}", entity)` |
| **Fatal** | Errores críticos | `_logger.LogFatal(ex, "Aplicación no puede iniciar")` |

---

## ?? **Uso en Código**

### **1. Inyectar ILogger:**

```csharp
@inject ILogger<Sidebar> Logger

// o en clase C#:
public class MiServicio
{
    private readonly ILogger<MiServicio> _logger;
    
    public MiServicio(ILogger<MiServicio> logger)
    {
        _logger = logger;
    }
}
```

### **2. Logging Estructurado (con propiedades):**

```csharp
// ? BIEN: Propiedades estructuradas
_logger.LogInformation(
    "Usuario {User} accedió a módulo {Module} con {Permisos} permisos",
    username,
    moduleId,
    permissions.Count);

// ? MAL: Concatenación de strings
_logger.LogInformation($"Usuario {username} accedió...");  // No indexable
```

### **3. Logging con Excepciones:**

```csharp
try
{
    await ProcesoRiesgoso();
}
catch (Exception ex)
{
    _logger.LogError(ex,
        "Error al procesar {Entidad} para usuario {User}",
        entidadId,
        username);
}
```

### **4. Medición de Performance:**

```csharp
var startTime = DateTime.UtcNow;

// ... código ...

_logger.LogInformation(
    "Operación {Operation} completada en {ElapsedMs}ms",
    nameof(CargarDatos),
    (DateTime.UtcNow - startTime).TotalMilliseconds);
```

---

## ?? **Estructura de Archivos de Log**

```
logs/
??? dev/                          # Logs de desarrollo
?   ??? vrm-20250110.log
?   ??? vrm-20250111.log
?   ??? vrm-20250112.log
??? prod/                         # Logs de producción
    ??? vrm-20250110.log         # Todos los logs (Info+)
    ??? errors/                   # Solo errores
        ??? vrm-errors-20250110.log
```

---

## ?? **Búsqueda en Logs**

### **Logs Estructurados:**
```
[22:15:30 INF] [Sidebar] Componentes cargados para usuario admin: Total=3, Raíz=2, Hijos=1, Tiempo=45.2ms
```

### **Filtrar por propiedades:**
```bash
# Buscar todos los logs de un usuario específico
grep "User=admin" logs/vrm-20250110.log

# Buscar logs lentos (>100ms)
grep -E "Tiempo=[0-9]{3,}" logs/vrm-20250110.log

# Solo errores
grep "ERR" logs/vrm-20250110.log
```

---

## ?? **Ejemplos Implementados**

### **Sidebar.razor:**
```csharp
Logger.LogInformation(
    "[Sidebar] Componentes cargados para usuario {User}: Total={TotalCount}, Raíz={RootCount}, Hijos={ChildCount}, Tiempo={ElapsedMs}ms",
    _currentUser,
    componentsCount,
    rootCount,
    childCount,
    elapsedMs);

Logger.LogWarning(
    "[Sidebar] No se encontraron componentes visibles para usuario {User}",
    _currentUser);

Logger.LogError(ex,
    "[Sidebar] Error crítico al cargar componentes para usuario {User}",
    _currentUser);
```

### **ModuleLoader.cs:**
```csharp
_logger.LogInformation(
    "[ModuleLoader] Módulo descubierto: {ModuleName} (ID: {IdModule}, Versión: {Version})",
    module.ModuleName,
    module.IdModule,
    module.Version);

_logger.LogInformation(
    "[ModuleLoader] ? Descubrimiento completado: {ModuleCount} módulos cargados en {ElapsedMs}ms",
    _loadedModules.Count,
    elapsedMs);
```

---

## ?? **Integración con Application Insights (Futuro)**

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

Log.Logger = new LoggerConfiguration()
    .WriteTo.ApplicationInsights(
        TelemetryConfiguration.Active,
        TelemetryConverter.Traces)
    .CreateLogger();
```

**Consultas en Azure:**
```kql
traces
| where customDimensions.SourceContext == "VRM_Plugin.Blazor.Server.Components.Layout.Sidebar"
| where customDimensions.User == "admin"
| summarize AvgLoadTime = avg(toreal(customDimensions.ElapsedMs)) by bin(timestamp, 1h)
```

---

## ? **Beneficios vs Console.WriteLine**

| Aspecto | Console.WriteLine | Serilog |
|---------|-------------------|---------|
| **Niveles** | ? No | ? 6 niveles |
| **Filtrado** | ? Difícil | ? Por namespace |
| **Propiedades** | ? Solo texto | ? Estructuradas |
| **Búsqueda** | ? grep manual | ? Indexado |
| **Producción** | ? No persistente | ? Archivos + Cloud |
| **Performance** | ?? Blocking | ? Async |
| **Integración** | ? No | ? APM, SIEM |

---

## ?? **Configuración por Entorno**

### **Cambiar nivel de log en runtime:**

```bash
# Development (más detalle)
export ASPNETCORE_ENVIRONMENT=Development
dotnet run

# Production (menos ruido)
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

### **Configurar para componente específico:**

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "VRM_Plugin.Blazor.Server.Components.Layout.Sidebar": "Debug"
      }
    }
  }
}
```

---

## ?? **Comandos Útiles**

```bash
# Ver logs en tiempo real (Development)
tail -f logs/dev/vrm-20250110.log

# Contar errores del día
grep "ERR" logs/prod/vrm-20250110.log | wc -l

# Ver solo logs de Sidebar
grep "Sidebar" logs/vrm-20250110.log

# Buscar logs lentos (>500ms)
grep -E "Tiempo=[0-9]{3,}" logs/vrm-20250110.log | sort -t= -k2 -n

# Limpiar logs antiguos (>30 días)
find logs/ -name "*.log" -mtime +30 -delete
```

---

## ?? **Referencias**

- [Serilog Documentation](https://serilog.net/)
- [Structured Logging Best Practices](https://github.com/serilog/serilog/wiki/Structured-Data)
- [ASP.NET Core Logging](https://docs.microsoft.com/aspnet/core/fundamentals/logging)

---

? **Serilog está completamente configurado y listo para usar en toda la aplicación**
