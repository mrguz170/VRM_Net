Modules.Prospectos: Plugin independiente

módulo Prospectos tendrá:
1.	Entidades de dominio: Prospecto, RevisionArea, DocumentoProspecto
2.	Interfaces de servicios: IProspectoService (contrato)
3.	Implementación de servicios: ProspectoService (lógica de negocio)
4.	Clase del módulo: ProspectosModule (implementa IModule)

📁 VRM_Plugin.Modules.Prospectos
├── 📁 Domain                    # Entidades del módulo
│   ├── Prospecto.cs
│   ├── RevisionArea.cs
│   ├── DocumentoProspecto.cs
│   └── Enums.cs
├── 📁 Services                  # Lógica de negocio
│   ├── IProspectoService.cs
│   └── ProspectoService.cs
└── ProspectosModule.cs          # Implementación del plugin

=============================================================

Puntos clave de ProspectosModule:
1.	✅ Implementa todas las propiedades de IModule
2.	✅ Registra sus servicios en ConfigureServices
3.	✅ Define permisos necesarios para usarlo
4.	✅ No tiene dependencias de otros módulos (es independiente)
5.	✅ Listo para carga dinámica por el host Blazor

┌─────────────────────────────────────────────────────────────┐
│  1. IModule define EL CONTRATO                              │
│     "Todo módulo debe tener ID, permisos, ConfigureServices"│
└────────────────────┬────────────────────────────────────────┘
                     │ implementa
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  2. ProspectosModule IMPLEMENTA IModule                     │
│     - ModuleId = "Prospectos"                               │
│     - ConfigureServices() registra IProspectoService        │
└────────────────────┬────────────────────────────────────────┘
                     │ registra en DI
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  3. IProspectoService → ProspectoService                    │
│     Cuando alguien pida IProspectoService, obtiene          │
│     ProspectoService con toda la lógica                     │
└────────────────────┬────────────────────────────────────────┘
                     │ opera sobre
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  4. Entidades de Dominio (Prospecto, RevisionArea, etc.)   │
│     El "lenguaje" que habla el módulo                       │
└────────────────────┬────────────────────────────────────────┘
                     │ consulta
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  5. ConfiguracionNegocio (Core.Domain)                      │
│     Valida si el cliente tiene acceso al módulo             │
└─────────────────────────────────────────────────────────────┘