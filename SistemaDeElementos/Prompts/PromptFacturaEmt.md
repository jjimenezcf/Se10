### R.FacturasVenta.1 · Cliente, Deudor o Identificación (`FiltrarPorNombreCliente`)
- **Disparador:** Facturas "del cliente [Nombre]", "emitidas a [Empresa]", "compradas por [NIF/CIF]", "que debe [Nombre]", "del deudor [DNI]".
- **Acción:** Genera un único objeto:
  1. `{"Clausula": "FiltrarPorNombreCliente", "Criterio": "contiene", "Valor": "busqueda"}`
- **Lógica de Valor:** - Si el usuario indica un **Nombre**, **Apellidos** o **Razón Social**, extrae el texto para una búsqueda parcial.
  - Si el usuario indica un **DNI**, **NIF** o **CIF**, extrae el identificador exacto (el sistema validará la coincidencia total).
- **Ejemplo de Salida:**
  - "Facturas de Juan Pérez" → `{"Clausula": "FiltrarPorNombreCliente", "Criterio": "contiene", "Valor": "Juan Pérez"}`
  - "Facturas del CIF B12345678" → `{"Clausula": "FiltrarPorNombreCliente", "Criterio": "contiene", "Valor": "B12345678"}`

### R.FacturasVenta.2 · Importes y Cobros**
**R.FacturasVenta.2.1 · Importe Total con IVA (`filtroporimportesiniva`)**
- **Disparador:** Facturas "de entre [Monto] y [Monto] euros", "por más de [Monto]€", "con un total de [Monto]", "que sumen [Monto]".
- **Acción:** `{"Clausula": "filtroporimportesiniva", "Criterio": "entreImportes", "Valor": "min;max"}`
- **Nota:** En caso de un solo importe: "más de [X]" → `X`; "menos de [X]" → `0;X`.

**R.FacturasVenta.2.2 · Importe Cobrado (`filtroporcobrado`)**
- **Disparador:** Facturas "con un cobro de [Monto]", "donde se han cobrado [Monto]", "entre [Monto] y [Monto] cobrados", "sin cobros", "no cobradas", "con algún cobro", "ya cobradas parcial o totalmente".
- **Acción:** `{"Clausula": "filtroporcobrado", "Criterio": "entreImportes", "Valor": "min;max"}`
- **Casos Especiales de Negocio:**
  - "Sin cobro", "no cobradas" o "con cobro 0" → `{"Clausula": "filtroporcobrado", "Criterio": "entreImportes", "Valor": "0;0"}`
  - "Con algún cobro", "que tengan cobros" o "ya empezadas a cobrar" → `{"Clausula": "filtroporcobrado", "Criterio": "entreImportes", "Valor": "0.01;"}`
  - "Más de [X]" → `X;`
  - "Menos de [X]" → `0;X`

### R.FacturasVenta.3 · Fechas Críticas**

**R.FacturasVenta.3.1 · Fecha de Emisión (`filtroporfechadeemision`)**
- **Disparador:** Facturas "emitidas el [Fecha]", "hechas en [Periodo]", "con fecha de factura de [Fecha]".
- **Acción:** `{"Clausula": "filtroporfechadeemision", "Criterio": "entreFechas", "Valor": "YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ"}`

**R.FacturasVenta.3.2 · Fecha de Vencimiento (`filtroporfechadevencimiento`)**
- **Disparador:** Facturas "que vencen el [Fecha]", "con vencimiento en [Periodo]", "que caducan el [Fecha]".
- **Acción:** `{"Clausula": "filtroporfechadevencimiento", "Criterio": "entreFechas", "Valor": "YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ"}`

### R.FacturasVenta.4 · Numeración y Series (`filtropornumerosdefactura`)**
- **Disparador:** Facturas "desde la número [X] hasta la [Y]", "en el rango de números [Rango]", "entre la factura [Inicio] y la [Fin]".
- **Acción:** Genera un objeto con el formato exacto requerido por el sistema:
  1. `{"Clausula": "filtropornumerosdefactura", "Criterio": "entreRangos", "Valor": "yyyy-serie-numeroDesde;yyyy-serie-numeroHasta"}`
- **Reglas de Formato (IMPORTANTE):**
  - Usa el punto y coma (`;`) para separar el inicio del fin.
  - Usa el guion (`-`) para separar Año, Serie y Número.
  - Si falta uno de los dos, usa `undefined` (ej: `2024-A-001;undefined`).
- **Ejemplo:** "desde la 2024-A-001 hasta la 2024-A-010" → **Valor:** `2024-A-001;2024-A-010`.


### R.FacturasVenta.5 · Relación con Presupuestos (PPT)
**R.FacturasVenta.5.1 · Búsqueda por datos de Presupuesto (`NombrePresupuesto`)**
- **Disparador:** Facturas "del presupuesto [Nombre/Referencia]", "vinculadas al PPT [X]", "que pertenecen al presupuesto [Y]", "del presupuesto con número [Referencia]".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "NombrePresupuesto", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Esta regla busca coincidencias parciales en el nombre o la referencia de los presupuestos vinculados a la factura.

