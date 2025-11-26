# ?? Índice de Documentación - VRM Plugin System

> **Guía rápida para encontrar la documentación que necesitas**

---

## ?? Para Empezar

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| **[QUICKSTART_ACCIONES.md](./QUICKSTART_ACCIONES.md)** | ?? Guía rápida de 5 minutos para usar `<AuthorizeAction>` | Desarrolladores |
| **[SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md)** | ?? Guía completa del sistema de acciones desde BD | Desarrolladores/Arquitectos |
| **[DIAGRAMAS_SISTEMA_ACCIONES.md](./DIAGRAMAS_SISTEMA_ACCIONES.md)** | ?? Diagramas visuales del flujo completo | Todos |

---

## ??? Arquitectura y Diseño

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [GUIA_DISENO_ARQUITECTURA.md](./GUIA_DISENO_ARQUITECTURA.md) | Arquitectura modular del sistema | Arquitectos |
| [PERMISOS_GRANULARES.md](./PERMISOS_GRANULARES.md) | Sistema de permisos por acción | Desarrolladores |
| [SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md](./SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md) | Render modes SSR + Interactive | Desarrolladores Blazor |

---

## ?? Autenticación y Autorización

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| **[QUICKSTART_ACCIONES.md](./QUICKSTART_ACCIONES.md)** | ? Cómo usar `<AuthorizeAction>` | Desarrolladores |
| [GUIA_AUTENTICACION_SIMULADA.md](./GUIA_AUTENTICACION_SIMULADA.md) | Sistema de autenticación de desarrollo | Desarrolladores/QA |
| [AUTENTICACION_Y_COOKIES.md](./AUTENTICACION_Y_COOKIES.md) | Cookies persistentes en Blazor | Desarrolladores Blazor |
| **[SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md)** | ? Autorización basada en BD | Arquitectos/Desarrolladores |

---

## ?? Módulos

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [../src/Modules/README.md](../src/Modules/README.md) | Cómo crear módulos nuevos | Desarrolladores |
| [../src/Modules/Finanzas/VRM_PluginDemo.Modules.Finanzas/Data/README.md](../src/Modules/Finanzas/VRM_PluginDemo.Modules.Finanzas/Data/README.md) | Capa de datos de Finanzas | Desarrolladores |
| [../src/Modules/Onboarding/VRM_PluginDemo.Modules.Prospectos/Data/README.md](../src/Modules/Onboarding/VRM_PluginDemo.Modules.Prospectos/Data/README.md) | Capa de datos de Prospectos | Desarrolladores |

---

## ??? Scripts y Herramientas

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [COMANDOS_SCRIPTS.md](./COMANDOS_SCRIPTS.md) | Scripts PowerShell útiles | DevOps/Desarrolladores |
| [../scripts/New-VRMPlugin.ps1](../scripts/New-VRMPlugin.ps1) | Script para crear módulos | Desarrolladores |

---

## ?? Planificación

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [ROADMAP_EMPRESARIAL.md](./ROADMAP_EMPRESARIAL.md) | Roadmap del proyecto | Product Owners/Management |

---

## ?? Búsqueda Rápida por Tema

### ?? "Quiero usar permisos en mi componente"
1. ? **[QUICKSTART_ACCIONES.md](./QUICKSTART_ACCIONES.md)** ? Empieza aquí
2. [SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md) ? Detalles técnicos
3. [DIAGRAMAS_SISTEMA_ACCIONES.md](./DIAGRAMAS_SISTEMA_ACCIONES.md) ? Visualización

### ?? "Quiero crear un módulo nuevo"
1. [../src/Modules/README.md](../src/Modules/README.md) ? Guía paso a paso
2. [../scripts/New-VRMPlugin.ps1](../scripts/New-VRMPlugin.ps1) ? Automatización

