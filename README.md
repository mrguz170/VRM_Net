# VRM Plugin Demo

**Versión:** 2.0 - Con Cookies y Renderizado Condicional  
**Framework:** .NET 8 con Blazor Server  
**Última actualización:** Enero 2025

---

## 🚀 Inicio Rápido

Para comenzar rápidamente, consulta:
- **[docs/QUICK_START.md](docs/QUICK_START.md)** - Ejecuta la aplicación en 5 minutos
- **[docs/INDICE_DOCUMENTACION.md](docs/INDICE_DOCUMENTACION.md)** - Índice completo de toda la documentación
- **[docs/ESTRUCTURA_PROYECTO.md](docs/ESTRUCTURA_PROYECTO.md)** - Estructura detallada del proyecto

---

## 🎯 Novedades en Versión 2.0

### 🍪 Integración de Cookies HTTP Persistentes
- ✅ Sesión persistente entre recargas del navegador
- ✅ Autenticación robusta con ASP.NET Core Authentication
- ✅ Preparado para migración a ASP.NET Core Identity
- 📖 **Ver:** [docs/AUTENTICACION_Y_COOKIES.md](docs/AUTENTICACION_Y_COOKIES.md)

### 🔄 Renderizado Condicional (SSR + Interactive Server)
- ✅ Login/Logout con SSR estático (escribe cookies correctamente)
- ✅ Páginas protegidas con Interactive Server (interactividad completa)
- ✅ Resuelto problema "Response ya comenzó"
- ✅ Compatible con módulos dinámicos
- 📖 **Ver:** [docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md](docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md)

---

## 📚 Documentación Principal

### Para Desarrolladores
- **[docs/QUICK_START.md](docs/QUICK_START.md)** - Guía de inicio en 5 minutos
- **[docs/GUIA_AUTENTICACION_SIMULADA.md](docs/GUIA_AUTENTICACION_SIMULADA.md)** - Sistema de autenticación
- **[docs/AUTENTICACION_Y_COOKIES.md](docs/AUTENTICACION_Y_COOKIES.md)** - Autenticación y cookies persistentes 🍪
- **[docs/GUIA_SISTEMA_PERMISOS_GRANULARES.md](docs/GUIA_SISTEMA_PERMISOS_GRANULARES.md)** - Permisos avanzados
- **[src/Core/README.md](src/Core/README.md)** - Abstracciones y dominio
- **[src/Host/README.md](src/Host/README.md)** - Aplicación host
- **[src/Modules/README.md](src/Modules/README.md)** - Crear módulos

### Para Arquitectos
- **[docs/GUIA_DISENO_ARQUITECTURA.md](docs/GUIA_DISENO_ARQUITECTURA.md)** - Arquitectura completa
- **[docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md](docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md)** - Arquitectura de renderizado 🔄
- **[docs/ROADMAP_EMPRESARIAL.md](docs/ROADMAP_EMPRESARIAL.md)** - Planificación por fases

### Para DevOps
- **[docs/COMANDOS_SCRIPTS.md](docs/COMANDOS_SCRIPTS.md)** - Scripts y comandos útiles
- **[docs/VERSION_HISTORY.md](docs/VERSION_HISTORY.md)** - Historial de versiones y changelog

### Índice Completo
- **[docs/INDICE_DOCUMENTACION.md](docs/INDICE_DOCUMENTACION.md)** - Índice maestro de toda la documentación
- **[docs/VERSION_HISTORY.md](docs/VERSION_HISTORY.md)** - Historial de versiones

---

## ✨ Características Principales

### 🧩 Sistema Modular Dinámico
- Carga de módulos en tiempo de ejecución
- Hot-reload de módulos sin recompilar el host
- Arquitectura extensible basada en `IModule`

### 🔐 Autenticación y Autorización
- Cookies HTTP persistentes con ASP.NET Core Authentication
- Sistema de roles jerárquico
- Permisos granulares por acción
- Renderizado condicional para compatibilidad cookies + interactive

### 🏢 Multi-Tenant Ready
- Soporte para múltiples clientes
- Configuración por tenant
- Aislamiento de datos

### 🏗️ Arquitectura Clean
- Separación de capas (Core, Infrastructure, Application)
- Inyección de dependencias
- Patrón Repository
- SOLID principles

---

## 📁 Estructura del Proyecto

