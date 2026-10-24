# Manual: exportación de preasientos a NCS y ContaSol

Este manual explica cómo, partiendo de los preasientos que se muestran en la pantalla **Gestión de preasientos** (`Preasientos.ts`), se genera un **lote contable** que exporta los asientos a **NCS** o a **ContaSol**, y qué opciones hay después para regenerarlo o anularlo desde los **circuitos documentales** de tipo *lote contable* y *estimación directa*.

---

## 1. Conceptos previos

| Concepto | Qué es |
|---|---|
| **Preasiento** | Asiento contable propuesto que genera SE a partir de un documento de origen: factura emitida, factura recibida, pago, cobro o movimiento de almacén. Tiene apuntes al debe y al haber. |
| **Etapas del preasiento** | `Pendiente` → `Contabilizado`. También puede estar `Cancelado` o `Anulado`. Solo se exportan los **pendientes**. |
| **Lote contable** | Circuito documental (tipo definido en el parámetro `CAD_Tipo_Para_Lote_de_Preasientos`) que agrupa los preasientos exportados y guarda, como archivos bloqueados, los ficheros que se importan en el programa de contabilidad. |
| **Estimación directa** | Circuito documental (tipo definido en `CAD_Tipo_Para_Estimacion_Directa`) que agrupa facturas emitidas, facturas recibidas y pagos de una sociedad que tributa en estimación directa. Genera los ficheros de NCS para este régimen. |
| **Trabajo sometido** | La creación y la anulación de lotes no se hacen al momento: se lanza un trabajo en segundo plano. Al acabar, el usuario recibe un correo con el enlace al lote, y el trabajo guarda trazas y errores. |

### 1.1 ¿Qué determina si se exporta a NCS o a ContaSol?

**No se elige en pantalla.** Lo decide la **sociedad**, mediante parámetros de negocio de *Preasientos*. Cada parámetro es un JSON con una entrada por sociedad:

| Parámetro | Para qué sirve | Valores de `Metodo` |
|---|---|---|
| `SPR_Contabilizar_En` | Cómo se crea el lote contable | `ContabilizarEnNcs` (NCS, contabilidad general)<br>`EstimacionDirectaEnNcs` (NCS, estimación directa)<br>`ContabilizarEnContasol` (ContaSol, contabilidad general) |
| `SPR_RegenerarLote_Para` | Cómo se regenera un lote ya creado | `RegenerarLoteContable` (NCS)<br>`RegenerarLoteContasol` (ContaSol) |
| `SPR_CuentasDeTerceros_En` | Cómo se crea el lote de terceros desde la sociedad | `ExportarTodosLosTercerosNcs` |
| `SPR_Plan_Contable` | Código de empresa / plan contable que se usa en el nombre de los ficheros | — |

Ejemplos:

```json
// SPR_Contabilizar_En
[
  { "IdSociedad": 1, "Metodo": "ContabilizarEnNcs",      "Nombre": "Contabilizar en NCS" },
  { "IdSociedad": 2, "Metodo": "EstimacionDirectaEnNcs", "Nombre": "Estimación directa en NCS" },
  { "IdSociedad": 3, "Metodo": "ContabilizarEnContasol", "Nombre": "Contabilizar en ContaSol", "Digitos": 8 }
]

// SPR_RegenerarLote_Para
[
  { "IdSociedad": 1, "Metodo": "RegenerarLoteContable", "Nombre": "Regenerar lote NCS" },
  { "IdSociedad": 3, "Metodo": "RegenerarLoteContasol", "Nombre": "Regenerar lote ContaSol" }
]
```

> **ContaSol** necesita además `"Digitos"` (entre 4 y 10): es la longitud de las subcuentas en ContaSol. Si falta o está fuera de rango, la exportación falla.
>
> El `Nombre` se usa para nombrar el lote: `"<Nombre> el yyyy-MM-dd HH-mm"`.

---

## 2. Exportar desde la pantalla de preasientos

### 2.1 Preparar la selección (filtros)

El lote se crea con **los preasientos que cumplen los filtros de la grid** en ese momento. No se usa la selección de filas: se usa el filtro.

