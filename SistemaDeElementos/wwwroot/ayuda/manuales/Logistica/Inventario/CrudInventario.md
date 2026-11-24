# Inventario de almacén

Consulta de las **existencias de un almacén a una fecha**: por cada material, su stock, precio medio ponderado y valor, tomados del
último movimiento del material hasta esa fecha. Desde aquí también se **vende material** directamente y se **prepara un recuento**. Es
una vista de consulta (sin edición).

Al entrar se propone la fecha de hoy y, si sólo tiene acceso a un almacén, se fija ese almacén: lo normal es ver el inventario de hoy
sin tocar el filtro.

## ¿Cómo hago...?

### ...para ver las existencias a una fecha?

En el filtro elija el **Almacén** y la **Fecha** (y, si quiere, un **Material**). Columnas:

| Columna | Qué es |
|---|---|
| *Stock* | Existencias del material a la fecha. |
| *Precio medio* | Precio medio ponderado en el almacén a la fecha. |
| *Valor* | Stock × precio medio. |
| *Realizado el* | Fecha del movimiento del que se toman stock y precio. |
| *Última compra* | Precio de la última entrada de compra hasta la fecha. |
| *Coste previsto* / *Precio de venta* | Los definidos en el material. |
| *%V* | Margen: (venta − valor de inventario) / venta. |

Pulsando el material se abre su ficha.

### ...para vender un material desde el inventario?

Seleccione la fila del material y, en el menú contextual, **Venta de material**. Se proponen el almacén, el material, la fecha de hoy y
el precio de venta del material. Indique:

- **Cliente**, **Fecha** (no posterior a hoy ni anterior al último cierre contable) y **Cantidad** (como mucho el stock a esa fecha).
- **Servido**: tras crear el documento de venta lo sirve (sale del almacén).
- **Facturar**: además crea la prefactura (marcarlo marca *Servido*; desmarcar *Servido* desmarca *Facturar*).

Se crea el **documento de venta** con su línea y se abre en otra pestaña (o la prefactura, si se ha facturado). El inventario se recarga.

### ...para preparar un recuento?

Menú contextual → **Preparar recuento**: elija la **Clase** de regularización y, opcionalmente, filtre materiales por **Referencia** o
**Nombre** (admiten `;` para alternativas, `%` cualquier cadena, `_` un carácter, `=` al inicio para coincidencia exacta y `|` al
inicio o al final para *empieza* / *termina*).

Se genera en segundo plano un **Excel** con el inventario de hoy: se le envía por correo, se anexa al almacén y se anota una
observación. En el Excel sólo se rellenan **Cantidad** (lo contado) y **Precio** (propuesto con el valor medio); en la clase *Ajuste de
precio* la cantidad sale bloqueada y sólo se cambia el precio. Con el Excel relleno, importe el recuento en una **regularización**.

---

#### Detalle técnico

- Controlador: `InventarioController` (Controllers/Logistica). Vista: `Views/Logistica/CrudInventario.cshtml`. Dto: `InventarioDto`.
- Descriptor: `DescriptorDeInventario`: filtros *Almacén*, *Fecha* y *Material*; modales *Venta de material* (`CrearVentaDto`) y
  *Preparar recuento* (`PrepararRecuentoDto`).
- ts: `wwwroot/ts/Logistica/Inventario.ts`: propone fecha y almacén, abre la venta con los datos de la fila (el id de la fila es su
  último movimiento), recalcula el total y enlaza los checks *Servido* / *Facturar*.