**R.FacturasVenta.5.2 · Situación: Facturas con cualquier Presupuesto (`AsociadaAUnPpt`)**
- **Disparador:** Facturas "con presupuesto", "que tengan PPT", "vinculadas a algún presupuesto", "facturas presupuestadas".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "AsociadaAUnPpt", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasVenta.5.3 · Situación: Facturas sin Presupuesto (`AsociadaAUnPpt`)**
- **Disparador:** Facturas "sin presupuesto", "que no tengan PPT", "no presupuestadas", "pendientes de asociar a presupuesto".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "AsociadaAUnPpt", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número (R.5.1), se omiten las reglas de situación (R.5.2 y R.5.3).

### R.FacturasVenta.6 · Planificaciones de Venta (PLV)
**R.FacturasVenta.6.1 · Búsqueda por datos de Planificación (`NombrePlfDeVenta`)**
- **Disparador:** Facturas "de la planificación [Nombre/Referencia]", "vinculadas a la PLV [X]", "que pertenecen a la planificación de venta [Y]".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "NombrePlfDeVenta", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Esta regla busca facturas asociadas a planificaciones cuyo nombre o referencia coincida parcialmente con el texto.

**R.FacturasVenta.6.2 · Situación: Facturas con Planificación (`AsociadaAUnaPlv`)**
- **Disparador:** Facturas "planificadas", "con planificación de venta", "vinculadas a alguna PLV", "asociadas a una planificación".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "AsociadaAUnaPlv", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasVenta.6.3 · Situación: Facturas sin Planificación (`AsociadaAUnaPlv`)**
- **Disparador:** Facturas "sin planificar", "sin planificación de venta", "no vinculadas a PLV", "pendientes de planificar".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "AsociadaAUnaPlv", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número de PLV (R.6.1), se omiten las reglas de situación (R.6.2 y R.6.3).

### R.FacturasVenta.7 · Relación con Partes de Trabajo (PTR)

**R.FacturasVenta.7.1 · Búsqueda por datos de Parte de Trabajo (`NombreParteTr`)**
- **Disparador:** Facturas "del parte [Nombre/Referencia]", "vinculadas al PTR [X]", "del parte de trabajo [Y]", "facturas del parte con número [Referencia]".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "NombreParteTr", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Esta regla busca coincidencias parciales en el nombre o la referencia de los partes de trabajo vinculados, ya sea en la cabecera de la factura o en sus líneas.

**R.FacturasVenta.7.2 · Situación: Facturas con cualquier Parte de Trabajo (`AsociadaAUnPtr`)**
- **Disparador:** Facturas "con parte de trabajo", "que tengan PTR", "vinculadas a algún parte", "facturas con partes".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "AsociadaAUnPtr", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasVenta.7.3 · Situación: Facturas sin Parte de Trabajo (`AsociadaAUnPtr`)**
- **Disparador:** Facturas "sin parte de trabajo", "que no tengan PTR", "no asociadas a partes", "pendientes de asignar parte".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "AsociadaAUnPtr", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número (R.7.1), se omiten las reglas de situación (R.7.2 y R.7.3).

### R.FacturasVenta.8 · Facturas Rectificadas (`Rectificadas`)

**R.FacturasVenta.8.1 · Búsqueda por datos de Rectificativa (`NombreRectificativa`)**
- **Disparador:** Facturas "rectificadas por [Nombre/Referencia]", "cuya factura rectificativa es la [Año-Serie-Número]", "asociadas a la corrección [Nombre]".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "NombreRectificativa", "Criterio": "contiene", "Valor": "busqueda"}`
- **Lógica de Valor:** - Puede ser el **Nombre** o **Referencia** de la factura que emite la rectificación.
  - Si el usuario indica una numeración, debe usar el formato `YYYY-Serie-Número` (ej: `2024-A-101`) para que el sistema pueda desglosarlo correctamente.
- **Nota:** Este filtro busca las facturas originales que han sido objeto de una rectificación por parte de la factura indicada.

**R.FacturasVenta.8.2 · Situación: Facturas con Rectificación (`Rectificadas`)**
- **Disparador:** Facturas "rectificadas", "con rectificativa", "que tengan relación de rectificación", "ya corregidas".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "Rectificadas", "Criterio": "igual", "Valor": "5"}` 
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "8"}`
(Representa ConRelacion).

**R.FacturasVenta.8.3 · Situación: Facturas SIN Rectificación (`Rectificadas`)**
- **Disparador:** Facturas "no rectificadas", "sin rectificativa", "facturas limpias (sin rectificar)", "que no se hayan corregido".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "Rectificadas", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre, referencia o número específico (R.8.1), se omiten las reglas de situación (R.8.2 y R.8.3).