1. Abre **Contabilidad → Preasientos**.
2. Filtra hasta ver los preasientos que quieres exportar: fechas, tipo, origen, centro gestor, etc.
3. **No filtres por estado.** Si lo haces, el proceso da error: *"Está intentando contabilizar y ha filtrado por estados, esto no está permitido"*. El estado lo decide el propio proceso, según la opción *Descontabilizar* de la ventana (ver 2.2):
   - *Descontabilizar* **desmarcado**: solo se incluyen los preasientos **pendientes**. Los contabilizados que cumplan el filtro se ignoran.
   - *Descontabilizar* **marcado**: se incluyen los **pendientes y los contabilizados**. Los contabilizados se devuelven primero a pendiente y se sacan de su lote, y después todos entran en el lote nuevo.
4. Si quieres ver los importes antes de exportar, usa **Mostrar totales**. Muestra los totales por cuenta de lo filtrado.

### 2.2 Lanzar «Crear lote contable»

Menú de la grid → **Crear lote contable**. Se abre una ventana con estos datos:

| Campo | Obligatorio | Descripción |
|---|---|---|
| **Sociedad** | Sí | Sociedad cuyos preasientos se van a contabilizar. El proceso añade el filtro por sociedad, así que solo se exportan los de esa sociedad aunque la grid muestre otros. Su configuración en `SPR_Contabilizar_En` decide si se exporta a NCS o a ContaSol. |
| **Ejercicio contable** | Sí | Ejercicio que se procesa. No puede ser mayor que el año actual ni anterior al año pasado. Solo entran los preasientos de ese ejercicio. Los de otro ejercicio se descartan y quedan anotados en la traza del trabajo. |
| **Fecha contable (reemplaza la definida)** | No | Si la indicas, **todos** los asientos usan esa fecha, y el ejercicio pasa a ser el de esa fecha. Si la dejas vacía, cada preasiento usa su propia fecha contable. |
| **Respetar fechas contables** | No | **Marcado:** cada asiento se exporta con su fecha contable (o con la fecha indicada arriba).<br>**Desmarcado:** si la fecha cae en un trimestre ya pasado, se lleva al **primer día del trimestre actual**. Si es del año anterior, se lleva al 1 de enero del año en curso. Así no se registran asientos en periodos que ya se han declarado. |
| **Descontabilizar (si lo están)** | No | Amplía la selección: además de los *pendientes*, incluye los preasientos filtrados que estén *contabilizados*. Antes de generar el lote, devuelve esos contabilizados a *pendiente* y los saca de su lote anterior; si ese lote se queda sin preasientos, se cancela. Después entran en el lote nuevo junto con los pendientes. Si no se marca, los contabilizados se ignoran. |

Al aceptar verás el mensaje *"Trabajo de creación de lote contable sometido correctamente"*. A partir de ahí el proceso sigue en segundo plano.

### 2.3 Qué hace el trabajo

1. Selecciona los preasientos que cumplen los filtros, son de la sociedad y están **pendientes** (más los contabilizados si marcaste *Descontabilizar*).
2. Valida cada preasiento (`DimeSiEstaBien`): cuentas, cuadre, datos del tercero, etc. Los que no pasan la validación **no se exportan**, y el motivo queda en la traza del preasiento y en los errores del trabajo.
3. Descarta los preasientos de otro ejercicio. Cancela los preasientos cuyo documento de origen está cancelado.
4. Pasa los preasientos válidos a la etapa **Contabilizado**.
5. Genera los ficheros según el sistema contable (ver apartados 3 y 4).
6. Crea un **circuito documental de tipo lote contable**, vincula los preasientos y anexa los ficheros **bloqueados**.
7. Marca los proveedores y clientes exportados como **migrados** a ese sistema y plan contable, para que no se vuelvan a exportar en lotes posteriores.
8. Envía un **correo** al usuario que lanzó el trabajo con el enlace al lote.

Si algo falla de forma global, el trabajo hace *rollback* y ningún preasiento cambia de etapa.

### 2.4 Contabilizar un único preasiento

Desde la **edición** de un preasiento, como administrador, tienes la opción **Crear lote contable** (contabilizar preasiento). Esta opción:

- Se ejecuta **en el momento**, sin trabajo sometido.
- No se permite si el preasiento está cancelado o anulado. En ese caso hay que regenerarlo primero.
- Si el preasiento ya estaba contabilizado, primero lo descontabiliza y lo saca de su lote.
- Usa **Respetar fecha contable = sí**, sin fecha de reemplazo.
- Crea un lote con ese único preasiento, con el sistema contable configurado en la sociedad.

