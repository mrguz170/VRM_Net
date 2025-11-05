# Sistema de Permisos Granulares - VRM_Net

**Proyecto:** VRM Plugin System  
**Branch:** dev-mainUI  
**Plataforma:** .NET 8 / Blazor Server  
**Fecha:** Enero 2024  
**Versión del Documento:** 1.0

---

## ?? Tabla de Contenidos

1. [Introducción](#introducción)
2. [Estructura Actual del Proyecto](#estructura-actual-del-proyecto)
3. [Arquitectura del Sistema de Permisos](#arquitectura-del-sistema-de-permisos)
4. [Roadmap de Implementación](#roadmap-de-implementación)
5. [Modelo de Datos](#modelo-de-datos)
6. [Guías de Implementación](#guías-de-implementación)
7. [Consideraciones de Diseño](#consideraciones-de-diseño)
8. [Plan de Migración](#plan-de-migración)
9. [Referencias y Recursos](#referencias-y-recursos)

---

## ?? Introducción

### Contexto del Proyecto

El sistema **VRM_Net** es una plataforma empresarial modular construida con Blazor Server que actualmente utiliza un sistema de permisos hardcodeado. Este documento detalla el plan completo para migrar a un **sistema de permisos granulares basado en base de datos** que permita:

- **Gestión dinámica de módulos y submódulos**
- **Permisos granulares a nivel de acción** (botones, operaciones específicas)
- **Multitenancy** (soporte para múltiples clientes/empresas)
- **Roles dinámicos** configurables por administrador
- **Auditoría completa** de accesos y cambios de permisos

### Repositorio

```
?? Repository: https://github.com/mrguz170/VRM_Net
?? Workspace: C:\Users\lobo_\Documents\VRM\VRM_Net\
?? Branch: dev-mainUI
```

---

## ??? Estructura Actual del Proyecto

### Proyectos en la Solución

```
VRM_Net/
??? src/
?   ??? Core/
?   ? ??? VRM_Plugin.Core.Abstractions     (Interfaces del sistema de plugins)
?   ?   ??? VRM_Plugin.Core.Domain    (Entidades de dominio compartidas)
?   ?
?   ??? Host/
?   ?   ??? VRM_Plugin.Blazor.Server         (Aplicación Blazor principal)
?   ?   ??? Sliced_web_app              (Host alternativo/legacy)
?   ?
?   ??? Modules/
?       ??? Finanzas/
?       ?   ??? VRM_Plugin.Finanzas          (Módulo de Finanzas - DLL)
?    ??? Onboarding/
?           ??? VRM_Plugin.Prospectos      (Módulo de Prospectos - DLL)
```

### Proyectos Clave

| Proyecto | Ruta | Propósito |
|----------|------|-----------|
| **VRM_Plugin.Core.Abstractions** | `src/Core/VRM_Plugin.Core.Abstractions/` | Define `IModule` y contratos base del sistema de plugins |
| **VRM_Plugin.Core.Domain** | `src/Core/VRM_Plugin.Core.Domain/` | Entidades de dominio compartidas (ConfiguracionNegocio, etc.) |
| **VRM_Plugin.Blazor.Server** | `src/Host/VRM_PluginDemo.Blazor.Server/` | Host principal Blazor Server, carga módulos dinámicamente |
| **VRM_Plugin.Finanzas** | `src/Modules/Finanzas/VRM_PluginDemo.Modules.Finanzas/` | Módulo de gestión financiera (Facturas, Pagos, Reportes) |
| **VRM_Plugin.Prospectos** | `src/Modules/Onboarding/VRM_PluginDemo.Modules.Prospectos/` | Módulo de onboarding de clientes |

### Archivos Críticos del Sistema Actual

```
src/Host/VRM_PluginDemo.Blazor.Server/
??? Program.cs       (Configuración principal, carga de módulos)
??? Services/
?   ??? DummyAuthenticationStateProvider.cs      (Autenticación simulada actual)
?   ??? ModuleLoader.cs               (Carga dinámica de DLLs)
?   ??? IModuleManager.cs       (Interfaz de gestión de módulos)
?   ??? ModuleAuthorizationService.cs            (Autorización granular actual)
?   ??? IModuleAuthorizationService.cs           (Interfaz de autorización)
??? Components/
?   ??? Layout/
?       ??? NavMenu.razor         (Menú lateral - requiere actualización)
```

---

## ?? Arquitectura del Sistema de Permisos

### Capas de la Arquitectura

```
???????????????????????????????????????????????????????????????????
?                CAPA DE PRESENTACIÓN            ?
?  • Blazor Server Components      ?
?  • NavMenu.razor (Menú dinámico)      ?
?  • <AuthorizeAction> Component (Permisos granulares en UI)     ?
???????????????????????????????????????????????????????????????????
            ?
???????????????????????????????????????????????????????????????????
?              CAPA DE SERVICIOS DE APLICACIÓN            ?
?  • IPermisosService (Gestión de permisos desde BD)         ?
?  • IModuleAuthorizationService (Autorización en runtime)  ?
?  • INavMenuService (Construcción de menú dinámico)           ?
?  • IRolesService (CRUD de roles)        ?
?  • IModulosService (Gestión de módulos)                 ?
???????????????????????????????????????????????????????????????????
         ?
???????????????????????????????????????????????????????????????????
?         CAPA DE DOMINIO              ?
?  • Modulo (Entidad: módulos y submódulos)        ?
?  • AccionModulo (Entidad: acciones granulares)  ?
?  • Rol (Entidad: roles del sistema)             ?
?  • PermisoAccion (Entidad: relación Rol-Acción)       ?
?  • Usuario (Entidad: usuarios del sistema)      ?
?  • UsuarioRol (Entidad: relación Usuario-Rol) ?
?  • ModuloCliente (Entidad: módulos habilitados por cliente)    ?
???????????????????????????????????????????????????????????????????
             ?
???????????????????????????????????????????????????????????????????
?         CAPA DE INFRAESTRUCTURA ?
?  • ApplicationDbContext (EF Core DbContext)              ?
?  • Repositorios (Acceso a datos)             ?
?  • PermisosService (Implementación de IPermisosService)  ?
?  • Redis Cache (Caché distribuido de permisos)      ?
???????????????????????????????????????????????????????????????????
 ?
???????????????????????????????????????????????????????????????????
?        BASE DE DATOS (SQL Server)      ?
?  • Modulos      ?
?  • AccionesModulo       ?
?  • Roles ?
?  • PermisosAccion       ?
?  • Usuarios             ?
?  • UsuariosRoles     ?
?  • ModulosClientes             ?
?  • AuditoriaPermisos            ?
???????????????????????????????????????????????????????????????????
```

### Flujo de Autorización

```
????????????????????????????????????????????????????????????????????
? 1. USUARIO INICIA SESIÓN           ?
????????????????????????????????????????????????????????????????????
             ?
             ?
????????????????????????????????????????????????????????????????????
? 2. DummyAuthenticationStateProvider   ?
?    • Valida credenciales contra BD      ?
?    • Consulta IPermisosService.ObtenerRolesUsuarioAsync()       ?
?    • Crea ClaimsPrincipal con roles dinámicos ?
????????????????????????????????????????????????????????????????????
      ?
   ?
????????????????????????????????????????????????????????????????????
? 3. NavMenu.razor SE RENDERIZA     ?
?    • Consulta IPermisosService.ObtenerModulosHabilitadosAsync() ?
?    • Filtra módulos según permisos del usuario               ?
?    • Construye árbol jerárquico de menú   ?
????????????????????????????????????????????????????????????????????
             ?
          ?
????????????????????????????????????????????????????????????????????
? 4. USUARIO NAVEGA A MÓDULO (ej: /finanzas)     ?
?    • Componente Finanzas.razor se renderiza              ?
?    • <AuthorizeAction> evalúa permisos de botones          ?
????????????????????????????????????????????????????????????????????
?
     ?
????????????????????????????????????????????????????????????????????
? 5. USUARIO HACE CLIC EN "TIMBRAR SAT"        ?
?    • <AuthorizeAction Action="Finanzas.Facturas.TimbrarSAT">   ?
?    • Consulta IPermisosService.UsuarioPuedeEjecutarAccionAsync()?
?  • Si true: muestra botón habilitado    ?
?    • Si false: muestra botón disabled con tooltip      ?
????????????????????????????????????????????????????????????????????
             ?
    ?
????????????????????????????????????????????????????????????????????
? 6. BACKEND VALIDA PERMISO (doble verificación)         ?
?    • FacturaService.TimbrarAsync() valida permiso nuevamente    ?
?    • Si no autorizado: throw UnauthorizedAccessException        ?
?    • Si autorizado: ejecuta lógica + auditoría         ?
????????????????????????????????????????????????????????????????????
```

---

## ??? Roadmap de Implementación

### Resumen Ejecutivo

| Fase | Duración Estimada | Complejidad | Prioridad |
|------|-------------------|-------------|-----------|
| **Fase 0: Análisis y Diseño** | 1-2 semanas | Media | ?? Crítica |
| **Fase 1: Infraestructura de BD** | 1-2 semanas | Alta | ?? Crítica |
| **Fase 2: Servicios y Repositorios** | 2-3 semanas | Alta | ?? Crítica |
| **Fase 3: Integración Auth** | 2 semanas | Media | ?? Alta |
| **Fase 4: NavMenu Dinámico** | 2 semanas | Media | ?? Alta |
| **Fase 5: Permisos Granulares UI** | 2-3 semanas | Alta | ?? Alta |
| **Fase 6: Panel Admin** | 3-4 semanas | Alta | ?? Media |
| **Fase 7: Optimización** | 1-2 semanas | Media | ?? Media |
| **Fase 8: Documentación** | 1 semana | Baja | ?? Media |
| **Fase 9: Deploy Producción** | 1 semana | Alta | ?? Crítica |

**Total Estimado:** 5-6 meses (con equipo de 2-3 desarrolladores)

---

### FASE 0: Análisis y Diseño (1-2 semanas)

#### Objetivo
Definir la estructura completa antes de escribir código.

#### Actividades

1. **Análisis de Módulos Actuales**
   - Inventariar todos los módulos existentes (Finanzas, Prospectos, etc.)
   - Identificar submódulos naturales (Facturas dentro de Finanzas, Pagos, etc.)
   - Documentar jerarquías de módulos

2. **Mapeo de Acciones Granulares**
   - Por cada módulo/submódulo, listar **todas** las acciones posibles:
   - Acciones CRUD estándar: Ver, Crear, Editar, Eliminar
     - Acciones especiales: Timbrar, Aprobar, Rechazar, Exportar, Enviar
   - Ejemplo para **Finanzas.Facturas**:
     - ? Ver listado de facturas
     - ? Crear nueva factura
     - ? Editar factura (solo si no está timbrada)
   - ? Eliminar factura (solo si no está timbrada)
     - ? Timbrar en SAT (acción crítica - solo gerentes)
     - ? Cancelar factura timbrada (acción crítica)
     - ? Descargar PDF
  - ? Descargar XML
     - ? Enviar por correo
 - ? Ver historial de timbrados

3. **Definición de Roles Empresariales**
   - Identificar roles reales del negocio
   - Definir jerarquías de roles (¿un Gerente hereda permisos de Coordinador?)
   - Documentar matriz de permisos

**Matriz de Permisos de Ejemplo:**

| Acción | Admin | Gerente Finanzas | Coordinador Finanzas | Contador |
|--------|-------|------------------|---------------------|----------|
| Ver Facturas | ? | ? | ? | ? |
| Crear Factura | ? | ? | ? | ? |
| Editar Factura | ? | ? | ? | ? |
| Eliminar Factura | ? | ? | ? | ? |
| Timbrar SAT | ? | ? | ? | ? |
| Cancelar Timbrada | ? | ? | ? | ? |

4. **Diseño de Base de Datos**
   - Crear diagrama entidad-relación (ERD)
   - Definir índices para optimización
   - Planificar estrategia de auditoría
   - Considerar versionado de permisos

5. **Definición de Multitenancy**
   - ¿Cómo se identifican los clientes? (ClienteId, TenantId, etc.)
   - ¿Base de datos compartida o separada por cliente?
   - ¿Roles compartidos entre clientes o personalizados?

#### Entregables
- ? Documento de análisis de módulos
- ? Matriz completa de permisos
- ? Diagrama de base de datos (ERD)
- ? Especificación de casos de uso
- ? Mockups de UI para gestión de permisos (admin)

---

### FASE 1: Infraestructura de Base de Datos (1-2 semanas)

#### Objetivo
Crear la base de datos y estructura de tablas.

#### Actividades

1. **Crear Proyecto de Infraestructura**
   - Nuevo proyecto `VRM_Plugin.Infrastructure` (.NET 8 Class Library)
   - Instalar paquetes:
     - `Microsoft.EntityFrameworkCore`
     - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.EntityFrameworkCore.Tools`

2. **Crear Entidades de Dominio en `Core.Domain`**
   - `Modulo.cs` - Representa módulos y submódulos
   - `AccionModulo.cs` - Acciones dentro de módulos
   - `Rol.cs` - Roles del sistema
 - `PermisoAccion.cs` - Relación Rol-Acción
   - `Usuario.cs` - Entidad de usuario
   - `UsuarioRol.cs` - Relación Usuario-Rol
- `ModuloCliente.cs` - Módulos habilitados por cliente
   - `AuditoriaPermiso.cs` - Log de cambios de permisos

3. **Crear DbContext**
   - `ApplicationDbContext.cs` con todas las entidades
   - Configurar relaciones (One-to-Many, Many-to-Many)
   - Configurar índices para performance
   - Configurar filtros globales para multitenancy

4. **Crear Migraciones**
   - Primera migración con todas las tablas
   - Scripts de seed data inicial (roles base, módulos existentes)

5. **Crear Stored Procedures Críticos**
   - `sp_ObtenerModulosUsuario` - Módulos permitidos para un usuario
   - `sp_VerificarPermisoAccion` - Validación rápida de permiso
   - `sp_ObtenerAccionesPermitidas` - Lista de acciones para un módulo
   - `sp_AuditarAccesoModulo` - Registrar intentos de acceso

6. **Configurar Connection String Seguro**
   - Usar Azure Key Vault o User Secrets en desarrollo
   - Nunca commitear connection strings reales

#### Entregables
- ? Proyecto `VRM_Plugin.Infrastructure` funcional
- ? Base de datos creada con todas las tablas
- ? Seed data inicial (roles, módulos actuales)
- ? Documentación de esquema de BD
- ? Scripts de respaldo y restauración

---

### FASE 2: Capa de Servicios y Repositorios (2-3 semanas)

#### Objetivo
Crear servicios que abstraigan la lógica de permisos.

#### Actividades

1. **Crear Proyecto de Aplicación**
   - Nuevo proyecto `VRM_Plugin.Core.Application` (.NET 8 Class Library)
   - Definir interfaces de servicios

2. **Implementar Servicio de Permisos (`IPermisosService`)**
   - **Métodos principales:**
     - `ObtenerModulosHabilitadosAsync(string clienteId)`
     - `UsuarioPuedeEjecutarAccionAsync(string usuarioId, string codigoAccion, string clienteId)`
     - `ObtenerAccionesPermitidasAsync(string usuarioId, string codigoModulo, string clienteId)`
     - `ObtenerRolesUsuarioAsync(string usuarioId, string clienteId)`

3. **Implementar Servicio de Gestión de Módulos (`IModulosService`)**
   - `RegistrarModuloAsync(ModuloDto)`
   - `ActualizarModuloAsync(ModuloDto)`
   - `DeshabilitarModuloAsync(int moduloId)`
   - `ObtenerJerarquiaModulosAsync()`

4. **Implementar Servicio de Gestión de Roles (`IRolesService`)**
 - `CrearRolAsync(RolDto)`
   - `AsignarPermisosARolAsync(int rolId, List<int> accionIds)`
   - `ClonarRolAsync(int rolId, string nuevoNombre)`
   - `ObtenerMatrizPermisosAsync()`

5. **Implementar Repositorios**
   - `IModuloRepository`
   - `IPermisoRepository`
   - `IUsuarioRepository`
 - Usar **patrón Repository + Unit of Work**

6. **Caché de Permisos**
   - Implementar `IMemoryCache` para permisos
   - Invalidar caché al modificar permisos
   - TTL configurable (ej: 30 minutos)

7. **Logs y Auditoría**
   - Registrar todos los cambios de permisos
   - Log de intentos de acceso denegado
   - Dashboard de auditoría

#### Entregables
- ? Proyecto `VRM_Plugin.Core.Application` con interfaces
- ? Proyecto `VRM_Plugin.Infrastructure` con implementaciones
- ? Unit tests para servicios críticos
- ? Documentación de APIs de servicios
- ? Estrategia de caché documentada

---

### FASE 3: Integración con Sistema de Autenticación (2 semanas)

#### Objetivo
Conectar el sistema de permisos con la autenticación actual.

#### Actividades

1. **Migrar de `DummyAuthenticationStateProvider` a Sistema Real**
   - **Opción A:** ASP.NET Core Identity
   - **Opción B:** Autenticación Personalizada + BD

2. **Modificar `GetAuthenticationStateAsync()`**
   - Consultar roles desde BD
   - Crear claims dinámicos basados en roles de BD

3. **Crear Middleware de Multitenancy**
   - Detectar cliente actual
 - Inyectar `ClienteId` en `HttpContext.Items`

4. **Actualizar `LoginAsync()`**
   - Validar credenciales contra BD
   - Cargar roles dinámicamente
   - Crear cookie con claims enriquecidos

5. **Implementar Política de Autorización Dinámica**
   - Crear `AuthorizationHandler` personalizado
   - Registrar políticas en `Program.cs`

6. **Testing de Seguridad**
   - Probar intentos de acceso sin permisos
   - Verificar aislamiento entre clientes
   - Validar expiración de roles

#### Entregables
- ? Sistema de autenticación integrado con BD
- ? Middleware de multitenancy funcional
- ? Authorization Handlers personalizados
- ? Tests de seguridad pasados
- ? Documentación de flujo de autenticación

---

### FASE 4: Actualización de UI - NavMenu Dinámico (2 semanas)

#### Objetivo
Menú lateral generado desde BD con permisos.

#### Actividades

1. **Crear Servicio de UI (`INavMenuService`)**
   - `ObtenerMenuUsuarioAsync(string userId, string clienteId)`

2. **Actualizar `NavMenu.razor`**
   - Eliminar enlaces hardcodeados
   - Consumir `INavMenuService`
   - Renderizar menú recursivo

3. **Componente Recursivo de Menú**
   - Crear `MenuItemComponent.razor`
   - Soporte para múltiples niveles

4. **Iconos Dinámicos**
   - Almacenar clase CSS en BD
   - Renderizar dinámicamente

5. **Caché de Menú**
   - Cachear estructura por usuario
   - Invalidar al cambiar permisos

#### Entregables
- ? `NavMenu.razor` completamente dinámico
- ? Soporte para 3+ niveles de jerarquía
- ? Componente reutilizable de menú
- ? Performance optimizada con caché
- ? UI/UX fluida y responsiva

---

### FASE 5: Permisos Granulares en Componentes (2-3 semanas)

#### Objetivo
Controlar botones/acciones dentro de componentes.

#### Actividades

1. **Crear Componente `<AuthorizeAction>`**
   - Evaluar permisos en runtime
   - Mostrar/ocultar elementos según permiso

2. **Actualizar Componentes de Módulos**
   - Envolver botones críticos con `<AuthorizeAction>`
   - Reemplazar lógica hardcodeada

3. **Validación en Backend**
   - Validar permiso en cada método de servicio
   - Nunca confiar solo en UI

4. **Mensajes de Error Personalizados**
   - Indicar qué permiso falta
   - Sugerir a quién contactar

5. **Testing de Permisos**
   - Tests para cada acción crítica
   - Verificar usuarios sin permiso reciban error

#### Entregables
- ? Componente `<AuthorizeAction>` reutilizable
- ? Todos los módulos actualizados
- ? Validación backend en todos los servicios
- ? Suite de tests de permisos
- ? Documentación de acciones por módulo

---

### FASE 6: Panel de Administración de Permisos (3-4 semanas)

#### Objetivo
UI para que admins gestionen roles y permisos.

#### Actividades

1. **Crear Módulo de Administración**
   - Nueva carpeta `src/Modules/Admin/VRM_Plugin.Modules.Admin`

2. **Pantalla: Gestión de Roles**
   - CRUD de roles
- Clonar rol existente

3. **Pantalla: Matriz de Permisos**
   - Tabla interactiva Rol vs Acciones
   - Guardado masivo

4. **Pantalla: Gestión de Usuarios**
   - Asignar/Remover roles
   - Ver historial de cambios

5. **Pantalla: Gestión de Módulos**
   - Habilitar/Deshabilitar módulos por cliente
   - Configurar fechas de expiración

6. **Pantalla: Auditoría**
   - Logs de cambios de permisos
   - Logs de intentos de acceso denegado

7. **Notificaciones**
   - Notificar a usuarios cuando se modifican permisos

#### Entregables
- ? Módulo de Admin funcional
- ? Pantallas CRUD de Roles, Usuarios, Módulos
- ? Matriz de permisos interactiva
- ? Sistema de auditoría
- ? Documentación de uso para administradores

---

### FASE 7: Optimización y Performance (1-2 semanas)

#### Objetivo
Asegurar escalabilidad del sistema.

#### Actividades

1. **Optimización de Consultas**
   - Analizar queries lentas
   - Crear índices faltantes

2. **Implementar Caché Distribuido**
   - Migrar a Redis
   - Cachear permisos por usuario

3. **Lazy Loading de Módulos**
   - Cargar módulos bajo demanda

4. **Paginación y Virtualización**
   - En tablas grandes

5. **Compresión de Respuestas**
   - Habilitar Gzip

6. **Load Testing**
   - Simular 100+ usuarios concurrentes

#### Entregables
- ? Benchmarks de performance
- ? Caché distribuido implementado
- ? Queries optimizadas (<100ms)
- ? Informe de load testing

---

### FASE 8: Documentación y Capacitación (1 semana)

#### Objetivo
Documentar todo y capacitar al equipo.

#### Actividades

1. **Documentación Técnica**
   - Diagrama de arquitectura
   - Documentación de APIs (Swagger)
   - Guía de desarrollo de módulos

2. **Documentación de Usuario**
   - Manual de administrador
 - Manual de usuario final

3. **Videos Tutoriales**
   - Cómo crear un nuevo rol
   - Cómo asignar permisos

4. **Capacitación al Equipo**
   - Sesión con desarrolladores
   - Sesión con QA
   - Sesión con soporte

#### Entregables
- ? Documentación completa
- ? Videos tutoriales
- ? Equipo capacitado

---

### FASE 9: Migración a Producción (1 semana)

#### Objetivo
Desplegar en producción de forma segura.

#### Actividades

1. **Preparación de Scripts de Migración**
   - Migrar usuarios actuales
   - Mapear roles existentes

2. **Deploy en Ambiente de Staging**
   - Probar con datos reales anonimizados

3. **Plan de Rollback**
   - Scripts para revertir cambios
   - Backup de DLLs anteriores

4. **Deploy en Producción**
   - Ventana de mantenimiento
   - Monitoreo en tiempo real

5. **Post-Deploy**
   - Verificar logs de errores
   - Validar accesos

#### Entregables
- ? Sistema en producción
- ? Cero downtime
- ? Post-mortem de migración

---

## ?? Modelo de Datos

### Diagrama Entidad-Relación (Simplificado)

```
???????????????     ????????????????????       ???????????????
?   Modulo    ??????????  AccionModulo    ???????????PermisoAccion?
???????????????    ????????????????????         ???????????????
? ModuloId PK ?     ? AccionId PK      ?         ? PermisoId PK?
? Codigo      ?         ? ModuloId FK  ?         ? AccionId FK ?
? Nombre      ?         ? Codigo ?         ? RolId FK    ?
? Icono       ?    ? Nombre     ?         ? Restricc.   ?
? ModuloPadre ?         ? Tipo (enum)      ?         ???????????????
???????????????         ? Activa           ?      ?
       ?      ????????????????????     ?
   ?  ?
       ?   ????????????????????     ?
       ??????????????????ModuloCliente     ?  ?
            ????????????????????                ?
              ? ModuloClienteId  ?  ?
         ? ModuloId FK      ?    ?
       ? ClienteId        ?  ???????????????
     ? Habilitado  ?         ?     Rol     ?
 ? FechaActivacion  ?   ???????????????
      ????????????????????         ? RolId PK    ?
                  ? Codigo    ?
     ? Nombre      ?
??????????????? ????????????????????         ? Activo      ?
?   Usuario   ??????????  UsuarioRol      ?????????????????????????
???????????????         ????????????????????
? UsuarioId PK?       ? UsuarioRolId PK  ?
? Email       ?         ? UsuarioId FK     ?
? NombreFull  ?         ? RolId FK  ?
? ClienteId   ?   ? ClienteId        ?
? Activo      ?    ? FechaAsignacion  ?
???????????????         ????????????????????
```

### Tablas Principales

#### 1. **Modulos**
Almacena módulos y submódulos del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ModuloId` | INT PK | Identificador único |
| `Codigo` | NVARCHAR(50) UNIQUE | Código del módulo (ej: "Finanzas") |
| `Nombre` | NVARCHAR(100) | Nombre amigable |
| `Descripcion` | NVARCHAR(500) | Descripción del módulo |
| `Icono` | NVARCHAR(100) | Clase CSS del icono (ej: "bi-currency-dollar") |
| `Orden` | INT | Orden de aparición en menú |
| `Activo` | BIT | Si está activo o deshabilitado |
| `RutaDll` | NVARCHAR(500) | Ruta al DLL (ej: "Modules/VRM_Plugin.Finanzas.dll") |
| `ModuloPadreId` | INT FK NULL | Referencia a módulo padre (para submódulos) |

**Relaciones:**
- Autorreferencial: `ModuloPadreId` ? `ModuloId`
- One-to-Many con `AccionesModulo`
- One-to-Many con `ModulosClientes`

---

#### 2. **AccionesModulo**
Define acciones específicas dentro de cada módulo.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `AccionId` | INT PK | Identificador único |
| `ModuloId` | INT FK | Módulo al que pertenece |
| `Codigo` | NVARCHAR(100) UNIQUE | Código de acción (ej: "Finanzas.Facturas.TimbrarSAT") |
| `Nombre` | NVARCHAR(100) | Nombre amigable |
| `Descripcion` | NVARCHAR(500) | Descripción de la acción |
| `Tipo` | INT | Enum: Ver=1, Crear=2, Editar=3, Eliminar=4, Custom=99 |
| `RequiereConfirmacion` | BIT | Si requiere confirmación antes de ejecutar |
| `Activa` | BIT | Si está disponible |

**Ejemplo de datos:**
```sql
INSERT INTO AccionesModulo (ModuloId, Codigo, Nombre, Tipo) VALUES
(1, 'Finanzas.Facturas.Ver', 'Ver Facturas', 1),
(1, 'Finanzas.Facturas.TimbrarSAT', 'Timbrar en SAT', 7);
```

---

#### 3. **Roles**
Define roles del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `RolId` | INT PK | Identificador único |
| `Codigo` | NVARCHAR(50) UNIQUE | Código del rol (ej: "GerenteFinanzas") |
| `Nombre` | NVARCHAR(100) | Nombre descriptivo |
| `Descripcion` | NVARCHAR(500) | Descripción del rol |
| `Activo` | BIT | Si está activo |

---

#### 4. **PermisosAccion**
Relaciona roles con acciones (matriz de permisos).

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PermisoAccionId` | INT PK | Identificador único |
| `AccionId` | INT FK | Acción permitida |
| `RolId` | INT FK | Rol que tiene el permiso |
| `Restricciones` | NVARCHAR(MAX) NULL | JSON con restricciones adicionales |
| `FechaCreacion` | DATETIME2 | Cuándo se otorgó el permiso |
| `CreadoPor` | NVARCHAR(100) | Quién otorgó el permiso |

**Constraint:** `UNIQUE (AccionId, RolId)`

---

#### 5. **Usuarios**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `UsuarioId` | NVARCHAR(50) PK | Identificador único |
| `Email` | NVARCHAR(255) UNIQUE | Email del usuario |
| `PasswordHash` | NVARCHAR(255) | Hash de contraseña (BCrypt) |
| `NombreCompleto` | NVARCHAR(200) | Nombre completo |
| `ClienteId` | NVARCHAR(50) | Cliente al que pertenece |
| `Activo` | BIT | Si está activo |
| `FechaCreacion` | DATETIME2 | Fecha de registro |

---

#### 6. **UsuariosRoles**
Asigna roles a usuarios (Many-to-Many).

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `UsuarioRolId` | INT PK | Identificador único |
| `UsuarioId` | NVARCHAR(50) FK | Usuario |
| `RolId` | INT FK | Rol asignado |
| `ClienteId` | NVARCHAR(50) | Cliente |
| `FechaAsignacion` | DATETIME2 | Cuándo se asignó |
| `FechaExpiracion` | DATETIME2 NULL | Cuándo expira |

---

#### 7. **ModulosClientes**
Define qué módulos están habilitados para cada cliente.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ModuloClienteId` | INT PK | Identificador único |
| `ModuloId` | INT FK | Módulo |
| `ClienteId` | NVARCHAR(50) | Cliente |
| `Habilitado` | BIT | Si está habilitado |
| `FechaActivacion` | DATETIME2 | Cuándo se habilitó |
| `FechaExpiracion` | DATETIME2 NULL | Cuándo expira |
| `ConfiguracionJson` | NVARCHAR(MAX) NULL | Configuraciones específicas |

---

#### 8. **AuditoriaPermisos**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `AuditoriaId` | BIGINT PK | Identificador único |
| `UsuarioId` | NVARCHAR(50) | Usuario que realizó la acción |
| `AccionId` | INT FK NULL | Acción ejecutada |
| `ModuloId` | INT FK NULL | Módulo accedido |
| `TipoEvento` | NVARCHAR(50) | "AccesoDenegado", "PermisoModificado" |
| `Detalles` | NVARCHAR(MAX) | JSON con detalles |
| `Fecha` | DATETIME2 | Timestamp |
| `DireccionIP` | NVARCHAR(50) | IP del usuario |

---

## ??? Guías de Implementación

### Nuevos Proyectos a Crear

```
src/
??? Core/
?   ??? VRM_Plugin.Core.Application/  (NUEVO)
?       ??? DTOs/
?       ?   ??? ModuloDto.cs
?       ?   ??? AccionDto.cs
?       ?   ??? RolDto.cs
?       ??? Services/
?       ?   ??? IPermisosService.cs
?   ?   ??? IRolesService.cs
?       ?   ??? IModulosService.cs
?       ?   ??? INavMenuService.cs
?       ??? VRM_Plugin.Core.Application.csproj
?
??? Infrastructure/
    ??? VRM_Plugin.Infrastructure/    (NUEVO)
        ??? Data/
        ?   ??? ApplicationDbContext.cs
        ?   ??? Configurations/
  ?   ?   ??? ModuloConfiguration.cs
        ?   ?   ??? PermisoConfiguration.cs
        ?   ??? Migrations/
??? Repositories/
        ?   ??? IModuloRepository.cs
        ?   ??? ModuloRepository.cs
        ?   ??? PermisoRepository.cs
        ??? Services/
      ?   ??? PermisosService.cs
    ?   ??? RolesService.cs
      ?   ??? NavMenuService.cs
     ??? VRM_Plugin.Infrastructure.csproj
```

---

### Script SQL de Creación de Tablas

```sql
-- =============================================
-- MÓDULOS Y PERMISOS GRANULARES
-- =============================================

-- Tabla: Roles
CREATE TABLE Roles (
    RolId INT PRIMARY KEY IDENTITY(1,1),
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT CHK_Rol_Codigo CHECK (Codigo NOT LIKE '%[^A-Za-z0-9]%')
);

-- Tabla: Módulos
CREATE TABLE Modulos (
    ModuloId INT PRIMARY KEY IDENTITY(1,1),
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    Icono NVARCHAR(100),
    Orden INT NOT NULL DEFAULT 0,
    Activo BIT NOT NULL DEFAULT 1,
    RutaDll NVARCHAR(500),
    ModuloPadreId INT NULL,
    CONSTRAINT FK_Modulo_Padre FOREIGN KEY (ModuloPadreId) 
        REFERENCES Modulos(ModuloId)
);

-- Tabla: Acciones de Módulos
CREATE TABLE AccionesModulo (
    AccionId INT PRIMARY KEY IDENTITY(1,1),
    ModuloId INT NOT NULL,
    Codigo NVARCHAR(100) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    Tipo INT NOT NULL,
    RequiereConfirmacion BIT NOT NULL DEFAULT 0,
    Activa BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Accion_Modulo FOREIGN KEY (ModuloId) 
        REFERENCES Modulos(ModuloId) ON DELETE CASCADE
);

-- Tabla: Permisos (Relación Rol-Acción)
CREATE TABLE PermisosAccion (
    PermisoAccionId INT PRIMARY KEY IDENTITY(1,1),
    AccionId INT NOT NULL,
    RolId INT NOT NULL,
    Restricciones NVARCHAR(MAX),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreadoPor NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_Permiso_Accion FOREIGN KEY (AccionId) 
        REFERENCES AccionesModulo(AccionId) ON DELETE CASCADE,
    CONSTRAINT FK_Permiso_Rol FOREIGN KEY (RolId) 
        REFERENCES Roles(RolId) ON DELETE CASCADE,
    CONSTRAINT UQ_Permiso_Accion_Rol UNIQUE (AccionId, RolId)
);

-- Tabla: Módulos por Cliente
CREATE TABLE ModulosClientes (
    ModuloClienteId INT PRIMARY KEY IDENTITY(1,1),
    ModuloId INT NOT NULL,
    ClienteId NVARCHAR(50) NOT NULL,
    Habilitado BIT NOT NULL DEFAULT 1,
    FechaActivacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaExpiracion DATETIME2 NULL,
    ConfiguracionJson NVARCHAR(MAX),
    CONSTRAINT FK_ModuloCliente_Modulo FOREIGN KEY (ModuloId) 
     REFERENCES Modulos(ModuloId) ON DELETE CASCADE,
    CONSTRAINT UQ_ModuloCliente UNIQUE (ModuloId, ClienteId)
);

-- Tabla: Usuarios-Roles
CREATE TABLE UsuariosRoles (
    UsuarioRolId INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId NVARCHAR(50) NOT NULL,
    RolId INT NOT NULL,
    ClienteId NVARCHAR(50) NOT NULL,
    FechaAsignacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaExpiracion DATETIME2 NULL,
    CONSTRAINT FK_UsuarioRol_Rol FOREIGN KEY (RolId) 
        REFERENCES Roles(RolId) ON DELETE CASCADE,
    CONSTRAINT UQ_Usuario_Rol_Cliente UNIQUE (UsuarioId, RolId, ClienteId)
);

-- Índices
CREATE INDEX IX_Modulos_ModuloPadre ON Modulos(ModuloPadreId);
CREATE INDEX IX_AccionesModulo_Modulo ON AccionesModulo(ModuloId);
CREATE INDEX IX_PermisosAccion_Accion ON PermisosAccion(AccionId);
CREATE INDEX IX_PermisosAccion_Rol ON PermisosAccion(RolId);
CREATE INDEX IX_ModulosClientes_Cliente ON ModulosClientes(ClienteId);
CREATE INDEX IX_UsuariosRoles_Usuario ON UsuariosRoles(UsuarioId);
CREATE INDEX IX_UsuariosRoles_Cliente ON UsuariosRoles(ClienteId);
```

---

### Script SQL de Seed Data

```sql
-- =============================================
-- SEED DATA: Roles, Módulos y Permisos
-- =============================================

-- 1. ROLES
INSERT INTO Roles (Codigo, Nombre, Descripcion) VALUES
('Admin', 'Administrador', 'Acceso total al sistema'),
('GerenteFinanzas', 'Gerente de Finanzas', 'Gestión completa de finanzas'),
('CoordinadorFinanzas', 'Coordinador de Finanzas', 'Operaciones diarias de finanzas'),
('Contador', 'Contador', 'Consulta de información financiera'),
('GestorProspectos', 'Gestor de Prospectos', 'Gestión de nuevos prospectos'),
('RevisorLegal', 'Revisor Legal', 'Revisión legal de documentos'),
('RevisorFinanzas', 'Revisor Financiero', 'Revisión financiera de prospectos');

-- 2. MÓDULOS PRINCIPALES
INSERT INTO Modulos (Codigo, Nombre, Descripcion, Icono, Orden, RutaDll, ModuloPadreId) VALUES
('Finanzas', 'Gestión de Finanzas', 'Módulo financiero completo', 'bi-currency-dollar', 10, 'Modules/VRM_Plugin.Finanzas.dll', NULL),
('Prospectos', 'Gestión de Prospectos', 'Onboarding de clientes', 'bi-people-fill', 20, 'Modules/VRM_Plugin.Prospectos.dll', NULL);

-- 3. SUBMÓDULOS
DECLARE @FinanzasId INT = (SELECT ModuloId FROM Modulos WHERE Codigo = 'Finanzas');

INSERT INTO Modulos (Codigo, Nombre, Descripcion, Icono, Orden, ModuloPadreId) VALUES
('Finanzas.Facturas', 'Facturas', 'Gestión de facturas electrónicas', 'bi-receipt', 1, @FinanzasId),
('Finanzas.Pagos', 'Pagos', 'Gestión de pagos', 'bi-credit-card', 2, @FinanzasId),
('Finanzas.Reportes', 'Reportes', 'Reportes financieros', 'bi-bar-chart', 3, @FinanzasId);

-- 4. ACCIONES
DECLARE @FacturasId INT = (SELECT ModuloId FROM Modulos WHERE Codigo = 'Finanzas.Facturas');

INSERT INTO AccionesModulo (ModuloId, Codigo, Nombre, Descripcion, Tipo) VALUES
(@FacturasId, 'Finanzas.Facturas.Ver', 'Ver Facturas', 'Consultar listado de facturas', 1),
(@FacturasId, 'Finanzas.Facturas.Crear', 'Crear Factura', 'Generar nueva factura', 2),
(@FacturasId, 'Finanzas.Facturas.Editar', 'Editar Factura', 'Modificar factura existente', 3),
(@FacturasId, 'Finanzas.Facturas.Eliminar', 'Eliminar Factura', 'Eliminar factura', 4),
(@FacturasId, 'Finanzas.Facturas.TimbrarSAT', 'Timbrar en SAT', 'Timbrado fiscal de factura', 7);

-- 5. PERMISOS
DECLARE @AdminId INT = (SELECT RolId FROM Roles WHERE Codigo = 'Admin');
DECLARE @GerenteFinId INT = (SELECT RolId FROM Roles WHERE Codigo = 'GerenteFinanzas');
DECLARE @CoordFinId INT = (SELECT RolId FROM Roles WHERE Codigo = 'CoordinadorFinanzas');

-- Admin: Todos los permisos
INSERT INTO PermisosAccion (AccionId, RolId, CreadoPor) 
SELECT AccionId, @AdminId, 'SYSTEM' FROM AccionesModulo WHERE ModuloId = @FacturasId;

-- Gerente: Todos menos eliminar
INSERT INTO PermisosAccion (AccionId, RolId, CreadoPor) 
SELECT AccionId, @GerenteFinId, 'SYSTEM' FROM AccionesModulo 
WHERE ModuloId = @FacturasId AND Codigo != 'Finanzas.Facturas.Eliminar';

-- Coordinador: Ver, Crear, Editar (NO Eliminar, NO Timbrar)
INSERT INTO PermisosAccion (AccionId, RolId, CreadoPor) 
SELECT AccionId, @CoordFinId, 'SYSTEM' FROM AccionesModulo 
WHERE ModuloId = @FacturasId AND Codigo IN (
    'Finanzas.Facturas.Ver',
    'Finanzas.Facturas.Crear',
    'Finanzas.Facturas.Editar'
);

-- 6. HABILITAR MÓDULOS PARA CLIENTE
INSERT INTO ModulosClientes (ModuloId, ClienteId, Habilitado) VALUES
(@FinanzasId, 'cliente-001', 1);
```

---

## ?? Consideraciones de Diseño

### Principios de UX Empresarial

#### 1. **Claridad ante todo**

**MAL:**
```razor
<button disabled>Timbrar SAT</button>
```

**BIEN:**
```razor
<AuthorizeAction Action="Finanzas.Facturas.TimbrarSAT">
    <Authorized>
   <MudButton Color="Color.Success">Timbrar SAT</MudButton>
    </Authorized>
    <NotAuthorized>
        <MudTooltip Text="No tienes permiso. Contacta a tu gerente.">
            <MudButton Color="Color.Default" Disabled="true">
         Timbrar SAT
    </MudButton>
        </MudTooltip>
    </NotAuthorized>
</AuthorizeAction>
```

---

#### 2. **Jerarquía visual de módulos**

```
?? Inicio
?? Finanzas ?
   ?? ?? Facturas
   ?? ?? Pagos
   ?? ?? Reportes
?? Prospectos ?
   ?? ?? Listado
   ?? ? Aprobaciones
?? Configuración (solo admin)
```

---

#### 3. **Estados de permisos visuales**

| Estado | Color | Icono | Ejemplo |
|--------|-------|-------|---------|
| **Permitido** | Verde | ? | Botón habilitado |
| **Denegado** | Gris | ?? | Botón disabled + tooltip |
| **Requiere aprobación** | Amarillo | ?? | Modal de confirmación |
| **Crítico** | Rojo | ? | Confirmación doble |

---

### Sistema de Diseño Recomendado

**MudBlazor** (Recomendado)
```sh
dotnet add package MudBlazor
```

Ventajas:
- ? Componentes empresariales listos
- ? Temas personalizables
- ? Soporte dark mode
- ? Buena documentación

---

## ?? Plan de Migración

### Estrategia de Rollback

**Antes del deploy:**
1. ? Backup completo de BD
2. ? Backup de binarios actuales
3. ? Documentar configuración actual

**Si algo falla:**
1. Restaurar BD desde backup
2. Revertir DLLs a versión anterior
3. Revertir código a commit anterior
4. Reiniciar aplicación

---

## ?? Referencias y Recursos

### Documentación Oficial

- **Authorization en ASP.NET Core:**  
https://learn.microsoft.com/en-us/aspnet/core/security/authorization/

- **Entity Framework Core:**  
  https://learn.microsoft.com/en-us/ef/core/

- **Blazor Server:**  
  https://learn.microsoft.com/en-us/aspnet/core/blazor/

### Librerías Recomendadas

- **MudBlazor:** https://mudblazor.com/
- **BCrypt.Net:** https://github.com/BcryptNet/bcrypt.net
- **Serilog:** https://serilog.net/

---

## ?? Consideraciones de Seguridad

### Checklist

- [ ] Contraseñas hasheadas (BCrypt)
- [ ] HTTPS obligatorio
- [ ] Validación doble (UI + Backend)
- [ ] Auditoría de cambios críticos
- [ ] Rate limiting en login
- [ ] Validación de ClienteId (evitar leaks)
- [ ] Sanitización de inputs
- [ ] Content Security Policy

---

## ?? Contacto

**Repositorio:** https://github.com/mrguz170/VRM_Net  
**Branch:** dev-mainUI

---

## ?? Historial de Cambios

| Fecha | Versión | Cambios |
|-------|---------|---------|
| 2024-01-XX | 1.0 | Documento inicial |

---

## ? Próximos Pasos

1. Revisar documento con equipo
2. Aprobar diseño de BD
3. Crear branch `feature/permisos-granulares`
4. Iniciar Fase 1: Infraestructura
5. Definir connection string

---

**?? Objetivo:** Sistema de permisos granulares 100% dinámico, escalable y mantenible.

---

*Generado por GitHub Copilot - Enero 2024*
