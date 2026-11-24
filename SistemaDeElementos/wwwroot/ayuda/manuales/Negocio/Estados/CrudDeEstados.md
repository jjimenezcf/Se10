# Estados (de un negocio)

Los **estados del flujo** de un negocio: nombre, **orden**, su **permiso** y si es **inicial**, **final** o **cancelado**. Es una vista común a
todos los negocios con flujo: se abre desde el menú de configuración de cada negocio y filtra por él (filtro **Negocio**). Los estados se
agrupan en **etapas** (parámetros del negocio) que son las que usa el código para decidir qué se puede hacer.

## ¿Cómo hago...?

### ...para ver el flujo de un negocio?

Seleccione un estado y, en el menú, **Flujo**: muestra el diagrama de estados y transiciones del negocio desde ese estado.

### ...para ver las transiciones que salen de un estado?

Menú → **Transiciones**, o en la edición, el apartado de transiciones (*Transicion*) con sus **Acciones**.

### ...para leer la ayuda del flujo?

En el apartado de transiciones, **Ayuda**: abre el documento del proceso del negocio (`ayuda/procesos/<negocio>/*.md`), que explica qué hace
cada transición. También se puede descargar.

### ...para controlar quién ve los elementos en un estado?

Con el **permiso** del estado (`<NEGOCIO>.ESTADO: <estado>`) en los roles.

---

#### Detalle técnico

- Controlador: `EstadosController` (Controllers/Negocio; `epAyudaDelFlujo`, `epAyudaDelFlujoHtml`, `epExplicarTransiciones`,
  `epAyudaDelNegocio`). Vista: `Views/Negocio/CrudDeEstados.cshtml`. Dto: `EstadoDto`.
- Descriptor: `DescriptorDeEstados` (opciones *Flujo* y *Transiciones*; span de transiciones con *Acciones* y *Ayuda*).
