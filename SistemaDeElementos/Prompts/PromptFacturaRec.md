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
- **Acción:** `{"Clausula": "FiltroPorFechaDeEmision", "Criterio": "entreFechas", "Valor": "YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ"}`

**R.FacturasRec.3.2 · Fecha de Vencimiento (`FiltroPorFechaDeVencimiento`)**
- **Disparador:** "que vencen el...", "vencimiento en [Periodo]", "para pagar en [Mes]".
- **Acción:** `{"Clausula": "FiltroPorFechaDeVencimiento", "Criterio": "entreFechas", "Valor": "YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ"}`

**R.FacturasRec.3.3 · Año fiscal de la factura (`FiltroPorEjercicioDeFactura`)**
- **Disparador:** "del año [YYYY]", "del ejercicio [YYYY]", "facturadas en [YYYY]" (cuando se indica solo el año sin rango de fechas).
- **Acción:** `{"Clausula": "FiltroPorEjercicioDeFactura", "Criterio": "igual", "Valor": "YYYY"}`
- **Nota:** Usar esta regla cuando el usuario indica un año concreto. Si indica un rango de fechas completo, usar R.3.1.

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

### R.FacturasRec.6 · Naturaleza contable (`NombreNaturaleza`)
- **Disparador positivo:** Facturas "de naturaleza [Nombre]", "con naturaleza [Sigla]", "de tipo contable [Nombre]", "de la naturaleza [X]".
- **Disparador negativo:** Facturas "que NO sean de naturaleza [X]", "sin naturaleza [X]", "cuya naturaleza no sea [X]".
- **Acción (positivo):** `{"Clausula": "NombreNaturaleza", "Criterio": "contiene", "Valor": "nombre_o_sigla"}`
- **Acción (negativo):** `{"Clausula": "NombreNaturaleza", "Criterio": "noContiene", "Valor": "nombre_o_sigla"}`
- **Nota:** Busca coincidencia parcial en el nombre o sigla de la naturaleza contable de las líneas de la factura.

### R.FacturasRec.7 · Forma de Pago (`FiltroPorFormaDePago`)
- **Disparador:** "pagadas por...", "forma de pago [Modo]".
- **Mapeo de Valores:**
  - "contado" → `FiltroDePagosContado`
  - "tarjeta" → `FiltroDePagosTarjeta`
  - "domiciliada" → `FiltroDePagosDomiciliado`
  - "transferencia" → `FiltroDePagosTransferencia`
  - "remesa" → `FiltroDePagosRemesa`
- **Acción:** `{"Clausula": "FiltroPorFormaDePago", "Criterio": "igual", "Valor": "ValorMapeado"}`

### R.FacturasRec.8 · Relación con Expedientes

