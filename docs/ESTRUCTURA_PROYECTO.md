# ?? ESTRUCTURA DEL PROYECTO

Estructura completa y organizada del proyecto VRM Plugin Demo.

---

## ?? Vista General

```
VRM_Net/
?
??? ?? README.md   # Punto de entrada principal
??? ?? QUICK_START.md      # Guía rápida de 5 minutos
?
??? ?? docs/    # Documentación completa
?   ??? ?? INDICE_DOCUMENTACION.md           # Índice maestro de toda la documentación
?   ??? ?? VERSION_HISTORY.md             # Historial de versiones y changelog
??
?   ??? ?? Autenticación y Seguridad
?   ??? ?? GUIA_AUTENTICACION_SIMULADA.md       # Sistema de autenticación dummy
?   ??? ?? GUIA_INTEGRACION_COOKIES.md          # Cookies HTTP persistentes
?   ??? ?? GUIA_SISTEMA_PERMISOS_GRANULARES.md  # Permisos por acción
?   ??? ?? GUIA_FILTRADO_MODULOS_POR_PERMISOS.md# Filtrado dinámico de módulos
?   ?
?   ??? ??? Arquitectura
?   ??? ?? GUIA_DISENO_ARQUITECTURA.md  # Arquitectura completa del sistema
?   ??? ?? SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md # SSR + Interactive Server
?   ?
?   ??? ?? Desarrollo y Operaciones
?   ??? ?? COMANDOS_SCRIPTS.md  # Scripts y comandos útiles
?   ??? ?? CAMBIOS_APLICADOS_COOKIES_Y_NAVIGATION.md # Historial de cambios técnicos
?   ??? ?? ROADMAP_EMPRESARIAL.md               # Planificación por fases
?   ?
?   ??? ?? Resúmenes
? ??? ?? RESUMEN_ACTUALIZACION_DOCUMENTACION.md # Resumen de updates
?       ??? ?? REORGANIZACION_DOCUMENTACION.md        # Este archivo
?
??? ?? src/              # Código fuente
?   ?
?   ??? ?? Core/        # Núcleo del sistema
?   ?   ??? VRM_Plugin.Core.Abstractions/       # Interfaces (IModule, IComponent, etc.)
?   ?   ?   ??? IModule.cs
?   ?   ?   ??? ModuleComponentInfo.cs
?   ??   ??? README.md
?   ?   ?
?   ?   ??? VRM_Plugin.Core.Domain/        # Modelos de dominio
?   ?       ??? ConfiguracionFiscal.cs
?   ?   ??? ConfiguracionNegocio.cs
?   ?       ??? ModuloHabilitado.cs
?   ?  ??? README.md
?   ?
?   ??? ?? Host/    # Aplicaciones host Blazor
?   ?   ?
?   ?   ??? VRM_Plugin.Blazor.Server/      # Proyecto principal
?   ?   ?   ??? Components/
?   ?   ?   ?   ??? App.razor   # Componente raíz
?   ?   ?   ?   ??? Routes.razor           # Enrutamiento condicional
?   ?   ?   ?   ??? Layout/
?   ?   ?   ?   ?   ??? MainLayout.razor   # Layout principal
?   ?   ?   ?   ?   ??? AuthLayout.razor # Layout para login/logout
? ?   ?   ?   ?   ??? Topbar.razor
?   ?   ?   ?   ?   ??? Sidebar.razor
?   ?   ?   ?   ??? Pages/
?   ?   ?   ?   ?   ??? Index.razor   # Dashboard principal
? ?   ?   ?   ?   ??? Login.razor  # Página de login (SSR)
?   ?   ?   ?   ?   ??? Logout.razor            # Página de logout (SSR)
?   ?   ?   ?   ??? Auth/
?   ?   ?   ?   ??? RedirectToLogin.razor
??   ?   ?
?   ?   ?   ??? Services/
?   ?   ?   ?   ??? ModuleLoader.cs    # Carga dinámica de módulos
?   ?   ?   ?   ??? DummyAuthenticationStateProvider.cs # Autenticación + cookies
?   ?   ?   ?
?   ?   ?   ??? Modules/    # Carpeta para DLLs de módulos
?   ?   ?   ?   ??? VRM_Plugin.Modules.Finanzas.dll
? ?   ?   ?   ??? VRM_Plugin.Modules.Prospectos.dll
?   ?   ?   ?
?   ?   ?   ??? wwwroot/        # Archivos estáticos
?   ?   ?   ?   ??? assets/
?   ?   ?   ?   ?   ??? css/
?   ?   ?   ?   ?   ??? js/
?   ? ?   ?   ?   ??? images/
??   ?   ?   ??? favicon.ico
?   ?   ?   ?
?   ? ?   ??? Program.cs             # Configuración de la app
?   ?   ?   ??? appsettings.json
?   ?   ?   ??? appsettings.Development.json
?   ?   ?   ??? README.md
?   ?   ?
?   ?   ??? Sliced_web_app/  # Proyecto demo simple
?   ?       ??? Components/
?   ?       ??? wwwroot/
?   ?       ??? README.md
?   ?
?   ??? ?? Modules/            # Módulos de negocio
?       ?
?       ??? Finanzas/
?    ?   ??? VRM_PluginDemo.Modules.Finanzas/
?       ?  ??? FinanzasModule.cs # Implementación de IModule
?       ?       ??? Components/
?       ?       ?   ??? Finanzas.razor          # Componente principal del módulo
?       ?       ??? Services/
?       ?       ?   ??? IFacturaService.cs
?       ?       ?   ??? FacturaService.cs
?       ?       ?   ??? IPagoService.cs
?       ?       ?   ??? PagoService.cs
?       ?       ??? Domain/
?       ?       ? ??? Factura.cs
?       ?       ???? Pago.cs
? ?       ??? README.md
?       ?
?       ??? Onboarding/
?       ?   ??? VRM_PluginDemo.Modules.Prospectos/
?       ?       ??? ProspectosModule.cs         # Implementación de IModule
?    ?     ??? Components/
?       ?       ?   ??? Prospectos.razor
?       ?       ??? Services/
?       ??   ??? IProspectoService.cs
?       ?       ?   ??? ProspectoService.cs
?     ?   ??? Domain/
?       ?       ?   ??? Prospecto.cs
?       ??   ??? RevisionArea.cs
?       ?       ?   ??? EstadoProspecto.cs
?     ?       ??? README.md
?       ?
?       ??? README.md    # Guía para crear módulos
?
??? ?? scripts/          # Scripts de automatización
?   ??? build-modules.ps1
?   ??? deploy.ps1
?   ??? backup-database.ps1
?
??? ?? tests/           # Tests (futuro)
?   ??? VRM_Plugin.UnitTests/
?   ??? VRM_Plugin.IntegrationTests/
?
??? ?? .gitignore
??? ?? LICENSE
??? ?? VRM_Net.sln       # Solution file
```

