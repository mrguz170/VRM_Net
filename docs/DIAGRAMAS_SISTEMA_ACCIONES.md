# ?? Diagramas del Sistema de Acciones Dinámicas

Este documento complementa [SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md) con diagramas visuales detallados.

---

## ?? Diagrama 1: Flujo Completo de Carga

```mermaid
sequenceDiagram
    participant App as ?? Program.cs
    participant MM as ?? ModuleManager
    participant Repo as ?? ModuleMetadataRepository
    participant DB as ??? SQL Server
    participant Mod as ?? FinanzasModule
    participant UI as ?? Facturas.razor
    participant Auth as ?? AuthorizeAction
    
    Note over App: ? Inicio de Aplicación
    App->>MM: new ModuleManager(logger, metadataService)
    App->>MM: DiscoverAndLoadModulesAsync("Modules/")
    
    Note over MM: ?? Descubrir DLLs
    MM->>MM: Buscar VRM_Plugin.*.dll
    MM->>Mod: Activator.CreateInstance(FinanzasModule)
    
    Note over MM: ?? Inyectar Metadata desde BD
    MM->>Repo: GetModuleMetadata(moduleId: 1)
    Repo->>DB: EXEC sp_get_module_info @module_id=1
    DB-->>Repo: { DisplayName: "Finanzas", Version: "1.0.0" }
    Repo-->>MM: ModuleDto
    
    MM->>Repo: GetActionsByModuleId(moduleId: 1)
    Repo->>DB: EXEC sp_get_actions @module_id=1
    DB-->>Repo: [ { ActionKey: "Finanzas.Facturas.Crear", Roles: "1,2,3" } ]
    Repo->>Repo: ParseRolesString("1,2,3") ? [1,2,3]
    Repo-->>MM: List<ModuleActionDto>
    
    MM->>Repo: GetActionPermissions("Finanzas")
    Repo->>DB: EXEC ConsultaPermisos @Modulo='Finanzas'
    DB-->>Repo: { "Finanzas.Facturas.Crear": ["Admin", "Gerente"] }
    Repo-->>MM: Dictionary<string, string[]>
    
    Note over MM: ?? Usar Reflexión para Inyectar
    MM->>Mod: SetActions(actions)
    MM->>Mod: SetActionPermissions(permissions)
    
    MM-->>App: ? 1 módulo cargado
    
    Note over App: ?? App.Run() ? Usuario navega
    
    Note over UI: ?? Usuario accede a /finanzas/facturas
    UI->>Auth: <AuthorizeAction Action="Finanzas.Facturas.Crear">
    
    Auth->>Auth: OnInitializedAsync()
    Auth->>Auth: moduleId = "Finanzas" (de Action.Split('.'))
    Auth->>Mod: GetActionPermission(roleId)
    Mod-->>Auth: { "Finanzas.Facturas.Crear": ["Admin", "Gerente"] }
    
    Auth->>Auth: user.IsInRole("Admin") ? true
    Auth->>Auth: HasPermission = true
    Auth->>UI: ? Renderiza <button>Crear</button>
```

---

## ??? Diagrama 2: Arquitectura de Capas

```mermaid
flowchart TB
    subgraph "?? Capa de Presentación"
        UI1["Facturas.razor"]
        UI2["Pagos.razor"]
        UI3["Prospectos.razor"]
    end
    
    subgraph "?? Capa de Autorización"
        Auth["AuthorizeAction.razor"]
    end
    
    subgraph "?? Capa de Módulos"
        Mod1["FinanzasModule"]
        Mod2["ProspectosModule"]
    end
    
    subgraph "?? Capa de Gestión"
        MM["ModuleManager"]
    end
    
    subgraph "?? Capa de Datos"
        Repo["ModuleMetadataRepository"]
        DH["DatabaseHelper"]
    end
    
    subgraph "??? Base de Datos"
        SP1["sp_get_actions"]
        SP2["ConsultaPermisos"]
        T1["AccionesGranulares"]
        T2["Rel_PermisoAccion"]
    end
    
    UI1 --> Auth
    UI2 --> Auth
    UI3 --> Auth
    
    Auth --> Mod1
    Auth --> Mod2
    
    Mod1 -.->|"Datos inyectados"| MM
    Mod2 -.->|"Datos inyectados"| MM
    
    MM --> Repo
    Repo --> DH
    
    DH --> SP1
    DH --> SP2
    
    SP1 --> T1
    SP1 --> T2
    SP2 --> T1
    SP2 --> T2
    
    style Auth fill:#F0E68C
    style MM fill:#90EE90
    style Repo fill:#ADD8E6
    style SP1 fill:#FFB6C1
    style SP2 fill:#FFB6C1
```

---

