# Gestión de actividades

Vista de los expedientes de **actividades formativas** (tipo *ACT: Actividades formativas*): cada actividad es un expediente con
valoración que puede usar presupuestos, y agrupa sus **actividades formativas** del sistema documental (circuitos documentales de
formación), que se ven en el apartado del mismo nombre de la edición (actividad y estado). Pulsando una se abre en *Actividades
formativas*.

Funciona como *Gestión de expedientes* (ver su manual), limitada a este tipo. El flujo del expediente está en la ayuda del proceso del
tipo *ACT: Actividades formativas* (el **?** de la ficha *Expedientes* del panel de control).

## ¿Cómo hago...?

### ...para dar de alta una actividad?

**Crear** (*Actividad*): solicitante, responsable y datos de la actividad. Después, desde el apartado de **actividades formativas**, cree o
consulte los circuitos de formación de la actividad.

### ...para ver en qué estado están sus actividades formativas?

Abra la actividad: el apartado de actividades formativas lista cada una con su **Estado**.

---

#### Detalle técnico

- Controlador: `ExpedientesController` (vista `CrudActividades`, `Views/Administracion/CrudActividades.cshtml`). Dto: `ExpedienteDto`.
- Descriptor: `DescriptorDeActividades`: etiquetas *Gestión de actividades* / *Actividad* y el span de actividades formativas
  (`CircuitosDocController`, restringido por `IdExpedientePadre`).
- ts: `wwwroot/ts/Administracion/Actividades.ts`.
