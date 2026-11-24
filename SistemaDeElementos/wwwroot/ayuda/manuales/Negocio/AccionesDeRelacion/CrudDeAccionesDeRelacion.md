# Acciones de relación

Relación de **acciones enganchadas a transiciones** (u otros objetos) de un negocio: qué acción del sistema (ver *Acciones del sistema*) se
ejecuta y si va **antes** o **después** de transitar. Son las *acciones de transición* que definen los parametrizadores y que se ven al pinchar
la flecha en el visor de flujo. Filtro **Negocio**.

## ¿Cómo hago...?

### ...para añadir una acción a una transición?

**Crear**: negocio, transición, acción y momento (antes / después).

---

#### Detalle técnico

- Controlador: `AccionesDeRelacionController` (Controllers/Negocio). Vista: `Views/Negocio/CrudDeAccionesDeRelacion.cshtml`.
  Descriptor: `DescriptorDeAccionesDeRelacion`.
