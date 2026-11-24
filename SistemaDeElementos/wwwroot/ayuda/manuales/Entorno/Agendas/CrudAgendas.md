# Agendas del entorno

Las **agendas** donde se anotan eventos (vencimientos de facturas, fechas de pago de remesas, asignaciones de partes de trabajo...): nombre,
url y ruta, la **acción** asociada y los **permisos** de intervención, gestión y consulta que dan acceso a ella.

## ¿Cómo hago...?

### ...para ver los eventos de una agenda?

Seleccione la agenda y **Abrir agenda** (visor de agenda).

### ...para dar acceso a una agenda?

Asigne sus permisos (*P. de Intervención*, *P. de Gestor*, *P. de Consultor*) a un rol o como permiso directo de un puesto.

---

#### Detalle técnico

- Controlador: `AgendasController` (Controllers/Entorno). Vista: `Views/Entorno/CrudAgendas.cshtml`. Dto: `AgendaDto`.
- Descriptor: `DescriptorDeAgendas` (opción *Abrir agenda*; visor en `DescriptorDeVisorDeAgenda`).
