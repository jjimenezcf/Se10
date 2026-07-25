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

### R.FacturasVenta.9 · Relación con Remesas

**R.FacturasVenta.9.1 · Búsqueda por datos de Remesa (`NombreRemesaFae`)**
- **Disparador positivo:** Facturas "de la remesa [Nombre/Referencia]", "incluidas en la remesa [X]", "de la remesa de cobros [Y]".
- **Disparador negativo:** Facturas "que NO sean de la remesa [X]", "no incluidas en la remesa [Y]", "fuera de la remesa [X]".
- **Acción (positivo):** `{"Clausula": "NombreRemesaFae", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreRemesaFae", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en el nombre o referencia de la remesa.

**R.FacturasVenta.9.2 · Situación: Facturas incluidas en alguna remesa (`IncluidaEnRemesa`)**
- **Disparador:** Facturas "remesadas", "incluidas en una remesa", "que estén en remesa", "enviadas en remesa", "con remesa".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "IncluidaEnRemesa", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasVenta.9.3 · Situación: Facturas NO incluidas en remesa (`IncluidaEnRemesa`)**
- **Disparador:** Facturas "sin remesar", "pendientes de remesa", "no incluidas en ninguna remesa", "que no tengan remesa".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "IncluidaEnRemesa", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre (R.9.1), se omiten las reglas de situación (R.9.2 y R.9.3).

### R.FacturasVenta.10 · Relación con Contratos

**R.FacturasVenta.10.1 · Búsqueda por datos de Contrato (`NombreContrato`)**
- **Disparador positivo:** Facturas "del contrato [Nombre/Referencia]", "vinculadas al contrato [X]", "del contrato de venta [Y]".
- **Disparador negativo:** Facturas "que NO sean del contrato [X]", "no vinculadas al contrato [Y]", "fuera del contrato [X]".
- **Acción (positivo):** `{"Clausula": "NombreContrato", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreContrato", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en el nombre o referencia del contrato.

**R.FacturasVenta.10.2 · Situación: Facturas vinculadas a un contrato (`AsociadaAUnContrato`)**
- **Disparador:** Facturas "de un contrato", "con contrato de venta", "vinculadas a algún contrato", "que tengan contrato".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "AsociadaAUnContrato", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasVenta.10.3 · Situación: Facturas sin contrato (`AsociadaAUnContrato`)**
- **Disparador:** Facturas "sin contrato", "que no pertenezcan a un contrato", "no contractuales", "pendientes de asignar contrato".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "AsociadaAUnContrato", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre (R.10.1), se omiten las reglas de situación (R.10.2 y R.10.3).

### R.FacturasVenta.11 · Relación con Estimaciones Directas

**R.FacturasVenta.11.1 · Búsqueda por datos de Estimación Directa (`NombreEstimacion`)**
- **Disparador positivo:** Facturas "de la estimación [Nombre/Referencia]", "incluidas en la estimación directa [X]", "de la estimación contable [Y]".
- **Disparador negativo:** Facturas "que NO sean de la estimación [X]", "no incluidas en la estimación directa [Y]", "fuera de la estimación [X]".
- **Acción (positivo):** `{"Clausula": "NombreEstimacion", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreEstimacion", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en el nombre o referencia de la estimación directa.

**R.FacturasVenta.11.2 · Situación: Facturas incluidas en alguna estimación (`VinculosAUnaEstimacion`)**
- **Disparador:** Facturas "en una estimación", "incluidas en estimación directa", "que estén en una estimación contable", "ya estimadas".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "VinculosAUnaEstimacion", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasVenta.11.3 · Situación: Facturas sin estimación (`VinculosAUnaEstimacion`)**
- **Disparador:** Facturas "pendientes de estimación", "no incluidas en ninguna estimación directa", "sin estimación contable", "fuera de estimación".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "VinculosAUnaEstimacion", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre (R.11.1), se omiten las reglas de situación (R.11.2 y R.11.3).

### R.FacturasVenta.12 · Relación con Lotes Contables

**R.FacturasVenta.12.1 · Búsqueda por datos de Lote Contable (`NombreLoteContable`)**
- **Disparador positivo:** Facturas "del lote [Nombre/Referencia]", "incluidas en el lote contable [X]", "del lote de contabilidad [Y]".
- **Disparador negativo:** Facturas "que NO sean del lote [X]", "no incluidas en el lote contable [Y]", "fuera del lote [X]".
- **Acción (positivo):** `{"Clausula": "NombreLoteContable", "Criterio": "contiene", "Valor": "nombre_o_referencia"}`
- **Acción (negativo):** `{"Clausula": "NombreLoteContable", "Criterio": "noContiene", "Valor": "nombre_o_referencia"}`
- **Nota:** Busca coincidencia parcial en el nombre o referencia del lote contable.

**R.FacturasVenta.12.2 · Situación: Facturas incluidas en algún lote contable (`VinculosAUnLote`)**
- **Disparador:** Facturas "en un lote contable", "contabilizadas en lote", "incluidas en lote contable", "que tengan lote", "ya en lote".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "VinculosAUnLote", "Criterio": "igual", "Valor": "5"}` (Representa ConRelacion).

**R.FacturasVenta.12.3 · Situación: Facturas pendientes de lote contable (`VinculosAUnLote`)**
- **Disparador:** Facturas "pendientes de lote", "sin lote contable", "no incluidas en ningún lote", "fuera de lote contable", "sin contabilizar en lote".
- **Acción:** Genera el objeto:
  1. `{"Clausula": "VinculosAUnLote", "Criterio": "igual", "Valor": "6"}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre (R.12.1), se omiten las reglas de situación (R.12.2 y R.12.3).

### R.FacturasVenta.13 · Serie y Año de facturación (`Serie`, `Ano`)
- **Disparador:** Facturas "de la serie [X]", "de la serie A", "del año [AAAA]", "de la serie A de 2025", "emitidas en la serie B".
- **Acción para serie:** `{"Clausula": "Serie", "Criterio": "igual", "Valor": "A"}`
- **Acción para año:** `{"Clausula": "Ano", "Criterio": "igual", "Valor": "2025"}`
- **Nota:** Ambas pueden combinarse. `Serie` es sensible a mayúsculas (usar el valor exacto que indique el usuario).

### R.FacturasVenta.14 · Creador de la factura (`idusuacrea`)
- **Disparador:** Facturas "creadas por [usuario]", "que introdujo [nombre]", "dadas de alta por [login]", "que ha creado [nombre]".
- **Acción:** Busca en `CONTEXTO DE DATOS :: Usuarios` y genera:
  1. `{"Clausula": "idusuacrea", "Criterio": "igual", "Valor": "id_encontrado"}`
  2. Si además pide un período: `{"Clausula": "fechacreacion", "Criterio": "entreFechas", "Valor": "inicio-fin"}`
- **Nota:** No confundir con el cliente; "creador" o "introdujo" refieren siempre al usuario interno del sistema.

### R.FacturasVenta.15 · Facturas rectificativas vs. facturas ordinarias
**R.FacturasVenta.15.1 · Solo rectificativas (notas de abono)**
- **Disparador:** "solo rectificativas", "notas de abono", "facturas correctoras", "abonos emitidos".
- **Acción:** `{"Clausula": "EsRectificativa", "Criterio": "igual", "Valor": "true"}`

**R.FacturasVenta.15.2 · Solo facturas ordinarias (no rectificativas)**
- **Disparador:** "facturas normales", "no rectificativas", "sin nota de abono", "facturas ordinarias".
- **Acción:** `{"Clausula": "EsRectificativa", "Criterio": "igual", "Valor": "false"}`

### R.FacturasVenta.17 · Cantidad de Tareas relacionadas (`CantidadDeTareas`)
- **Disparador:** Facturas "relacionadas con N tareas", "que tengan exactamente N tareas", "con más de N tareas", "con menos de N tareas", "vinculadas a N tareas", "que solo tengan N tareas".
- **Acción:** Genera el objeto con el criterio adecuado:
  - Exactamente N → `{"Clausula": "CantidadDeTareas", "Criterio": "igual", "Valor": "N"}`
  - Más de N → `{"Clausula": "CantidadDeTareas", "Criterio": "mayor", "Valor": "N"}`
  - Al menos N / N o más → `{"Clausula": "CantidadDeTareas", "Criterio": "mayorIgual", "Valor": "N"}`
  - Menos de N → `{"Clausula": "CantidadDeTareas", "Criterio": "menor", "Valor": "N"}`
  - Como mucho N / N o menos → `{"Clausula": "CantidadDeTareas", "Criterio": "menorIgual", "Valor": "N"}`
- **Nota:** El sistema cuenta las tareas que tienen el campo `IdFacturaEmt` apuntando a la factura. Solo usa un número entero en `Valor`.
- **Ejemplo:** "facturas que solo estén relacionadas con 4 tareas" → `{"Clausula": "CantidadDeTareas", "Criterio": "igual", "Valor": "4"}`

### R.FacturasVenta.18 · Cantidad de Partes de Trabajo relacionados (`CantidadDePartesTr`)
- **Disparador:** Facturas "relacionadas con N partes de trabajo", "que tengan exactamente N partes", "con más de N PTR", "con menos de N partes de trabajo", "vinculadas a N partes", "que solo tengan N partes de trabajo".
- **Acción:** Genera el objeto con el criterio adecuado:
  - Exactamente N → `{"Clausula": "CantidadDePartesTr", "Criterio": "igual", "Valor": "N"}`
  - Más de N → `{"Clausula": "CantidadDePartesTr", "Criterio": "mayor", "Valor": "N"}`
  - Al menos N / N o más → `{"Clausula": "CantidadDePartesTr", "Criterio": "mayorIgual", "Valor": "N"}`
  - Menos de N → `{"Clausula": "CantidadDePartesTr", "Criterio": "menor", "Valor": "N"}`
  - Como mucho N / N o menos → `{"Clausula": "CantidadDePartesTr", "Criterio": "menorIgual", "Valor": "N"}`
- **Nota:** Cuenta los partes de trabajo (PTR) que tienen el campo `IdFacturaEmt` apuntando a la factura. Solo usa un número entero en `Valor`.
- **Ejemplo:** "facturas relacionadas con más de 2 partes de trabajo" → `{"Clausula": "CantidadDePartesTr", "Criterio": "mayor", "Valor": "2"}`

### R.FacturasVenta.19 · Cantidad de Planificaciones de Venta relacionadas (`CantidadDePlanificacionesDeVenta`)
- **Disparador:** Facturas "relacionadas con N planificaciones", "que tengan exactamente N PLV", "con más de N planificaciones de venta", "con menos de N PLV", "vinculadas a N planificaciones", "que solo tengan N planificaciones".
- **Acción:** Genera el objeto con el criterio adecuado:
  - Exactamente N → `{"Clausula": "CantidadDePlanificacionesDeVenta", "Criterio": "igual", "Valor": "N"}`
  - Más de N → `{"Clausula": "CantidadDePlanificacionesDeVenta", "Criterio": "mayor", "Valor": "N"}`
  - Al menos N / N o más → `{"Clausula": "CantidadDePlanificacionesDeVenta", "Criterio": "mayorIgual", "Valor": "N"}`
  - Menos de N → `{"Clausula": "CantidadDePlanificacionesDeVenta", "Criterio": "menor", "Valor": "N"}`
  - Como mucho N / N o menos → `{"Clausula": "CantidadDePlanificacionesDeVenta", "Criterio": "menorIgual", "Valor": "N"}`
- **Nota:** Cuenta las planificaciones de venta (PLV) que tienen el campo `IdFacturaEmt` apuntando a la factura. Solo usa un número entero en `Valor`.
- **Ejemplo:** "facturas que tengan exactamente una planificación de venta" → `{"Clausula": "CantidadDePlanificacionesDeVenta", "Criterio": "igual", "Valor": "1"}`

### R.FacturasVenta.21 · Facturas con o sin retención IRPF (`TieneIrpf`)
- **Disparador con IRPF:** "con retención", "que tengan IRPF", "con IRPF", "sujetas a retención", "con retención de IRPF".
  - **Acción:** `{"Clausula": "TieneIrpf", "Criterio": "igual", "Valor": "true"}`
- **Disparador sin IRPF:** "sin retención", "que no tengan IRPF", "sin IRPF", "no sujetas a retención", "sin retención de IRPF".
  - **Acción:** `{"Clausula": "TieneIrpf", "Criterio": "igual", "Valor": "false"}`
- **Regla de agrupación:** Si el usuario pide "agrupar por porcentaje" o "por tipo de retención", usa la clave de agrupación `PorcentajeIrpf` (NO como filtro). El filtro `TieneIrpf=true` ya garantiza que solo aparezcan facturas con IRPF.
- **Regla de importe retenido:** Si el usuario pide la suma o media de la retención, usa `calculado:ImporteIrpf` como campo de métrica.
- **Ejemplo completo:** "cuántas facturas tienen IRPF y agrúpalas por porcentaje aplicado" →
  ```json
  {
    "Filtros": [{"Clausula": "TieneIrpf", "Criterio": "igual", "Valor": "true"}],
    "AgruparPor": ["PorcentajeIrpf"],
    "Metricas": [{"Operacion": "Cuenta", "Campo": "", "Alias": "Facturas"}]
  }
  ```

### R.FacturasVenta.22 · IVA exento (`TieneIvaExento`, `PorcentajeIva`, `BiIvaExento`)

**R.FacturasVenta.22.1 · Filtro: facturas con o sin IVA exento**
- **Disparador con exento:** "con IVA exento", "que tengan IVA al 0%", "exentas de IVA", "con líneas exentas".
  - **Acción:** `{"Clausula": "TieneIvaExento", "Criterio": "igual", "Valor": "true"}`
- **Disparador sin exento:** "sin IVA exento", "todas con IVA", "que no tengan exención".
  - **Acción:** `{"Clausula": "TieneIvaExento", "Criterio": "igual", "Valor": "false"}`

**R.FacturasVenta.22.2 · Agrupación por tipo de IVA**
- **Disparador:** "agrúpalas por tipo de IVA", "por porcentaje de IVA", "distribución de IVA".
- **Clave de agrupación:** `PorcentajeIva` (devuelve la combinación de porcentajes de la factura: "21 %", "Exento", "21 % / 10 %", etc.)

**R.FacturasVenta.22.3 · Métrica: base imponible exenta**
- **Disparador:** "qué media de BI tienen", "base imponible exenta", "cuánto representan en BI", "importe de la base exenta".
- **Campo de métrica:** `calculado:BiIvaExento`

**Ejemplo completo:** "cuántas facturas tienen IVA exento y qué media tienen de BI" →
```json
{
  "Filtros": [{"Clausula": "TieneIvaExento", "Criterio": "igual", "Valor": "true"}],
  "AgruparPor": [],
  "Metricas": [
    {"Operacion": "Cuenta",  "Campo": "",                   "Alias": "Facturas"},
    {"Operacion": "Media",   "Campo": "calculado:BiIvaExento", "Alias": "Media BI exenta"}
  ]
}
```

### R.FacturasVenta.20 · Concepto de línea de factura (`ConceptoDeLinea`)
- **Disparador:** Facturas "que en su detalle incluyan [servicio]", "cuyas líneas contengan [concepto]", "que facturen el servicio de [X]", "que hayan facturado [X] y [Y]", "que incluyan tanto [X] como [Y] en sus líneas".
- **Criterios disponibles:**
  - **`sonTodos` (AND):** Cuando el usuario exige que la factura incluya **todos** los conceptos mencionados (palabras clave como "y", "tanto … como", "también", "además"):
    - `{"Clausula": "ConceptoDeLinea", "Criterio": "sonTodos", "Valor": "concepto1;concepto2"}`
  - **`esAlgunoDe` (OR):** Cuando basta con que la factura incluya **alguno** de los conceptos (palabras clave como "o", "alguno de"):
    - `{"Clausula": "ConceptoDeLinea", "Criterio": "esAlgunoDe", "Valor": "concepto1;concepto2"}`
  - **`contiene` (único término):** Cuando el usuario menciona un único concepto:
    - `{"Clausula": "ConceptoDeLinea", "Criterio": "contiene", "Valor": "limpieza"}`
- **Nota:** Los términos van separados por `;`. El sistema busca coincidencia parcial (contiene) en el campo `Concepto` de cada línea de la factura.
- **Ejemplos:**
  - "facturas que en su detalle hayan facturado el servicio de limpieza y el de consultoría" → `{"Clausula": "ConceptoDeLinea", "Criterio": "sonTodos", "Valor": "limpieza;consultoría"}`
  - "facturas que incluyan mantenimiento o reparación" → `{"Clausula": "ConceptoDeLinea", "Criterio": "esAlgunoDe", "Valor": "mantenimiento;reparación"}`
  - "facturas de suministros" → `{"Clausula": "ConceptoDeLinea", "Criterio": "contiene", "Valor": "suministros"}`
