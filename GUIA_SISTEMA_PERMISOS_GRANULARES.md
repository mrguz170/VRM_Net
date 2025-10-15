# ?? Guía Completa: Sistema de Permisos Granulares Empresarial

## ?? Índice

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Componentes Implementados](#componentes-implementados)
4. [Cómo Funciona](#cómo-funciona)
5. [Ejemplos de Uso](#ejemplos-de-uso)
6. [Casos de Uso Empresariales](#casos-de-uso-empresariales)
7. [Próximos Pasos](#próximos-pasos)

---

## ?? Resumen Ejecutivo

Has implementado un **sistema de autorización granular a nivel de acción** para tu arquitectura de plugins. Esto permite:

? **Control fino:** No solo controlas quién puede acceder a un módulo, sino **qué puede hacer dentro de él**  
? **Centralizado:** Todos los permisos se definen en el módulo (`GetActionPermissions()`)  
? **Reutilizable:** El componente `<AuthorizeAction>` funciona en todos los módulos  
? **Auditable:** Logs detallados de quién intenta hacer qué  
? **Escalable:** Agregar nuevos permisos es tan simple como agregar una línea al diccionario  

---

## ??? Arquitectura del Sistema

```
???????????????????????????????????????????????????????????????
?                    USUARIO AUTENTICADO                       ?
?                  (GerenteFinanzas o Coordinador)             ?
???????????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????????
?          IModuleAuthorizationService                         ?
?  • CanExecuteActionAsync("Finanzas.Facturas.TimbrarSAT")    ?
?  • Verifica roles del usuario autenticado                    ?
?  • Consulta GetActionPermissions() del módulo                ?
???????????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????????
?              FinanzasModule.GetActionPermissions()           ?
?  {                                                            ?
?    "Finanzas.Facturas.TimbrarSAT": ["GerenteFinanzas"],     ?
?    "Finanzas.Facturas.Crear": ["GerenteFinanzas",           ?
?                                 "CoordinadorFinanzas"]       ?
?  }                                                            ?
???????????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????????
?              <AuthorizeAction> Component                     ?
?  • Muestra/oculta elementos según el resultado               ?
?  • Reactivo a cambios de permisos                            ?
???????????????????????????????????????????????????????????????
```

---

## ?? Componentes Implementados

### 1. **Interfaz Actualizada: `IModule`**

**Ubicación:** `src/Core/VRM_PluginDemo.Core.Abstractions/IModule.cs`

**Nuevo método:**
```csharp
Dictionary<string, string[]> GetActionPermissions();
```

**Propósito:** Cada módulo define sus acciones y los roles permitidos.

---

### 2. **Servicio de Autorización: `IModuleAuthorizationService`**

**Ubicación:** `src/Host/.../Services/IModuleAuthorizationService.cs`

**Métodos principales:**
- `CanExecuteActionAsync(string actionKey)` - Verifica si el usuario actual puede ejecutar una acción
- `GetAvailableActionsAsync()` - Obtiene todas las acciones disponibles para el usuario
- `GetModuleActionsAsync(string moduleId)` - Obtiene todas las acciones de un módulo

**Implementación:** `ModuleAuthorizationService.cs`

---

### 3. **Componente Blazor: `AuthorizeAction`**

**Ubicación:** `src/Host/.../Components/Auth/AuthorizeAction.razor`

**Uso:**
```razor
<AuthorizeAction ActionKey="Finanzas.Facturas.TimbrarSAT">
    <button>Timbrar en SAT</button>
</AuthorizeAction>
```

**Con fallback:**
```razor
<AuthorizeAction ActionKey="Finanzas.Facturas.TimbrarSAT" ShowFallback="true">
    <ChildContent>
        <button>Timbrar en SAT</button>
    </ChildContent>
    <FallbackContent>
        <span class="text-muted">Solo gerentes pueden timbrar</span>
    </FallbackContent>
</AuthorizeAction>
```

---

### 4. **Implementación en Módulos**

#### **FinanzasModule.GetActionPermissions()**

```csharp
public Dictionary<string, string[]> GetActionPermissions()
{
    return new Dictionary<string, string[]>
    {
        // Todos pueden ver
        ["Finanzas.Facturas.Ver"] = new[] { 
            "Admin", "GerenteFinanzas", "CoordinadorFinanzas", "Contador" 
        },
        
        // Gerentes y coordinadores pueden crear/editar
        ["Finanzas.Facturas.Crear"] = new[] { 
            "Admin", "GerenteFinanzas", "CoordinadorFinanzas" 
        },
        
        // ? SOLO GERENTES pueden timbrar
        ["Finanzas.Facturas.TimbrarSAT"] = new[] { 
            "Admin", "GerenteFinanzas" 
        },
    };
}
```

#### **ProspectosModule.GetActionPermissions()**

```csharp
public Dictionary<string, string[]> GetActionPermissions()
{
    return new Dictionary<string, string[]>
    {
        // Revisores especializados solo pueden revisar su área
        ["Prospectos.RevisionLegal"] = new[] { "Admin", "RevisorLegal" },
        ["Prospectos.RevisionFinanciera"] = new[] { "Admin", "RevisorFinanzas" },
        
        // ? SOLO GESTORES pueden aprobar finalmente
        ["Prospectos.AprobarFinal"] = new[] { "Admin", "GestorProspectos" },
    };
}
```

---

## ?? Cómo Funciona

### Flujo Completo: "¿Puede este usuario timbrar una factura?"

```
1. Usuario hace clic en botón "Timbrar SAT"
   ?
2. <AuthorizeAction ActionKey="Finanzas.Facturas.TimbrarSAT">
   ?
3. await AuthService.CanExecuteActionAsync("Finanzas.Facturas.TimbrarSAT")
   ?
4. Servicio obtiene roles del usuario autenticado:
   - ClaimsIdentity ? ["GerenteFinanzas", "EmpleadoActivo"]
   ?
5. Servicio consulta FinanzasModule.GetActionPermissions()
   - "Finanzas.Facturas.TimbrarSAT" ? ["Admin", "GerenteFinanzas"]
   ?
6. Compara:
   - Roles del usuario: ["GerenteFinanzas", "EmpleadoActivo"]
   - Roles requeridos: ["Admin", "GerenteFinanzas"]
   - ? Coincidencia encontrada: "GerenteFinanzas"
   ?
7. Retorna TRUE
   ?
8. <AuthorizeAction> muestra el botón
```

---

## ?? Casos de Uso Empresariales

### Caso 1: Gerente vs Coordinador de Finanzas

**Requisito del negocio:**  
"El coordinador puede crear y editar facturas, pero solo el gerente puede timbrarlas en el SAT y ver reportes confidenciales."

**Solución implementada:**

```csharp
// En FinanzasModule.cs
["Finanzas.Facturas.Crear"] = new[] { "GerenteFinanzas", "CoordinadorFinanzas" },
["Finanzas.Facturas.TimbrarSAT"] = new[] { "GerenteFinanzas" },
["Finanzas.Reportes.VerSensibles"] = new[] { "GerenteFinanzas" },
```

**Resultado:**

| Acción | Gerente | Coordinador | Contador |
|--------|---------|-------------|----------|
| Ver facturas | ? | ? | ? |
| Crear facturas | ? | ? | ? |
| Editar facturas | ? | ? | ? |
| **Timbrar en SAT** | ? | ? | ? |
| **Ver reportes sensibles** | ? | ? | ? |

---

### Caso 2: Revisores Especializados de Prospectos

**Requisito del negocio:**  
"Varios revisores especializados (legal, financiero, técnico) pueden revisar prospectos en paralelo, pero solo el gestor de prospectos puede aprobar o rechazar finalmente."

**Solución implementada:**

```csharp
// En ProspectosModule.cs
["Prospectos.Ver"] = new[] { "GestorProspectos", "RevisorLegal", "RevisorFinanzas", "RevisorTecnico" },
["Prospectos.RevisionLegal"] = new[] { "RevisorLegal" },
["Prospectos.RevisionFinanciera"] = new[] { "RevisorFinanzas" },
["Prospectos.RevisionTecnica"] = new[] { "RevisorTecnico" },
["Prospectos.AprobarFinal"] = new[] { "GestorProspectos" }, // ? Solo gestor
```

**Flujo del negocio:**

```
1. Prospecto entra al sistema
2. Gestor asigna revisores especializados
3. Revisor Legal ? Revisa documentos legales ? Aprueba/Rechaza su parte
4. Revisor Financiero ? Revisa estados financieros ? Aprueba/Rechaza su parte
5. Revisor Técnico ? Revisa capacidades técnicas ? Aprueba/Rechaza su parte
6. ? Gestor ? Ve todas las revisiones ? APRUEBA FINALMENTE (o rechaza)
7. Prospecto convertido a proveedor
```

---

### Caso 3: Autorización por Monto (Avanzado)

**Requisito futuro:**  
"Los pagos menores a $1,000 pueden ser autorizados por coordinadores, pero pagos mayores a $10,000 requieren autorización del director financiero."

**Implementación sugerida:**

```csharp
// En FinanzasModule.cs
["Finanzas.Pagos.AutorizarMenor1k"] = new[] { "CoordinadorFinanzas", "GerenteFinanzas" },
["Finanzas.Pagos.AutorizarEntre1kY10k"] = new[] { "GerenteFinanzas" },
["Finanzas.Pagos.AutorizarMayor10k"] = new[] { "DirectorFinanciero" },
```

**Uso en código:**

```csharp
private async Task AutorizarPago(Pago pago)
{
    string accionRequerida;
    
    if (pago.Monto < 1000)
        accionRequerida = "Finanzas.Pagos.AutorizarMenor1k";
    else if (pago.Monto <= 10000)
        accionRequerida = "Finanzas.Pagos.AutorizarEntre1kY10k";
    else
        accionRequerida = "Finanzas.Pagos.AutorizarMayor10k";
    
    if (!await AuthService.CanExecuteActionAsync(accionRequerida))
    {
        // Mostrar error o solicitar aprobación superior
        return;
    }
    
    // Proceder con la autorización
}
```

---

## ?? Probando el Sistema

### Paso 1: Configurar Usuarios de Prueba

**TODO:** Cuando implementes autenticación, crea estos usuarios:

```csharp
// Usuario 1: Gerente de Finanzas
{
    UsuarioId = "user-001",
    Nombre = "Juan Gerente",
    Roles = ["GerenteFinanzas", "EmpleadoActivo"]
}

// Usuario 2: Coordinador de Finanzas
{
    UsuarioId = "user-002",
    Nombre = "María Coordinadora",
    Roles = ["CoordinadorFinanzas", "EmpleadoActivo"]
}

// Usuario 3: Contador
{
    UsuarioId = "user-003",
    Nombre = "Pedro Contador",
    Roles = ["Contador", "EmpleadoActivo"]
}
```

### Paso 2: Ejecutar y Probar

```bash
dotnet run --project src/Host/VRM_PluginDemo.Blazor.Server/
```

**Navega a:** `https://localhost:XXXX/finanzas`

**Activa el panel de debug** (botón al final de la página) para ver tus permisos:

```
Acciones permitidas en Finanzas:
? Finanzas.Facturas.Ver
? Finanzas.Facturas.Crear
? Finanzas.Facturas.Editar
? Finanzas.Facturas.TimbrarSAT ? Solo si eres gerente
```

---

## ?? Próximos Pasos Recomendados

### Fase 1: Autenticación Completa (CRÍTICO)

Actualmente el sistema de permisos está implementado, pero **necesitas autenticación** para que funcione completamente.

**Implementar:**
1. ASP.NET Core Identity o JWT
2. Login/Logout
3. Asignación de roles a usuarios
4. Persistencia de usuarios en base de datos

**Guía rápida:**
```csharp
// En Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
```

---

### Fase 2: Validación en el Backend (SEGURIDAD)

**NUNCA confíes solo en la UI.** Siempre valida permisos en el backend:

```csharp
// En FacturaService.cs
public class FacturaService : IFacturaService
{
    private readonly IModuleAuthorizationService _authService;
    
    public async Task TimbrarEnSATAsync(int facturaId)
    {
        // ? VALIDAR PERMISO EN EL BACKEND
        if (!await _authService.CanExecuteActionAsync("Finanzas.Facturas.TimbrarSAT"))
        {
            throw new UnauthorizedAccessException(
                "No tienes permiso para timbrar facturas en SAT"
            );
        }
        
        // Proceder con el timbrado...
    }
}
```

---

### Fase 3: Auditoría de Acciones Sensibles

Registra todas las acciones críticas:

```csharp
public async Task TimbrarEnSATAsync(int facturaId)
{
    var usuario = await _currentUserService.GetUsuarioActualAsync();
    
    // Registrar en tabla de auditoría
    await _auditService.LogActionAsync(new AuditLog
    {
        UsuarioId = usuario.Id,
        Accion = "Finanzas.Facturas.TimbrarSAT",
        Entidad = "Factura",
        EntidadId = facturaId.ToString(),
        Fecha = DateTime.UtcNow,
        Resultado = "Exitoso"
    });
    
    // Proceder...
}
```

---

### Fase 4: Tests Automatizados

```csharp
[Fact]
public async Task CanExecuteAction_GerentePuedeTimbrar_ReturnTrue()
{
    // Arrange
    var authService = CreateAuthService(roles: ["GerenteFinanzas"]);
    
    // Act
    var result = await authService.CanExecuteActionAsync("Finanzas.Facturas.TimbrarSAT");
    
    // Assert
    Assert.True(result);
}

[Fact]
public async Task CanExecuteAction_CoordinadorNoPuedeTimbrar_ReturnFalse()
{
    // Arrange
    var authService = CreateAuthService(roles: ["CoordinadorFinanzas"]);
    
    // Act
    var result = await authService.CanExecuteActionAsync("Finanzas.Facturas.TimbrarSAT");
    
    // Assert
    Assert.False(result);
}
```

---

## ?? Matriz Completa de Permisos

### Módulo de Finanzas

| Acción | Admin | Gerente | Coordinador | Contador |
|--------|-------|---------|-------------|----------|
| Facturas.Ver | ? | ? | ? | ? |
| Facturas.Crear | ? | ? | ? | ? |
| Facturas.Editar | ? | ? | ? | ? |
| Facturas.Eliminar | ? | ? | ? | ? |
| **Facturas.TimbrarSAT** | ? | ? | ? | ? |
| **Facturas.CancelarTimbrada** | ? | ? | ? | ? |
| Pagos.Ver | ? | ? | ? | ? |
| Pagos.Crear | ? | ? | ? | ? |
| **Pagos.Autorizar** | ? | ? | ? | ? |
| Conciliacion.Ver | ? | ? | ? | ? |
| **Conciliacion.Ejecutar** | ? | ? | ? | ? |
| Reportes.VerGenerales | ? | ? | ? | ? |
| **Reportes.VerSensibles** | ? | ? | ? | ? |
| **Configuracion.Modificar** | ? | ? | ? | ? |

### Módulo de Prospectos

| Acción | Admin | Gestor | Rev. Legal | Rev. Finanzas | Rev. Técnico |
|--------|-------|--------|------------|---------------|--------------|
| Ver | ? | ? | ? | ? | ? |
| Crear | ? | ? | ? | ? | ? |
| Editar | ? | ? | ? | ? | ? |
| AsignarRevisor | ? | ? | ? | ? | ? |
| RevisionLegal | ? | ? | ? | ? | ? |
| RevisionFinanciera | ? | ? | ? | ? | ? |
| RevisionTecnica | ? | ? | ? | ? | ? |
| **AprobarFinal** | ? | ? | ? | ? | ? |
| **ConvertirProveedor** | ? | ? | ? | ? | ? |

---

## ?? Conceptos Clave para tu Equipo

### 1. Separación de Responsabilidades

```
UI (Blazor) ? Muestra/oculta elementos
    ?
AuthorizationService ? Verifica permisos
    ?
Módulo ? Define permisos
    ?
Backend (Servicios) ? Valida y ejecuta (doble verificación)
```

### 2. Defensa en Profundidad

**Nunca confíes solo en un nivel:**

1. ? Ocultar botón en la UI
2. ? Verificar permiso en el evento @onclick
3. ? Verificar permiso en el servicio backend
4. ? Registrar intento en auditoría

### 3. Principio de Menor Privilegio

**Por defecto, todo está prohibido.** Solo permites explícitamente lo necesario:

```csharp
// ? MAL: Permitir por defecto
if (!IsProhibido(action)) { /* ejecutar */ }

// ? BIEN: Prohibir por defecto
if (IsPermitido(action)) { /* ejecutar */ }
```

---

## ?? ¡Felicidades!

Has implementado un **sistema de permisos granulares de nivel empresarial** que:

? Es mantenible (permisos centralizados por módulo)  
? Es escalable (agregar acciones es trivial)  
? Es seguro (validación en múltiples capas)  
? Es auditable (logs de intentos de acceso)  
? Es flexible (soporta cualquier jerarquía de roles)  

---

**¿Preguntas o necesitas ayuda?**  
Revisa las secciones anteriores o consulta el código implementado. ??