**R.FacturasRec.8.1 · Búsqueda por datos de Expediente (`NombreExpediente`)**
- **Disparador positivo:** Facturas "del expediente [Nombre/Referencia]", "imputadas al expediente [X]", "vinculadas al expediente [Y]".
- **Disparador negativo:** Facturas "que NO sean del expediente [X]", "no imputadas al expediente [Y]", "fuera del expediente [X]".
- **Acción (positivo):** `{"Clausula": "NombreExpediente", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreExpediente", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en nombre o referencia del expediente.

**R.FacturasRec.8.2 · Situación: Facturas con expediente (`AsociadaAUnExpediente`)**
- **Disparador:** Facturas "con expediente", "imputadas a algún expediente", "que tengan expediente".
- **Acción:** `{"Clausula": "AsociadaAUnExpediente", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasRec.8.3 · Situación: Facturas sin expediente (`AsociadaAUnExpediente`)**
- **Disparador:** Facturas "sin expediente", "no imputadas a ningún expediente", "pendientes de imputar".
- **Acción:** `{"Clausula": "AsociadaAUnExpediente", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.8.1), se omiten las reglas de situación (R.8.2 y R.8.3).

### R.FacturasRec.9 · Relación con Contratos

**R.FacturasRec.9.1 · Búsqueda por datos de Contrato (`NombreContrato`)**
- **Disparador positivo:** Facturas "del contrato [Nombre/Referencia]", "vinculadas al contrato [X]", "imputadas al contrato [Y]".
- **Disparador negativo:** Facturas "que NO sean del contrato [X]", "no vinculadas al contrato [Y]", "fuera del contrato [X]".
- **Acción (positivo):** `{"Clausula": "NombreContrato", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreContrato", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en nombre o referencia del contrato.

**R.FacturasRec.9.2 · Situación: Facturas con contrato (`AsociadaAUnContrato`)**
- **Disparador:** Facturas "con contrato", "vinculadas a algún contrato", "que tengan contrato".
- **Acción:** `{"Clausula": "AsociadaAUnContrato", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasRec.9.3 · Situación: Facturas sin contrato (`AsociadaAUnContrato`)**
- **Disparador:** Facturas "sin contrato", "no contractuales", "pendientes de asignar contrato".
- **Acción:** `{"Clausula": "AsociadaAUnContrato", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.9.1), se omiten las reglas de situación (R.9.2 y R.9.3).

### R.FacturasRec.10 · Relación con Remesas de Pago

**R.FacturasRec.10.1 · Búsqueda por datos de Remesa (`NombreRemesaPag`)**
- **Disparador positivo:** Facturas "de la remesa [Nombre/Referencia]", "incluidas en la remesa de pago [X]", "de la remesa [Y]".
- **Disparador negativo:** Facturas "que NO sean de la remesa [X]", "no incluidas en la remesa de pago [Y]", "fuera de la remesa [X]".
- **Acción (positivo):** `{"Clausula": "NombreRemesaPag", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreRemesaPag", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en nombre o referencia de la remesa de pago.

**R.FacturasRec.10.2 · Situación: Facturas con pagos remesados (`FiltroPorRemesaPag`)**
- **Disparador:** Facturas "remesadas", "cuyos pagos están en una remesa", "con pago remesado", "pagadas por remesa".
- **Acción:** `{"Clausula": "FiltroPorRemesaPag", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasRec.10.3 · Situación: Facturas sin pagos remesados (`FiltroPorRemesaPag`)**
- **Disparador:** Facturas "sin remesar", "cuyos pagos no están en remesa", "sin pago remesado".
- **Acción:** `{"Clausula": "FiltroPorRemesaPag", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.10.1), se omiten las reglas de situación (R.10.2 y R.10.3).

### R.FacturasRec.11 · Relación con Estimaciones Directas

**R.FacturasRec.11.1 · Búsqueda por datos de Estimación Directa (`NombreEstimacion`)**
- **Disparador positivo:** Facturas "de la estimación [Nombre/Referencia]", "incluidas en la estimación directa [X]", "de la estimación contable [Y]".
- **Disparador negativo:** Facturas "que NO sean de la estimación [X]", "no incluidas en la estimación directa [Y]", "fuera de la estimación [X]".
- **Acción (positivo):** `{"Clausula": "NombreEstimacion", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreEstimacion", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en nombre o referencia de la estimación directa.

**R.FacturasRec.11.2 · Situación: Facturas en alguna estimación (`VinculosAUnaEstimacion`)**
- **Disparador:** Facturas "en una estimación", "incluidas en estimación directa", "ya estimadas", "con estimación".
- **Acción:** `{"Clausula": "VinculosAUnaEstimacion", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasRec.11.3 · Situación: Facturas sin estimación (`VinculosAUnaEstimacion`)**
- **Disparador:** Facturas "pendientes de estimación", "sin estimación", "no incluidas en ninguna estimación directa".
- **Acción:** `{"Clausula": "VinculosAUnaEstimacion", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.11.1), se omiten las reglas de situación (R.11.2 y R.11.3).

### R.FacturasRec.12 · Relación con Lotes Contables

**R.FacturasRec.12.1 · Búsqueda por datos de Lote Contable (`NombreLoteContable`)**
- **Disparador positivo:** Facturas "del lote [Nombre/Referencia]", "incluidas en el lote contable [X]", "del lote de contabilidad [Y]".
- **Disparador negativo:** Facturas "que NO sean del lote [X]", "no incluidas en el lote contable [Y]", "fuera del lote [X]".
- **Acción (positivo):** `{"Clausula": "NombreLoteContable", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreLoteContable", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en nombre o referencia del lote contable.

**R.FacturasRec.12.2 · Situación: Facturas en algún lote contable (`VinculosAUnLote`)**
- **Disparador:** Facturas "en un lote contable", "contabilizadas en lote", "incluidas en lote", "que tengan lote".
- **Acción:** `{"Clausula": "VinculosAUnLote", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasRec.12.3 · Situación: Facturas sin lote contable (`VinculosAUnLote`)**
- **Disparador:** Facturas "pendientes de lote", "sin lote contable", "fuera de lote contable", "sin contabilizar en lote".
- **Acción:** `{"Clausula": "VinculosAUnLote", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.12.1), se omiten las reglas de situación (R.12.2 y R.12.3).
