# Acciones del sistema

Catálogo de **acciones** ejecutables que se pueden enganchar a las **transiciones** de los negocios (acciones de transición, antes o después
de transitar) o a las agendas: nombre, descripción, **clase de acción** y qué ejecuta según la clase: un método .NET (**Dll**, **Clase**,
**Método**), un **procedimiento almacenado** (con su **esquema**), un **bloque Sql** o un **programa**.

## ¿Cómo hago...?

### ...para que una transición haga algo más?

1. Dé de alta la acción aquí (clase de acción y qué ejecuta).
2. En *Transiciones* del negocio, añádala a la transición (acciones de relación) indicando si va antes o después.

### ...para buscar?

Filtro **Acción**.

---

#### Detalle técnico

- Controlador: `AccionesController` (Controllers/Entorno). Vista: `Views/Entorno/CrudDeAcciones.cshtml`. Dto: `AccionDto`.
- Descriptor: `DescriptorDeAcciones`.