## ?? Diagrama 3: Ciclo de Vida de una Acción

```mermaid
stateDiagram-v2
    [*] --> Diseño
    
    Diseño --> BD_Insert: DBA crea acción
    
    BD_Insert --> App_Restart: Reiniciar aplicación
    
    App_Restart --> Carga_Modulos: ModuleManager.DiscoverAndLoadModulesAsync()
    
    Carga_Modulos --> Query_BD: sp_get_actions
    
    Query_BD --> Parseo: ModuleMetadataRepository
    
    Parseo --> Inyeccion: Reflexión (SetActions)
    
    Inyeccion --> Modulo_Listo: FinanzasModule con datos
    
    Modulo_Listo --> Usuario_Navega: Usuario abre /finanzas/facturas
    
    Usuario_Navega --> AuthorizeAction_Check: <AuthorizeAction Action="...">
    
    AuthorizeAction_Check --> Verificar_Permiso: GetActionPermission()
    
    Verificar_Permiso --> Render_Autorizado: user.IsInRole() = true
    Verificar_Permiso --> Render_Negado: user.IsInRole() = false
    
    Render_Autorizado --> [*]: ? Botón visible
    Render_Negado --> [*]: ? Botón oculto
    
    note right of BD_Insert
        INSERT INTO AccionesGranulares
        (CodigoAccion, NombreAccion, ...)
        VALUES ('Finanzas.Facturas.Crear', ...)
    end note
    
    note right of Parseo
        "1,2,3" ? [1, 2, 3]
    end note
    
    note right of Inyeccion
        moduleType.GetMethod("SetActions")
        .Invoke(module, [actions])
    end note
```

---

## ?? Diagrama 4: Estructura de Datos

```mermaid
erDiagram
    MODULOS ||--o{ COMPONENTES : contiene
    MODULOS ||--o{ ACCIONES_GRANULARES : contiene
    COMPONENTES ||--o{ ACCIONES_GRANULARES : agrupa
    ACCIONES_GRANULARES ||--o{ REL_PERMISO_ACCION : requiere
    PERMISOS ||--o{ REL_PERMISO_ACCION : autoriza
    
    MODULOS {
        int IdModulo PK
        string Codigo "ej: Finanzas"
        string NombreModulo
        string Version
    }
    
    COMPONENTES {
        int IdComponente PK
        int IdModulo FK
        int IdParent FK "null = raíz"
        string NombreComponente
        string Ruta
        string Roles "1,2,3"
    }
    
    ACCIONES_GRANULARES {
        int IdAccionGranular PK
        int IdModulo FK
        int IdComponente FK
        string CodigoAccion "Finanzas.Facturas.Crear"
        string NombreAccion
        bool Activo
    }
    
    REL_PERMISO_ACCION {
        int IdAccionGranular FK
        int IdPermiso FK
    }
    
    PERMISOS {
        int IdPermiso PK
        string NombrePermiso "Admin, Gerente, ..."
        string Descripcion
    }
```

---

## ?? Diagrama 5: Flujo de Autorización en Runtime

```mermaid
flowchart TD
    A["?? Usuario navega a<br/>/finanzas/facturas"] --> B["?? Facturas.razor<br/>se renderiza"]
    
    B --> C["?? Encuentra<br/><AuthorizeAction>"]
    
    C --> D{"?? Usuario<br/>autenticado?"}
    
    D -->|No| E["? No renderiza<br/>ChildContent"]
    
    D -->|Sí| F["?? Extraer moduleId<br/>de Action"]
    
    F --> G["?? Buscar módulo<br/>en IEnumerable<IModule>"]
    
    G --> H{"?? Módulo<br/>encontrado?"}
    
    H -->|No| E
    
    H -->|Sí| I["?? Obtener roleId<br/>del usuario (Claims)"]
    
    I --> J["?? module.GetActionPermission(roleId)"]
    
    J --> K["?? Retorna<br/>Dictionary<string, string[]>"]
    
    K --> L{"?? Action existe<br/>en Dictionary?"}
    
    L -->|No| E
    
    L -->|Sí| M["? Obtener<br/>allowedRoles[]"]
    
    M --> N{"?? user.IsInRole()<br/>en allowedRoles?"}
    
    N -->|No| E
    
    N -->|Sí| O["? HasPermission = true"]
    
    O --> P["?? Renderiza<br/>ChildContent"]
    
    P --> Q["??? Usuario ve<br/>el botón/control"]
    
    style E fill:#ffcccc
    style Q fill:#ccffcc
    style J fill:#fff4cc
```

---

## ?? Diagrama 6: Inyección de Dependencias

