# ?? Módulo Prospectos - Ejemplo de Conexión a Base de Datos

## ?? OBJETIVO

Este módulo es un **ejemplo simplificado** para demostrar **cómo conectarse a la base de datos** en la arquitectura de plugins.

---

## ?? Lo Esencial: 3 Componentes Clave

### 1?? **DatabaseHelper** (Infraestructura del Sistema)

**Ubicación:** Registrado en `Program.cs` del Host

```csharp
// ? Program.cs
builder.Services.AddScoped<DatabaseHelper>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new DatabaseHelper(connectionString);
});
```

**¿Qué hace?**
- Lee el connection string de `appsettings.json`
- Ejecuta stored procedures en MySQL
- Mapea resultados automáticamente (snake_case ? PascalCase)

---

### 2?? **Repositorios** (Acceso a Datos del Módulo)

**Ubicación:** `Data/Repositories/ProspectoRepository.cs`

```csharp
public class ProspectoRepository : IProspectoRepository
{
    private readonly DatabaseHelper _dbHelper;
    
    // ? DatabaseHelper se inyecta automáticamente por DI
    public ProspectoRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }
    
    // ? Ejemplo 1: Ejecutar SP sin parámetros
    public async Task<List<ProspectoDto>> GetAllAsync()
    {
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<ProspectoDto>("sp_Prospectos_GetAll"));
    }
    
    // ? Ejemplo 2: Ejecutar SP con parámetros
    public async Task<ProspectoDto?> GetByIdAsync(int id)
    {
        var parametros = new Dictionary<string, object> { ["p_id_prospecto"] = id };
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedureSingle<ProspectoDto>("sp_Prospectos_GetById", parametros));
    }
    
    // ? Ejemplo 3: Ejecutar INSERT y obtener ID
    public async Task<int> CreateAsync(CreateProspectoDto dto)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_razon_social"] = dto.RazonSocial,
            ["p_rfc"] = dto.RFC,
            ["p_correo_electronico"] = dto.CorreoElectronico
        };
        
        var resultado = await Task.Run(() => 
            _dbHelper.ExecuteScalar("sp_Prospectos_Insert", parametros));
        
        return Convert.ToInt32(resultado ?? 0);
    }
}
```

---

### 3?? **Registro en el Módulo**

**Ubicación:** `ProspectosModule.cs`

```csharp
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // ? Registrar repositorio (que usa DatabaseHelper)
    services.AddScoped<IProspectoRepository, ProspectoRepository>();
    
    // ? Registrar servicio de negocio (que usa repositorio)
    services.AddScoped<IProspectoService, ProspectoService>();
}
```

---

## ?? Flujo Completo de Conexión

```
1. appsettings.json
   ??> "DefaultConnection": "Server=localhost;Database=vrm;..."
        ?
        ?
2. Program.cs (Host)
   ??> builder.Services.AddScoped<DatabaseHelper>()
        ? (lee connection string y crea DatabaseHelper)
        ?
3. ProspectosModule.ConfigureServices()
   ??> services.AddScoped<IProspectoRepository, ProspectoRepository>()
        ? (registra repositorio que necesita DatabaseHelper)
        ?
4. ProspectoRepository (Constructor)
   ??> public ProspectoRepository(DatabaseHelper dbHelper)
        ? (recibe DatabaseHelper automáticamente por DI)
        ?
5. Método del Repositorio
   ??> _dbHelper.ExecuteStoredProcedure<ProspectoDto>("sp_Prospectos_GetAll")
        ? (ejecuta SP en MySQL)
        ?
6. Base de Datos MySQL
   ??> CALL sp_Prospectos_GetAll();
        ? (retorna columnas: id_prospecto, razon_social, rfc, ...)
        ?
7. DatabaseHelper (Mapeo Automático)
   ??> Convierte snake_case a PascalCase:
        • id_prospecto ? IdProspecto
        • razon_social ? RazonSocial
        ?
8. List<ProspectoDto>
   ??> Retorna objetos C# tipados listos para usar
```

---

## ?? Estructura Simplificada