---

## ?? Estadísticas del Proyecto

### Documentación
- **Total de archivos de documentación:** 13
- **Archivos en raíz:** 2 (README.md, QUICK_START.md)
- **Archivos en docs/:** 13
- **Guías técnicas:** 10
- **Resúmenes y referencias:** 3

### Código Fuente

#### Proyectos
- **Core:** 2 proyectos (Abstractions, Domain)
- **Host:** 2 proyectos (Blazor.Server, Sliced_web_app)
- **Módulos:** 2 módulos (Finanzas, Prospectos)
- **Total:** 6 proyectos principales

#### Componentes Clave
- **Interfaces:** IModule, IComponent
- **Proveedores:** DummyAuthenticationStateProvider
- **Loaders:** ModuleLoader
- **Layouts:** MainLayout, AuthLayout
- **Páginas:** Login, Logout, Index

---

## ?? Puntos de Entrada por Rol

### ????? Desarrollador Nuevo
1. `README.md` ? Visión general
2. `QUICK_START.md` ? Ejecutar en 5 minutos
3. `docs/INDICE_DOCUMENTACION.md` ? Navegación completa
4. `docs/GUIA_AUTENTICACION_SIMULADA.md` ? Entender autenticación
5. `src/Modules/README.md` ? Crear módulos

### ??? Arquitecto
1. `README.md` ? Visión general
2. `docs/GUIA_DISENO_ARQUITECTURA.md` ? Arquitectura completa
3. `docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md` ? Renderizado
4. `src/Core/` ? Abstracciones y dominio
5. `docs/ROADMAP_EMPRESARIAL.md` ? Planificación

### ?? DevOps
1. `README.md` ? Visión general
2. `docs/COMANDOS_SCRIPTS.md` ? Scripts útiles
3. `scripts/` ? Scripts de automatización
4. `docs/VERSION_HISTORY.md` ? Historial de versiones
5. `docs/CAMBIOS_APLICADOS_COOKIES_Y_NAVIGATION.md` ? Cambios recientes

### ?? QA/Tester
1. `QUICK_START.md` ? Ejecutar la app
2. `docs/GUIA_AUTENTICACION_SIMULADA.md` ? Usuarios de prueba
3. `docs/GUIA_SISTEMA_PERMISOS_GRANULARES.md` ? Permisos a probar
4. `docs/INDICE_DOCUMENTACION.md` ? Casos de prueba

---

## ?? Convenciones de Organización

### Archivos en Raíz
**Solo incluir:**
- `README.md` - Punto de entrada principal
- `QUICK_START.md` - Guía de inicio rápido
- Archivos de configuración del proyecto (.gitignore, LICENSE, .sln)

**NO incluir:**
- Documentación técnica detallada ? va en `docs/`
- Scripts ? van en `scripts/`
- Código fuente ? va en `src/`

### Carpeta `docs/`
**Incluir:**
- Toda la documentación técnica
- Guías de arquitectura
- Manuales de usuario
- Resúmenes y referencias
- Historial de versiones

**Organización interna:**
- Por tema (Autenticación, Arquitectura, etc.)
- Índice maestro (INDICE_DOCUMENTACION.md)
- Versionamiento (VERSION_HISTORY.md)