```mermaid
flowchart LR
    subgraph "?? Program.cs"
        PS[("Service<br/>Collection")]
    end
    
    subgraph "?? Servicios Registrados"
        IMS["IModuleMetadataService<br/>(Scoped)"]
        IMM["IModuleManager<br/>(Singleton)"]
        DH["DatabaseHelper<br/>(Scoped)"]
    end
    
    subgraph "??? Implementaciones"
        MMR["ModuleMetadataRepository"]
        MM["ModuleManager"]
        DHI["DatabaseHelper"]
    end
    
    subgraph "?? Componentes Blazor"
        Auth["AuthorizeAction.razor"]
        Sidebar["Sidebar.razor"]
    end
    
    PS --> IMS
    PS --> IMM
    PS --> DH
    
    IMS -.implements.-> MMR
    IMM -.implements.-> MM
    DH -.implements.-> DHI
    
    MMR --> DHI
    MM --> MMR
    
    Auth -->|"@inject IEnumerable<IModule>"| IMM
    Sidebar -->|"@inject IModuleManager"| IMM
    
    style PS fill:#FFE4B5
    style IMS fill:#ADD8E6
    style IMM fill:#90EE90
    style Auth fill:#F0E68C
```

---

## ?? Diagrama 7: Comparación Antes vs Después

```mermaid
flowchart TB
    subgraph "? ANTES: Permisos Hardcodeados"
        A1["?? FinanzasModule.cs<br/>GetActionPermissions()"]
        A2["?? Diccionario estático<br/>en código"]
        A3["?? Cambio requiere<br/>recompilar"]
        A4["?? Desplegar DLL<br/>nueva"]
        
        A1 --> A2 --> A3 --> A4
    end
    
    subgraph "? DESPUÉS: Permisos Dinámicos"
        B1["??? SQL Server<br/>AccionesGranulares"]
        B2["?? sp_get_actions<br/>Stored Procedure"]
        B3["?? ModuleMetadataRepository<br/>Parsea datos"]
        B4["?? ModuleManager<br/>Inyecta con reflexión"]
        B5["?? FinanzasModule<br/>_actionPermissions"]
        B6["?? AuthorizeAction<br/>Consulta en runtime"]
        
        B1 --> B2 --> B3 --> B4 --> B5 --> B6
    end
    
    style A1 fill:#ffcccc
    style A2 fill:#ffcccc
    style A3 fill:#ffcccc
    style A4 fill:#ffcccc
    
    style B1 fill:#ccffcc
    style B2 fill:#ccffcc
    style B3 fill:#ccffcc
    style B4 fill:#ccffcc
    style B5 fill:#ccffcc
    style B6 fill:#ccffcc
```

---

## ?? Diagrama 8: Matriz de Permisos (Ejemplo)

```mermaid
graph TB
    subgraph "?? Roles"
        R1["1?? Admin"]
        R2["2?? Gerente Finanzas"]
        R3["3?? Coordinador Finanzas"]
        R4["4?? Contador"]
    end
    
    subgraph "? Acciones"
        A1["Finanzas.Facturas.Ver"]
        A2["Finanzas.Facturas.Crear"]
        A3["Finanzas.Facturas.Editar"]
        A4["Finanzas.Facturas.Eliminar"]
        A5["Finanzas.Facturas.TimbrarSAT"]
        A6["Finanzas.Reportes.Confidencial"]
    end
    
    R1 -->|"?"| A1
    R1 -->|"?"| A2
    R1 -->|"?"| A3
    R1 -->|"?"| A4
    R1 -->|"?"| A5
    R1 -->|"?"| A6
    
    R2 -->|"?"| A1
    R2 -->|"?"| A2
    R2 -->|"?"| A3
    R2 -->|"?"| A4
    R2 -->|"?"| A5
    R2 -->|"?"| A6
    
    R3 -->|"?"| A1
    R3 -->|"?"| A2
    R3 -->|"?"| A3
    R3 -->|"?"| A4
    R3 -->|"?"| A5
    R3 -->|"?"| A6
    
    R4 -->|"?"| A1
    R4 -->|"?"| A2
    R4 -->|"?"| A3
    R4 -->|"?"| A4
    R4 -->|"?"| A5
    R4 -->|"?"| A6
    
    style R1 fill:#ff6b6b
    style R2 fill:#4ecdc4
    style R3 fill:#ffe66d
    style R4 fill:#a8dadc
```

---

## ?? Diagrama 9: Performance y Caché

