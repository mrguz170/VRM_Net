# ?? Documentación VRM System - Índice Principal

Bienvenido a la documentación completa del sistema VRM. Esta guía te ayudará a entender, usar y extender el proyecto.

---

## ? PARA PRESENTACIONES RÁPIDAS (NUEVO)

### **?? Documentos Autocontenidos - Sin Referencias Externas**

Si tienes **poco tiempo** para preparar una presentación, usa estos **2 archivos independientes**:

?? **[PRESENTACION_01_ARQUITECTURA.md](PRESENTACION_01_ARQUITECTURA.md)**
- ? Composición de la arquitectura (3 capas completas)
- ? Componentes principales con código completo
- ? Flujo de ejecución detallado
- ? Sistema de permisos de dos niveles
- ? Ventajas y comparaciones
- ? **TODO explicado en un solo archivo**
- **Tiempo de lectura**: 15 minutos

?? **[PRESENTACION_02_PLUGINS_DETALLE.md](PRESENTACION_02_PLUGINS_DETALLE.md)**
- ? Anatomía completa de un plugin
- ? ModuleLoader paso a paso
- ? Creación manual vs automática
- ? Generador automático (uso completo)
- ? Mejores prácticas
- ? **TODO explicado en un solo archivo**
- **Tiempo de lectura**: 20 minutos

?? **[INDICE_PRESENTACION.md](INDICE_PRESENTACION.md)**
- Guía completa de cómo usar los documentos de presentación
- Estrategias para diferentes escenarios (15min / 30min / 1hora)
- Estructura sugerida para PowerPoint
- Tips y checklist final

**Ventaja**: No necesitas saltar entre archivos, todo está explicado directamente.

---

## ?? Documentos Disponibles

### **1. Para Presentaciones y Decisores**

?? **[RESUMEN_EJECUTIVO.md](RESUMEN_EJECUTIVO.md)**
- Visión general del proyecto
- Arquitectura en alto nivel
- Ventajas del sistema
- Roadmap y tecnologías
- **Tiempo de lectura**: 10 minutos

---

### **2. Para Arquitectos y Desarrolladores Senior**

??? **[ARQUITECTURA_PLUGINS.md](ARQUITECTURA_PLUGINS.md)**
- Paradigma de arquitectura basada en plugins
- Estructura de carpetas detallada
- Componentes principales (`IModule`, `ModuleComponent`, `ModuleAction`)
- Flujo completo de carga de módulos
- Comparación con arquitecturas tradicionales
- **Tiempo de lectura**: 20 minutos

---

### **3. Para Desarrolladores Frontend**

?? **[ADAPTACION_SLICED_TEMPLATE.md](ADAPTACION_SLICED_TEMPLATE.md)**
- Qué es Sliced y por qué se eligió
- Cambios realizados a la plantilla
- Sistema de autenticación integrado
- Componentes de autorización (`AuthorizeModule`, `AuthorizeAction`)
- Cómo funciona el menú dinámico
- Estilos y assets conservados
- **Tiempo de lectura**: 15 minutos

---

### **4. Para Desarrolladores que Crearán Módulos**

?? **[GENERADOR_PLUGINS.md](GENERADOR_PLUGINS.md)**
- Guía completa del script `New-VRMPlugin.ps1`
- Ejemplos de uso paso a paso
- Estructura generada automáticamente
- Nomenclatura de IDs
- Tips y mejores prácticas
- **Tiempo de lectura**: 15 minutos

?? **[New-VRMPlugin.ps1](../New-VRMPlugin.ps1)** (Script ejecutable)
- Script PowerShell listo para usar
- Genera módulos completos en segundos
- Sin palabra "Demo" en ningún lugar

---

## ?? Rutas de Aprendizaje por Rol

### **????? Gerente de Proyecto / Product Owner**

1. Leer: [RESUMEN_EJECUTIVO.md](RESUMEN_EJECUTIVO.md)
2. Revisar: Roadmap y ventajas de negocio
3. **Tiempo total**: 15 minutos

---

### **????? Arquitecto de Software**

