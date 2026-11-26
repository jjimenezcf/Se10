## El mantenimiento: buscar, ver y seleccionar elementos

Toda vista de gestión (por ejemplo "Gestión de facturas emitidas") tiene la misma estructura:

- **Cabecera**: el menú de la vista a la izquierda, el título en el centro y, a la derecha, *Ocultar filtro* y los iconos de los menús
  flotantes (filtro, contextual, individual y de relaciones).
- **Zona de filtro**: los criterios para buscar.
- **Grid**: los elementos encontrados, en filas (en el móvil, en fichas).
- **Pie del grid**: el navegador de páginas y los elementos seleccionados.
- **Pie de página**: los mensajes (con el historial a la izquierda y la papelera para quitarlos), el `?` que muestra este manual y los
  datos de la conexión.

### ¿Cómo busco un elemento?

1. Escriba en el filtro de texto de la zona de filtro (según la vista busca por nombre, referencia u otros datos: el texto de ayuda del
   campo lo indica) y pulse Intro. Puede afinar la búsqueda con el botón de criterio que hay a la derecha del campo
   (contiene, igual, distinto, no contiene, comienza, termina) o con los símbolos `=`, `|`, `%`, `_`, `;` y `!`: vea más abajo *Cómo escribir en los
   filtros de texto*.
2. Si necesita más criterios, abra los bloques de filtro (fechas, importes, listas de valores, relaciones con otros elementos...) y
   rellénelos.
3. Para volver a empezar use, en el menú de filtro, **Blanquear filtros**.

### ¿Cómo guardo unos filtros que uso a menudo?

En el menú de filtro: **Guardar filtros** les da un nombre y los guarda como plantilla de filtrado; **Eliminar filtros** borra una
plantilla guardada. Sólo está en las vistas de un negocio.

### ¿Cómo oculto o muestro columnas del grid?

Menú de filtro → **Columnas visibles**: marque las columnas que quiere ver. Se recuerda para su usuario.

### ¿Cómo ordeno el grid?

Pulse en la cabecera de la columna. Cada pulsación cambia el sentido (ascendente / descendente). El orden que deje se recuerda para
la próxima vez. Si no hay orden guardado, se ordena por fecha de creación descendente, por referencia o por nombre.

### ¿Cómo selecciono elementos?

- Clic en una fila: la selecciona (y deselecciona las demás).
- **Ctrl** + clic: añade o quita la fila de la selección.
- **Mayúsculas** + clic: selecciona el rango de filas.
- En el pie del grid se puede ver sólo lo seleccionado, seleccionar todo o anular la selección.
- En el móvil, las fichas: un toque selecciona y un doble toque abre la edición.

### ¿Qué hace cada opción del menú *Opciones* del pie del grid?

Debajo del grid, junto al navegador de páginas, está el menú **Opciones** (en el móvil sólo se ve su icono de tres rayas, a la
derecha). Al pulsarlo se despliega esta lista; cada opción tiene su icono:

| Icono | Opción | Para qué sirve |
|---|---|---|
| Lupa | **Buscar** | Vuelve a leer el grid con los filtros que hay escritos. |
| Casilla con marca | **Seleccionar todo** | Selecciona todas las filas que se ven en el grid (las de la página actual). |
| Casilla con aspa | **Anular selección** | Quita la selección de todas las filas. |
| Embudo | **Solo las seleccionadas** | Muestra únicamente las filas seleccionadas; púlsela otra vez para volver a ver todas. |
| Flechas de orden tachadas | **Anular orden** | Quita el orden de las columnas, para ver las filas tal como las devuelve la base de datos. |
| Círculo con aspa | **Blanquear filtros** | Vacía todos los criterios de la zona de filtro. |
| Flechas circulares | **Orden inicial** | Vuelve al orden con el que se abre la vista (el guardado para su usuario o, si no hay, el de la vista). |
| Libro abierto | **Resetar columnas** | Deja las columnas visibles y su anchura como vienen de origen. Sólo sale en los grids que permiten personalizar las columnas. |

### ¿Cómo paso de página?

Con el navegador del pie del grid (primera, anterior, siguiente, última) o escribiendo el número de página. También se puede indicar
cuántas filas se leen por página.

### ¿Cómo abro un elemento?

Selecciónelo y pulse **Editar** en el menú de la vista (o doble clic en la ficha en el móvil). Si selecciona varios, la edición le
permite pasar de uno a otro con las flechas del navegador de la edición.

### ¿Cómo creo un elemento?

Pulse **Crear** en el menú de la vista. Si la vista tiene plantillas de creación, aparece un desplegable *Opciones de creación* para
crear a partir de una plantilla guardada. Ver *La creación*.

### ¿Cómo borro un elemento?

Selecciónelo y pulse **Borrar**: pide confirmación. Sólo aparece si la vista lo permite y su usuario tiene permiso para ello.

### ¿Cómo exporto los datos?

Menú contextual → **Exportar datos**. Se indica el motivo de la exportación y qué filas exportar; el fichero se genera como un trabajo
sometido y se le avisa cuando está listo.

### Otras opciones habituales

Dependen de la vista y de sus permisos:

- **Menú contextual** (de la vista): exportar, mostrar totales de la selección y las propias de cada vista.
- **Menú individual** (del elemento seleccionado): permisos del elemento, dar de alta / de baja, gestionar observaciones,
  direcciones, eventos, archivadores, imprimir, enviar por correo, compartir, transitar a otro estado...
- **Menú de relaciones**: ir a los elementos relacionados (por ejemplo, de una factura a sus partes de trabajo).
- **Historial**: en los elementos con proceso (flujo de estados), las transiciones que ha tenido.
- **Visor de detalle** y **mapa**: en algunas vistas, iconos de la cabecera para ver el detalle del elemento seleccionado o su dirección
  en el mapa.
- **Preguntar a la IA**: en algunas vistas, para resumir o preguntar sobre los elementos o sus archivos.

### ¿Por qué no veo una opción?

Las opciones se muestran según los permisos de su usuario sobre la vista y sobre el elemento (consultor, gestor, interventor,
administrador) y según el estado del elemento. Si una opción está gris, el elemento seleccionado no la admite o hay que seleccionar
exactamente uno.

---

#### Detalle técnico del mantenimiento

- Servidor: `DescriptorDeCrud<T>` crea `DescriptorDeMantenimiento<T>` (`Mnt`) con `ZonaDeMenu`, `ZonaDeFiltro` y `ZonaDeDatos`
  (grid). Añade *Crear* y *Editar* al menú, *Exportar datos* al contextual y, si la vista es de un negocio, *Guardar filtros*,
  *Eliminar filtros*, *Blanquear filtros* y *Columnas visibles* al de filtro. El orden inicial sale del parámetro de usuario
  `USU_Ordenacion_Del_Resultado` o, si no hay, de `CreadoEl` / `Referencia` / `Nombre`. Los permisos de crear, editar y borrar
  salen de los atributos del dto (`OpcionDeCrear`, `OpcionDeEditar`, `OpcionDeBorrar`) y de que el negocio esté activo.
- Cliente: `Crud.CrudMnt` (ts/_Crud/CrudMantenimiento.ts) sobre `GridDeDatos`, con los eventos de `Crud.EventosDelMantenimiento`
  (crear, editar, eliminar, ocultar/mostrar filtro, ordenar, fila pulsada...).
- Cada vista específica hereda de ambos: `DescriptorDe{Xxx}` en el servidor y una clase de su ts que extiende `Crud.CrudMnt`.
