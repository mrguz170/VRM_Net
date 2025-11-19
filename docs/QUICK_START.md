# ? Quick Start - VRM System

## ?? Ejecutar el Proyecto (5 minutos)

### **Opción 1: Desde Terminal**

```bash
# 1. Navegar al Host
cd src/Host/VRM_Plugin.Blazor.Server

# 2. Ejecutar
dotnet run

# 3. Abrir navegador
# http://localhost:5000

# Credenciales:
# Usuario: admin
# Password: admin123
```

### **Opción 2: Desde Visual Studio**

1. Abrir solución: `VRM_Net.sln`
2. Establecer como proyecto de inicio: `VRM_Plugin.Blazor.Server`
3. Presionar `F5`
4. Login: `admin` / `admin123`

---

## ?? Crear Nuevo Módulo (2 minutos)

### **Paso 1: Ejecutar Script**

```powershell
# Desde la raíz del proyecto
.\New-VRMPlugin.ps1 `
    -ModuleName "Inventario" `
    -IdModule 3 `
    -Category "Operaciones" `
    -StartIdComponent 100 `
    -StartIdAction 100 `
    -IconRoot "ri-box-line"
```

### **Paso 2: Reiniciar App**

```bash
# Detener app (Ctrl+C)
# Iniciar nuevamente
dotnet run
```

### **Paso 3: Navegar**

```
http://localhost:5000/inventario
```

**¡Listo!** Tu módulo ya está funcionando ??

---

## ?? Documentación Completa

Para más detalles, consulta:

- **[docs/README.md](docs/README.md)** - Índice de documentación
- **[docs/ARQUITECTURA_PLUGINS.md](docs/ARQUITECTURA_PLUGINS.md)** - Arquitectura del sistema
- **[docs/GENERADOR_PLUGINS.md](docs/GENERADOR_PLUGINS.md)** - Guía del generador

---

## ?? Ejemplos Rápidos

### **Proteger un Componente**

```razor
@page "/mimodulo"
@inject IModuleAuthorizationService AuthService

<AuthorizeModule IdComponent="100">
    <h1>Mi Módulo Protegido</h1>
    <!-- Solo usuarios con permiso ven esto -->
</AuthorizeModule>
```

### **Proteger una Acción**

```razor
<AuthorizeAction ActionKey="MiModulo.Eliminar">
    <button class="btn btn-danger">
        Eliminar (Solo Admin)
    </button>
</AuthorizeAction>
```

### **Agregar Servicio al Módulo**

```csharp
public class MiModuloModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IMiServicio, MiServicio>();
    }
}
```

---

## ? Problemas Comunes

### **El módulo no aparece en el menú**

1. ? Verificar que DLL está en `Host/bin/Debug/net8.0/Modules/`
2. ? Reiniciar aplicación
3. ? Verificar logs en consola

### **Error al compilar módulo**

```bash
# Limpiar y recompilar
dotnet clean
dotnet build --configuration Release
```

### **Permiso denegado**

- ? Verificar que el usuario tiene el permiso requerido
- ? Revisar `RequiredPermissionIds` en el módulo
- ? Login como `admin` (tiene todos los permisos)

---

## ?? Ayuda

- **Documentación**: `docs/README.md`
- **GitHub**: [VRM_Net Repository](https://github.com/mrguz170/VRM_Net)
- **Equipo**: Canal de Slack/Teams

---

**¡Feliz Desarrollo!** ??