### Carpeta `src/`
**Estructura:**
```
src/
??? Core/       # Abstracciones y dominio compartido
??? Host/       # Aplicaciones host
??? Modules/    # Módulos de negocio
```

### Carpeta `scripts/`
**Incluir:**
- Scripts de PowerShell (.ps1)
- Scripts de Bash (.sh)
- Scripts de deployment
- Scripts de mantenimiento

---

## ?? Búsqueda Rápida

### Por Concepto

**Autenticación:**
- Guía: `docs/GUIA_AUTENTICACION_SIMULADA.md`
- Implementación: `src/Host/VRM_Plugin.Blazor.Server/Services/DummyAuthenticationStateProvider.cs`
- Login UI: `src/Host/VRM_Plugin.Blazor.Server/Components/Pages/Login.razor`

**Cookies:**
- Guía: `docs/GUIA_INTEGRACION_COOKIES.md`
- Configuración: `src/Host/VRM_Plugin.Blazor.Server/Program.cs`
- Uso: `DummyAuthenticationStateProvider.cs`

**Permisos:**
- Guía: `docs/GUIA_SISTEMA_PERMISOS_GRANULARES.md`
- Definición: `src/Modules/*/FinanzasModule.cs` (método GetActionPermissions)
- Uso en UI: `src/Modules/*/Components/*.razor` (AuthorizeView)

**Módulos:**
- Guía: `src/Modules/README.md`
- Abstracciones: `src/Core/VRM_Plugin.Core.Abstractions/IModule.cs`
- Ejemplos: `src/Modules/Finanzas/` y `src/Modules/Onboarding/`

**Renderizado:**
- Guía: `docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md`
- Implementación: `src/Host/VRM_Plugin.Blazor.Server/Components/Routes.razor`
- Configuración: `src/Host/VRM_Plugin.Blazor.Server/Components/App.razor`

---

## ?? Archivos Críticos

### Configuración
- `src/Host/VRM_Plugin.Blazor.Server/Program.cs` - Configuración de servicios y middleware
- `src/Host/VRM_Plugin.Blazor.Server/appsettings.json` - Configuración de la aplicación

### Autenticación
- `src/Host/VRM_Plugin.Blazor.Server/Services/DummyAuthenticationStateProvider.cs` - Proveedor de autenticación
- `src/Host/VRM_Plugin.Blazor.Server/Components/Pages/Login.razor` - Página de login (SSR)
- `src/Host/VRM_Plugin.Blazor.Server/Components/Pages/Logout.razor` - Página de logout (SSR)

### Módulos
- `src/Host/VRM_Plugin.Blazor.Server/Services/ModuleLoader.cs` - Carga dinámica de módulos
- `src/Core/VRM_Plugin.Core.Abstractions/IModule.cs` - Interface de módulos

### UI Principal
- `src/Host/VRM_Plugin.Blazor.Server/Components/App.razor` - Componente raíz
- `src/Host/VRM_Plugin.Blazor.Server/Components/Routes.razor` - Enrutamiento condicional
- `src/Host/VRM_Plugin.Blazor.Server/Components/Layout/MainLayout.razor` - Layout principal

---

## ?? Patrones Visuales

### En Código
```
?? - Comentarios importantes
? - Implementación correcta
?? - Advertencias
?? - Debugging
?? - Seguridad
```

### En Documentación
```
?? - Documentación
?? - Inicio rápido
??? - Arquitectura
?? - Desarrollo
?? - Tips
? - FAQ
```

---

## ? Checklist de Navegación

Cuando explores el proyecto, sigue este orden:

**Día 1: Exploración Inicial**
- [ ] Leer `README.md` completo
- [ ] Ejecutar según `QUICK_START.md`
- [ ] Probar login con usuarios de prueba
- [ ] Explorar módulos disponibles

**Día 2: Documentación**
- [ ] Revisar `docs/INDICE_DOCUMENTACION.md`
- [ ] Leer `docs/GUIA_AUTENTICACION_SIMULADA.md`
- [ ] Leer `docs/GUIA_SISTEMA_PERMISOS_GRANULARES.md`
- [ ] Revisar estructura de un módulo

**Día 3: Arquitectura**
- [ ] Leer `docs/GUIA_DISENO_ARQUITECTURA.md`
- [ ] Entender `docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md`
- [ ] Explorar código de `ModuleLoader.cs`
- [ ] Revisar implementación de un módulo

**Día 4: Desarrollo**
- [ ] Crear un módulo de prueba siguiendo `src/Modules/README.md`
- [ ] Implementar permisos granulares
- [ ] Probar integración con el host
- [ ] Revisar `docs/COMANDOS_SCRIPTS.md`

---

## ?? Enlaces Útiles

### Documentación Externa
- [Documentación de Blazor](https://learn.microsoft.com/aspnet/core/blazor/)
- [Blazor Render Modes](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes)
- [ASP.NET Core Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/)

### Repositorio
- [GitHub - VRM_Net](https://github.com/mrguz170/VRM_Net)
- [Issues](https://github.com/mrguz170/VRM_Net/issues)

---

**?? Última actualización:** Enero 2025  
**?? Versión:** 2.0  
**?? Mantenedor:** Equipo VRM
