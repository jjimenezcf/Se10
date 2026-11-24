# Facturas de una remesa de cobro

Lista de las facturas emitidas incluidas en una remesa de cobro. Se abre desde la remesa y está siempre filtrada por ella (filtro
**Remesa**). Aquí se incluyen y excluyen facturas y se registran las **devoluciones** de recibos.

El funcionamiento de la remesa (estados, generación del fichero SEPA, cargo y cierre) está en el manual de *Gestión de remesas de cobro*.

## ¿Cómo hago...?

### ...para incluir facturas?

**Incluir facturas a la remesa** en el menú de la vista. Sólo con la remesa en estado **inicial** (cumplimentación) y si tiene permiso
de gestor. Se admiten facturas de la misma sociedad que la remesa, en estado *emitida* (remesa de clase *Emitidas*) o *devuelta*
(clase *Devueltas*), con importe, cuyo cliente tenga cuenta bancaria de pago y que no estén en otra remesa viva. La factura pasa a
**remesada**.

### ...para quitar una factura de la remesa?

Selecciónela y **Borrar**. Sólo con la remesa en estado inicial; la factura vuelve a **emitida**.

### ...para registrar la devolución de un recibo?

Con la remesa **presentada**, edite la factura e indique **Devuelto el** y el **Motivo** (obligatorio). La factura pasa a **devuelta**.

- La fecha de devolución no puede ser anterior a la presentación de la remesa.
- Si la remesa aún no se ha cargado, o la devolución es posterior a la fecha máxima de devolución, hace falta permiso de interventor.
- Para **anular la devolución** borre la fecha e indique un motivo distinto del anterior: la factura vuelve a *remesada* (o a *cobrada*
  si la remesa ya estaba cargada).
- Una vez devuelta, la fecha de cargo de esa factura no se puede tocar: anule antes la devolución.

### ¿Por qué aparece "Consultar" en lugar de "Editar"?

Si no tiene permiso de gestor sobre la remesa, o la remesa está en un estado que no admite cambios (cancelada, inicial, generada,
cerrada...), las opciones *Crear*, *Borrar* e *Incluir facturas* se bloquean y *Editar* pasa a *Consultar*. La factura y la fecha de
cargo nunca se cambian desde aquí.

---

#### Detalle técnico

- Controlador: `FacturasEmtDeUnaRemesaController` (Controllers/Venta). Vista: `Views/Venta/CrudFacturasEmtDeUnaRemesa.cshtml`.
- Descriptor: `DescriptorFacturasEmtDeUnaRemesa`: restrictor por remesa, filtro *Factura* y la modal *Incluir facturas a la remesa*.
- Reglas: `GestorDeFacturasEmtDeUnaRemesa` (incluir/excluir sólo en `REM_Etapa_De_Cumplimentacion`, devoluciones y anulación de
  devoluciones, permisos de interventor).
- ts: `wwwroot/ts/Venta/FacturasEmtDeUnaRemesa.ts`: lee la remesa del restrictor y, según su etapa y el permiso, bloquea las
  opciones del menú y cambia *Editar* por *Consultar*.