1. Leer: [ARQUITECTURA_PLUGINS.md](ARQUITECTURA_PLUGINS.md)
2. Explorar: Código de `ModuleLoader.cs`
3. Revisar: Interfaces en `VRM_Plugin.Core.Abstractions`
4. **Tiempo total**: 1 hora

---

### **?? Desarrollador Frontend (Blazor/UI)**

1. Leer: [ADAPTACION_SLICED_TEMPLATE.md](ADAPTACION_SLICED_TEMPLATE.md)
2. Explorar: Componentes en `src/Host/.../Components/`
3. Revisar: `Sidebar.razor` y `AuthorizeModule.razor`
4. **Tiempo total**: 45 minutos

---

### **?? Desarrollador Backend (Nuevos Módulos)**

1. Leer: [GENERADOR_PLUGINS.md](GENERADOR_PLUGINS.md)
2. Ejecutar: Script para generar módulo de prueba
3. Explorar: Código generado
4. Personalizar: Agregar lógica de negocio
5. **Tiempo total**: 1.5 horas

---

### **?? Desarrollador Nuevo en el Proyecto**

**Día 1: Entendimiento General**
1. ? [RESUMEN_EJECUTIVO.md](RESUMEN_EJECUTIVO.md) (10 min)
2. ? [ARQUITECTURA_PLUGINS.md](ARQUITECTURA_PLUGINS.md) (20 min)
3. ? Explorar módulo Finanzas existente (30 min)

**Día 2: Práctica**
4. ? [GENERADOR_PLUGINS.md](GENERADOR_PLUGINS.md) (15 min)
5. ? Generar módulo de prueba con script (10 min)
6. ? Personalizar módulo generado (1 hora)

**Día 3: Profundización**
7. ? [ADAPTACION_SLICED_TEMPLATE.md](ADAPTACION_SLICED_TEMPLATE.md) (15 min)
8. ? Crear componente UI personalizado (1 hora)
9. ? Implementar permisos granulares (30 min)

**Total**: ~4 horas de estudio + práctica

---

## ?? Estructura de la Documentación

```
docs/
??? README.md                           # Este archivo (índice)
??? RESUMEN_EJECUTIVO.md                # Visión general
??? ARQUITECTURA_PLUGINS.md             # Arquitectura técnica
??? ADAPTACION_SLICED_TEMPLATE.md       # Frontend y UI
??? GENERADOR_PLUGINS.md                # Script generador
?
??? OPTIMIZACION_SCRIPT_BUILD.md        # Builds optimizados
??? RECOMPILACION_MODULOS_BLAZOR.md     # Hot reload
??? SERILOG_LOGGING.md                  # Sistema de logs
??? PASO_1_COMPLETADO.md                # Milestone histórico
```

---

## ?? Quick Start

### **Quiero Ejecutar el Proyecto (5 minutos)**

```bash
# 1. Clonar repositorio
git clone https://github.com/mrguz170/VRM_Net
cd VRM_Net

# 2. Ejecutar
cd src/Host/VRM_Plugin.Blazor.Server
dotnet run

# 3. Navegar a http://localhost:5000
# Usuario: admin
# Password: admin123
```

---

### **Quiero Crear un Nuevo Módulo (2 minutos)**

```powershell
# 1. Ejecutar script desde raíz del proyecto
.\New-VRMPlugin.ps1 `
    -ModuleName "MiModulo" `
    -IdModule 10 `
    -Category "MiCategoria" `
    -StartIdComponent 1000 `
    -StartIdAction 1000 `
    -IconRoot "ri-star-line"

# 2. Reiniciar aplicación
cd src/Host/VRM_Plugin.Blazor.Server
dotnet run

# 3. Navegar a http://localhost:5000/mimodulo
```

---

### **Quiero Entender la Arquitectura (30 minutos)**

1. Leer [ARQUITECTURA_PLUGINS.md](ARQUITECTURA_PLUGINS.md)
2. Explorar `src/Core/VRM_Plugin.Core.Abstractions/IModule.cs`
3. Revisar `src/Host/.../Services/ModuleLoader.cs`
4. Examinar `src/Modules/Finanzas/FinanzasModule.cs`

---

## ?? Glosario de Términos

