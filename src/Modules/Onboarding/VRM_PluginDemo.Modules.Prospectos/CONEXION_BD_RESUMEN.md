# ?? RESUMEN: Conexión a Base de Datos en Módulos

## ? Lo Esencial en 3 Pasos

### **Paso 1: Host Registra DatabaseHelper**
```csharp
// ?? Program.cs
builder.Services.AddScoped<DatabaseHelper>(provider =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new DatabaseHelper(connectionString);
});
```

### **Paso 2: Módulo Crea Repositorio**
```csharp
// ?? ProspectoRepository.cs
public class ProspectoRepository : IProspectoRepository
{
    private readonly DatabaseHelper _dbHelper;
    
    public ProspectoRepository(DatabaseHelper dbHelper)  // ? Inyectado automáticamente
    {
        _dbHelper = dbHelper;
    }
    
    public async Task<List<ProspectoDto>> GetAllAsync()
    {
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<ProspectoDto>("sp_Prospectos_GetAll"));
    }
}
```

### **Paso 3: Módulo Registra Repositorio**
```csharp
// ?? ProspectosModule.cs
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IProspectoRepository, ProspectoRepository>();
    services.AddScoped<IProspectoService, ProspectoService>();
}
```

---

## ?? Flujo Visual

```
appsettings.json
    ? (connection string)
Program.cs ? DatabaseHelper
    ? (DI)
ProspectoRepository ? _dbHelper.ExecuteStoredProcedure<T>()
    ? (SQL)
MySQL ? sp_Prospectos_GetAll
    ? (resultados)
DatabaseHelper ? Mapeo automático (snake_case ? PascalCase)
    ?
List<ProspectoDto> ?
```

---

## ?? 3 Tipos de Operaciones

| Operación | Método | Retorna |
|-----------|--------|---------|
| SELECT múltiple | `ExecuteStoredProcedure<T>()` | `List<T>` |
| SELECT único | `ExecuteStoredProcedureSingle<T>()` | `T` o `null` |
| INSERT/UPDATE | `ExecuteScalar()` | ID o filas afectadas |

---

## ?? Archivos Clave

```
Host/
??? Program.cs                    ? Registra DatabaseHelper

Módulo Prospectos/
??? Data/
?   ??? DTOs/
?   ?   ??? ProspectoDto.cs      ? 6 propiedades
?   ?   ??? CreateProspectoDto.cs
?   ?   ??? UpdateProspectoDto.cs
?   ??? Repositories/
?       ??? IProspectoRepository.cs    ? 3 métodos
?       ??? ProspectoRepository.cs     ? Implementación con DatabaseHelper
??? ProspectosModule.cs           ? Registra repositorio
```

---

## ? Compilación Exitosa

- ? Sin errores de compilación
- ? DatabaseHelper inyectado correctamente
- ? Repositorio registrado en DI
- ? Mismo patrón que módulo Finanzas

---

## ?? Archivos Eliminados (Innecesarios para Demo)

- ? SQL/Schema.sql
- ? SQL/StoredProcedures.sql
- ? RevisionAreaDto y repositorio
- ? DocumentoProspectoDto y repositorio

**Motivo:** Este es un módulo de prueba para demostrar la conexión, no necesita scripts SQL completos.

---

**Resultado:** Módulo simplificado y funcional que demuestra claramente cómo conectarse a la BD usando DatabaseHelper y repositorios.
