Core.Abstractions: Contiene las interfaces IModule, IModuleDescriptor - 
El "contrato" que todos los plugins deben cumplir

Piénsalo como un manual de instrucciones universal

┌─────────────────────────────────────────┐
│    VRM_Plugin.Blazor.Server         │
│    (Tu aplicación principal)            │
│                                         │
│  Solo sabe: "Dame algo que sea IModule" │
└─────────────────────────────────────────┘
              ↓ ↓ ↓
    ┌─────────┴─┴─┴─────────┐
    │     IModule           │  ← EL CONTRATO
    │  - ModuleId           │
    │  - DisplayName        │
    │  - Description        │
    │  - Version            │
    │  - Author             │
    │  - Category           │
    │  - Dependencies       │
    │  - RequiredPermissions│
    │  - ConfigureServices  │
    │  - IsEnabledForClient │
    └───────────────────────┘
         ↑         ↑         ↑
         │         │         │
    ┌────┴───┐ ┌──┴────┐ ┌──┴─────┐
    │Prospec-│ │Factu- │ │Expedien│
    │tos     │ │ras    │ │tes     │
    │Plugin  │ │Plugin │ │Plugin  │
    └────────┘ └───────┘ └────────┘

    Analogía: Es como el puerto USB de tu computadora. No importa qué dispositivo conectes (mouse, teclado, disco duro), 
    mientras cumplan el estándar USB, funcionarán.