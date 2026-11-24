# Pagos de una remesa de pago

Lista de los pagos incluidos en una **remesa de pago**. Se abre desde la remesa y está siempre filtrada por ella (filtro **Remesa**). Aquí
se incluyen y excluyen pagos y se registran las **anulaciones** de pagos que el banco no ejecutó.

El funcionamiento de la remesa (estados, fichero SEPA, cierre) está en el manual de *Gestión de remesas de pago*.

## ¿Cómo hago...?

### ...para incluir pagos?

**Incluir pagos** en el menú de la vista. Sólo con la remesa en estado **inicial** y permiso de gestor. Se admiten pagos de la misma
sociedad, remesables, **pendientes**, que no estén en otra remesa ni ya pagados. El pago pasa a **remesado**.

### ...para quitar un pago?

Selecciónelo y **Borrar**. Sólo con la remesa en estado inicial; el pago vuelve a **pendiente**.

### ...para anular un pago?

Con la remesa **presentada**, edite el pago e indique la **fecha de anulación** y el **motivo** (obligatorio). No puede ser anterior a la
presentación; si la remesa aún no se ha pagado, sólo un interventor. Para deshacer la anulación, borre la fecha con un motivo distinto
del anterior.

### ¿Por qué aparece "Consultar" en lugar de "Editar"?

Si no tiene permiso de gestor sobre la remesa, o la remesa no admite cambios en su estado, las opciones *Crear*, *Borrar* e *Incluir
pagos* se bloquean y *Editar* pasa a *Consultar*. El pago y su fecha de pago nunca se cambian desde aquí.

---

#### Detalle técnico

- Controlador: `PagosDeUnaRemesaController` (Controllers/Gastos). Vista: `Views/Gasto/CrudPagosDeUnaRemesa.cshtml`.
- Descriptor: `DescriptorPagosDeUnaRemesa`: restrictor por remesa, filtro *Pago*, modal para incluir pagos.
- Reglas: `GestorDePagosDeUnaRemesa`.
- ts: `wwwroot/ts/Gasto/PagosDeUnaRemesa.ts`: lee la remesa del restrictor y, según su etapa y el permiso, bloquea opciones y cambia
  *Editar* por *Consultar*.
