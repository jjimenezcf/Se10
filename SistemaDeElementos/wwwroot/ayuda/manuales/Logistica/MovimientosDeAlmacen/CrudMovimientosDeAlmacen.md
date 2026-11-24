# Movimientos de almacén

Consulta de **todas las entradas y salidas** de los almacenes: compras (recepciones de pedidos), ventas, devoluciones, regularizaciones,
stock inicial... Cada movimiento indica la cantidad, el precio, el **stock resultante** y el valor, la operación que lo generó y su
preasiento. Los movimientos no se crean aquí: los generan las operaciones de compra, venta, devolución y regularización.

Al entrar se fija el primer almacén de la lista (el stock y el valor se calculan por almacén y material) y el filtro empieza plegado:
despliéguelo con *Mostrar filtro* (o el embudo en el móvil).

## ¿Cómo hago...?

### ...para ver la historia de un material?

Filtre por **Almacén** y **Material** (y, si quiere, por fechas en **Realizado** y por **Tipo** de movimiento). La columna *Stock* muestra
las existencias tras cada movimiento.

### ...para ir al documento que generó un movimiento?

Pulse en la columna **Origen** (pedido, documento de venta, devolución, regularización...). En las recepciones de pedidos, **Proveedor**
abre el proveedor; **Preasiento** abre el preasiento contable del movimiento.

### ¿Por qué no se puede cambiar un movimiento antiguo?

El **candado contable** de la fila indica si el periodo está cerrado: lo está si el movimiento es anterior o igual al último cierre
contable de la sociedad o si su preasiento ya está contabilizado. Esos movimientos no se modifican; las correcciones se hacen con
movimientos nuevos en el periodo abierto (p. ej. una devolución o una regularización).

---

#### Detalle técnico

- Controlador: `MovimientosDeAlmacenController` (Controllers/Logistica). Vista: `Views/Logistica/CrudMovimientosDeAlmacen.cshtml`.
  Dto: `MovimientoDeAlmacenDto`.
- Descriptor: `DescriptorDeMovimientosDeAlmacen`: filtros *Almacén*, *Tipo*, *Material* y *Realizado*.
- Reglas: `GestorDeMovimientosDeAlmacen` (al persistir recalcula inventario, cancela y rehace preasientos y barre los movimientos
  posteriores).
- ts: `wwwroot/ts/Logistica/MovimientosDeAlmacen.ts`: enlaces a origen, proveedor y preasiento; fija el primer almacén y pliega el
  filtro al entrar.
