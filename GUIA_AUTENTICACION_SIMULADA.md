# ?? Guía: Sistema de Autenticación Simulado (Datos Dummy)

## ?? Resumen

Se ha implementado un sistema de autenticación **simulado con datos dummy** que te permite probar el sistema de permisos granulares mientras desarrollas la integración con SQL Server.

---

## ?? IMPORTANTE: Esto es SOLO para Desarrollo

Este sistema **NO debe usarse en producción**. Es temporal y será reemplazado por:
- ASP.NET Core Identity
- Conexión a SQL Server
- Contraseñas hasheadas con BCrypt/PBKDF2
- Autenticación de dos factores (opcional)

---

## ?? Usuarios de Prueba Disponibles

| Username | Nombre Completo | Roles | Permisos Clave |
|----------|-----------------|-------|----------------|
| `admin` | Administrador del Sistema | Admin | ? Acceso completo a todo |
| `gerente.finanzas` | Juan Gerente de Finanzas | GerenteFinanzas | ? Puede timbrar facturas en SAT<br/>? Ver reportes sensibles |
| `coordinador.finanzas` | María Coordinadora | CoordinadorFinanzas | ? Crear/editar facturas<br/>? NO puede timbrar |
| `contador` | Pedro Contador | Contador | ? Solo lectura en finanzas |
| `gestor.prospectos` | Ana Gestora | GestorProspectos | ? Aprobar/rechazar prospectos |
| `revisor.legal` | Carlos Revisor Legal | RevisorLegal | ? Solo revisión legal |
| `revisor.finanzas` | Laura Revisora Financiera | RevisorFinanzas | ? Solo revisión financiera |

---

## ?? Cómo Usar

### Opción 1: Login Manual

1. Navega a `/login`
2. Ingresa cualquier username de la tabla anterior
3. Ingresa **cualquier contraseña** (no importa cuál, todas funcionan)
4. Click en "Ingresar"

### Opción 2: Login Rápido (Recomendado para Pruebas)

1. Navega a `/login`
2. En la tabla de usuarios, click en "Login Rápido" del usuario que quieras probar
3. ¡Listo! Serás autenticado instantáneamente

---

## ?? Probando Permisos Granulares

### Escenario 1: Gerente vs Coordinador de Finanzas

**Paso 1:** Login como `gerente.finanzas`
- Navega a `/finanzas`
- Observa que **ves el botón "Timbrar SAT"** ?
- Activa el panel de debug al final de la página
- Verás todas las acciones permitidas incluyendo `Finanzas.Facturas.TimbrarSAT`

**Paso 2:** Logout y login como `coordinador.finanzas`
- Navega a `/finanzas`
- Observa que **NO ves el botón "Timbrar SAT"** ?
- El panel de debug no mostrará `Finanzas.Facturas.TimbrarSAT`

### Escenario 2: Revisores Especializados

**Paso 1:** Login como `revisor.legal`
- Navega a `/prospectos`
- Solo podrás realizar revisiones legales
- No podrás aprobar finalmente el prospecto

**Paso 2:** Login como `gestor.prospectos`
- Podrás aprobar/rechazar finalmente
- Tienes acceso completo al módulo

---

## ?? Cómo Funciona Internamente

### Archivo: `DummyAuthenticationStateProvider.cs`

```csharp
// Lista estática de usuarios dummy
private static readonly List<DummyUser> DummyUsers = new()
{
    new DummyUser
    {
        Id = "user-001",
        Username = "admin",
        Roles = new List<string> { "Admin" }
    },
    // ... más usuarios
};
```

**Login:**
```csharp
public async Task<bool> LoginAsync(string username, string password)
{
    var usuario = GetDummyUserByUsername(username);
    
    if (usuario == null) return false;
    
    // ?? DUMMY: Cualquier contraseña funciona
    if (string.IsNullOrWhiteSpace(password)) return false;
    
    await _sessionStorage.SetAsync("userId", usuario.Id);
    return true;
}
```

**GetAuthenticationStateAsync:**
```csharp
public override async Task<AuthenticationState> GetAuthenticationStateAsync()
{
    var userIdResult = await _sessionStorage.GetAsync<string>("userId");
    var usuario = GetDummyUser(userIdResult.Value);
    
    // Crear claims del usuario
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id),
        new Claim(ClaimTypes.Name, usuario.Username),
        // ... más claims
    };
    
    // Agregar roles
    foreach (var rol in usuario.Roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, rol));
    }
    
    return new AuthenticationState(user);
}
```