| Término | Descripción |
|---------|-------------|
| **Plugin** | Módulo independiente que extiende la funcionalidad del sistema |
| **IModule** | Interfaz base que deben implementar todos los plugins |
| **ModuleComponent** | Representa una página o sección UI en el menú |
| **ModuleAction** | Representa una operación específica (Ver, Crear, Eliminar, etc.) |
| **IdComponent** | ID numérico único de un componente |
| **IdAction** | ID numérico único de una acción |
| **ActionKey** | Clave única de una acción (ej: "Finanzas.Facturas.TimbrarSAT") |
| **Hot-Deployment** | Agregar módulos sin recompilar la aplicación completa |
| **Sliced** | Plantilla UI profesional basada en Tailwind CSS |
| **AuthorizeModule** | Componente Blazor que protege por IdComponent |
| **AuthorizeAction** | Componente Blazor que protege por ActionKey |

---

## ?? Enlaces Útiles

### **Código Fuente**
- [GitHub Repository](https://github.com/mrguz170/VRM_Net)
- [Módulo Finanzas](../src/Modules/Finanzas/VRM_Plugin.Modules.Finanzas/)
- [Módulo Prospectos](../src/Modules/Onboarding/VRM_Plugin.Modules.Prospectos/)
- [Host Application](../src/Host/VRM_Plugin.Blazor.Server/)

### **Tecnologías**
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Tailwind CSS](https://tailwindcss.com/)
- [Remix Icons](https://remixicon.com/)
- [Serilog](https://serilog.net/)

---

## ?? Métricas del Proyecto

| Métrica | Valor |
|---------|-------|
| **Líneas de Código** | ~15,000 |
| **Módulos Implementados** | 2 (Finanzas, Prospectos) |
| **Componentes UI** | 4 |
| **Acciones Definidas** | 36 |
| **Tiempo de Arranque** | < 3 segundos |
| **Tiempo para Nuevo Módulo** | < 30 segundos (con script) |

---

## ?? Próximos Pasos Recomendados

### **Para el Equipo**

1. ? Revisar esta documentación (todos)
2. ? Ejecutar proyecto localmente (todos)
3. ? Generar módulo de prueba con script (desarrolladores)
4. ? Definir próximos módulos a implementar (PM + Arquitectos)
5. ? Configurar base de datos (Backend)
6. ? Implementar tests unitarios (QA + Devs)

### **Para Nuevos Integrantes**

1. Leer [RESUMEN_EJECUTIVO.md](RESUMEN_EJECUTIVO.md)
2. Seguir ruta de aprendizaje según rol
3. Generar módulo de prueba
4. Revisar código de módulos existentes
5. Hacer preguntas al equipo

---

## ?? Soporte

Si tienes dudas sobre la documentación o el proyecto:

1. **Consulta primero**: Esta documentación
2. **Revisa código**: Los módulos existentes tienen ejemplos claros
3. **Experimenta**: Genera un módulo de prueba con el script
4. **Pregunta**: Al equipo en canal de Slack/Teams

---

## ? Checklist de Onboarding

Para nuevos desarrolladores:

- [ ] Leí el RESUMEN_EJECUTIVO.md
- [ ] Entendí la ARQUITECTURA_PLUGINS.md
- [ ] Ejecuté el proyecto localmente
- [ ] Generé un módulo de prueba con el script
- [ ] Exploré el código de módulos existentes
- [ ] Probé crear un componente UI personalizado
- [ ] Implementé permisos granulares en un componente
- [ ] Hice commit de mi primer cambio

**Tiempo estimado para completar**: 1-2 días

---

## ?? Contribuir a la Documentación

Si encuentras errores o quieres mejorar la documentación:

1. Crea un branch: `docs/mejora-seccion-X`
2. Edita los archivos Markdown
3. Haz commit con mensaje descriptivo
4. Abre Pull Request

---

**¡Bienvenido a VRM System!** ??

Si completaste el checklist de onboarding, estás listo para contribuir al proyecto.

---

**Última actualización**: $(Get-Date -Format "dd/MM/yyyy")  
**Versión documentación**: 1.0.0
