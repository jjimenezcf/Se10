# Gestión de expedientes

Un expediente agrupa todo lo relacionado con un **asunto** de un solicitante (cliente): sus **presupuestos** y valoraciones, **contratos**,
**tareas**, **registros de entrada/salida**, **facturas recibidas y emitidas**, **apuntes** de gastos e ingresos y las **entregas de pedidos**
imputadas. Muestra el **resumen económico** del asunto: presupuestado, gastos, ingresos, pagado y cobrado.

## Tipos de expediente

Cada tipo tiene su propio flujo; el detalle de cada uno está en la ayuda del proceso (el **?** de la ficha *Expedientes* del panel de
control, un documento por tipo). En resumen:

| Tipo | Para qué sirve |
|---|---|
| **SCV: Venta** | Solicitud de un cliente que acabará (o no) en contratos de venta; usa presupuestos. |
| **SCC: Compra** | Necesidad de contratar un servicio o suministro con un proveedor; se le asocian contratos de compra. |
| **OBR: Obra** | Presupuestos, tareas y gastos de una obra (con valoración). |
| **TLL: Reparación de vehículos** | Un vehículo desde que llega al taller hasta que se recoge y se paga. |
| **ACT: Actividades formativas** | Una actividad formativa de la organización (con valoración). |
| **CIV: Procedimiento civil** / **JUR: Jurídico** | Asuntos jurídicos, con datos jurídicos (NIG, procedimiento). |
| **CGI: Contabilización**, **FCT: Facturación**, **IRPF: Renta**, **PIM: Impuestos** | Servicios de asesoría a un cliente; al crearlos se crean sus archivadores del año. |
| **SPR: Sprint** | Tareas evolutivas y correctivas planificadas y valoradas de una vez. |

## ¿Cómo hago...?

### ...para crear un expediente?

**Crear**: tipo, solicitante, responsable y datos del asunto. Según el tipo, al crearlo se generan archivadores o datos jurídicos.

### ...para presupuestar un expediente?

En la etapa de **presupuestación** del tipo, desde el apartado de presupuestos o el menú de la edición (gestor):

- **Crear presupuesto**, **Crear valoración** o **Asociar presupuesto** existente.

Fuera de esa etapa (o si el tipo no usa presupuestos) estas opciones no están disponibles.

### ...para imputar gastos e ingresos al expediente?

Menú de la edición (gestor; no con el expediente terminado o cancelado):

- **Asociar factura recibida** o **Crear factura recibida** ya imputada.
- **Añadir apunte** (gasto o ingreso manual).
- **Asociar registro de E/S**.

Desde la vista, con un expediente seleccionado, **Imputar facturas** (interventor). Las entregas de pedidos con destino la obra del
expediente se imputan desde el pedido.

### ...para ver todo lo relacionado?

En la edición, los apartados (sólo se ven si tienen datos): presupuestos (*Total sin iva*, *Facturado*), contratos, registros de E/S,
facturas recibidas (*Pagado*) y emitidas (*Cobrado*), apuntes y recepciones de pedidos. Menú de relaciones: **Tareas vinculadas**,
**Facturas recibidas**. **Gestionar interlocutores** para las personas del asunto. **Mostrar totales** (menú contextual) de los expedientes
seleccionados.

## Filtros propios

**Filtros de expedientes** (solicitante, responsable) y **Elementos relacionados** (presupuesto, tarea, registro E/S, cada uno con su tipo
y la opción *con / sin*).

---

#### Detalle técnico

- Controlador: `ExpedientesController` (Controllers/Expediente). Vista: `Views/Administracion/CrudExpedientes.cshtml`. Dto:
  `ExpedienteDto`. La misma vista se usa por clase de expediente (jurídico / no jurídico).
- Descriptor: `DescriptorDeExpedientes`: datos jurídicos, spans de presupuestos, registros, facturas, apuntes y recepciones; opciones de
  asociar/crear; totales; bloques de filtros.
- Reglas: `GestorDeExpedientes`, `GestorDePresupuestos` / `ExtensorDePresupuestos.CrearValoracion` (sólo en presupuestación). Flujos:
  `wwwroot/ayuda/procesos/Expediente/*.md`.
- ts: `wwwroot/ts/Administracion/Expedientes.ts`: oculta los apartados que el tipo no usa, muestra la fila de importes sólo si hay datos,
  habilita presupuestos sólo en presupuestación y facturas/apuntes sólo con el expediente abierto.