```
VRM_Net/
├── README.md          ← Este archivo (ÚNICO en raíz)
├── docs/          ← 📚 Toda la documentación
│├── QUICK_START.md
│   ├── INDICE_DOCUMENTACION.md
│   ├── VERSION_HISTORY.md
│   ├── ESTRUCTURA_PROYECTO.md
│   ├── AUTENTICACION_Y_COOKIES.md
│   ├── GUIA_AUTENTICACION_SIMULADA.md
│   ├── GUIA_SISTEMA_PERMISOS_GRANULARES.md
│   ├── GUIA_DISENO_ARQUITECTURA.md
│   ├── SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md
│   ├── COMANDOS_SCRIPTS.md
│   └── ROADMAP_EMPRESARIAL.md
├── src/
│   ├── Core/          # Núcleo del sistema
│   │   ├── VRM_Plugin.Core.Abstractions/
│   │   └── VRM_Plugin.Core.Domain/
│   ├── Host/   # Aplicación host Blazor
│   │   ├── Sliced_web_app/
│   │   └── VRM_Plugin.Blazor.Server/
│   └── Modules/       # Módulos de negocio
│       ├── Finanzas/
│       └── Onboarding/Prospectos/
└── scripts/           # Scripts de automatización
```

---

## 🏃‍♂️ Ejecutar el Proyecto

### Prerrequisitos
- .NET 8 SDK
- Visual Studio 2022 o VS Code
- Git

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/mrguz170/VRM_Net
cd VRM_Net

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# 4. Ejecutar
cd src/Host/VRM_Plugin.Blazor.Server
dotnet run

# O simplemente presionar F5 en Visual Studio
```

### Login de Prueba

Usuarios disponibles (cualquier password funciona):
- `admin@vrm.com` - Administrador (acceso completo)
- `gerente.finanzas@vrm.com` - Gerente de Finanzas
- `contador@vrm.com` - Contador (solo lectura)

Ver todos los usuarios en [docs/GUIA_AUTENTICACION_SIMULADA.md](docs/GUIA_AUTENTICACION_SIMULADA.md)

---

## 🔧 Crear un Nuevo Módulo

```bash
# 1. Crear proyecto
dotnet new razorclasslib -n VRM_Plugin.Modules.MiModulo -o src/Modules/MiModulo

# 2. Implementar IModule
# Ver guía completa en src/Modules/README.md

# 3. Compilar y copiar DLL
dotnet build src/Modules/MiModulo
copy src/Modules/MiModulo/bin/Debug/net8.0/*.dll src/Host/VRM_Plugin.Blazor.Server/Modules/

# 4. Reiniciar aplicación
# El módulo se cargará automáticamente
```

Ver guía completa: **[src/Modules/README.md](src/Modules/README.md)**

---

## 🗺️ Roadmap

### ✅ Fase 1: Fundamentos (Completado)
- [x] Sistema modular dinámico
- [x] Autenticación simulada
- [x] Permisos granulares
- [x] Módulos Finanzas y Prospectos
- [x] **Integración de cookies persistentes** 🍪
- [x] **Renderizado condicional SSR + Interactive** 🔄

### 🔄 Fase 2: Base de Datos (En progreso)
- [ ] Entity Framework Core
- [ ] Migraciones
- [ ] Repositorios
- [ ] ASP.NET Core Identity

### 📋 Fase 3: Producción (Planeado)
- [ ] Docker
- [ ] CI/CD
- [ ] Logging avanzado (Serilog + Seq)
- [ ] Health checks

Ver roadmap completo: **[docs/ROADMAP_EMPRESARIAL.md](docs/ROADMAP_EMPRESARIAL.md)**

---

## 💬 Soporte

### Documentación
- Consulta **[docs/INDICE_DOCUMENTACION.md](docs/INDICE_DOCUMENTACION.md)** para encontrar la guía que necesitas
- Busca en el código con `Ctrl+Shift+F` (los comentarios tienen emojis 📌)

### Issues
- Reporta bugs en [GitHub Issues](https://github.com/mrguz170/VRM_Net/issues)
- Usa labels: `bug`, `enhancement`, `documentation`, `question`

### Preguntas Frecuentes
- **"¿Por qué Login no es Interactive Server?"** → Ver [docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md](docs/SOLUCION_FINAL_RENDERIZADO_CONDICIONAL.md)
- **"¿Cómo funcionan las cookies?"** → Ver [docs/AUTENTICACION_Y_COOKIES.md](docs/AUTENTICACION_Y_COOKIES.md)
- **"¿Cómo crear un módulo?"** → Ver [src/Modules/README.md](src/Modules/README.md)

---

## 👥 Equipo

**Mantenedor:** Equipo VRM  
**Repositorio:** [https://github.com/mrguz170/VRM_Net](https://github.com/mrguz170/VRM_Net)  
**Licencia:** MIT

---

## 📖 Recursos

### Oficial
- [Documentación de Blazor](https://learn.microsoft.com/aspnet/core/blazor/)
- [Blazor Render Modes (.NET 8)](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes)
- [ASP.NET Core Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/)

### Comunidad
- [Blazor University](https://blazor-university.com/)
- [Awesome Blazor](https://github.com/AdrienTorris/awesome-blazor)

---

**🚀 ¡Empieza explorando [docs/QUICK_START.md](docs/QUICK_START.md)!**