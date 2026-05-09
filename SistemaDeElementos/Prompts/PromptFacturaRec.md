# PROMPT DE EXTRACCIÓN: FACTURAS RECIBIDAS

Este bloque de reglas se integra en la sección **## REGLAS ESPECÍFICAS DEL ELEMENTO** cuando el `{NegocioTratado}` es "Facturas Recibidas". Utiliza el prefijo **R.FacturasRec**.

### R.FacturasRec.1 · Proveedor (`FiltroPorProveedor`)
- **Disparador:** Facturas "del proveedor [Nombre]", "compradas a [Empresa]", "de [Nombre de empresa/CIF]".
- **Acción:** Generar una lista de objetos en el array de filtros:
    1. **Búsqueda:** 
       `{"Clausula": "FiltroPorProveedor", "Criterio": "contiene", "Valor": "[texto_buscado]"}`
    2. **Vista:** 
       `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "8"}`
- **Nota:** 
  * Extrae tanto el nombre comercial como el CIF/NIF si el usuario lo proporciona.
  * La inclusión de la cláusula `quemostrar` con valor `8` se aplica si pide "todas".

### R.FacturasRec.2 · Importes y Totales
**R.FacturasRec.2.1 · Base Imponible (`FiltroPorImporteSinIva`)**
- **Disparador:** "con base de [Monto]", "BI de [Monto]", "importe neto de [Monto]".
- **Acción:** `{"Clausula": "FiltroPorImporteSinIva", "Criterio": "entreImportes", "Valor": "min;max"}`

**R.FacturasRec.2.2 · Total Factura (`FiltroPorTotalFactura`)**
- **Disparador:** "total de [Monto]", "por valor de [Monto] con iva", "que sumen [Monto] en total".
- **Acción:** `{"Clausula": "FiltroPorTotalFactura", "Criterio": "entreImportes", "Valor": "min;max"}`

### R.FacturasRec.3 · Fechas Operativas
**R.FacturasRec.3.1 · Fecha de Emisión/Factura (`FiltroPorFechaDeEmision`)**
- **Disparador:** "emitidas el...", "con fecha de factura de...", "facturadas en [Periodo]".
- **Acción:** `{"Clausula": "FiltroPorFechaDeEmision", "Criterio": "entreFechas", "Valor": "inicio-fin"}`

**R.FacturasRec.3.2 · Fecha de Vencimiento (`FiltroPorFechaDeVencimiento`)**
- **Disparador:** "que vencen el...", "vencimiento en [Periodo]", "para pagar en [Mes]".
- **Acción:** `{"Clausula": "FiltroPorFechaDeVencimiento", "Criterio": "entreFechas", "Valor": "inicio-fin"}`

### R.FacturasRec.4 · Impuestos y Retenciones (`FiltroDeIvaIrpf`)
- **Disparador:** Menciona tipos de impuestos específicos.
- **Mapeo de Valores Obligatorio:**
  - "con iva" (normal) → `"FiltroConIva"`
  - "con irpf" o "con retención" → `"FiltroConIrpf"`
  - "iva exento" → `"FiltroConIvaExento"`
  - "sujeto pasivo" o "isp" → `"FiltroConIvaIsp"`
  - "no sujeto" → `"FiltroConIvaNsj"`
  - "sin iva ni irpf" → `"FiltroSinIvaNiIrpf"`
- **Acción:** `{"Clausula": "FiltroDeIvaIrpf", "Criterio": "esAlgunoDe", "Valor": "Valor1;Valor2"}`

### R.FacturasRec.5 · Preasientos y Contabilización (`FiltroSiHayPreasiento`)
- **Disparador:** Estado de contabilización o existencia de preasiento.
- **Mapeo de Valores Obligatorio:**
  - "con preasiento" → `"FiltroConSpr"`
  - "sin preasiento" → `"FiltroSinSpr"`
  - "preasiento cancelado" → `"FiltroConSprCan"`
- **Acción:** `{"Clausula": "FiltroSiHayPreasiento", "Criterio": "igual", "Valor": "ValorMapeado"}`

### R.FacturasRec.6 · Relaciones (Expedientes y Contratos)
**R.FacturasRec.6.1 · Por Expediente (`AsociadaAUnExpediente`)**
- "con expediente" → Valor: `"5"`
- "sin expediente" → Valor: `"6"`
- "del expediente [Ref]" → Generar dos objetos: 1. `IdExpediente` (contiene Ref) y 2. `AsociadaAUnExpediente` (valor "5").

**R.FacturasRec.6.2 · Por Contrato (`AsociadaAUnContrato`)**
- "con contrato" → Valor: `"5"`
- "sin contrato" → Valor: `"6"`
- "del contrato [Ref]" → Generar dos objetos: 1. `IdContrato` (contiene Ref) y 2. `AsociadaAUnContrato` (valor "5").

### R.FacturasRec.7 · Forma de Pago (`FiltroPorFormaDePago`)
- **Disparador:** "pagadas por...", "forma de pago [Modo]".
- **Mapeo de Valores:** 
  - "contado" → `FiltroDePagosContado`
  - "tarjeta" → `FiltroDePagosTarjeta`
  - "domiciliada" → `FiltroDePagosDomiciliado`
  - "transferencia" → `FiltroDePagosTransferencia`
  - "remesa" → `FiltroDePagosRemesa`
- **Acción:** `{"Clausula": "FiltroPorFormaDePago", "Criterio": "igual", "Valor": "ValorMapeado"}`