Desde la edición también tienes **Ir al lote contable**, que abre el último lote en el que está el preasiento, e **Ir al origen**, que abre la factura, pago, cobro o documento de almacén.

---

## 3. Exportación a NCS

### 3.1 Contabilidad general en NCS (`ContabilizarEnNcs`)

Se usa para sociedades que llevan contabilidad general en NCS. Se lanza desde la pantalla de **Preasientos** (apartado 2) o desde **Lotes contables** (apartado 5.1).

Ficheros que se anexan al lote:

| Fichero | Contenido |
|---|---|
| `DiarioNNNN_AAAA.xml` | Diario de asientos. `NNNN` es el código de empresa / plan contable (`SPR_Plan_Contable`) y `AAAA` el ejercicio. Un asiento por preasiento: asiento con IVA en facturas, asiento libre en pagos, cobros y almacén. |
| `CtaDetalleNNNN_AAAA.xml` | Proveedores y clientes que **aún no estaban migrados** a NCS para ese plan contable. Solo se genera si hay terceros nuevos. Este archivo también se vincula a cada proveedor y cliente. |

Importación en NCS: primero se importan los terceros (`CtaDetalle…`) y después el diario (`Diario…`). La estructura de los ficheros está en la carpeta `Documentación/Preasientos/NCS`.

### 3.2 Estimación directa en NCS (`EstimacionDirectaEnNcs`)

Se usa para sociedades en **estimación directa**. **No parte de los preasientos**: parte de las **facturas emitidas, facturas recibidas y pagos** de la sociedad. Se lanza desde **Circuitos documentales → Estimaciones directas → Generar estimación directa** (apartado 5.2).

Para cada **actividad** configurada en `SPR_Codigos_De_Actividad` (sociedad + negocio + tipo → actividad), el proceso:

- Selecciona los documentos del ejercicio que no están en ninguna estimación, o que solo están en estimaciones canceladas:
  - **Facturas emitidas**: que no sean prefactura ni estén anuladas, y cuyo año sea el ejercicio.
  - **Facturas recibidas**: en etapa de pago o pagadas, contabilizadas, y con fecha de factura en el ejercicio.
  - **Pagos**: pagados, con naturaleza, y con fecha de pago en el ejercicio.
- Genera `DiarioEOS_<empresa>_<ejercicio>_<actividad>.xml` y, si hay terceros, `Terceros<empresa>.xml`.
- Crea **un circuito de estimación directa por actividad** y le vincula los documentos.
- Cada actividad va en su propia transacción. Si una falla, las demás siguen adelante y el error queda en el trabajo.

> Para estas sociedades **no hay que usar la pantalla de Preasientos para exportar**. Hay que ir a Estimaciones directas.

---

## 4. Exportación a ContaSol (`ContabilizarEnContasol`)

Se usa para sociedades que llevan contabilidad general en ContaSol. Se lanza igual que NCS, desde **Preasientos** o desde **Lotes contables**.

Se anexa al lote un único **zip**, `ContasolNNNN_AAAA.zip`, con los ficheros Excel que ContaSol importa con nombre fijo:

| Fichero | Contenido |
|---|---|
| `APU.xlsx` | Apuntes del diario de todos los preasientos. El número de asiento va en blanco porque lo asigna ContaSol, y el orden 1 marca el inicio de cada asiento. Los apuntes del tercero llevan el tipo de IVA (S/R) y el código de IVA, que enlazan con IVS/IVR. Si un preasiento está descuadrado, falla la generación. |
| `IVS.xlsx` | Registro de IVA soportado de las facturas recibidas (hasta 5 bases). |
| `IVR.xlsx` | Registro de IVA repercutido de las facturas emitidas. |
| `PRO.xlsx` | Proveedores aún no migrados a ContaSol. |
| `CLI.xlsx` | Clientes aún no migrados a ContaSol. |

Las subcuentas se ajustan a los `Digitos` configurados en la sociedad. La fecha de cada asiento sigue la misma regla de *Respetar fechas contables* que NCS.

Importación en ContaSol: descomprime el zip en una carpeta y usa la importación desde Excel/Calc, empezando por PRO/CLI y siguiendo con APU, IVS e IVR. La documentación está en `Documentación/Preasientos/ContaSol` (*CONTASOL_Importación Excel_Calc.pdf* y las plantillas XLSX).

