# Gestión de presupuestos

Un presupuesto es la **oferta** que se hace a un solicitante (cliente): sus **líneas** (unitarios, partidas libres o partidas alzadas, con
cantidad, precio, descuento e IVA). Se elabora, se envía al cliente, se acepta o se rechaza y, aceptado, se ejecuta con **partes de
trabajo** y se **factura**. Puede pertenecer a un expediente (presupuestos y valoraciones del expediente).

## El ciclo de vida de un presupuesto

Los estados dependen del tipo de presupuesto, pero se agrupan en estas **etapas**:

| Etapa | Qué significa | Qué se puede hacer |
|---|---|---|
| **Elaboración** | Se está preparando. | Cambiar líneas, *Renombrar*. |
| **Pendiente de respuesta** | Enviado al cliente. | Esperar aceptación o rechazo. |
| **Aceptado** | El cliente lo acepta. | Ejecutarlo. |
| **Asociar parte de trabajo** | Admite partes. | Crear partes de trabajo desde el presupuesto. |
| **Permite facturar** | Admite facturas. | Crear prefacturas del presupuesto. |
| **Rechazado** / **Cancelado** | Finales. | Sólo consulta. |

Los partes de trabajo y las facturas sólo se crean o asocian cuando el presupuesto está en la etapa correspondiente, y han de ser de la
misma sociedad.

## ¿Cómo hago...?

### ...para hacer un presupuesto?

1. **Crear**: tipo, centro gestor, solicitante y responsable (o desde el expediente: *Crear presupuesto* / *Crear valoración*).
2. Añada las **líneas** en su apartado. Según el tipo de línea:
   - **Unitario**: concepto, precio, naturaleza y unidad salen del unitario.
   - **Libre**: se escriben concepto, precio, naturaleza y unidad.
   - **Partida alzada**: un concepto con su importe.
   Puede indicar que **el precio incluye el IVA** (entonces se escribe el importe y se calcula el precio) y proponer descuento e IVA por
   línea. El presupuesto totaliza *B.I.* e *Importe*.
3. Avance el estado con las flechas de transitar (enviar, aceptar...).

### ...para copiar un presupuesto?

Menú contextual → **Copiar presupuesto**: elija el presupuesto o la plantilla de origen.

### ...para cambiar el nombre?

**Renombrar** (menú de la edición, interventor) en las etapas que lo permiten.

### ...para vincularlo a un expediente?

**Asociar a Expediente** (gestor): elija el expediente.

### ...para ejecutar y facturar un presupuesto aceptado?

Desde la edición, en las etapas que lo permiten, cree los **partes de trabajo** (se rellenan con los datos del presupuesto y el tipo de
parte por defecto) y las **facturas**. Menú de relaciones: **Tareas vinculadas**, partes de trabajo y facturas emitidas.

## Filtros propios

**Datos del presupuesto**: *Expediente* (y *Mostra expediente*), *Solicitante*, *Responsable*, *Parte de trabajo* y *Factura emitida* (con /
sin).

---

#### Detalle técnico

- Controlador: `PresupuestosController` (Controllers/Presupuesto). Vista: `Views/Presupuesto/CrudPresupuestos.cshtml`. Dto:
  `PresupuestoDto`. Descriptor en `Descriptores/DescriptoresDeVentas/DescriptorDePresupuestos.cs`.
- Descriptor: ampliación de líneas, modales *Asociar expediente al presupuesto*, *Renombrar*, *Seleccionar presupuesto o plantilla*,
  totales; bloque *Datos del presupuesto*.
- Etapas: `enumEtapasDePpts` (`ServicioDeDatos/Ventas/Presupuestos/VariablesDePpt.cs`), entre ellas `PPT_Etapa_AsociarParteTr` y
  `PPT_Etapa_PermiteFacturar`.
- ts: `wwwroot/ts/Presupuesto/Presupuestos.ts`: habilita crear partes y facturas según la etapa, *Renombrar* según estado, aplica el
  tipo de línea y el cálculo con *el precio incluye el IVA*.
