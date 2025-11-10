# ??? GUÍA: Implementación de Infrastructure Layer con Entity Framework Core

**Fecha:** Enero 10, 2025  
**Versión:** 1.0.0  
**Estado:** ?? GUÍA PASO A PASO  
**Prerrequisito:** ? Fase 1 completada (ver `ESTADO_ACTUAL_SISTEMA.md`)

---

## ?? **TABLA DE CONTENIDOS**

1. [Introducción](#introducción)
2. [Arquitectura Propuesta](#arquitectura-propuesta)
3. [Paso 1: Crear Proyecto Infrastructure](#paso-1-crear-proyecto-infrastructure)
4. [Paso 2: Instalar Paquetes NuGet](#paso-2-instalar-paquetes-nuget)
5. [Paso 3: Crear ApplicationDbContext](#paso-3-crear-applicationdbcontext)
6. [Paso 4: Configurar Entities](#paso-4-configurar-entities)
7. [Paso 5: Crear Migrations](#paso-5-crear-migrations)
8. [Paso 6: Seed Data](#paso-6-seed-data)
9. [Paso 7: Configurar Dependency Injection](#paso-7-configurar-dependency-injection)
10. [Paso 8: Migrar ModuleLoader a BD](#paso-8-migrar-moduleloader-a-bd)
11. [Testing y Verificación](#testing-y-verificación)
12. [Troubleshooting](#troubleshooting)

---

## ?? **INTRODUCCIÓN**

### **¿Por qué Infrastructure Layer?**

Actualmente, los módulos se cargan desde **DLLs en memoria** usando `ModuleLoader.cs`. Este enfoque tiene limitaciones:

? **PROBLEMAS ACTUALES:**
- Módulos hardcodeados en código
- Cambios requieren recompilación
- Sin gestión centralizada de permisos
- Sin auditoría de cambios
- No escalable para multi-tenancy

? **SOLUCIÓN: INFRASTRUCTURE LAYER**
- Módulos almacenados en **SQL Server**
- Gestión dinámica desde UI (futuro)
- Auditoría completa
- Multi-tenancy ready
- Escalabilidad

### **¿Qué vamos a construir?**

```
VRM_Net/
??? src/
?   ??? Infrastructure/
?   ?   ??? VRM_Plugin.Infrastructure/        ?? NUEVO PROYECTO
?   ?       ??? Data/
?   ?       ?   ??? ApplicationDbContext.cs   ?? DbContext principal
?   ?       ?   ??? Configurations/           ?? Fluent API configs
?   ?       ?   ?   ??? ModuleConfiguration.cs
?   ?       ?   ?   ??? ModuleComponentConfiguration.cs
?   ?       ?   ?   ??? ModuleActionConfiguration.cs
?   ?       ?   ??? Seed/                     ?? Seed data
?   ?       ?       ??? ModuleSeedData.cs
?   ?       ?       ??? PermissionSeedData.cs
?   ?       ??? Repositories/                 ?? Repository pattern
?   ?       ?   ??? IModuleRepository.cs
?   ?       ?   ??? ModuleRepository.cs
?   ?       ??? Migrations/                   ?? EF Migrations
?   ?           ??? YYYYMMDDHHMMSS_InitialCreate.cs
?   ??? Core/                                 ? EXISTENTE
?   ?   ??? VRM_Plugin.Core.Abstractions/     ? Ya listo para EF
?   ?       ??? Entities/
?   ?           ??? Module.cs
?   ?           ??? ModuleComponent.cs
?   ?           ??? ModuleAction.cs
?   ??? Host/
?       ??? VRM_PluginDemo.Blazor.Server/     ? EXISTENTE
?           ??? Program.cs                    ?? Modificar para usar BD
?           ??? Services/
?               ??? ModuleLoader.cs           ?? Modificar para leer de BD
```

---

## ??? **ARQUITECTURA PROPUESTA**

### **Clean Architecture - Capas**

```
???????????????????????????????????????????????????????????????
?  PRESENTACIÓN (Blazor Server)                               ?
?  - Components (Razor)                                       ?
?  - Pages                                                    ?
?  - Services (UI Logic)                                      ?
???????????????????????????????????????????????????????????????
                          ? Depende de
???????????????????????????????????????????????????????????????
?  CORE (Abstracciones)                                       ?
?  - IModule                                                  ?
?  - Entities (Module, ModuleComponent, ModuleAction)        ?
?  - Interfaces (IModuleRepository, IUnitOfWork)             ?
???????????????????????????????????????????????????????????????
                          ? Implementado por
???????????????????????????????????????????????????????????????
?  INFRASTRUCTURE (Persistencia)                              ?
?  - ApplicationDbContext                                     ?
?  - Repositories (Implementaciones)                          ?
?  - Migrations                                               ?
?  - Seed Data                                                ?
???????????????????????????????????????????????????????????????
                          ? Persiste en
???????????????????????????????????????????????????????????????
?  DATABASE (SQL Server)                                      ?
?  - Modulos                                                  ?
?  - ModuloComponentes                                        ?
?  - AccionesGranulares                                       ?
?  - Permisos                                                 ?
???????????????????????????????????????????????????????????????
```

### **Flujo de Datos**

```
1. Usuario accede a /index
   ?
2. Sidebar.razor llama AuthService.GetVisibleComponentsAsync()
   ?
3. AuthService llama ModuleRepository.GetComponentsByPermissionsAsync()
   ?
4. ModuleRepository consulta ApplicationDbContext
   ?
5. EF Core genera SQL y consulta SQL Server
   ?
6. Datos regresan como List<ModuleComponent>
   ?
7. Sidebar renderiza menú dinámicamente
```

---

## ?? **PASO 1: CREAR PROYECTO INFRASTRUCTURE**

### **1.1. Crear el proyecto**

```bash
# Ir a la carpeta del workspace
cd C:\Users\lobo_\Documents\VRM\VRM_Net

# Crear carpeta Infrastructure si no existe
mkdir -p src\Infrastructure

# Crear proyecto Class Library
dotnet new classlib -n VRM_Plugin.Infrastructure -o src\Infrastructure\VRM_Plugin.Infrastructure -f net8.0
```

### **1.2. Agregar al solution**

```bash
# Agregar proyecto al solution
dotnet sln VRM_Net.sln add src\Infrastructure\VRM_Plugin.Infrastructure\VRM_Plugin.Infrastructure.csproj
```

### **1.3. Verificar creación**

```bash
# Debe mostrar el proyecto en la lista
dotnet sln list
```

**Salida esperada:**
```
Proyecto(s)
-----------
src\Core\VRM_Plugin.Core.Abstractions\VRM_Plugin.Core.Abstractions.csproj
src\Core\VRM_Plugin.Core.Domain\VRM_Plugin.Core.Domain.csproj
src\Host\VRM_PluginDemo.Blazor.Server\VRM_Plugin.Blazor.Server.csproj
src\Infrastructure\VRM_Plugin.Infrastructure\VRM_Plugin.Infrastructure.csproj  ? NUEVO
src\Modules\Finanzas\VRM_PluginDemo.Modules.Finanzas\VRM_Plugin.Finanzas.csproj
src\Modules\Onboarding\VRM_PluginDemo.Modules.Prospectos\VRM_Plugin.Prospectos.csproj
```

---

## ?? **PASO 2: INSTALAR PAQUETES NUGET**

### **2.1. Entity Framework Core**

```bash
# SQL Server provider
dotnet add src\Infrastructure\VRM_Plugin.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11

# EF Core Tools (para migrations)
dotnet add src\Infrastructure\VRM_Plugin.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 8.0.11

# EF Core Design (para migrations desde CLI)
dotnet add src\Infrastructure\VRM_Plugin.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.0.11
```

### **2.2. Agregar referencia a Core.Abstractions**

```bash
# Referencia al proyecto Core donde están las entidades
dotnet add src\Infrastructure\VRM_Plugin.Infrastructure reference src\Core\VRM_Plugin.Core.Abstractions\VRM_Plugin.Core.Abstractions.csproj
```

### **2.3. Verificar paquetes instalados**

```bash
dotnet list src\Infrastructure\VRM_Plugin.Infrastructure package
```

**Salida esperada:**
```
Paquetes de nivel superior del proyecto 'VRM_Plugin.Infrastructure'
   [net8.0]:
   Paquete de nivel superior                         Solicitado
   > Microsoft.EntityFrameworkCore.Design            8.0.11
   > Microsoft.EntityFrameworkCore.SqlServer         8.0.11
   > Microsoft.EntityFrameworkCore.Tools             8.0.11
```

---

## ??? **PASO 3: CREAR APPLICATIONDBCONTEXT**

### **3.1. Crear carpeta Data**

```bash
mkdir src\Infrastructure\VRM_Plugin.Infrastructure\Data
```

### **3.2. Crear ApplicationDbContext.cs**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Data\ApplicationDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Infrastructure.Data;

/// <summary>
/// DbContext principal para el sistema de módulos VRM_Net.
/// Gestiona la persistencia de módulos, componentes, acciones y permisos.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ==================== DbSets ====================
    
    /// <summary>
    /// Tabla: Modulos
    /// Almacena los módulos del sistema (Finanzas, Prospectos, etc.)
    /// </summary>
    public DbSet<Module> Modulos { get; set; } = null!;

    /// <summary>
    /// Tabla: ModuloComponentes
    /// Almacena los componentes de navegación (menú jerárquico)
    /// </summary>
    public DbSet<ModuleComponent> ModuloComponentes { get; set; } = null!;

    /// <summary>
    /// Tabla: AccionesGranulares
    /// Almacena las acciones de negocio (crear, editar, eliminar, etc.)
    /// </summary>
    public DbSet<ModuleAction> AccionesGranulares { get; set; } = null!;

    /// <summary>
    /// Tabla: TiposAccion
    /// Catálogo de tipos de acción (Lectura, Escritura, Crítica)
    /// </summary>
    public DbSet<ActionType> TiposAccion { get; set; } = null!;

    // ==================== Configuración ====================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones desde archivos separados
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    // ==================== Auditoría Automática ====================

    public override int SaveChanges()
    {
        AplicarAuditoria();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AplicarAuditoria();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AplicarAuditoria()
    {
        var entidades = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entidad in entidades)
        {
            // TODO: Obtener usuario actual desde IHttpContextAccessor
            var userId = 1; // Por ahora hardcodeado

            if (entidad.State == EntityState.Added)
            {
                // Establecer CreatedAt y CreatedBy
                if (entidad.Entity is Module module)
                {
                    module.CreatedAt = DateTime.UtcNow;
                    module.CreatedBy = userId;
                }
                else if (entidad.Entity is ModuleComponent component)
                {
                    component.CreatedAt = DateTime.UtcNow;
                    component.CreatedBy = userId;
                }
                else if (entidad.Entity is ModuleAction action)
                {
                    action.CreatedAt = DateTime.UtcNow;
                    action.CreatedBy = userId;
                }
            }
            else if (entidad.State == EntityState.Modified)
            {
                // Establecer UpdatedAt y UpdatedBy
                if (entidad.Entity is Module module)
                {
                    module.UpdatedAt = DateTime.UtcNow;
                    module.UpdatedBy = userId;
                }
                else if (entidad.Entity is ModuleComponent component)
                {
                    component.UpdatedAt = DateTime.UtcNow;
                    component.UpdatedBy = userId;
                }
                else if (entidad.Entity is ModuleAction action)
                {
                    action.UpdatedAt = DateTime.UtcNow;
                    action.UpdatedBy = userId;
                }
            }
        }
    }
}
```

**? Características:**
- ? DbSets para todas las entidades
- ? Auditoría automática (CreatedAt, UpdatedAt)
- ? Configuración modular (Fluent API en archivos separados)

---

## ?? **PASO 4: CONFIGURAR ENTITIES**

### **4.1. Crear carpeta Configurations**

```bash
mkdir src\Infrastructure\VRM_Plugin.Infrastructure\Data\Configurations
```

### **4.2. Configuración para Module**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Data\Configurations\ModuleConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad Module (tabla Modulos).
/// </summary>
public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        // ===== TABLA =====
        builder.ToTable("Modulos");

        // ===== PRIMARY KEY =====
        builder.HasKey(e => e.IdModule);
        builder.Property(e => e.IdModule)
            .ValueGeneratedOnAdd()  // IDENTITY(1,1)
            .HasComment("ID único del módulo (auto-incremental)");

        // ===== PROPIEDADES REQUERIDAS =====
        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Código técnico del módulo (ej: 'Finanzas')");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Nombre para mostrar en la UI");

        builder.Property(e => e.Descripcion)
            .HasMaxLength(500)
            .HasComment("Descripción del módulo");

        builder.Property(e => e.Version)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Versión del módulo (formato: '1.0.0')");

        // ===== ÍNDICES =====
        builder.HasIndex(e => e.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_Modulos_Codigo");

        // ===== AUDITORÍA =====
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indica si el módulo está activo (soft delete)");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(e => e.UpdatedAt)
            .HasComment("Fecha de última actualización");

        builder.Property(e => e.CreatedBy)
            .HasComment("ID del usuario que creó el registro");

        builder.Property(e => e.UpdatedBy)
            .HasComment("ID del usuario que actualizó el registro");

        // ===== RELACIONES =====
        // Un módulo tiene muchos componentes
        builder.HasMany<ModuleComponent>()
            .WithOne(c => c.Module)
            .HasForeignKey(c => c.IdModule)
            .OnDelete(DeleteBehavior.Restrict);  // No borrar en cascada
    }
}
```

### **4.3. Configuración para ModuleComponent**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Data\Configurations\ModuleComponentConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad ModuleComponent (tabla ModuloComponentes).
/// </summary>
public class ModuleComponentConfiguration : IEntityTypeConfiguration<ModuleComponent>
{
    public void Configure(EntityTypeBuilder<ModuleComponent> builder)
    {
        // ===== TABLA =====
        builder.ToTable("ModuloComponentes");

        // ===== PRIMARY KEY =====
        builder.HasKey(e => e.IdComponent);
        builder.Property(e => e.IdComponent)
            .ValueGeneratedOnAdd()
            .HasComment("ID único del componente (auto-incremental)");

        // ===== FOREIGN KEYS =====
        builder.Property(e => e.IdModule)
            .IsRequired()
            .HasComment("ID del módulo al que pertenece");

        builder.Property(e => e.IdParent)
            .HasComment("ID del componente padre (NULL = raíz)");

        // ===== PROPIEDADES REQUERIDAS =====
        builder.Property(e => e.ComponentCode)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Código técnico del componente (ej: 'Finanzas.Facturas')");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Nombre para mostrar en el menú");

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Route)
            .HasMaxLength(500)
            .HasComment("Ruta del componente Blazor (ej: '/finanzas/facturas')");

        builder.Property(e => e.Icon)
            .HasMaxLength(100)
            .HasComment("Clase CSS del icono (Remix Icons)");

        // ===== ÍNDICES =====
        builder.HasIndex(e => e.ComponentCode)
            .IsUnique()
            .HasDatabaseName("IX_ModuloComponentes_ComponentCode");

        builder.HasIndex(e => e.IdModule)
            .HasDatabaseName("IX_ModuloComponentes_IdModule");

        builder.HasIndex(e => e.IdParent)
            .HasDatabaseName("IX_ModuloComponentes_IdParent");

        // ===== RELACIONES =====
        // FK a Módulo
        builder.HasOne(e => e.Module)
            .WithMany()
            .HasForeignKey(e => e.IdModule)
            .OnDelete(DeleteBehavior.Restrict);

        // Auto-referencia (IdParent)
        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.IdParent)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== AUDITORÍA =====
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // ===== IGNORAR PROPIEDADES NO MAPEADAS =====
        builder.Ignore(e => e.ComponentType);  // No se persiste en BD
        builder.Ignore(e => e.RequiredPermissionIds);  // Se maneja en tabla intermedia
    }
}
```

### **4.4. Configuración para ModuleAction**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Data\Configurations\ModuleActionConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad ModuleAction (tabla AccionesGranulares).
/// </summary>
public class ModuleActionConfiguration : IEntityTypeConfiguration<ModuleAction>
{
    public void Configure(EntityTypeBuilder<ModuleAction> builder)
    {
        // ===== TABLA =====
        builder.ToTable("AccionesGranulares");

        // ===== PRIMARY KEY =====
        builder.HasKey(e => e.IdAction);
        builder.Property(e => e.IdAction)
            .ValueGeneratedOnAdd()
            .HasComment("ID único de la acción (auto-incremental)");

        // ===== FOREIGN KEYS =====
        builder.Property(e => e.IdComponent)
            .HasComment("ID del componente (NULL = acción global del módulo)");

        builder.Property(e => e.IdActionType)
            .IsRequired()
            .HasComment("ID del tipo de acción (1=Lectura, 2=Escritura, 3=Crítica)");

        // ===== PROPIEDADES REQUERIDAS =====
        builder.Property(e => e.ActionKey)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Clave única de la acción (ej: 'Finanzas.Facturas.TimbrarSAT')");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Nombre descriptivo de la acción");

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // ===== ÍNDICES =====
        builder.HasIndex(e => e.ActionKey)
            .IsUnique()
            .HasDatabaseName("IX_AccionesGranulares_ActionKey");

        builder.HasIndex(e => e.IdComponent)
            .HasDatabaseName("IX_AccionesGranulares_IdComponent");

        builder.HasIndex(e => e.IdActionType)
            .HasDatabaseName("IX_AccionesGranulares_IdActionType");

        // ===== RELACIONES =====
        builder.HasOne(e => e.Component)
            .WithMany(c => c.Actions)
            .HasForeignKey(e => e.IdComponent)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ActionType)
            .WithMany()
            .HasForeignKey(e => e.IdActionType)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== AUDITORÍA =====
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // ===== IGNORAR PROPIEDADES NO MAPEADAS =====
        builder.Ignore(e => e.RequiredPermissionIds);  // Se maneja en tabla intermedia
    }
}
```

### **4.5. Configuración para ActionType**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Data\Configurations\ActionTypeConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración de Fluent API para la entidad ActionType (tabla TiposAccion).
/// </summary>
public class ActionTypeConfiguration : IEntityTypeConfiguration<ActionType>
{
    public void Configure(EntityTypeBuilder<ActionType> builder)
    {
        // ===== TABLA =====
        builder.ToTable("TiposAccion");

        // ===== PRIMARY KEY =====
        builder.HasKey(e => e.IdActionType);
        builder.Property(e => e.IdActionType)
            .ValueGeneratedOnAdd()
            .HasComment("ID único del tipo de acción");

        // ===== PROPIEDADES =====
        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Código del tipo (ej: 'Lectura')");

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(500);

        builder.Property(e => e.Orden)
            .IsRequired()
            .HasComment("Orden de severidad (1=Lectura, 2=Escritura, 3=Crítica)");

        // ===== ÍNDICES =====
        builder.HasIndex(e => e.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_TiposAccion_Codigo");

        // ===== SEED DATA =====
        builder.HasData(
            new ActionType
            {
                IdActionType = 1,
                Codigo = "Lectura",
                Nombre = "Lectura",
                Descripcion = "Operaciones de consulta (ver, listar, buscar)",
                Orden = 1
            },
            new ActionType
            {
                IdActionType = 2,
                Codigo = "Escritura",
                Nombre = "Escritura",
                Descripcion = "Operaciones de modificación (crear, editar, actualizar)",
                Orden = 2
            },
            new ActionType
            {
                IdActionType = 3,
                Codigo = "Critica",
                Nombre = "Crítica",
                Descripcion = "Operaciones sensibles (eliminar, aprobar, timbrar)",
                Orden = 3
            }
        );
    }
}
```

---

## ?? **PASO 5: CREAR MIGRATIONS**

### **5.1. Configurar Connection String**

**Archivo:** `src\Host\VRM_PluginDemo.Blazor.Server\appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=VRM_Net;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Serilog": {
    // ... configuración existente
  }
}
```

**Notas:**
- `Server=.` ? SQL Server local
- `Database=VRM_Net` ? Nombre de la BD
- `Trusted_Connection=True` ? Windows Authentication
- `TrustServerCertificate=True` ? Para desarrollo local
- `MultipleActiveResultSets=true` ? Para queries complejas

### **5.2. Crear Migration Inicial**

```bash
# Desde la raíz del proyecto
dotnet ef migrations add InitialModulesStructure \
  --project src\Infrastructure\VRM_Plugin.Infrastructure \
  --startup-project src\Host\VRM_PluginDemo.Blazor.Server \
  --context ApplicationDbContext \
  --output-dir Data\Migrations
```

**Salida esperada:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

### **5.3. Revisar Migration Generada**

**Archivo:** `src\Infrastructure\VRM_Plugin.Infrastructure\Data\Migrations\YYYYMMDDHHMMSS_InitialModulesStructure.cs`

```csharp
public partial class InitialModulesStructure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Crear tabla TiposAccion
        migrationBuilder.CreateTable(
            name: "TiposAccion",
            columns: table => new
            {
                IdActionType = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Codigo = table.Column<string>(maxLength: 50, nullable: false),
                Nombre = table.Column<string>(maxLength: 100, nullable: false),
                // ...
            });

        // Crear tabla Modulos
        migrationBuilder.CreateTable(
            name: "Modulos",
            columns: table => new
            {
                IdModule = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Codigo = table.Column<string>(maxLength: 100, nullable: false),
                // ...
            });

        // Crear tabla ModuloComponentes con FK y auto-referencia
        // Crear tabla AccionesGranulares con FKs
        // ...
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback
    }
}
```

### **5.4. Aplicar Migration a BD**

```bash
# Aplicar migration
dotnet ef database update \
  --project src\Infrastructure\VRM_Plugin.Infrastructure \
  --startup-project src\Host\VRM_PluginDemo.Blazor.Server \
  --context ApplicationDbContext
```

**Salida esperada:**
```
Build started...
Build succeeded.
Applying migration '20250110220000_InitialModulesStructure'.
Done.
```

### **5.5. Verificar en SQL Server**

```sql
-- Conectar a SQL Server Management Studio
-- Base de datos: VRM_Net

-- Verificar tablas creadas
SELECT * FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

/*
Resultado esperado:
- AccionesGranulares
- ModuloComponentes
- Modulos
- TiposAccion
- __EFMigrationsHistory  (control de migrations)
*/

-- Verificar datos seed de TiposAccion
SELECT * FROM TiposAccion;

/*
IdActionType | Codigo    | Nombre    | Descripcion
-------------|-----------|-----------|-------------
1            | Lectura   | Lectura   | Operaciones de consulta...
2            | Escritura | Escritura | Operaciones de modificación...
3            | Critica   | Crítica   | Operaciones sensibles...
*/
```

---

## ?? **PASO 6: SEED DATA**

### **6.1. Crear Carpeta Seed**

```bash
mkdir src\Infrastructure\VRM_Plugin.Infrastructure\Data\Seed
```

### **6.2. Crear ModuleSeedData.cs**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Data\Seed\ModuleSeedData.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Infrastructure.Data.Seed;

/// <summary>
/// Seed data para poblar módulos iniciales desde código (FinanzasModule, ProspectosModule).
/// </summary>
public static class ModuleSeedData
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        ILogger logger)
    {
        logger.LogInformation("[Seed] Iniciando seed de módulos...");

        // Verificar si ya hay módulos
        if (await context.Modulos.AnyAsync())
        {
            logger.LogInformation("[Seed] Ya existen módulos en BD. Omitiendo seed.");
            return;
        }

        // ==================== MÓDULO: FINANZAS ====================
        logger.LogInformation("[Seed] Creando módulo Finanzas...");
        
        var finanzas = new Module
        {
            Codigo = "Finanzas",
            Nombre = "Gestión de Finanzas",
            Descripcion = "Módulo para gestionar operaciones financieras. Incluye facturas, pagos, conciliaciones y cuentas por pagar.",
            Version = "1.0.0",
            IsActive = true
        };
        context.Modulos.Add(finanzas);
        await context.SaveChangesAsync();  // Guardar para obtener IdModule

        // Componente raíz: Finanzas
        var finanzasRoot = new ModuleComponent
        {
            IdModule = finanzas.IdModule,
            IdParent = null,  // Raíz
            ComponentCode = "Finanzas.Root",
            Name = "Finanzas",
            Description = "Módulo principal de finanzas",
            Route = "",  // Sin ruta, solo contenedor
            Icon = "ri-money-dollar-circle-line",
            MenuOrder = 20,
            ShowInMenu = true,
            IsActive = true
        };
        context.ModuloComponentes.Add(finanzasRoot);
        await context.SaveChangesAsync();

        // Componente hijo: Facturas
        var facturas = new ModuleComponent
        {
            IdModule = finanzas.IdModule,
            IdParent = finanzasRoot.IdComponent,  // Hijo de Finanzas
            ComponentCode = "Finanzas.Facturas",
            Name = "Facturas",
            Description = "Gestión de facturas",
            Route = "/finanzas/facturas",
            Icon = "ri-file-list-3-line",
            MenuOrder = 1,
            ShowInMenu = true,
            IsActive = true
        };
        context.ModuloComponentes.Add(facturas);

        // Componente hijo: Cobros y Pagos
        var cobrosYPagos = new ModuleComponent
        {
            IdModule = finanzas.IdModule,
            IdParent = finanzasRoot.IdComponent,
            ComponentCode = "Finanzas.CobrosYPagos",
            Name = "Cobros y Pagos",
            Description = "Gestión de cobros y pagos",
            Route = "/finanzas/cobros-pagos",
            Icon = "ri-exchange-dollar-line",
            MenuOrder = 2,
            ShowInMenu = true,
            IsActive = true
        };
        context.ModuloComponentes.Add(cobrosYPagos);
        await context.SaveChangesAsync();

        // Acciones de Facturas
        var accionesFinanzas = new List<ModuleAction>
        {
            new() { IdComponent = facturas.IdComponent, ActionKey = "Finanzas.Facturas.Ver", Name = "Ver Facturas", Description = "Permite visualizar el listado de facturas", IdActionType = 1, IsActive = true },
            new() { IdComponent = facturas.IdComponent, ActionKey = "Finanzas.Facturas.Crear", Name = "Crear Factura", Description = "Permite crear nuevas facturas", IdActionType = 2, IsActive = true },
            new() { IdComponent = facturas.IdComponent, ActionKey = "Finanzas.Facturas.Editar", Name = "Editar Factura", Description = "Permite modificar facturas existentes", IdActionType = 2, IsActive = true },
            new() { IdComponent = facturas.IdComponent, ActionKey = "Finanzas.Facturas.Eliminar", Name = "Eliminar Factura", Description = "Permite eliminar facturas", IdActionType = 3, IsActive = true },
            new() { IdComponent = facturas.IdComponent, ActionKey = "Finanzas.Facturas.TimbrarSAT", Name = "Timbrar en SAT", Description = "Envía factura al SAT para timbrado fiscal", IdActionType = 3, IsActive = true }
        };
        context.AccionesGranulares.AddRange(accionesFinanzas);

        logger.LogInformation("[Seed] ? Módulo Finanzas creado: {Components} componentes, {Actions} acciones",
            3, accionesFinanzas.Count);

        // ==================== MÓDULO: PROSPECTOS ====================
        logger.LogInformation("[Seed] Creando módulo Prospectos...");
        
        var prospectos = new Module
        {
            Codigo = "Prospectos",
            Nombre = "Gestión de Prospectos",
            Descripcion = "Módulo para gestionar solicitudes de proveedores. Permite recibir, revisar y aprobar empresas que desean ser proveedores.",
            Version = "1.0.0",
            IsActive = true
        };
        context.Modulos.Add(prospectos);
        await context.SaveChangesAsync();

        // Componente raíz: Prospectos (sin hijos)
        var prospectosRoot = new ModuleComponent
        {
            IdModule = prospectos.IdModule,
            IdParent = null,
            ComponentCode = "Prospectos.Root",
            Name = "Prospectos",
            Description = "Gestión de solicitudes de proveedores",
            Route = "/prospectos",
            Icon = "ri-list-check-3",
            MenuOrder = 10,
            ShowInMenu = true,
            IsActive = true
        };
        context.ModuloComponentes.Add(prospectosRoot);
        await context.SaveChangesAsync();

        // Acciones de Prospectos
        var accionesProspectos = new List<ModuleAction>
        {
            new() { IdComponent = prospectosRoot.IdComponent, ActionKey = "Prospectos.Ver", Name = "Ver Prospectos", Description = "Permite visualizar prospectos", IdActionType = 1, IsActive = true },
            new() { IdComponent = prospectosRoot.IdComponent, ActionKey = "Prospectos.Crear", Name = "Crear Prospecto", Description = "Permite crear nuevos prospectos", IdActionType = 2, IsActive = true },
            new() { IdComponent = prospectosRoot.IdComponent, ActionKey = "Prospectos.Editar", Name = "Editar Prospecto", Description = "Permite modificar prospectos", IdActionType = 2, IsActive = true },
            new() { IdComponent = prospectosRoot.IdComponent, ActionKey = "Prospectos.AprobarFinal", Name = "Aprobar Prospecto", Description = "Aprobación final del prospecto", IdActionType = 3, IsActive = true }
        };
        context.AccionesGranulares.AddRange(accionesProspectos);

        logger.LogInformation("[Seed] ? Módulo Prospectos creado: {Components} componentes, {Actions} acciones",
            1, accionesProspectos.Count);

        // ==================== GUARDAR CAMBIOS ====================
        await context.SaveChangesAsync();
        
        logger.LogInformation("[Seed] ? Seed completado: {ModuleCount} módulos creados", 2);
    }
}
```

### **6.3. Ejecutar Seed desde Program.cs**

**Modificar:** `src\Host\VRM_PluginDemo.Blazor.Server\Program.cs`

```csharp
// Después de app.Build() y antes de app.Run()

// ==================== SEED DATA (SOLO EN DESARROLLO) ====================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        Log.Information("[Program] Ejecutando seed de base de datos...");
        await ModuleSeedData.SeedAsync(dbContext, logger);
        Log.Information("[Program] ? Seed completado exitosamente");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "[Program] ? Error al ejecutar seed de base de datos");
    }
}
```

---

## ?? **PASO 7: CONFIGURAR DEPENDENCY INJECTION**

### **7.1. Agregar referencia a Infrastructure en Host**

```bash
dotnet add src\Host\VRM_PluginDemo.Blazor.Server reference src\Infrastructure\VRM_Plugin.Infrastructure
```

### **7.2. Configurar DbContext en Program.cs**

**Modificar:** `src\Host\VRM_PluginDemo.Blazor.Server\Program.cs`

```csharp
using VRM_Plugin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// ... código existente ...

// ==================== BASE DE DATOS ====================
// ? NUEVO: Configurar ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
    
    // Logging de SQL en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ==================== SISTEMA DE PLUGINS ====================
// ... código existente de ModuleLoader ...
```

---

## ?? **PASO 8: MIGRAR MODULELOADER A BD**

### **8.1. Crear IModuleRepository**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Repositories\IModuleRepository.cs`

```csharp
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Infrastructure.Repositories;

public interface IModuleRepository
{
    Task<List<Module>> GetAllActiveModulesAsync();
    Task<Module?> GetModuleByIdAsync(int idModule);
    Task<Module?> GetModuleByCodeAsync(string codigo);
    Task<List<ModuleComponent>> GetComponentsByModuleIdAsync(int idModule);
    Task<List<ModuleAction>> GetActionsByModuleIdAsync(int idModule);
    Task<List<ModuleComponent>> GetVisibleComponentsByPermissionsAsync(List<int> permissionIds);
}
```

### **8.2. Implementar ModuleRepository**

**Ruta:** `src\Infrastructure\VRM_Plugin.Infrastructure\Repositories\ModuleRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VRM_Plugin.Core.Abstractions.Entities;
using VRM_Plugin.Infrastructure.Data;

namespace VRM_Plugin.Infrastructure.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ModuleRepository> _logger;

    public ModuleRepository(
        ApplicationDbContext context,
        ILogger<ModuleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Module>> GetAllActiveModulesAsync()
    {
        _logger.LogDebug("[ModuleRepository] Obteniendo todos los módulos activos");

        return await _context.Modulos
            .Where(m => m.IsActive)
            .Include(m => m.Components)
                .ThenInclude(c => c.Children)
            .Include(m => m.Actions)
                .ThenInclude(a => a.ActionType)
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    }

    public async Task<Module?> GetModuleByIdAsync(int idModule)
    {
        return await _context.Modulos
            .Include(m => m.Components)
            .Include(m => m.Actions)
            .FirstOrDefaultAsync(m => m.IdModule == idModule);
    }

    public async Task<Module?> GetModuleByCodeAsync(string codigo)
    {
        return await _context.Modulos
            .Include(m => m.Components)
            .Include(m => m.Actions)
            .FirstOrDefaultAsync(m => m.Codigo == codigo);
    }

    public async Task<List<ModuleComponent>> GetComponentsByModuleIdAsync(int idModule)
    {
        return await _context.ModuloComponentes
            .Where(c => c.IdModule == idModule && c.IsActive)
            .Include(c => c.Parent)
            .Include(c => c.Children)
            .OrderBy(c => c.MenuOrder)
            .ToListAsync();
    }

    public async Task<List<ModuleAction>> GetActionsByModuleIdAsync(int idModule)
    {
        return await _context.AccionesGranulares
            .Where(a => a.Component!.IdModule == idModule && a.IsActive)
            .Include(a => a.ActionType)
            .Include(a => a.Component)
            .ToListAsync();
    }

    public async Task<List<ModuleComponent>> GetVisibleComponentsByPermissionsAsync(List<int> permissionIds)
    {
        // TODO: Implementar filtrado por permisos cuando exista tabla ComponentePermisos
        // Por ahora retornar todos los componentes activos
        
        _logger.LogDebug(
            "[ModuleRepository] Obteniendo componentes visibles para permisos: {Permissions}",
            string.Join(", ", permissionIds));

        return await _context.ModuloComponentes
            .Where(c => c.IsActive)
            .Include(c => c.Module)
            .Include(c => c.Parent)
            .Include(c => c.Children)
            .OrderBy(c => c.MenuOrder)
            .ToListAsync();
    }
}
```

### **8.3. Registrar Repository en DI**

**Modificar:** `src\Host\VRM_PluginDemo.Blazor.Server\Program.cs`

```csharp
using VRM_Plugin.Infrastructure.Repositories;

// ==================== REPOSITORIES ====================
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
```

### **8.4. Modificar ModuleAuthorizationService**

**Modificar:** `src\Host\VRM_PluginDemo.Blazor.Server\Services\ModuleAuthorizationService.cs`

```csharp
using VRM_Plugin.Infrastructure.Repositories;

public class ModuleAuthorizationService : IModuleAuthorizationService
{
    private readonly IModuleRepository _repository;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<ModuleAuthorizationService> _logger;

    public ModuleAuthorizationService(
        IModuleRepository repository,  // ? Inyectar repository en lugar de ModuleManager
        AuthenticationStateProvider authStateProvider,
        ILogger<ModuleAuthorizationService> logger)
    {
        _repository = repository;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public async Task<List<ModuleComponent>> GetVisibleComponentsAsync()
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var username = user.Identity?.Name ?? "Anonymous";

            _logger.LogInformation(
                "[AuthService] Obteniendo componentes visibles para usuario: {User}",
                username);

            // Si es Admin, retornar todos los componentes
            if (user.IsInRole("Admin"))
            {
                var allComponents = await _repository.GetVisibleComponentsByPermissionsAsync(new List<int> { 1 });
                
                _logger.LogInformation(
                    "[AuthService] Usuario {User} es Admin: {Count} componentes visibles, Tiempo={ElapsedMs}ms",
                    username,
                    allComponents.Count,
                    (DateTime.UtcNow - startTime).TotalMilliseconds);
                
                return allComponents;
            }

            // Obtener IDs de permisos del usuario desde claims
            var permissionIds = GetUserPermissionIds(user);

            var components = await _repository.GetVisibleComponentsByPermissionsAsync(permissionIds);

            _logger.LogInformation(
                "[AuthService] Componentes visibles para usuario {User}: {Count}, Tiempo={ElapsedMs}ms",
                username,
                components.Count,
                (DateTime.UtcNow - startTime).TotalMilliseconds);

            return components;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[AuthService] Error al obtener componentes visibles, Tiempo={ElapsedMs}ms",
                (DateTime.UtcNow - startTime).TotalMilliseconds);
            throw;
        }
    }

    private List<int> GetUserPermissionIds(ClaimsPrincipal user)
    {
        // TODO: Leer desde claims cuando esté implementado
        // Por ahora simular según roles
        
        var permissions = new List<int>();

        if (user.IsInRole("Admin"))
            permissions.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        else if (user.IsInRole("Gerente"))
            permissions.AddRange(new[] { 2, 3, 5 });
        else if (user.IsInRole("Coordinador"))
            permissions.AddRange(new[] { 3, 4, 6 });

        return permissions;
    }
}
```

---

## ? **TESTING Y VERIFICACIÓN**

### **1. Verificar Tablas en SQL Server**

```sql
-- Contar módulos
SELECT COUNT(*) AS TotalModulos FROM Modulos;

-- Contar componentes
SELECT COUNT(*) AS TotalComponentes FROM ModuloComponentes;

-- Contar acciones
SELECT COUNT(*) AS TotalAcciones FROM AccionesGranulares;

-- Ver jerarquía de Finanzas
SELECT 
    c.IdComponent,
    c.Name,
    c.IdParent,
    CASE WHEN c.IdParent IS NULL THEN 'Raíz' ELSE 'Hijo' END AS Tipo
FROM ModuloComponentes c
INNER JOIN Modulos m ON c.IdModule = m.IdModule
WHERE m.Codigo = 'Finanzas'
ORDER BY c.IdParent, c.MenuOrder;
```

### **2. Probar Aplicación**

```bash
# Ejecutar aplicación
dotnet run --project src\Host\VRM_PluginDemo.Blazor.Server
```

**Verificaciones:**
1. ? Login con usuario Admin
2. ? Sidebar muestra módulos desde BD
3. ? Dropdown de Finanzas funciona
4. ? Navegación a Facturas funciona
5. ? Logs de Serilog muestran queries EF

### **3. Verificar Logs**

```bash
# Ver logs en tiempo real
tail -f logs/dev/vrm-20250110.log

# Buscar queries EF
grep "SELECT" logs/dev/vrm-20250110.log
```

---

## ?? **TROUBLESHOOTING**

### **Error: "Cannot connect to database"**

```bash
# Verificar SQL Server está corriendo
Get-Service MSSQLSERVER

# Iniciar si está detenido
Start-Service MSSQLSERVER

# Verificar connection string
Server=.;Database=VRM_Net;Trusted_Connection=True;TrustServerCertificate=True
```

### **Error: "Migration already applied"**

```bash
# Ver migrations aplicadas
dotnet ef migrations list --project src\Infrastructure\VRM_Plugin.Infrastructure

# Revertir última migration
dotnet ef database update PreviousMigrationName --project src\Infrastructure\VRM_Plugin.Infrastructure

# Eliminar migration
dotnet ef migrations remove --project src\Infrastructure\VRM_Plugin.Infrastructure
```

### **Error: "Duplicate key in seed data"**

```bash
# Limpiar tablas
DELETE FROM AccionesGranulares;
DELETE FROM ModuloComponentes;
DELETE FROM Modulos;
DBCC CHECKIDENT ('Modulos', RESEED, 0);
DBCC CHECKIDENT ('ModuloComponentes', RESEED, 0);
DBCC CHECKIDENT ('AccionesGranulares', RESEED, 0);

# Ejecutar seed nuevamente
```

---

## ?? **PRÓXIMOS PASOS**

Después de completar esta guía:

1. ? **Fase 2 Completada:** Infrastructure Layer implementado
2. ?? **Siguiente:** Implementar gestión de Permisos en BD
3. ?? **Siguiente:** UI para administrar módulos dinámicamente
4. ?? **Siguiente:** Multi-tenancy (habilitar módulos por cliente)

---

## ?? **REFERENCIAS**

- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)

---

**Última actualización:** Enero 10, 2025  
**Autor:** Equipo VRM_Net  
**Estado:** ?? GUÍA COMPLETA PARA IMPLEMENTACIÓN
