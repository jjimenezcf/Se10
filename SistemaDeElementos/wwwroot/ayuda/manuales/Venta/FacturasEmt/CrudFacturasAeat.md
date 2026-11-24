# Consultar en AEAT

Consulta, **directamente en la AEAT** (servicio SII de la sociedad), las facturas emitidas que la AEAT tiene registradas de una sociedad
en un mes. Sirve para comprobar que lo comunicado coincide con lo que hay en la aplicación. Es una vista de sólo consulta: no se crea,
edita ni borra nada.

## ¿Cómo hago...?

### ...para consultar las facturas registradas en la AEAT?

1. En el filtro elija la **Sociedad** (sólo aparecen las sociedades que gestiona).
2. Indique el **Periodo facturación**: el año (por defecto el actual) y el **mes**.
3. Pulse buscar. La consulta se hace en ese momento a la AEAT, así que puede tardar unos segundos.

Año y mes son obligatorios. Si la AEAT no tiene facturas de esa sociedad en ese periodo se indica con un mensaje.

### ...para comparar con las facturas de la aplicación?

Anote el número de factura (*Nº de factura* = año-serie-número), el cliente, la fecha (*Facturada El*), la base imponible (*BI*) y los
*Impuestos*, y búsquelos en **Gestión de facturas emitidas** con el filtro *Varios* `n:año-serie-nº`.

---

#### Detalle técnico

- Controlador: `FacturasEmtController` (vista `CrudFacturasAeat`, `Views/Venta/CrudFacturasAeat.cshtml`). Dto: `FacturaAeatDto`.
- Descriptor: `DescriptorFacturasAeat`: quita el filtro de nombre y pone *Sociedad*, *Periodo facturación* (año) y mes.
- Los datos no vienen de la base de datos: `FacturasEmtController.LeerOtrosElementos` llama a `GeneradorSii.ConsultaDeFacturas(año, mes)`
  y pagina el resultado.
- ts: `wwwroot/ts/Venta/FacturasAeat.ts` (sin comportamiento propio relevante).
