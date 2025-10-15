# ?? QUICK START: Autenticación y Permisos Granulares

## ? Inicio Rápido (5 minutos)

### 1. Ejecutar el Proyecto

```bash
cd src/Host/VRM_PluginDemo.Blazor.Server
dotnet run
```

### 2. Abrir en Navegador

```
https://localhost:XXXX/login
```

### 3. Probar Usuarios

Click en "Login Rápido" para cualquiera de estos usuarios:

| Usuario | Verás en /finanzas |
|---------|-------------------|
| `admin` | ? TODO (todos los botones) |
| `gerente.finanzas` | ? Botón "SAT", Reportes Sensibles |
| `coordinador.finanzas` | ? NO botón "SAT", NO Reportes Sensibles |
| `contador` | ? SOLO botón "Ver" (lectura) |

---

## ?? Archivos Importantes

### Para Modificar Usuarios Dummy:
```
src/Host/.../Services/DummyAuthenticationStateProvider.cs
```

**Línea 120-200:** Lista `DummyUsers`

### Para Ver Permisos por Módulo:
```
src/Modules/Finanzas/.../FinanzasModule.cs
```

**Línea 52-90:** Método `GetActionPermissions()`

### Para Modificar UI del Módulo:
```
src/Modules/Finanzas/.../Components/Finanzas.razor
```

**Buscar:** `<AuthorizeView Roles="...">` para ver/modificar qué roles ven qué botones

---

## ?? Casos de Uso Comunes

### Agregar un Nuevo Usuario de Prueba

**Editar:** `DummyAuthenticationStateProvider.cs`

```csharp
// Agregar al final de DummyUsers:
new DummyUser
{
    Id = "user-008",
    Username = "director",
    Email = "director@vrm.com",
    NombreCompleto = "Roberto Director",
    ClienteId = "cliente-001",
    Roles = new List<string> { "Director", "GerenteFinanzas" }
}
```

---

### Agregar un Nuevo Permiso Granular

**1. Definir en el módulo** (`FinanzasModule.cs`):

```csharp
public Dictionary<string, string[]> GetActionPermissions()
{
    return new Dictionary<string, string[]>
    {
        // ... permisos existentes ...
        
        // ? NUEVO PERMISO
        ["Finanzas.Pagos.AutorizarMayor10k"] = new[] { "Director" },
    };
}
```

**2. Usar en la UI** (`Finanzas.razor`):

```razor
<AuthorizeView Roles="Director">
    <Authorized>
        <button @onclick="AutorizarPagoGrande">
            Autorizar Pago Mayor a $10,000
        </button>
    </Authorized>
</AuthorizeView>
```

---

### Proteger una Página Completa

Agregar al inicio del componente `.razor`:

```razor
@page "/finanzas"
@attribute [Authorize(Roles = "Admin,GerenteFinanzas,CoordinadorFinanzas")]
```

---

## ?? Cambiar Entre Usuarios Rápidamente

1. Ir a `/logout`
2. Ir a `/login`
3. Click "Login Rápido" en el usuario deseado
4. ¡Listo!

---

## ?? Problemas Comunes

### "No veo los módulos después de login"

**Causa:** Usuario no tiene el rol correcto  
**Solución:** Verificar `Roles` en `DummyAuthenticationStateProvider.cs`

### "Todos ven todos los botones"

**Causa:** Falta `AuthorizeView` en el componente  
**Solución:** Envolver botones con `<AuthorizeView Roles="...">`

### "Sesión se pierde al refrescar"

**Causa:** Comportamiento esperado con autenticación simulada  
**Solución:** Hacer login nuevamente (en producción usarás cookies persistentes)

---

## ?? Matriz Rápida de Roles

| Rol | Descripción | Módulos |
|-----|-------------|---------|
| Admin | Administrador total | ? Todos |
| GerenteFinanzas | Gerente de Finanzas | ? Finanzas (completo) |
| CoordinadorFinanzas | Coordinador | ? Finanzas (sin timbrar) |
| Contador | Contador | ? Finanzas (solo lectura) |
| GestorProspectos | Gestor de Prospectos | ? Prospectos (completo) |
| RevisorLegal | Revisor Legal | ? Prospectos (solo legal) |
| RevisorFinanzas | Revisor Financiero | ? Prospectos (solo finanzas) |

---

## ?? Comandos Útiles

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run --project src/Host/VRM_PluginDemo.Blazor.Server/

# Limpiar y recompilar
dotnet clean
dotnet build

# Ver logs en tiempo real
dotnet run --project src/Host/VRM_PluginDemo.Blazor.Server/ | Select-String "módulo"
```

---

## ?? Documentación Completa

- **Autenticación:** `GUIA_AUTENTICACION_SIMULADA.md`
- **Permisos Granulares:** `GUIA_SISTEMA_PERMISOS_GRANULARES.md`
- **Resumen Ejecutivo:** `RESUMEN_EJECUTIVO_PERMISOS.md`
- **Estado Actual:** `RESUMEN_AUTENTICACION_IMPLEMENTADA.md`

---

## ? Checklist Pre-Demo

- [ ] Proyecto compila sin errores: `dotnet build`
- [ ] Puedes hacer login como `admin`
- [ ] Puedes ver módulo de Finanzas
- [ ] Gerente ve botón "SAT", Coordinador NO
- [ ] Puedes hacer logout y volver a login

---

**Si todo lo anterior funciona, ¡estás listo para demostrar el sistema!** ??

---

**Contacto Rápido:**  
Si encuentras problemas, revisa los archivos de documentación arriba o busca en los comentarios del código (tienen emojis ? para encontrarlos fácilmente).