---

## ?? Agregar un Nuevo Usuario de Prueba

**Editar:** `src/Host/.../Services/DummyAuthenticationStateProvider.cs`

```csharp
private static readonly List<DummyUser> DummyUsers = new()
{
    // ... usuarios existentes ...
    
    // ? NUEVO USUARIO
    new DummyUser
    {
        Id = "user-008",
        Username = "director.financiero",
        Email = "director@vrm.com",
        NombreCompleto = "Roberto Director Financiero",
        ClienteId = "cliente-001",
        Roles = new List<string> { "DirectorFinanciero", "GerenteFinanzas" }
    }
};
```

---

## ?? Migración a SQL Server (Próximo Paso)

Cuando estés listo para migrar a producción, estos son los pasos:

### 1. Crear Tabla de Usuarios en SQL Server

```sql
CREATE TABLE Usuarios (
    UsuarioId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL, -- Hasheada con BCrypt
    NombreCompleto NVARCHAR(100),
    ClienteId NVARCHAR(50),
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE UsuarioRoles (
    UsuarioId INT,
    Rol NVARCHAR(50),
    PRIMARY KEY (UsuarioId, Rol),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
);
```

### 2. Instalar Paquetes NuGet

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package BCrypt.Net-Next
```

### 3. Crear DbContext

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}
```

### 4. Reemplazar DummyAuthenticationStateProvider

```csharp
// En Program.cs

// ? COMENTAR ESTO (dummy):
// builder.Services.AddScoped<AuthenticationStateProvider, DummyAuthenticationStateProvider>();

// ? DESCOMENTAR ESTO (producción):
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
```

### 5. Migrar Datos

Crear un script para insertar los usuarios dummy en SQL Server con contraseñas reales hasheadas.

---

## ?? Cómo Desactivar/Activar Autenticación Simulada

### Para DESACTIVAR (volver anónimo):

**Opción 1:** Comentar registro en `Program.cs`
```csharp
// builder.Services.AddScoped<AuthenticationStateProvider, DummyAuthenticationStateProvider>();
```

**Opción 2:** Modificar `GetAuthenticationStateAsync` para siempre devolver anónimo:
```csharp
public override async Task<AuthenticationState> GetAuthenticationStateAsync()
{
    return new AuthenticationState(_anonymous); // Siempre anónimo
}
```

### Para RE-ACTIVAR:

Descomentar las líneas correspondientes.

---

## ?? Troubleshooting

### Problema: "No puedo ver los módulos después de login"

**Solución:** Asegúrate de que el usuario tenga el rol correcto.

Verifica en `DummyAuthenticationStateProvider.cs` que el usuario tenga los roles apropiados:
```csharp
Roles = new List<string> { "GerenteFinanzas" } // ? Revisar esto
```

### Problema: "Los permisos granulares no funcionan"

**Solución:** Verifica que `ModuleAuthorizationService` esté registrado:
```csharp
builder.Services.AddScoped<IModuleAuthorizationService, ModuleAuthorizationService>();
```

### Problema: "Session perdida después de refresh"

**Solución:** Es esperado. `ProtectedSessionStorage` solo persiste durante la sesión del navegador. Para persistencia, usa cookies o JWT.

---

## ?? Matriz de Permisos vs Usuarios de Prueba

| Usuario | Admin | Finanzas.Facturas.TimbrarSAT | Finanzas.Reportes.VerSensibles | Prospectos.AprobarFinal |
|---------|-------|------------------------------|--------------------------------|-------------------------|
| admin | ? | ? | ? | ? |
| gerente.finanzas | ? | ? | ? | ? |
| coordinador.finanzas | ? | ? | ? | ? |
| contador | ? | ? | ? | ? |
| gestor.prospectos | ? | ? | ? | ? |
| revisor.legal | ? | ? | ? | ? |

---

## ?? Conclusión

Ahora tienes un sistema de autenticación completamente funcional para desarrollo que:

? Permite probar todos los roles y permisos  
? No requiere base de datos  
? Es fácil de modificar  
? Incluye documentación clara  
? Está listo para ser reemplazado por autenticación real  

**Próximo paso:** Cuando estés listo para producción, sigue la sección "Migración a SQL Server" de esta guía.

---

**Preparado por:** GitHub Copilot  
**Fecha:** 2025  
**Proyecto:** VRM Plugin Demo - Autenticación Simulada  
