# VRM Net - Sistema Modular de Gestión Empresarial

Sistema empresarial basado en arquitectura de plugins dinámicos construido con Blazor Server (.NET 8).

## Arquitectura del Sistema

Este proyecto utiliza una **arquitectura modular basada en plugins** que permite:

- ✅ Cargar módulos dinámicamente en tiempo de ejecución
- ✅ Habilitar/deshabilitar funcionalidades por cliente
- ✅ Agregar nuevos módulos sin modificar el código base
- ✅ Sistema de permisos granular por acción
- ✅ Hot deployment (copiar DLL → reiniciar → módulo disponible)

## Estructura del Proyecto

```
VRM_Net/
├── src/
│   ├── Core/                    # 🔧 Abstracciones y contratos compartidos
│   ├── Host/                    # 🚀 Aplicación principal Blazor Server
│   └── Modules/                 # 📦 Módulos de negocio (plugins)
│       ├── Finanzas/
│       └── Onboarding/
└── README.md
```

---

## Core - Fundamentos del Sistema

**Ubicación:** `src/Core/`

Contiene las **abstracciones y contratos** que definen cómo funciona el sistema de plugins.

### Proyectos:

#### `VRM_Plugin.Core.Abstractions`
Define las interfaces que todos los módulos deben implementar:

- **`IModule`** - Contrato principal que implementa cada módulo
- **`ModuleComponentInfo`** - Metadata de componentes Blazor
- Interfaces para permisos, dependencias y configuración

#### `VRM_Plugin.Core.Domain`
Modelos compartidos entre módulos:

- Configuración fiscal (`ConfiguracionFiscal`)
- Configuración de negocio (`ConfiguracionNegocio`)
- Entidades comunes del dominio

### Concepto clave: `IModule`

Cada módulo implementa esta interfaz para integrarse al sistema:

```csharp
public interface IModule
{
    string ModuleId { get; }                    // Identificador único
    string DisplayName { get; }                 // Nombre visible
    List<ModuleComponentInfo> GetComponents();  // Componentes Blazor
    void ConfigureServices(...);                // Servicios del módulo
    Dictionary<string,string[]> GetActionPermissions(); // Permisos granulares
}
```

---

## Host - Aplicación Principal

**Ubicación:** `src/Host/VRM_PluginDemo.Blazor.Server/`

Es la **aplicación Blazor Server** que orquesta todo el sistema.

### Responsabilidades:

- ✅ **Descubre y carga módulos** dinámicamente desde la carpeta `Modules/`
- ✅ **Configura autenticación y autorización** con cookies
- ✅ **Registra servicios** de cada módulo en el contenedor DI
- ✅ **Renderiza componentes** de módulos en el routing
- ✅ **Protege rutas** según permisos del usuario

### Componentes clave:

#### `Program.cs`
- Configura autenticación con cookies
- Descubre módulos con `ModuleLoader`
- Registra servicios de cada módulo
- Configura rutas dinámicas

#### `Services/ModuleLoader.cs`
- Busca DLLs en la carpeta `Modules/`
- Carga ensamblados dinámicamente
- Instancia las clases que implementan `IModule`

#### `Components/Routes.razor`
- Router principal con `<AuthorizeRouteView>`
- Protege todas las rutas (excepto login)
- Redirige usuarios no autenticados a `/login`

#### `Services/DummyAuthenticationStateProvider.cs`
- Sistema de autenticación simulado (solo desarrollo)
- Usa cookies persistentes con `SignInAsync`/`SignOutAsync`
- Mantiene estado entre SSR e Interactive Server

### Sistema de Seguridad:
1. Usuario intenta acceder a `/finanzas` sin login
2. `[Authorize]` en el componente detecta que no está autenticado
3. `<AuthorizeRouteView>` activa `<RedirectToLogin />`
4. Usuario es redirigido a `/login?ReturnUrl=finanzas`
5. Después del login exitoso → vuelve a `/finanzas`

---

## Modules - Módulos de Negocio. Los modulos son plugins dinámicos y autónomos.
## Modulos prueba para demostrar la arquitectura modular.

**Ubicación:** `src/Modules/`

Cada módulo es un **proyecto independiente** que se compila a DLL y se carga dinámicamente.

### Módulos Disponibles:

#### 1. **Finanzas** (`src/Modules/Finanzas/VRM_Plugin.Modules.Finanzas/`)

**Propósito:** Gestión de facturas y pagos

**Ruta:** `/finanzas`

**Permisos:**
- Ver facturas: `Admin`, `GerenteFinanzas`, `CoordinadorFinanzas`, `Contador`
- Crear facturas: `Admin`, `GerenteFinanzas`, `CoordinadorFinanzas`
- Timbrar SAT: `Admin`, `GerenteFinanzas` (granular)

#### 2. **Prospectos** (`src/Modules/Onboarding/VRM_Plugin.Modules.Prospectos/`)

**Propósito:** Onboarding de nuevos proveedores