```
src/Modules/Onboarding/VRM_PluginDemo.Modules.Prospectos/
??? Data/
?   ??? DTOs/
?   ?   ??? ProspectoDto.cs           ? 6 propiedades básicas
?   ?   ??? CreateProspectoDto.cs     ? 4 propiedades
?   ?   ??? UpdateProspectoDto.cs     ? 4 propiedades
?   ?
?   ??? Repositories/
?   ?   ??? IProspectoRepository.cs   ? 3 métodos de ejemplo
?   ?   ??? ProspectoRepository.cs    ? Implementación comentada
?   ?
?   ??? README.md                      ? Explicación del patrón
?
??? ProspectosModule.cs                ? Registro de repositorio
??? DATA_LAYER_IMPLEMENTATION.md       ? Este documento
```

---

## ?? 3 Tipos de Operaciones

### **TIPO 1: SELECT Lista**
```csharp
// Retorna List<ProspectoDto>
var prospectos = await _dbHelper.ExecuteStoredProcedure<ProspectoDto>(
    "sp_Prospectos_GetAll");
```

### **TIPO 2: SELECT Único**
```csharp
// Retorna ProspectoDto o null
var prospecto = await _dbHelper.ExecuteStoredProcedureSingle<ProspectoDto>(
    "sp_Prospectos_GetById", 
    new Dictionary<string, object> { ["p_id"] = 1 });
```

### **TIPO 3: INSERT/UPDATE con Valor de Retorno**
```csharp
// Retorna ID generado o filas afectadas
var resultado = await _dbHelper.ExecuteScalar(
    "sp_Prospectos_Insert", 
    parametros);
int id = Convert.ToInt32(resultado ?? 0);
```

---

## ? Ventajas de Este Patrón

1. **Separación de responsabilidades**
   - Host: Infraestructura compartida (DatabaseHelper)
   - Módulo: Repositorios específicos del dominio

2. **Inyección de Dependencias**
   - Todo se resuelve automáticamente
   - No código de creación manual

3. **Mapeo automático**
   - DatabaseHelper convierte resultados
   - No mapeo manual fila por fila

4. **Tipo seguro**
   - DTOs con IntelliSense completo
   - Detección de errores en tiempo de compilación

5. **Testeable**
   - Interfaces permiten mocks
   - Fácil crear repositorios falsos para pruebas

6. **Consistente**
   - Mismo patrón en todos los módulos
   - Finanzas y Prospectos usan la misma estructura

---

## ?? Cómo Agregar Más Métodos

### En la Interface:
```csharp
public interface IProspectoRepository
{
    Task<List<ProspectoDto>> GetAllAsync();
    Task<ProspectoDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateProspectoDto dto);
    
    // ? Agregar nuevo método
    Task<List<ProspectoDto>> GetByEstadoAsync(int estado);
}
```

### En la Implementación:
```csharp
public async Task<List<ProspectoDto>> GetByEstadoAsync(int estado)
{
    var parametros = new Dictionary<string, object>
    {
        ["p_estado"] = estado
    };
    
    return await Task.Run(() => 
        _dbHelper.ExecuteStoredProcedure<ProspectoDto>(
            "sp_Prospectos_GetByEstado", 
            parametros));
}
```

---

## ?? Documentación Relacionada

- **Capa de Datos:** [Data/README.md](Data/README.md)
- **DatabaseHelper:** `src/Core/VRM_Plugin.Core.Abstractions/Infrastructure/Data/DatabaseHelper.cs`
- **Ejemplo en Finanzas:** `src/Modules/Finanzas/VRM_PluginDemo.Modules.Finanzas/Data/`

---

## ? Estado Actual

- ? **DatabaseHelper registrado** en el Host
- ? **IProspectoRepository registrado** en el módulo
- ? **3 métodos de ejemplo** implementados
- ? **DTOs simplificados** (6 propiedades básicas)
- ? **Comentarios explicativos** en todo el código
- ? **Compilación exitosa** sin errores

---

**Este módulo es un EJEMPLO DIDÁCTICO para entender la conexión a BD.**  
**Los métodos y DTOs están simplificados para facilitar el aprendizaje.**  
**Para producción, se agregarían más métodos según las necesidades del negocio.**
