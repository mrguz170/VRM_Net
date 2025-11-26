Modules.Prospectos: Services
============================

Servicios del Módulo Prospectos
La lógica de negocio. 

Usaremos el patrón Interface + Implementación para facilitar testing y mantener el código desacoplado.
¿Qué contien?
1.	IProspectoService: Contrato que define las operaciones
2.	ProspectoService: Implementación con la lógica de negocio
3.	ProspectosModule: La clase que implementa IModule y conecta todo