### ?? "Quiero entender la autenticación"
1. [GUIA_AUTENTICACION_SIMULADA.md](./GUIA_AUTENTICACION_SIMULADA.md) ? Usuarios de prueba
2. [AUTENTICACION_Y_COOKIES.md](./AUTENTICACION_Y_COOKIES.md) ? Sesión persistente
3. [SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md](./SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md) ? Render modes

### ?? "Quiero entender la arquitectura"
1. [GUIA_DISENO_ARQUITECTURA.md](./GUIA_DISENO_ARQUITECTURA.md) ? Visión general
2. [DIAGRAMAS_SISTEMA_ACCIONES.md](./DIAGRAMAS_SISTEMA_ACCIONES.md) ? Flujos visuales
3. [SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md) ? Sistema de permisos

### ??? "Quiero trabajar con base de datos"
1. **[SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md)** ? Stored Procedures
2. [../src/Modules/Finanzas/.../Data/README.md](../src/Modules/Finanzas/VRM_PluginDemo.Modules.Finanzas/Data/README.md) ? Repositorios

### ?? "Tengo un problema"
1. **[QUICKSTART_ACCIONES.md](./QUICKSTART_ACCIONES.md)** ? Sección Troubleshooting
2. [SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md) ? Debugging detallado

---

## ?? Documentos Nuevos (Enero 2025)

- ? **[QUICKSTART_ACCIONES.md](./QUICKSTART_ACCIONES.md)** - Guía rápida para usar AuthorizeAction
- ? **[SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md)** - Documentación técnica completa
- ? **[DIAGRAMAS_SISTEMA_ACCIONES.md](./DIAGRAMAS_SISTEMA_ACCIONES.md)** - 11 diagramas visuales

---

## ?? Niveles de Experiencia

### ?? Nivel Principiante
- [QUICKSTART_ACCIONES.md](./QUICKSTART_ACCIONES.md)
- [GUIA_AUTENTICACION_SIMULADA.md](./GUIA_AUTENTICACION_SIMULADA.md)
- [../src/Modules/README.md](../src/Modules/README.md)

### ?? Nivel Intermedio
- [SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md)
- [PERMISOS_GRANULARES.md](./PERMISOS_GRANULARES.md)
- [SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md](./SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md)

### ?? Nivel Avanzado
- [GUIA_DISENO_ARQUITECTURA.md](./GUIA_DISENO_ARQUITECTURA.md)
- [DIAGRAMAS_SISTEMA_ACCIONES.md](./DIAGRAMAS_SISTEMA_ACCIONES.md)
- [ROADMAP_EMPRESARIAL.md](./ROADMAP_EMPRESARIAL.md)

---

## ?? Enlaces Rápidos

| Acción | Link |
|--------|------|
| ?? README Principal | [../README.md](../README.md) |
| ?? Código de Host | [../src/Host/VRM_PluginDemo.Blazor.Server/](../src/Host/VRM_PluginDemo.Blazor.Server/) |
| ?? Código de Módulos | [../src/Modules/](../src/Modules/) |
| ?? Scripts | [../scripts/](../scripts/) |
| ?? Core/Abstractions | [../src/Core/VRM_Plugin.Core.Abstractions/](../src/Core/VRM_Plugin.Core.Abstractions/) |

---

## ?? Tips para Navegar

1. **Usa Ctrl+F** en GitHub para buscar dentro de un documento
2. **Los emojis** ayudan a identificar secciones rápidamente:
   - ? = Completo/Recomendado
   - ?? = Importante/Advertencia
   - ?? = Tip/Consejo
   - ?? = Técnico
   - ?? = Listado/Tabla
   - ?? = Objetivo/Meta

3. **Orden de lectura recomendado para nuevos desarrolladores:**
   ```
   QUICKSTART_ACCIONES.md
   ?
   SISTEMA_DE_ACCIONES_DINAMICAS.md
   ?
   src/Modules/README.md
   ?
   GUIA_DISENO_ARQUITECTURA.md
   ```

---

**?? Última actualización:** Enero 2025  
**?? Versión:** 2.0 (con sistema de acciones dinámicas)