**Ruta:** `/prospectos`

**Permisos:**
- Ver prospectos: `Admin`, `GestorProspectos`, `RevisorProspectos`
- Crear prospectos: `Admin`, `GestorProspectos`
- Aprobar final: `Admin`, `GestorProspectos` (granular)

### Estructura de un Módulo:

```
VRM_Plugin.Modules.Finanzas/
├── Components/
│   ├── Finanzas.razor          # Componente con @page "/finanzas"
│   └── _Imports.razor           # Importaciones (incluye [Authorize])
├── Domain/
│   ├── Factura.cs
│   └── Pago.cs
├── Services/
│   ├── IFacturaService.cs
│   └── FacturaService.cs
└── FinanzasModule.cs           # Implementa IModule
```


---

## Flujo de Carga de Módulos

```
1. 🏁 App inicia (Program.cs)
      ⬇️
2. 🔍 ModuleLoader busca DLLs en Modules/
      ⬇️
3. 📦 Carga ensamblados con Assembly.LoadFrom()
      ⬇️
4. 🔎 Busca clases que implementen IModule
      ⬇️
5. 🎯 Crea instancia de cada módulo
      ⬇️
6. ⚙️ Llama a ConfigureServices() de cada módulo
      ⬇️
7. 🗺️ Registra componentes en Routes.razor
      ⬇️
8. ✅ Módulos disponibles en el menú (según permisos)
```

```

**🎉 El módulo aparece automáticamente en el menú**

---

## 👥 Usuarios de Prueba

El sistema incluye usuarios dummy para desarrollo:

| Email | Roles | Acceso |
|-------|-------|--------|
| `admin@vrm.com` | Admin | Todo el sistema |
| `gerente.finanzas@vrm.com` | GerenteFinanzas | Finanzas (completo) |
| `coordinador.finanzas@vrm.com` | CoordinadorFinanzas | Finanzas (limitado) |
| `contador@vrm.com` | Contador | Finanzas (solo lectura) |
| `gestor.prospectos@vrm.com` | GestorProspectos | Prospectos (completo) |

**Contraseña:** Cualquiera (es un sistema dummy)

```
---

```
## 🔧 Tecnologías Utilizadas

- **.NET 8** - Framework base
- **Blazor Server** - UI interactiva
- **ASP.NET Core Identity** - Autenticación con cookies
- **Reflection** - Carga dinámica de módulos
- **Dependency Injection** - Inyección de servicios por módulo
- **Tailwind CSS** - Estilos (tema Sliced)
- **Remix Icons** - Iconografía
```
---
```
## Estructura de Archivos Importantes


VRM_Net/
├── src/
│   ├── Core/
│   │   ├── VRM_Plugin.Core.Abstractions/
│   │   │   └── IModule.cs                          # ⭐ Interfaz principal
│   │   └── VRM_Plugin.Core.Domain/
│   │       └── ConfiguracionNegocio.cs
│   │
│   ├── Host/
│   │   └── VRM_PluginDemo.Blazor.Server/
│   │       ├── Program.cs                          # ⭐ Configuración principal
│   │       ├── Components/
│   │       │   ├── Routes.razor                    # ⭐ Router con seguridad
│   │       │   ├── Pages/
│   │       │   │   └── Login.razor                 # Login SSR
│   │       │   └── Auth/
│   │       │       └── RedirectToLogin.razor       # Redirección automática
│   │       ├── Services/
│   │       │   ├── ModuleLoader.cs                 # ⭐ Cargador de módulos
│   │       │   └── DummyAuthenticationStateProvider.cs
│   │       └── Modules/                            # 📂 Aquí se copian las DLLs
│   │
│   └── Modules/
│       ├── Finanzas/
│       │   └── VRM_PluginDemo.Modules.Finanzas/
│       │       ├── FinanzasModule.cs               # ⭐ Implementación IModule
│       │       ├── Components/
│       │       │   ├── Finanzas.razor              # ⭐ Componente principal
│       │       │   └── _Imports.razor              # Importaciones
│       │       ├── Domain/
│       │       │   └── Factura.cs
│       │       └── Services/
│       │           └── FacturaService.cs
│       │
│       └── Onboarding/
│           └── VRM_PluginDemo.Modules.Prospectos/
│               ├── ProspectosModule.cs             # ⭐ Implementación IModule
│               └── Components/
│                   └── Prospectos.razor            # ⭐ Componente principal
│
└── README.md                                       # Este archivo
```

---
```
## Ventajas de Esta Arquitectura

✅ **Modularidad** - Cada módulo es independiente  
✅ **Escalabilidad** - Agregar funcionalidades sin tocar el core  
✅ **Hot Deployment** - Copiar DLL y reiniciar  
✅ **Seguridad Granular** - Permisos por acción  
✅ **Separación de Responsabilidades** - Cada módulo gestiona su dominio  
✅ **Reutilización** - Módulos compartibles entre proyectos  

---

```