> **Contabilidad general en ContaSol** es la única modalidad disponible para ContaSol. No hay estimación directa para ContaSol.

---

## 5. Opciones desde circuitos documentales

### 5.1 Lotes contables (Circuitos documentales → Lotes contables)

La pantalla muestra solo los circuitos del tipo *lote contable*. El filtro **Varios** permite buscar por nombre del lote (`l:texto`) o por referencia de un preasiento (`p:referencia`). Al editar un lote, el expansor **Preasientos** lista los preasientos vinculados, y en *Archivos* están los ficheros generados.

| Opción | Dónde | Quién | Qué hace |
|---|---|---|---|
| **Crear lote contable** | Menú de la grid | Gestor | Abre la misma ventana del apartado 2.2, pero **sin la opción Descontabilizar**. Crea el lote con los preasientos **pendientes** de la sociedad y el ejercicio, sin el filtro de la pantalla de preasientos. Es útil para exportar «todo lo pendiente». Usa `SPR_Contabilizar_En`, igual que desde Preasientos. |
| **Regenerar lote contable** | Edición del lote | Administrador del lote | Ver 5.1.1. |
| **Anular lote contable** | Edición del lote | Administrador del lote | Ver 5.1.2. |

#### 5.1.1 Regenerar lote contable

Sirve para **volver a generar los ficheros** de un lote cuando has corregido algo después de crearlo: una cuenta del plan, los datos de un tercero, los parámetros, un error del generador, etc. **No cambia qué preasientos forman el lote.**

Requisitos:

- El circuito tiene que ser de tipo *lote contable*.
- El lote tiene que estar en etapa **Abierto**.
- Tienes que ser **administrador** del lote.
- La sociedad tiene que tener configurado `SPR_RegenerarLote_Para`.

Qué hace (de forma síncrona, en una transacción):

1. Marca como **cancelados** los archivos que ya tenía el lote. Se conservan como histórico.
2. Vuelve a generar los ficheros con los **mismos preasientos vinculados**:
   - **NCS** (`RegenerarLoteContable`): genera un `Diario…xml` nuevo. **No regenera el fichero de terceros**, porque esos terceros ya están marcados como migrados.
   - **ContaSol** (`RegenerarLoteContasol`): genera un zip nuevo e incluye **todos** los proveedores y clientes de los preasientos del lote. Al importar, marca en ContaSol el control de duplicados para que se sustituyan los que ya existan.
3. Usa *Respetar fecha contable = sí*, sin fecha de reemplazo y con el ejercicio actual.
4. Anexa los ficheros nuevos bloqueados y deja una traza *"El lote ha sido regenerado por el usuario …"*.

> Si lo que quieres es **cambiar el contenido** de un asiento, primero regenera el **preasiento** (apartado 6) y después regenera el **lote**. Si tienes que **quitar o añadir preasientos**, anula el lote y crea uno nuevo.

#### 5.1.2 Anular lote contable

Se usa cuando el lote no se ha importado, o hay que deshacerlo, y quieres que sus preasientos vuelvan a estar disponibles.

- Lanza un **trabajo sometido**, con un semáforo que impide anular dos veces el mismo lote a la vez.
- Pasa el lote a **Cancelado**.
- Todos los preasientos del lote vuelven a **Pendiente**, con una traza *"Retrocedido la contabilización"*.
- Si el lote ya estaba **Cerrado** (contabilizado), también se puede cancelar, y queda una traza de *Anulación de contabilización*.

Después puedes crear un lote nuevo con esos preasientos, con las mismas opciones o con otras.

### 5.2 Estimaciones directas (Circuitos documentales → Estimaciones directas)

| Opción | Dónde | Quién | Qué hace |
|---|---|---|---|
| **Generar estimación directa** | Menú de la grid | Gestor | Abre la ventana con **Sociedad** y **Ejercicio**. Fecha contable, Respetar fechas y Descontabilizar están ocultas porque no aplican; si se envían, el servidor da error. Lanza el trabajo `EstimacionDirectaEnNcs` del apartado 3.2. |
| **Anular estimación** | Edición del circuito | Administrador | Trabajo sometido que pasa la estimación a **Cancelado** y desvincula sus facturas emitidas, facturas recibidas y pagos. Así vuelven a estar pendientes y entran en la próxima estimación. |

