# ??? ESTRUCTURA DE BASE DE DATOS - VRM_Net Sistema de Módulos

**Fecha:** Enero 10, 2025  
**Versión:** 1.0.0  
**Motor:** SQL Server 2019+  
**Estado:** ? LISTO PARA IMPLEMENTACIÓN

---

## ?? **TABLA DE CONTENIDOS**

1. [Visión General](#visión-general)
2. [Diagrama ER](#diagrama-er)
3. [Tablas del Sistema](#tablas-del-sistema)
4. [Scripts SQL Completos](#scripts-sql-completos)
5. [Datos Iniciales (Seed)](#datos-iniciales-seed)
6. [Índices y Constraints](#índices-y-constraints)
7. [Vistas Útiles](#vistas-útiles)
8. [Stored Procedures](#stored-procedures)
9. [Queries de Ejemplo](#queries-de-ejemplo)
10. [Migración desde Código](#migración-desde-código)

---

## ?? **VISIÓN GENERAL**

### **Propósito de la Base de Datos**

Esta base de datos almacena la configuración dinámica del sistema de módulos VRM_Net, permitiendo:

- ? **Gestión dinámica** de módulos sin recompilar
- ? **Permisos granulares** por navegación y por acción
- ? **Jerarquía de componentes** (menú multinivel)
- ? **Auditoría completa** de cambios
- ? **Multi-tenancy** (módulos por cliente)
- ? **Soft delete** (no borrado físico)

### **Tablas Principales**

| Tabla | Propósito | Registros Estimados |
|-------|-----------|---------------------|
| `Modulos` | Módulos del sistema (Finanzas, Prospectos) | 10-50 |
| `ModuloComponentes` | Componentes de navegación (menú jerárquico) | 50-500 |
| `AccionesGranulares` | Acciones de negocio (crear, editar, etc.) | 100-1000 |
| `TiposAccion` | Catálogo de tipos (Lectura, Escritura, Crítica) | 3 |
| `Permisos` | Permisos del sistema | 50-200 |
| `ComponentePermisos` | Relación Componente-Permiso (Many-to-Many) | 200-2000 |
| `AccionPermisos` | Relación Acción-Permiso (Many-to-Many) | 500-5000 |
| `Usuarios` | Usuarios del sistema | 100-10000 |
| `UsuarioPermisos` | Relación Usuario-Permiso (Many-to-Many) | 500-50000 |
| `Clientes` | Clientes del sistema (Multi-tenancy) | 10-1000 |
| `ClienteModulos` | Módulos habilitados por cliente | 50-5000 |

---

## ?? **DIAGRAMA ER**

```
???????????????????
?   Clientes      ?
?                 ?
? IdCliente (PK)  ?
? RazonSocial     ?
? RFC             ?
? IsActive        ?
???????????????????
         ?
         ? 1:N
         ?
         ?
???????????????????         ???????????????????
? ClienteModulos  ???????????    Modulos      ?
?                 ?   N:1   ?                 ?
? IdCliente (FK)  ?         ? IdModule (PK)   ?
? IdModule (FK)   ?         ? Codigo          ?
? FechaHabilitado ?         ? Nombre          ?
???????????????????         ? Descripcion     ?
                            ? Version         ?
                            ? IsActive        ?
                            ? CreatedAt       ?
                            ? UpdatedAt       ?
                            ???????????????????
                                     ?
                                     ? 1:N
                                     ?
                                     ?
                            ???????????????????????
                            ? ModuloComponentes   ???????
                            ?                     ?     ?
                            ? IdComponent (PK)    ?     ?
                            ? IdModule (FK)       ?     ?
                            ? IdParent (FK) ????????????? Auto-referencia
                            ? ComponentCode       ?
                            ? Name                ?
                            ? Route               ?
                            ? Icon                ?
                            ? ShowInMenu          ?
                            ? MenuOrder           ?
                            ? IsActive            ?
                            ? CreatedAt           ?
                            ???????????????????????
                                     ?
                                     ? 1:N
                                     ?
                                     ?
                            ???????????????????????         ????????????????
                            ? AccionesGranulares  ??????????? TiposAccion  ?
                            ?                     ?   N:1   ?              ?
                            ? IdAction (PK)       ?         ? IdActionType ?
                            ? IdComponent (FK)    ?         ? Codigo       ?
                            ? IdActionType (FK)   ?         ? Nombre       ?
                            ? ActionKey           ?         ? Descripcion  ?
                            ? Name                ?         ? Orden        ?
                            ? Description         ?         ????????????????
                            ? IsActive            ?
                            ? CreatedAt           ?
                            ???????????????????????
                                     ?
                                     ? N:M
                                     ?
                                     ?
                            ???????????????????????
                            ?   AccionPermisos    ?
                            ?                     ?
                            ? IdAction (FK)       ?
                            ? IdPermission (FK)   ?
                            ???????????????????????
                                     ?
                                     ? N:1
                                     ?
                                     ?
                            ???????????????????????
                            ?     Permisos        ?
                            ?                     ?
                            ? IdPermission (PK)   ?
                            ? Codigo              ?
                            ? Nombre              ?
                            ? Descripcion         ?
                            ? IsActive            ?
                            ???????????????????????
                                     ?
                                     ? N:M
                                     ?
                                     ?
                            ???????????????????????
                            ?  UsuarioPermisos    ?
                            ?                     ?
                            ? IdUser (FK)         ?
                            ? IdPermission (FK)   ?
                            ? FechaAsignacion     ?
                            ???????????????????????
                                     ?
                                     ? N:1
                                     ?
                                     ?
                            ???????????????????????
                            ?     Usuarios        ?
                            ?                     ?
                            ? IdUser (PK)         ?
                            ? Username            ?
                            ? Email               ?
                            ? PasswordHash        ?
                            ? IsActive            ?
                            ???????????????????????
```

---

## ??? **TABLAS DEL SISTEMA**

### **1. Modulos**

Almacena los módulos del sistema (Finanzas, Prospectos, Inventario, etc.)

```sql
CREATE TABLE Modulos (
    IdModule INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(100) NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Version NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    CONSTRAINT PK_Modulos PRIMARY KEY CLUSTERED (IdModule ASC),
    CONSTRAINT UQ_Modulos_Codigo UNIQUE (Codigo)
);
```

**Columnas:**
- `IdModule`: ID único auto-incremental (PK)
- `Codigo`: Código técnico del módulo (ej: "Finanzas") - UNIQUE
- `Nombre`: Nombre para mostrar en UI (ej: "Gestión de Finanzas")
- `Descripcion`: Descripción del módulo
- `Version`: Versión del módulo (ej: "1.0.0")
- `IsActive`: Soft delete (1 = activo, 0 = inactivo)
- `CreatedAt/UpdatedAt`: Auditoría de fechas
- `CreatedBy/UpdatedBy`: Auditoría de usuarios (FK a Usuarios)

**Ejemplo:**
```sql
INSERT INTO Modulos (Codigo, Nombre, Descripcion, Version)
VALUES ('Finanzas', 'Gestión de Finanzas', 'Módulo para operaciones financieras', '1.0.0');
```

---

### **2. ModuloComponentes**

Almacena los componentes de navegación con jerarquía (menú multinivel)

```sql
CREATE TABLE ModuloComponentes (
    IdComponent INT IDENTITY(1,1) NOT NULL,
    IdModule INT NOT NULL,
    IdParent INT NULL,
    ComponentCode NVARCHAR(200) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    Route NVARCHAR(500) NULL,
    Icon NVARCHAR(100) NULL,
    ShowInMenu BIT NOT NULL DEFAULT 1,
    MenuOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    CONSTRAINT PK_ModuloComponentes PRIMARY KEY CLUSTERED (IdComponent ASC),
    CONSTRAINT UQ_ModuloComponentes_ComponentCode UNIQUE (ComponentCode),
    CONSTRAINT FK_ModuloComponentes_Modulos FOREIGN KEY (IdModule) 
        REFERENCES Modulos(IdModule) ON DELETE NO ACTION,
    CONSTRAINT FK_ModuloComponentes_Parent FOREIGN KEY (IdParent) 
        REFERENCES ModuloComponentes(IdComponent) ON DELETE NO ACTION
);
```

**Columnas:**
- `IdComponent`: ID único auto-incremental (PK)
- `IdModule`: ID del módulo padre (FK)
- `IdParent`: ID del componente padre (NULL = raíz) - Auto-referencia
- `ComponentCode`: Código técnico (ej: "Finanzas.Facturas") - UNIQUE
- `Name`: Nombre para mostrar en menú
- `Description`: Descripción opcional
- `Route`: Ruta Blazor (ej: "/finanzas/facturas") - NULL si es solo categoría
- `Icon`: Clase CSS del icono (Remix Icons)
- `ShowInMenu`: Mostrar en menú (1) o oculto (0)
- `MenuOrder`: Orden de aparición en menú
- `IsActive`: Soft delete

**Jerarquía:**
```sql
-- Raíz (Categoría)
INSERT INTO ModuloComponentes (IdModule, IdParent, ComponentCode, Name, Route, Icon, MenuOrder)
VALUES (1, NULL, 'Finanzas.Root', 'Finanzas', '', 'ri-money-dollar-circle-line', 20);

-- Hijo
INSERT INTO ModuloComponentes (IdModule, IdParent, ComponentCode, Name, Route, Icon, MenuOrder)
VALUES (1, 1, 'Finanzas.Facturas', 'Facturas', '/finanzas/facturas', 'ri-file-list-3-line', 1);
```

---

### **3. TiposAccion**

Catálogo de tipos de acción (Lectura, Escritura, Crítica)

```sql
CREATE TABLE TiposAccion (
    IdActionType INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Orden INT NOT NULL,
    CONSTRAINT PK_TiposAccion PRIMARY KEY CLUSTERED (IdActionType ASC),
    CONSTRAINT UQ_TiposAccion_Codigo UNIQUE (Codigo)
);

-- Datos iniciales (Seed Data)
INSERT INTO TiposAccion (Codigo, Nombre, Descripcion, Orden) VALUES
('Lectura', 'Lectura', 'Operaciones de consulta (ver, listar, buscar)', 1),
('Escritura', 'Escritura', 'Operaciones de modificación (crear, editar, actualizar)', 2),
('Critica', 'Crítica', 'Operaciones sensibles (eliminar, aprobar, timbrar)', 3);
```

**Columnas:**
- `IdActionType`: ID único (PK)
- `Codigo`: Código del tipo (ej: "Lectura")
- `Nombre`: Nombre para UI
- `Descripcion`: Descripción del tipo
- `Orden`: Nivel de severidad (1=Lectura < 2=Escritura < 3=Crítica)

---

### **4. AccionesGranulares**

Almacena las acciones de negocio (crear, editar, eliminar, etc.)

```sql
CREATE TABLE AccionesGranulares (
    IdAction INT IDENTITY(1,1) NOT NULL,
    IdComponent INT NULL,
    IdActionType INT NOT NULL,
    ActionKey NVARCHAR(200) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    CONSTRAINT PK_AccionesGranulares PRIMARY KEY CLUSTERED (IdAction ASC),
    CONSTRAINT UQ_AccionesGranulares_ActionKey UNIQUE (ActionKey),
    CONSTRAINT FK_AccionesGranulares_Componentes FOREIGN KEY (IdComponent) 
        REFERENCES ModuloComponentes(IdComponent) ON DELETE NO ACTION,
    CONSTRAINT FK_AccionesGranulares_TiposAccion FOREIGN KEY (IdActionType) 
        REFERENCES TiposAccion(IdActionType) ON DELETE NO ACTION
);
```

**Columnas:**
- `IdAction`: ID único auto-incremental (PK)
- `IdComponent`: ID del componente (NULL = acción global del módulo)
- `IdActionType`: Tipo de acción (1=Lectura, 2=Escritura, 3=Crítica)
- `ActionKey`: Clave única (ej: "Finanzas.Facturas.TimbrarSAT") - UNIQUE
- `Name`: Nombre descriptivo (ej: "Timbrar en SAT")
- `Description`: Descripción de la acción
- `IsActive`: Soft delete

**Ejemplo:**
```sql
INSERT INTO AccionesGranulares (IdComponent, IdActionType, ActionKey, Name, Description)
VALUES (2, 3, 'Finanzas.Facturas.TimbrarSAT', 'Timbrar en SAT', 
        'Envía factura al SAT para timbrado fiscal');
```

---

### **5. Permisos**

Catálogo de permisos del sistema

```sql
CREATE TABLE Permisos (
    IdPermission INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(100) NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Permisos PRIMARY KEY CLUSTERED (IdPermission ASC),
    CONSTRAINT UQ_Permisos_Codigo UNIQUE (Codigo)
);
```

**Ejemplo de Permisos:**
```sql
INSERT INTO Permisos (Codigo, Nombre, Descripcion) VALUES
('Admin', 'Administrador', 'Acceso completo al sistema'),
('GerenteFinanzas', 'Gerente de Finanzas', 'Gerente del módulo de finanzas'),
('CoordinadorFinanzas', 'Coordinador de Finanzas', 'Coordinador de finanzas'),
('Contador', 'Contador', 'Personal contable'),
('GestorProspectos', 'Gestor de Prospectos', 'Gestor de prospectos'),
('RevisorLegal', 'Revisor Legal', 'Revisor legal de prospectos');
```

---

### **6. ComponentePermisos**

Relación Many-to-Many: ModuloComponentes ? Permisos

```sql
CREATE TABLE ComponentePermisos (
    IdComponent INT NOT NULL,
    IdPermission INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ComponentePermisos PRIMARY KEY CLUSTERED (IdComponent, IdPermission),
    CONSTRAINT FK_ComponentePermisos_Componente FOREIGN KEY (IdComponent) 
        REFERENCES ModuloComponentes(IdComponent) ON DELETE CASCADE,
    CONSTRAINT FK_ComponentePermisos_Permission FOREIGN KEY (IdPermission) 
        REFERENCES Permisos(IdPermission) ON DELETE CASCADE
);
```

**Ejemplo:**
```sql
-- Facturas visible para Admin, Gerente y Coordinador
INSERT INTO ComponentePermisos (IdComponent, IdPermission)
VALUES 
    (2, 1),  -- Admin
    (2, 2),  -- Gerente Finanzas
    (2, 3);  -- Coordinador Finanzas
```

---

### **7. AccionPermisos**

Relación Many-to-Many: AccionesGranulares ? Permisos

```sql
CREATE TABLE AccionPermisos (
    IdAction INT NOT NULL,
    IdPermission INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AccionPermisos PRIMARY KEY CLUSTERED (IdAction, IdPermission),
    CONSTRAINT FK_AccionPermisos_Accion FOREIGN KEY (IdAction) 
        REFERENCES AccionesGranulares(IdAction) ON DELETE CASCADE,
    CONSTRAINT FK_AccionPermisos_Permission FOREIGN KEY (IdPermission) 
        REFERENCES Permisos(IdPermission) ON DELETE CASCADE
);
```

**Ejemplo:**
```sql
-- Timbrar SAT solo para Admin y Gerente
INSERT INTO AccionPermisos (IdAction, IdPermission)
VALUES 
    (5, 1),  -- Admin
    (5, 2);  -- Gerente Finanzas
```

---

### **8. Usuarios**

Usuarios del sistema

```sql
CREATE TABLE Usuarios (
    IdUser INT IDENTITY(1,1) NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Nombre NVARCHAR(200) NULL,
    Apellido NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    LastLogin DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (IdUser ASC),
    CONSTRAINT UQ_Usuarios_Username UNIQUE (Username),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email)
);
```

---

### **9. UsuarioPermisos**

Relación Many-to-Many: Usuarios ? Permisos

```sql
CREATE TABLE UsuarioPermisos (
    IdUser INT NOT NULL,
    IdPermission INT NOT NULL,
    FechaAsignacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AsignadoPor INT NULL,
    CONSTRAINT PK_UsuarioPermisos PRIMARY KEY CLUSTERED (IdUser, IdPermission),
    CONSTRAINT FK_UsuarioPermisos_Usuario FOREIGN KEY (IdUser) 
        REFERENCES Usuarios(IdUser) ON DELETE CASCADE,
    CONSTRAINT FK_UsuarioPermisos_Permission FOREIGN KEY (IdPermission) 
        REFERENCES Permisos(IdPermission) ON DELETE CASCADE,
    CONSTRAINT FK_UsuarioPermisos_AsignadoPor FOREIGN KEY (AsignadoPor) 
        REFERENCES Usuarios(IdUser) ON DELETE NO ACTION
);
```

---

### **10. Clientes**

Clientes del sistema (Multi-tenancy)

```sql
CREATE TABLE Clientes (
    IdCliente INT IDENTITY(1,1) NOT NULL,
    RazonSocial NVARCHAR(300) NOT NULL,
    NombreComercial NVARCHAR(200) NULL,
    RFC NVARCHAR(13) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    Telefono NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FechaAlta DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Clientes PRIMARY KEY CLUSTERED (IdCliente ASC),
    CONSTRAINT UQ_Clientes_RFC UNIQUE (RFC)
);
```

---

### **11. ClienteModulos**

Módulos habilitados por cliente (Multi-tenancy)

```sql
CREATE TABLE ClienteModulos (
    IdCliente INT NOT NULL,
    IdModule INT NOT NULL,
    FechaHabilitado DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaVencimiento DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    HabilitadoPor INT NULL,
    CONSTRAINT PK_ClienteModulos PRIMARY KEY CLUSTERED (IdCliente, IdModule),
    CONSTRAINT FK_ClienteModulos_Cliente FOREIGN KEY (IdCliente) 
        REFERENCES Clientes(IdCliente) ON DELETE CASCADE,
    CONSTRAINT FK_ClienteModulos_Modulo FOREIGN KEY (IdModule) 
        REFERENCES Modulos(IdModule) ON DELETE CASCADE,
    CONSTRAINT FK_ClienteModulos_HabilitadoPor FOREIGN KEY (HabilitadoPor) 
        REFERENCES Usuarios(IdUser) ON DELETE NO ACTION
);
```

---

## ?? **SCRIPTS SQL COMPLETOS**

### **Script 1: Crear Base de Datos**

```sql
-- ==================== CREAR BASE DE DATOS ====================
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'VRM_Net')
BEGIN
    CREATE DATABASE VRM_Net
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT '? Base de datos VRM_Net creada';
END
ELSE
BEGIN
    PRINT '?? Base de datos VRM_Net ya existe';
END
GO

USE VRM_Net;
GO
```

### **Script 2: Crear Todas las Tablas**

```sql
-- ==================== CREAR TABLAS ====================
USE VRM_Net;
GO

-- 1. TiposAccion (Catálogo)
CREATE TABLE TiposAccion (
    IdActionType INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Orden INT NOT NULL,
    CONSTRAINT PK_TiposAccion PRIMARY KEY CLUSTERED (IdActionType ASC),
    CONSTRAINT UQ_TiposAccion_Codigo UNIQUE (Codigo)
);

-- 2. Modulos
CREATE TABLE Modulos (
    IdModule INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(100) NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Version NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    CONSTRAINT PK_Modulos PRIMARY KEY CLUSTERED (IdModule ASC),
    CONSTRAINT UQ_Modulos_Codigo UNIQUE (Codigo)
);

-- 3. ModuloComponentes
CREATE TABLE ModuloComponentes (
    IdComponent INT IDENTITY(1,1) NOT NULL,
    IdModule INT NOT NULL,
    IdParent INT NULL,
    ComponentCode NVARCHAR(200) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    Route NVARCHAR(500) NULL,
    Icon NVARCHAR(100) NULL,
    ShowInMenu BIT NOT NULL DEFAULT 1,
    MenuOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    CONSTRAINT PK_ModuloComponentes PRIMARY KEY CLUSTERED (IdComponent ASC),
    CONSTRAINT UQ_ModuloComponentes_ComponentCode UNIQUE (ComponentCode),
    CONSTRAINT FK_ModuloComponentes_Modulos FOREIGN KEY (IdModule) 
        REFERENCES Modulos(IdModule),
    CONSTRAINT FK_ModuloComponentes_Parent FOREIGN KEY (IdParent) 
        REFERENCES ModuloComponentes(IdComponent)
);

-- 4. AccionesGranulares
CREATE TABLE AccionesGranulares (
    IdAction INT IDENTITY(1,1) NOT NULL,
    IdComponent INT NULL,
    IdActionType INT NOT NULL,
    ActionKey NVARCHAR(200) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    CONSTRAINT PK_AccionesGranulares PRIMARY KEY CLUSTERED (IdAction ASC),
    CONSTRAINT UQ_AccionesGranulares_ActionKey UNIQUE (ActionKey),
    CONSTRAINT FK_AccionesGranulares_Componentes FOREIGN KEY (IdComponent) 
        REFERENCES ModuloComponentes(IdComponent),
    CONSTRAINT FK_AccionesGranulares_TiposAccion FOREIGN KEY (IdActionType) 
        REFERENCES TiposAccion(IdActionType)
);

-- 5. Permisos
CREATE TABLE Permisos (
    IdPermission INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(100) NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Permisos PRIMARY KEY CLUSTERED (IdPermission ASC),
    CONSTRAINT UQ_Permisos_Codigo UNIQUE (Codigo)
);

-- 6. ComponentePermisos (Many-to-Many)
CREATE TABLE ComponentePermisos (
    IdComponent INT NOT NULL,
    IdPermission INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ComponentePermisos PRIMARY KEY CLUSTERED (IdComponent, IdPermission),
    CONSTRAINT FK_ComponentePermisos_Componente FOREIGN KEY (IdComponent) 
        REFERENCES ModuloComponentes(IdComponent) ON DELETE CASCADE,
    CONSTRAINT FK_ComponentePermisos_Permission FOREIGN KEY (IdPermission) 
        REFERENCES Permisos(IdPermission) ON DELETE CASCADE
);

-- 7. AccionPermisos (Many-to-Many)
CREATE TABLE AccionPermisos (
    IdAction INT NOT NULL,
    IdPermission INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AccionPermisos PRIMARY KEY CLUSTERED (IdAction, IdPermission),
    CONSTRAINT FK_AccionPermisos_Accion FOREIGN KEY (IdAction) 
        REFERENCES AccionesGranulares(IdAction) ON DELETE CASCADE,
    CONSTRAINT FK_AccionPermisos_Permission FOREIGN KEY (IdPermission) 
        REFERENCES Permisos(IdPermission) ON DELETE CASCADE
);

-- 8. Usuarios
CREATE TABLE Usuarios (
    IdUser INT IDENTITY(1,1) NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Nombre NVARCHAR(200) NULL,
    Apellido NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    LastLogin DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (IdUser ASC),
    CONSTRAINT UQ_Usuarios_Username UNIQUE (Username),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email)
);

-- 9. UsuarioPermisos (Many-to-Many)
CREATE TABLE UsuarioPermisos (
    IdUser INT NOT NULL,
    IdPermission INT NOT NULL,
    FechaAsignacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AsignadoPor INT NULL,
    CONSTRAINT PK_UsuarioPermisos PRIMARY KEY CLUSTERED (IdUser, IdPermission),
    CONSTRAINT FK_UsuarioPermisos_Usuario FOREIGN KEY (IdUser) 
        REFERENCES Usuarios(IdUser) ON DELETE CASCADE,
    CONSTRAINT FK_UsuarioPermisos_Permission FOREIGN KEY (IdPermission) 
        REFERENCES Permisos(IdPermission) ON DELETE CASCADE,
    CONSTRAINT FK_UsuarioPermisos_AsignadoPor FOREIGN KEY (AsignadoPor) 
        REFERENCES Usuarios(IdUser)
);

-- 10. Clientes
CREATE TABLE Clientes (
    IdCliente INT IDENTITY(1,1) NOT NULL,
    RazonSocial NVARCHAR(300) NOT NULL,
    NombreComercial NVARCHAR(200) NULL,
    RFC NVARCHAR(13) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    Telefono NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FechaAlta DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Clientes PRIMARY KEY CLUSTERED (IdCliente ASC),
    CONSTRAINT UQ_Clientes_RFC UNIQUE (RFC)
);

-- 11. ClienteModulos (Many-to-Many)
CREATE TABLE ClienteModulos (
    IdCliente INT NOT NULL,
    IdModule INT NOT NULL,
    FechaHabilitado DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaVencimiento DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    HabilitadoPor INT NULL,
    CONSTRAINT PK_ClienteModulos PRIMARY KEY CLUSTERED (IdCliente, IdModule),
    CONSTRAINT FK_ClienteModulos_Cliente FOREIGN KEY (IdCliente) 
        REFERENCES Clientes(IdCliente) ON DELETE CASCADE,
    CONSTRAINT FK_ClienteModulos_Modulo FOREIGN KEY (IdModule) 
        REFERENCES Modulos(IdModule) ON DELETE CASCADE,
    CONSTRAINT FK_ClienteModulos_HabilitadoPor FOREIGN KEY (HabilitadoPor) 
        REFERENCES Usuarios(IdUser)
);

PRINT '? Todas las tablas creadas exitosamente';
GO
```

### **Script 3: Crear Índices**

```sql
-- ==================== ÍNDICES ====================
USE VRM_Net;
GO

-- Índices para ModuloComponentes
CREATE NONCLUSTERED INDEX IX_ModuloComponentes_IdModule 
    ON ModuloComponentes(IdModule) INCLUDE (IsActive);

CREATE NONCLUSTERED INDEX IX_ModuloComponentes_IdParent 
    ON ModuloComponentes(IdParent) INCLUDE (MenuOrder, IsActive);

CREATE NONCLUSTERED INDEX IX_ModuloComponentes_ShowInMenu 
    ON ModuloComponentes(ShowInMenu, IsActive) INCLUDE (MenuOrder, Name);

-- Índices para AccionesGranulares
CREATE NONCLUSTERED INDEX IX_AccionesGranulares_IdComponent 
    ON AccionesGranulares(IdComponent) INCLUDE (IsActive);

CREATE NONCLUSTERED INDEX IX_AccionesGranulares_IdActionType 
    ON AccionesGranulares(IdActionType);

-- Índices para ComponentePermisos
CREATE NONCLUSTERED INDEX IX_ComponentePermisos_IdPermission 
    ON ComponentePermisos(IdPermission);

-- Índices para AccionPermisos
CREATE NONCLUSTERED INDEX IX_AccionPermisos_IdPermission 
    ON AccionPermisos(IdPermission);

-- Índices para UsuarioPermisos
CREATE NONCLUSTERED INDEX IX_UsuarioPermisos_IdPermission 
    ON UsuarioPermisos(IdPermission);

CREATE NONCLUSTERED INDEX IX_UsuarioPermisos_FechaAsignacion 
    ON UsuarioPermisos(FechaAsignacion);

-- Índices para Usuarios
CREATE NONCLUSTERED INDEX IX_Usuarios_IsActive 
    ON Usuarios(IsActive) INCLUDE (Username, Email);

CREATE NONCLUSTERED INDEX IX_Usuarios_LastLogin 
    ON Usuarios(LastLogin DESC);

-- Índices para ClienteModulos
CREATE NONCLUSTERED INDEX IX_ClienteModulos_IdModule 
    ON ClienteModulos(IdModule) INCLUDE (IsActive);

CREATE NONCLUSTERED INDEX IX_ClienteModulos_IsActive 
    ON ClienteModulos(IsActive) INCLUDE (FechaVencimiento);

PRINT '? Índices creados exitosamente';
GO
```

---

## ?? **DATOS INICIALES (SEED)**

### **Script 4: Seed Data Completo**

```sql
-- ==================== SEED DATA ====================
USE VRM_Net;
GO

-- 1. TiposAccion
PRINT 'Insertando TiposAccion...';
INSERT INTO TiposAccion (Codigo, Nombre, Descripcion, Orden) VALUES
('Lectura', 'Lectura', 'Operaciones de consulta (ver, listar, buscar)', 1),
('Escritura', 'Escritura', 'Operaciones de modificación (crear, editar, actualizar)', 2),
('Critica', 'Crítica', 'Operaciones sensibles (eliminar, aprobar, timbrar)', 3);

-- 2. Permisos
PRINT 'Insertando Permisos...';
INSERT INTO Permisos (Codigo, Nombre, Descripcion) VALUES
('Admin', 'Administrador', 'Acceso completo al sistema'),
('GerenteFinanzas', 'Gerente de Finanzas', 'Gerente del módulo de finanzas'),
('CoordinadorFinanzas', 'Coordinador de Finanzas', 'Coordinador de finanzas'),
('Contador', 'Contador', 'Personal contable'),
('GestorProspectos', 'Gestor de Prospectos', 'Gestor de prospectos'),
('CoordinadorProspectos', 'Coordinador de Prospectos', 'Coordinador de prospectos'),
('RevisorLegal', 'Revisor Legal', 'Revisor legal de prospectos'),
('RevisorFinanciero', 'Revisor Financiero', 'Revisor financiero de prospectos'),
('RevisorTecnico', 'Revisor Técnico', 'Revisor técnico de prospectos');

-- 3. Módulo Finanzas
PRINT 'Insertando Módulo Finanzas...';
INSERT INTO Modulos (Codigo, Nombre, Descripcion, Version)
VALUES ('Finanzas', 'Gestión de Finanzas', 
        'Módulo para gestionar operaciones financieras. Incluye facturas, pagos, conciliaciones y cuentas por pagar.', 
        '1.0.0');

DECLARE @IdModuloFinanzas INT = SCOPE_IDENTITY();

-- Componente raíz: Finanzas
INSERT INTO ModuloComponentes (IdModule, IdParent, ComponentCode, Name, Description, Route, Icon, ShowInMenu, MenuOrder)
VALUES (@IdModuloFinanzas, NULL, 'Finanzas.Root', 'Finanzas', 'Módulo principal de finanzas', '', 
        'ri-money-dollar-circle-line', 1, 20);

DECLARE @IdFinanzasRoot INT = SCOPE_IDENTITY();

-- Componente hijo: Facturas
INSERT INTO ModuloComponentes (IdModule, IdParent, ComponentCode, Name, Description, Route, Icon, ShowInMenu, MenuOrder)
VALUES (@IdModuloFinanzas, @IdFinanzasRoot, 'Finanzas.Facturas', 'Facturas', 'Gestión de facturas', 
        '/finanzas/facturas', 'ri-file-list-3-line', 1, 1);

DECLARE @IdFacturas INT = SCOPE_IDENTITY();

-- Componente hijo: Cobros y Pagos
INSERT INTO ModuloComponentes (IdModule, IdParent, ComponentCode, Name, Description, Route, Icon, ShowInMenu, MenuOrder)
VALUES (@IdModuloFinanzas, @IdFinanzasRoot, 'Finanzas.CobrosYPagos', 'Cobros y Pagos', 'Gestión de cobros y pagos', 
        '/finanzas/cobros-pagos', 'ri-exchange-dollar-line', 1, 2);

DECLARE @IdCobrosYPagos INT = SCOPE_IDENTITY();

-- Acciones de Facturas
INSERT INTO AccionesGranulares (IdComponent, IdActionType, ActionKey, Name, Description) VALUES
(@IdFacturas, 1, 'Finanzas.Facturas.Ver', 'Ver Facturas', 'Permite visualizar el listado de facturas'),
(@IdFacturas, 2, 'Finanzas.Facturas.Crear', 'Crear Factura', 'Permite crear nuevas facturas'),
(@IdFacturas, 2, 'Finanzas.Facturas.Editar', 'Editar Factura', 'Permite modificar facturas existentes'),
(@IdFacturas, 3, 'Finanzas.Facturas.Eliminar', 'Eliminar Factura', 'Permite eliminar facturas'),
(@IdFacturas, 3, 'Finanzas.Facturas.TimbrarSAT', 'Timbrar en SAT', 'Envía factura al SAT para timbrado fiscal');

-- Acciones de Cobros y Pagos
INSERT INTO AccionesGranulares (IdComponent, IdActionType, ActionKey, Name, Description) VALUES
(@IdCobrosYPagos, 1, 'Finanzas.Pagos.Ver', 'Ver Pagos', 'Permite visualizar pagos'),
(@IdCobrosYPagos, 2, 'Finanzas.Pagos.Crear', 'Crear Pago', 'Permite crear nuevos pagos'),
(@IdCobrosYPagos, 2, 'Finanzas.Pagos.Editar', 'Editar Pago', 'Permite modificar pagos existentes'),
(@IdCobrosYPagos, 3, 'Finanzas.Pagos.Autorizar', 'Autorizar Pago', 'Autoriza un pago para su ejecución'),
(@IdCobrosYPagos, 3, 'Finanzas.Pagos.Cancelar', 'Cancelar Pago', 'Cancela un pago autorizado');

-- 4. Módulo Prospectos
PRINT 'Insertando Módulo Prospectos...';
INSERT INTO Modulos (Codigo, Nombre, Descripcion, Version)
VALUES ('Prospectos', 'Gestión de Prospectos', 
        'Módulo para gestionar solicitudes de proveedores. Permite recibir, revisar y aprobar empresas que desean ser proveedores.', 
        '1.0.0');

DECLARE @IdModuloProspectos INT = SCOPE_IDENTITY();

-- Componente raíz: Prospectos
INSERT INTO ModuloComponentes (IdModule, IdParent, ComponentCode, Name, Description, Route, Icon, ShowInMenu, MenuOrder)
VALUES (@IdModuloProspectos, NULL, 'Prospectos.Root', 'Prospectos', 'Gestión de solicitudes de proveedores', 
        '/prospectos', 'ri-list-check-3', 1, 10);

DECLARE @IdProspectosRoot INT = SCOPE_IDENTITY();

-- Acciones de Prospectos
INSERT INTO AccionesGranulares (IdComponent, IdActionType, ActionKey, Name, Description) VALUES
(@IdProspectosRoot, 1, 'Prospectos.Ver', 'Ver Prospectos', 'Permite visualizar prospectos'),
(@IdProspectosRoot, 2, 'Prospectos.Crear', 'Crear Prospecto', 'Permite crear nuevos prospectos'),
(@IdProspectosRoot, 2, 'Prospectos.Editar', 'Editar Prospecto', 'Permite modificar prospectos'),
(@IdProspectosRoot, 3, 'Prospectos.Eliminar', 'Eliminar Prospecto', 'Permite eliminar prospectos'),
(@IdProspectosRoot, 2, 'Prospectos.AsignarRevisor', 'Asignar Revisor', 'Asigna un revisor a un prospecto'),
(@IdProspectosRoot, 2, 'Prospectos.RevisionLegal', 'Revisión Legal', 'Realiza revisión legal del prospecto'),
(@IdProspectosRoot, 2, 'Prospectos.RevisionFinanciera', 'Revisión Financiera', 'Realiza revisión financiera'),
(@IdProspectosRoot, 2, 'Prospectos.RevisionTecnica', 'Revisión Técnica', 'Realiza revisión técnica'),
(@IdProspectosRoot, 3, 'Prospectos.AprobarFinal', 'Aprobar Prospecto', 'Aprobación final del prospecto'),
(@IdProspectosRoot, 3, 'Prospectos.RechazarFinal', 'Rechazar Prospecto', 'Rechazo final del prospecto'),
(@IdProspectosRoot, 3, 'Prospectos.ConvertirProveedor', 'Convertir a Proveedor', 'Convierte prospecto aprobado en proveedor');

-- 5. Usuario Admin inicial
PRINT 'Insertando Usuario Admin...';
INSERT INTO Usuarios (Username, Email, PasswordHash, Nombre, Apellido, EmailConfirmed)
VALUES ('admin', 'admin@vrm.com', 
        -- TODO: Hash real cuando se implemente autenticación
        'TEMP_HASH_REPLACE_IN_PRODUCTION', 
        'Administrador', 'Sistema', 1);

DECLARE @IdUserAdmin INT = SCOPE_IDENTITY();

-- Asignar permiso Admin
INSERT INTO UsuarioPermisos (IdUser, IdPermission)
VALUES (@IdUserAdmin, 1);  -- Admin

-- 6. Cliente Demo
PRINT 'Insertando Cliente Demo...';
INSERT INTO Clientes (RazonSocial, NombreComercial, RFC, Email, Telefono)
VALUES ('Empresa Demo S.A. de C.V.', 'Demo Corp', 'XAXX010101000', 'contacto@demo.com', '5555555555');

DECLARE @IdClienteDemo INT = SCOPE_IDENTITY();

-- Habilitar módulos para cliente demo
INSERT INTO ClienteModulos (IdCliente, IdModule, HabilitadoPor)
VALUES 
    (@IdClienteDemo, @IdModuloFinanzas, @IdUserAdmin),
    (@IdClienteDemo, @IdModuloProspectos, @IdUserAdmin);

PRINT '? Seed data completado exitosamente';

-- Mostrar resumen
SELECT 
    (SELECT COUNT(*) FROM Modulos) AS Modulos,
    (SELECT COUNT(*) FROM ModuloComponentes) AS Componentes,
    (SELECT COUNT(*) FROM AccionesGranulares) AS Acciones,
    (SELECT COUNT(*) FROM Permisos) AS Permisos,
    (SELECT COUNT(*) FROM Usuarios) AS Usuarios,
    (SELECT COUNT(*) FROM Clientes) AS Clientes;
GO
```

---

## ?? **VISTAS ÚTILES**

### **Vista 1: Jerarquía de Componentes**

```sql
CREATE VIEW vw_ComponentesJerarquia
AS
SELECT 
    m.Codigo AS ModuloCodigo,
    m.Nombre AS ModuloNombre,
    c.IdComponent,
    c.ComponentCode,
    c.Name AS ComponenteName,
    c.Route,
    c.Icon,
    c.MenuOrder,
    c.IdParent,
    CASE WHEN c.IdParent IS NULL THEN 'Raíz' ELSE 'Hijo' END AS TipoNodo,
    c.IsActive
FROM ModuloComponentes c
INNER JOIN Modulos m ON c.IdModule = m.IdModule
WHERE c.IsActive = 1 AND m.IsActive = 1;
GO

-- Uso:
SELECT * FROM vw_ComponentesJerarquia ORDER BY ModuloCodigo, MenuOrder;
```

### **Vista 2: Acciones con Permisos**

```sql
CREATE VIEW vw_AccionesConPermisos
AS
SELECT 
    m.Nombre AS Modulo,
    c.Name AS Componente,
    a.ActionKey,
    a.Name AS AccionNombre,
    t.Nombre AS TipoAccion,
    t.Orden AS SeveridadOrden,
    STRING_AGG(p.Nombre, ', ') AS PermisosRequeridos,
    a.IsActive
FROM AccionesGranulares a
INNER JOIN TiposAccion t ON a.IdActionType = t.IdActionType
LEFT JOIN ModuloComponentes c ON a.IdComponent = c.IdComponent
LEFT JOIN Modulos m ON c.IdModule = m.IdModule
LEFT JOIN AccionPermisos ap ON a.IdAction = ap.IdAction
LEFT JOIN Permisos p ON ap.IdPermission = p.IdPermission
GROUP BY m.Nombre, c.Name, a.ActionKey, a.Name, t.Nombre, t.Orden, a.IsActive;
GO

-- Uso:
SELECT * FROM vw_AccionesConPermisos ORDER BY Modulo, TipoAccion;
```

### **Vista 3: Usuarios con Permisos**

```sql
CREATE VIEW vw_UsuariosConPermisos
AS
SELECT 
    u.IdUser,
    u.Username,
    u.Email,
    u.Nombre + ' ' + u.Apellido AS NombreCompleto,
    STRING_AGG(p.Nombre, ', ') AS Permisos,
    u.IsActive,
    u.LastLogin
FROM Usuarios u
LEFT JOIN UsuarioPermisos up ON u.IdUser = up.IdUser
LEFT JOIN Permisos p ON up.IdPermission = p.IdPermission
GROUP BY u.IdUser, u.Username, u.Email, u.Nombre, u.Apellido, u.IsActive, u.LastLogin;
GO

-- Uso:
SELECT * FROM vw_UsuariosConPermisos WHERE IsActive = 1;
```

---

## ?? **STORED PROCEDURES**

### **SP 1: Obtener Componentes Visibles para Usuario**

```sql
CREATE PROCEDURE sp_GetComponentesVisiblesPorUsuario
    @IdUser INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Obtener permisos del usuario
    DECLARE @PermisosUsuario TABLE (IdPermission INT);
    INSERT INTO @PermisosUsuario
    SELECT IdPermission FROM UsuarioPermisos WHERE IdUser = @IdUser;

    -- Si el usuario tiene permiso Admin (IdPermission = 1), retornar todos
    IF EXISTS (SELECT 1 FROM @PermisosUsuario WHERE IdPermission = 1)
    BEGIN
        SELECT 
            c.IdComponent,
            c.IdModule,
            c.IdParent,
            c.ComponentCode,
            c.Name,
            c.Route,
            c.Icon,
            c.MenuOrder,
            m.Codigo AS ModuloCodigo,
            m.Nombre AS ModuloNombre
        FROM ModuloComponentes c
        INNER JOIN Modulos m ON c.IdModule = m.IdModule
        WHERE c.IsActive = 1 AND c.ShowInMenu = 1 AND m.IsActive = 1
        ORDER BY c.MenuOrder;
        RETURN;
    END

    -- Filtrar por permisos
    SELECT DISTINCT
        c.IdComponent,
        c.IdModule,
        c.IdParent,
        c.ComponentCode,
        c.Name,
        c.Route,
        c.Icon,
        c.MenuOrder,
        m.Codigo AS ModuloCodigo,
        m.Nombre AS ModuloNombre
    FROM ModuloComponentes c
    INNER JOIN Modulos m ON c.IdModule = m.IdModule
    INNER JOIN ComponentePermisos cp ON c.IdComponent = cp.IdComponent
    INNER JOIN @PermisosUsuario pu ON cp.IdPermission = pu.IdPermission
    WHERE c.IsActive = 1 AND c.ShowInMenu = 1 AND m.IsActive = 1
    ORDER BY c.MenuOrder;
END
GO

-- Uso:
EXEC sp_GetComponentesVisiblesPorUsuario @IdUser = 1;
```

### **SP 2: Verificar Permiso para Acción**

```sql
CREATE PROCEDURE sp_CanUserPerformAction
    @IdUser INT,
    @ActionKey NVARCHAR(200),
    @TienePermiso BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @TienePermiso = 0;

    -- Si es Admin, siempre tiene permiso
    IF EXISTS (
        SELECT 1 FROM UsuarioPermisos 
        WHERE IdUser = @IdUser AND IdPermission = 1
    )
    BEGIN
        SET @TienePermiso = 1;
        RETURN;
    END

    -- Verificar permiso específico
    IF EXISTS (
        SELECT 1
        FROM AccionesGranulares a
        INNER JOIN AccionPermisos ap ON a.IdAction = ap.IdAction
        INNER JOIN UsuarioPermisos up ON ap.IdPermission = up.IdPermission
        WHERE a.ActionKey = @ActionKey 
          AND up.IdUser = @IdUser
          AND a.IsActive = 1
    )
    BEGIN
        SET @TienePermiso = 1;
    END
END
GO

-- Uso:
DECLARE @TienePermiso BIT;
EXEC sp_CanUserPerformAction @IdUser = 1, @ActionKey = 'Finanzas.Facturas.TimbrarSAT', @TienePermiso = @TienePermiso OUTPUT;
SELECT @TienePermiso AS TienePermiso;
```

---

## ?? **QUERIES DE EJEMPLO**

### **Query 1: Ver jerarquía de módulo Finanzas**

```sql
WITH Jerarquia AS (
    -- Nivel 0: Raíz
    SELECT 
        c.IdComponent,
        c.Name,
        c.ComponentCode,
        c.Route,
        c.IdParent,
        0 AS Nivel,
        CAST(c.Name AS NVARCHAR(MAX)) AS Ruta
    FROM ModuloComponentes c
    INNER JOIN Modulos m ON c.IdModule = m.IdModule
    WHERE m.Codigo = 'Finanzas' AND c.IdParent IS NULL
    
    UNION ALL
    
    -- Niveles siguientes
    SELECT 
        c.IdComponent,
        c.Name,
        c.ComponentCode,
        c.Route,
        c.IdParent,
        j.Nivel + 1,
        j.Ruta + ' > ' + c.Name
    FROM ModuloComponentes c
    INNER JOIN Jerarquia j ON c.IdParent = j.IdComponent
)
SELECT 
    IdComponent,
    REPLICATE('  ', Nivel) + Name AS NombreIndentado,
    ComponentCode,
    Route,
    Nivel,
    Ruta
FROM Jerarquia
ORDER BY Ruta;
```

### **Query 2: Acciones por tipo de módulo**

```sql
SELECT 
    m.Nombre AS Modulo,
    t.Nombre AS TipoAccion,
    COUNT(*) AS CantidadAcciones
FROM AccionesGranulares a
INNER JOIN ModuloComponentes c ON a.IdComponent = c.IdComponent
INNER JOIN Modulos m ON c.IdModule = m.IdModule
INNER JOIN TiposAccion t ON a.IdActionType = t.IdActionType
WHERE a.IsActive = 1
GROUP BY m.Nombre, t.Nombre, t.Orden
ORDER BY m.Nombre, t.Orden;
```

### **Query 3: Permisos más asignados**

```sql
SELECT 
    p.Nombre AS Permiso,
    COUNT(DISTINCT up.IdUser) AS UsuariosConPermiso,
    COUNT(DISTINCT cp.IdComponent) AS ComponentesProtegidos,
    COUNT(DISTINCT ap.IdAction) AS AccionesProtegidas
FROM Permisos p
LEFT JOIN UsuarioPermisos up ON p.IdPermission = up.IdPermission
LEFT JOIN ComponentePermisos cp ON p.IdPermission = cp.IdPermission
LEFT JOIN AccionPermisos ap ON p.IdPermission = ap.IdPermission
WHERE p.IsActive = 1
GROUP BY p.Nombre
ORDER BY UsuariosConPermiso DESC;
```

---

## ?? **MIGRACIÓN DESDE CÓDIGO**

Si ya tienes módulos en código (FinanzasModule.cs, ProspectosModule.cs), este script los migra a BD:

```sql
-- Script de migración desde código
-- EJECUTAR DESPUÉS de tener el seed data básico

USE VRM_Net;
GO

-- Este script asume que ya ejecutaste el seed data
-- y solo necesitas ajustar IDs o agregar componentes/acciones faltantes

-- Verificar módulos existentes
SELECT * FROM Modulos;
SELECT * FROM ModuloComponentes ORDER BY IdModule, IdParent, MenuOrder;
SELECT * FROM AccionesGranulares ORDER BY IdComponent, IdActionType;

-- Si necesitas resetear y volver a ejecutar seed:
/*
DELETE FROM AccionPermisos;
DELETE FROM ComponentePermisos;
DELETE FROM ClienteModulos;
DELETE FROM UsuarioPermisos;
DELETE FROM AccionesGranulares;
DELETE FROM ModuloComponentes;
DELETE FROM Modulos;
DELETE FROM Usuarios;
DELETE FROM Clientes;
DELETE FROM Permisos;

DBCC CHECKIDENT ('Modulos', RESEED, 0);
DBCC CHECKIDENT ('ModuloComponentes', RESEED, 0);
DBCC CHECKIDENT ('AccionesGranulares', RESEED, 0);
DBCC CHECKIDENT ('Usuarios', RESEED, 0);
DBCC CHECKIDENT ('Clientes', RESEED, 0);
DBCC CHECKIDENT ('Permisos', RESEED, 0);

-- Volver a ejecutar seed data
*/
```

---

## ? **CHECKLIST DE IMPLEMENTACIÓN**

```markdown
### Fase 1: Crear Base de Datos
- [ ] Ejecutar Script 1: Crear BD `VRM_Net`
- [ ] Ejecutar Script 2: Crear todas las tablas
- [ ] Ejecutar Script 3: Crear índices
- [ ] Verificar en SSMS que todas las tablas existen

### Fase 2: Datos Iniciales
- [ ] Ejecutar Script 4: Seed data completo
- [ ] Verificar TiposAccion (3 registros)
- [ ] Verificar Permisos (9 registros)
- [ ] Verificar Modulos (2 registros: Finanzas, Prospectos)
- [ ] Verificar ModuloComponentes (4 registros)
- [ ] Verificar AccionesGranulares (16 registros)
- [ ] Verificar Usuario Admin creado

### Fase 3: Vistas y SPs
- [ ] Crear vw_ComponentesJerarquia
- [ ] Crear vw_AccionesConPermisos
- [ ] Crear vw_UsuariosConPermisos
- [ ] Crear sp_GetComponentesVisiblesPorUsuario
- [ ] Crear sp_CanUserPerformAction

### Fase 4: Testing
- [ ] Ejecutar queries de ejemplo
- [ ] Verificar jerarquía de Finanzas
- [ ] Verificar permisos asignados
- [ ] Probar SP con usuario Admin
```

---

## ?? **DOCUMENTOS RELACIONADOS**

- ?? [`GUIA_INFRASTRUCTURE_ENTITY_FRAMEWORK.md`](./GUIA_INFRASTRUCTURE_ENTITY_FRAMEWORK.md) - Cómo conectar Entity Framework
- ?? [`ESTADO_ACTUAL_SISTEMA.md`](./ESTADO_ACTUAL_SISTEMA.md) - Arquitectura completa del sistema
- ?? [`SERILOG_LOGGING.md`](./SERILOG_LOGGING.md) - Sistema de logging

---

**Última actualización:** Enero 10, 2025  
**Autor:** Equipo VRM_Net  
**Estado:** ? LISTO PARA IMPLEMENTACIÓN EN SQL SERVER
