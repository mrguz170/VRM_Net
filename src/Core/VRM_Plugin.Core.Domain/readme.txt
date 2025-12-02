Core.Domain: Entidades base que todos los módulos van a usar

Propósito: Permitir que el sistema decida dinámicamente qué mostrar a cada cliente.

Tendrá ConfiguracionNegocio, ConfiguracionFiscal, entidades compartidas
Entidades compartidas:
1.	ConfiguracionNegocio: Define qué módulos están habilitados para cada cliente
2.	ConfiguracionFiscal: Almacena datos fiscales del SAT por cliente
3.	ModuloHabilitado: Representa un módulo activado con su configuración específica


¿Por qué estas entidades son "compartidas"?
Porque TODOS los módulos necesitan saber:
•	✅ ¿Qué permisos tiene este usuario?