En la edición de una estimación, los expansores **Facturas Emitidas**, **Facturas Recibidas** y **Pagos** muestran los documentos incluidos.

> Las estimaciones directas **no tienen opción de regenerar**. Para rehacer una, anúlala y genérala de nuevo.

### 5.3 Lote de terceros (Sociedades)

Desde la **edición de una sociedad**, como administrador, la opción **Crear lote terceros** lanza un trabajo que usa `SPR_CuentasDeTerceros_En` (`ExportarTodosLosTercerosNcs`). Exporta a NCS **todos los proveedores y clientes no dados de baja** y crea un circuito de lote de terceros con el XML. Es útil para la carga inicial de terceros en NCS.

---

## 6. Regenerar preasiento (pantalla de preasientos)

No hay que confundirlo con *Regenerar lote*. La opción **Regenerar preasiento**, en la edición de un preasiento, vuelve a calcular el preasiento a partir de su **documento de origen**. Sirve, por ejemplo, después de corregir una factura, una cuenta contable o la parametrización.

Requisitos:

- Ser **administrador** del preasiento.
- Que el preasiento esté en **Pendiente**. Si está contabilizado, primero hay que anular su lote o descontabilizarlo.
- Que el origen sea una factura emitida, una factura recibida, un pago, un cobro o un movimiento de almacén.

Qué hace:

- Si el documento de origen apunta a **este** preasiento, le pide a su gestor que lo genere de nuevo.
- Si el documento de origen apunta a **otro** preasiento más reciente, **cancela** este.

Después recarga la cabecera, los hitos, las trazas y los archivadores del preasiento.

---

## 7. Flujos habituales

### A. Exportación periódica (contabilidad general, NCS o ContaSol)
1. Preasientos → filtra el periodo (sin filtrar por estado) → **Mostrar totales** para revisar.
2. **Crear lote contable** → sociedad, ejercicio, *Respetar fechas* según convenga.
3. Espera el correo. Abre el lote y descarga el XML (NCS) o el zip (ContaSol).
4. Importa el fichero en el programa de contabilidad.
5. Revisa los errores del trabajo: los preasientos que no pasaron la validación siguen pendientes.

### B. Error detectado después de generar el lote
- **Error de configuración o de un tercero** (el preasiento está bien): corrígelo → **Regenerar lote contable** → vuelve a importar el fichero nuevo.
- **Error en un preasiento concreto**: **Anular lote contable** → corrige el origen → **Regenerar preasiento** → crea un lote nuevo.
- **Quieres volver a exportar preasientos ya contabilizados** sin anular el lote entero: filtra esos preasientos → **Crear lote contable** con **Descontabilizar** marcado.

### C. Sociedad en estimación directa
1. Estimaciones directas → **Generar estimación directa** → sociedad y ejercicio.
2. Se crea un circuito por actividad con `DiarioEOS_…xml` (y terceros, si los hay).
3. Si hay que rehacerla: **Anular estimación** → **Generar estimación directa** de nuevo.

---

## 8. Errores frecuentes

| Mensaje | Causa / solución |
|---|---|
| *Ha de configurar el parámetro 'SPR_Contabilizar_En' para la sociedad…* | Falta la entrada de la sociedad en el parámetro. |
| *Ha de configurar el parámetro 'SPR_RegenerarLote_Para'…* | Falta la configuración para regenerar los lotes de esa sociedad. |
| *…los dígitos de las cuentas de ContaSol ("Digitos": entre 4 y 10)* | Añade `"Digitos"` en `SPR_Contabilizar_En` para esa sociedad. |
| *Está intentando contabilizar y ha filtrado por estados…* | Quita el filtro por estado de la grid de preasientos. |
| *El ejercicio 'X' no puede ser mayor que el ejercicio actual / menor dos años…* | Solo se pueden procesar el ejercicio actual y el anterior. |
| *No hay preasientos para enviar a … para la sociedad…* | No hay preasientos pendientes y válidos con esos filtros. |
| *El preasiento '…' está descuadrado* (ContaSol) | El debe no coincide con el haber. Regenera el preasiento. |
| *El circuito '…' no está abierto, no se puede regenerar* | Solo se regeneran los lotes en etapa Abierto. |
| *El preasiento '…' está anulado o cancelado, regenérelo* | Usa **Regenerar preasiento** antes de contabilizarlo. |