```mermaid
flowchart TD
    A["? App Startup<br/>t=0ms"] --> B["?? Discover Modules<br/>t=50ms"]
    
    B --> C["??? Query BD<br/>sp_get_actions<br/>t=100ms"]
    
    C --> D["?? Parse DTOs<br/>t=120ms"]
    
    D --> E["?? Inject to Modules<br/>Reflexión<br/>t=150ms"]
    
    E --> F["? Modules Ready<br/>t=200ms"]
    
    F --> G["?? App Running"]
    
    G --> H["?? User Request 1<br/>/finanzas/facturas"]
    
    H --> I["?? AuthorizeAction<br/>Check Permissions"]
    
    I --> J["?? module.GetActionPermission()<br/>? CACHÉ en memoria"]
    
    J --> K["? Render<br/>~1ms (sin query BD)"]
    
    K --> L["?? User Request 2<br/>/finanzas/pagos"]
    
    L --> M["?? AuthorizeAction<br/>Check Permissions"]
    
    M --> N["?? Mismo módulo<br/>? CACHÉ reutilizada"]
    
    N --> O["? Render<br/>~1ms"]
    
    style J fill:#90EE90
    style N fill:#90EE90
    
    note right of J
        ? Sin queries a BD
        ? Datos en RAM
        ? Ultra rápido
    end note
```

---

## ??? Diagrama 10: Troubleshooting Decision Tree

```mermaid
flowchart TD
    A["? Botón no aparece<br/>con AuthorizeAction"] --> B{"?? Usuario<br/>autenticado?"}
    
    B -->|No| C["?? Verificar Login<br/>AuthenticationState"]
    
    B -->|Sí| D{"?? Módulo<br/>cargado?"}
    
    D -->|No| E["?? Verificar logs<br/>[ModuleManager]<br/>Módulo descubierto"]
    
    D -->|Sí| F{"?? Action existe<br/>en permisos?"}
    
    F -->|No| G["??? Verificar BD<br/>AccionesGranulares<br/>CodigoAccion"]
    
    F -->|Sí| H{"?? Usuario tiene<br/>rol correcto?"}
    
    H -->|No| I["?? Verificar Claims<br/>ClaimTypes.Role<br/>vs BD Permisos"]
    
    H -->|Sí| J{"?? Permisos<br/>actualizados?"}
    
    J -->|No| K["?? Reiniciar App<br/>Recargar caché"]
    
    J -->|Sí| L["?? Debug<br/>AuthorizeAction<br/>OnInitializedAsync"]
    
    C --> M["? Solucionado"]
    E --> M
    G --> M
    I --> M
    K --> M
    L --> M
    
    style C fill:#ffcccc
    style E fill:#ffcccc
    style G fill:#ffcccc
    style I fill:#ffcccc
    style K fill:#ffcccc
    style L fill:#ffcccc
    style M fill:#ccffcc
```

---

## ?? Diagrama 11: Escalabilidad

```mermaid
graph LR
    subgraph "?? Sistema Actual"
        M1["Módulo 1<br/>Finanzas<br/>10 acciones"]
        M2["Módulo 2<br/>Prospectos<br/>8 acciones"]
        M3["Módulo 3<br/>Inventario<br/>12 acciones"]
    end
    
    subgraph "?? Futuro Extensible"
        M4["Módulo 4<br/>CRM<br/>15 acciones"]
        M5["Módulo 5<br/>Logística<br/>20 acciones"]
        M6["Módulo N<br/>...<br/>? acciones"]
    end
    
    subgraph "??? Base de Datos Centralizada"
        DB["AccionesGranulares<br/>Rel_PermisoAccion<br/>? Un solo esquema"]
    end
    
    M1 --> DB
    M2 --> DB
    M3 --> DB
    
    M4 -.->|"Nuevo módulo"| DB
    M5 -.->|"Nuevo módulo"| DB
    M6 -.->|"Nuevo módulo"| DB
    
    DB --> Tot["?? Total: 65+ acciones<br/>? Sin cambio de código<br/>? Solo INSERT en BD"]
    
    style DB fill:#90EE90
    style Tot fill:#FFE4B5
```

---

## ?? Conclusión Visual

```mermaid
mindmap
  root((Sistema de<br/>Acciones Dinámicas))
    ??? Base de Datos
      AccionesGranulares
      Rel_PermisoAccion
      Stored Procedures
    ?? Módulos
      FinanzasModule
      ProspectosModule
      Inyección por Reflexión
    ?? UI Components
      AuthorizeAction
      Facturas.razor
      Sidebar.razor
    ? Ventajas
      Sin recompilar
      Un punto de verdad
      Auditable
      Escalable
```

---

**?? Documentos Relacionados:**
- [SISTEMA_DE_ACCIONES_DINAMICAS.md](./SISTEMA_DE_ACCIONES_DINAMICAS.md) - Guía completa con código
- [README.md](../README.md) - Documentación principal del proyecto

---

**?? Herramientas para visualizar diagramas:**
- [Mermaid Live Editor](https://mermaid.live)
- GitHub (renderiza automáticamente)
- VS Code con extensión "Markdown Preview Mermaid Support"
