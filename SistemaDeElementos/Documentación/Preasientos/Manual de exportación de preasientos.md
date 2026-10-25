# Manual: cierres contables y exportación de preasientos a NCS y ContaSol

Este manual explica cómo se traspasan a **NCS** o a **ContaSol** los preasientos que se ven en la pantalla **Gestión de preasientos** (`Preasientos.ts`).

El traspaso se hace mediante un **cierre contable** de la sociedad (negocio *CierreContable*, pantalla **Contabilidad → Cierres contables**). El cierre crea el **lote contable** con los ficheros de importación, y al eliminar el cierre se anula ese lote.

La **estimación directa** no cambia: se sigue generando desde los circuitos documentales de tipo *estimación directa*.

---

## 1. Conceptos previos

| Concepto | Qué es |
|---|---|
| **Preasiento** | Asiento contable propuesto que genera SE a partir de un documento de origen: factura emitida, factura recibida, pago, cobro o movimiento de almacén. Tiene apuntes al debe y al haber. |
| **Etapas del preasiento** | `Pendiente` → `Contabilizado`. También puede estar `Cancelado` o `Anulado`. Solo se traspasan los **pendientes**. |
| **Cierre contable** | Registro que indica **hasta qué fecha está cerrada una sociedad**. Cada cierre tiene un único lote contable asociado y muestra cuántos asientos de cada origen contiene: recibidas, cobros, pagos, emitidas y almacén. |
| **Lote contable** | Circuito documental (tipo definido en `CAD_Tipo_Para_Lote_de_Preasientos`) que agrupa los preasientos traspasados y guarda, como archivos bloqueados, los ficheros que se importan en la contabilidad. |
| **Estimación directa** | Circuito documental (tipo definido en `CAD_Tipo_Para_Estimacion_Directa`) que agrupa facturas emitidas, facturas recibidas y pagos de una sociedad en estimación directa, y genera los ficheros de NCS para ese régimen. |
| **Trabajo sometido** | Crear y eliminar un cierre no se hace al momento: se lanza un trabajo en segundo plano. Al terminar, el usuario recibe un correo con el enlace al lote, y el trabajo guarda trazas y errores. |

### 1.1 Parametrización

La parametrización es la misma que antes. Son parámetros de negocio de *Preasientos* en formato JSON, con una entrada por sociedad:

| Parámetro | Para qué sirve | Valores de `Metodo` |
|---|---|---|
| `SPR_Contabilizar_En` | Con qué sistema contable se genera el lote | `ContabilizarEnNcs` (NCS, contabilidad general)<br>`ContabilizarEnContasol` (ContaSol, contabilidad general)<br>`EstimacionDirectaEnNcs` (NCS, estimación directa) |
| `SPR_RegenerarLote_Para` | Cómo se regenera un lote (ver 5.3) | `RegenerarLoteContable` (NCS)<br>`RegenerarLoteContasol` (ContaSol) |
| `SPR_CuentasDeTerceros_En` | Lote de terceros desde la sociedad | `ExportarTodosLosTercerosNcs` |
| `SPR_Plan_Contable` | Código de empresa / plan contable, que se usa en el nombre de los ficheros | — |

```json
// SPR_Contabilizar_En
[
  { "IdSociedad": 1, "Metodo": "ContabilizarEnNcs",      "Nombre": "Contabilizar en NCS" },
  { "IdSociedad": 2, "Metodo": "EstimacionDirectaEnNcs", "Nombre": "Estimación directa en NCS" },
  { "IdSociedad": 3, "Metodo": "ContabilizarEnContasol", "Nombre": "Contabilizar en ContaSol", "Digitos": 8 }
]
```

> **Novedad:** no hace falta editar `SPR_Contabilizar_En` a mano para la contabilidad general. La ventana de **Realizar cierre** muestra la contabilidad y los dígitos que tiene configurados la sociedad. Si eliges otros, el sistema **actualiza el parámetro** de esa sociedad antes de lanzar el cierre:
> - **NCS**: siempre 11 dígitos; el campo no se puede modificar.
> - **ContaSol**: entre 4 y 10 dígitos. Por defecto, los que tenga la sociedad, o 8 si no tiene ninguno.

---

## 2. Revisar los preasientos antes de cerrar

La pantalla **Contabilidad → Preasientos** ya **no crea lotes contables**: se han quitado *Crear lote contable* de la lista y *Contabilizar preasiento* de la edición. Sirve para revisar y corregir antes del cierre:

- **Filtrar y Mostrar totales**: muestra los totales por cuenta de los preasientos filtrados. Es útil para ver qué va a entrar en el cierre: preasientos **pendientes** de la sociedad con fecha contable hasta la fecha de cierre.
- **Regenerar preasiento** (edición): ver apartado 6.
- **Ir al origen** / **Ir al lote contable**: abren el documento de origen, o el lote en el que está el preasiento.

---

## 3. Realizar un cierre contable

### 3.1 Dónde y quién

**Contabilidad → Cierres contables** → menú de la lista → **Realizar cierre**.

- Solo aparecen las sociedades **gestionadas** (con centros gestores), que **no son de autónomos** y de las que el usuario conectado es **interventor**.
- La pantalla es de consulta: los cierres solo los crean y eliminan los trabajos sometidos. No se pueden editar ni borrar a mano.

### 3.2 Datos de la ventana

| Campo | Descripción |
|---|---|
| **Sociedad** | Sociedad que se cierra. Al seleccionarla se rellenan *Último cierre*, *Contabilizar en* y *Dígitos*. |
| **Último cierre** | Solo informativo. Es la fecha hasta la que ya está cerrada la sociedad; si nunca se ha cerrado, el 1 de enero del año en curso. |
| **Cerrar hasta** | Fecha hasta la que se cierra. Por defecto, hoy. Tiene que ser **posterior al último cierre** y **no posterior a hoy**. **Su año es el ejercicio que se cierra**, que tiene que ser el actual o el anterior. |
| **Contabilizar en** | NCS o ContaSol. Se propone la contabilidad configurada en la sociedad. Si no tiene ninguna, aparece *Seleccionar…* y hay que elegir una. |
| **Dígitos** | Longitud de las cuentas auxiliares: 11 fijos en NCS; entre 4 y 10 en ContaSol. |

### 3.3 Validaciones al aceptar

El servidor comprueba lo siguiente antes de lanzar el trabajo, y lo vuelve a comprobar al ejecutarlo:

1. El usuario es **interventor** de la sociedad.
2. *Cerrar hasta* es posterior al último cierre y no posterior a hoy.
3. Hay **preasientos pendientes** de la sociedad del ejercicio con fecha contable **hasta** *Cerrar hasta*. Si no hay ninguno, no se cierra.
4. No quedan **preasientos pendientes de ejercicios anteriores**. Si quedan, el mensaje indica que hay que darlos por contabilizados o cerrar antes ese ejercicio.
5. Si se ha cambiado la contabilidad o los dígitos, se actualiza `SPR_Contabilizar_En`.

Al aceptar verás el mensaje *"Trabajo de creación del cierre contable sometido correctamente"*.

### 3.4 Qué hace el trabajo

Todo se hace en **una única transacción**. Si algo falla de forma global, se deshace todo.

1. **Ajusta las fechas anteriores al último cierre.** Si hay preasientos pendientes con fecha contable anterior al último cierre, su fecha pasa a ser la **fecha del último cierre**, y queda anotado en la traza del trabajo. Así nunca se contabiliza en un periodo ya cerrado. El cambio de fecha es permanente: se mantiene aunque después se elimine el cierre.
2. **Selecciona los preasientos**: los pendientes de la sociedad, del ejercicio, con fecha contable hasta *Cerrar hasta*. No se aplica ningún otro filtro ni se descontabiliza nada.
3. **Valida cada preasiento** (`DimeSiEstaBien`): cuentas, cuadre, tercero, etc. Los que no pasan la validación **se quedan pendientes**, y el motivo queda en la traza del preasiento y en los errores del trabajo. Se cancelan los preasientos cuyo documento de origen está cancelado.
4. Pasa los preasientos válidos a **Contabilizado** y los traspasa **respetando su fecha contable**.
5. Genera los ficheros del sistema contable (apartado 4), crea el **lote contable**, vincula los preasientos y anexa los ficheros **bloqueados**.
6. Marca los proveedores y clientes exportados como **migrados** a ese sistema y plan contable.
7. **Registra el cierre**: sociedad, lote y *Cerrada el* = fecha de *Cerrar hasta*.
8. Envía un **correo** al usuario con el enlace al lote.

### 3.5 Consultar los cierres

La lista de **Cierres contables** se puede filtrar por **lote** (nombre o referencia), por **sociedad** y por rango de fechas de **Cerrada el**. Cada fila muestra:

- el número de asientos por origen;
- un enlace a la **sociedad**;
- un enlace al **lote contable**, donde se descargan los ficheros y se ven los preasientos.

---

## 4. Ficheros que se generan

### 4.1 NCS, contabilidad general (`ContabilizarEnNcs`)

| Fichero | Contenido |
|---|---|
| `DiarioNNNN_AAAA.xml` | Diario de asientos. `NNNN` es el plan contable (`SPR_Plan_Contable`) y `AAAA` el ejercicio. Un asiento por preasiento: asiento con IVA en facturas, asiento libre en pagos, cobros y almacén. |
| `CtaDetalleNNNN_AAAA.xml` | Proveedores y clientes aún no migrados a NCS. Solo se genera si hay terceros nuevos. |

Importación: primero los terceros (`CtaDetalle…`) y después el diario. La documentación está en `Documentación/Preasientos/NCS`.

### 4.2 ContaSol, contabilidad general (`ContabilizarEnContasol`)

Se anexa un único zip, `ContasolNNNN_AAAA.zip`, con estos ficheros:

| Fichero | Contenido |
|---|---|
| `APU.xlsx` | Apuntes del diario. El número de asiento lo asigna ContaSol. Si un preasiento está descuadrado, falla la generación. |
| `IVS.xlsx` / `IVR.xlsx` | IVA soportado (facturas recibidas) y repercutido (facturas emitidas). |
| `PRO.xlsx` / `CLI.xlsx` | Proveedores y clientes aún no migrados a ContaSol. |

Las subcuentas se ajustan a los *Dígitos* de la sociedad. Importación: descomprime el zip e impórtalo desde Excel/Calc, empezando por PRO/CLI y siguiendo con APU, IVS e IVR. La documentación está en `Documentación/Preasientos/ContaSol`.

---

## 5. Eliminar un cierre (anular el lote)

**Contabilidad → Cierres contables** → selecciona **un** cierre → menú de la fila → **Eliminar cierre**.

- Solo se puede eliminar el **último cierre** de la sociedad. La opción solo se activa si el cierre seleccionado es el último. Si hay cierres posteriores, hay que eliminarlos antes, empezando por el más reciente.
- Se lanza un **trabajo sometido**, con un semáforo que impide anular dos veces el mismo lote a la vez. El trabajo:
  1. Pasa el **lote contable** a **Cancelado**.
  2. Devuelve todos sus preasientos a **Pendiente** y los desvincula del lote, con la traza *"Retrocedido la contabilización"*.
  3. **Elimina el cierre**. La sociedad vuelve a estar cerrada hasta la fecha del cierre anterior.

Después puedes corregir lo necesario y **Realizar cierre** de nuevo.

### 5.1 La pantalla de lotes contables

**Circuitos documentales → Lotes contables** queda como **consulta**: ver el lote, sus preasientos (expansor *Preasientos*) y descargar sus ficheros. El filtro **Varios** busca por nombre del lote (`l:texto`) o por referencia de un preasiento (`p:referencia`).

Ya **no** tiene *Crear lote contable* ni *Anular lote contable*: esas operaciones se hacen con *Realizar cierre* y *Eliminar cierre*.

### 5.2 Corregir un cierre

| Situación | Qué hacer |
|---|---|
| Un preasiento estaba mal | **Eliminar cierre** → corregir el origen → **Regenerar preasiento** → **Realizar cierre** con la misma fecha. |
| Mala configuración (cuentas, terceros, parámetros) | **Eliminar cierre** → corregir → **Realizar cierre**. |
| El error está en un cierre que no es el último | Eliminar los cierres en orden, del más reciente al afectado, y volver a cerrarlos. |

### 5.3 Regenerar lote contable

La opción **Regenerar lote contable** se ha quitado del menú de lotes contables, así que ahora no se puede usar desde la interfaz. Para rehacer los ficheros de un lote hay que eliminar su cierre y volver a cerrar (5.2).

El parámetro `SPR_RegenerarLote_Para` y los métodos `RegenerarLoteContable` / `RegenerarLoteContasol` siguen existiendo por si se vuelve a habilitar.

---

## 6. Regenerar preasiento

En la **edición de un preasiento**, para los usuarios del parámetro `SPR_Usuarios_Con_Permiso_Para_Generar` que sean administradores del preasiento, la opción **Regenerar preasiento** vuelve a calcularlo a partir de su documento de origen.

- Solo funciona con preasientos **pendientes**. Si el preasiento ya está en un cierre, primero hay que eliminar ese cierre.
- Orígenes admitidos: factura emitida, factura recibida, pago, cobro y movimiento de almacén.
- Si el documento de origen apunta a otro preasiento más reciente, este se **cancela**.

---

## 7. Estimación directa (sin cambios)

Las sociedades en estimación directa (`EstimacionDirectaEnNcs`) **no usan los cierres contables**. Todo se hace desde **Circuitos documentales → Estimaciones directas**.

| Opción | Quién | Qué hace |
|---|---|---|
| **Generar estimación directa** (menú de la lista) | Gestor | Pide **Sociedad** y **Ejercicio**. Lanza un trabajo que, **por cada actividad** de `SPR_Codigos_De_Actividad`, selecciona los documentos del ejercicio que no están en ninguna estimación, o solo en estimaciones canceladas:<br>• facturas emitidas que no sean prefactura ni estén anuladas;<br>• facturas recibidas contabilizadas, en etapa de pago o pagadas;<br>• pagos pagados con naturaleza.<br>Genera `DiarioEOS_<empresa>_<ejercicio>_<actividad>.xml` (y `Terceros<empresa>.xml`, si hay terceros) y crea un circuito por actividad. |
| **Anular estimación** (edición) | Administrador | Trabajo sometido que cancela la estimación y desvincula sus facturas y pagos, que vuelven a estar disponibles. |

Las estimaciones directas no tienen opción de regenerar: se anulan y se generan de nuevo.

## 8. Lote de terceros (Sociedades)

En la **edición de una sociedad**, como administrador, **Crear lote terceros** exporta a NCS todos los proveedores y clientes no dados de baja (`SPR_CuentasDeTerceros_En`). Es útil para la carga inicial.

---

## 9. Flujo habitual de un cierre

1. **Preasientos**: filtra por la sociedad y el periodo, revisa con **Mostrar totales** y corrige o **regenera** lo necesario.
2. **Cierres contables → Realizar cierre**: sociedad, *Cerrar hasta*, y comprueba la contabilidad y los dígitos.
3. Espera el correo, abre el lote desde el cierre y descarga el XML (NCS) o el zip (ContaSol).
4. Importa el fichero en la contabilidad.
5. Revisa los errores del trabajo: los preasientos que no pasaron la validación siguen pendientes, y entrarán en el siguiente cierre con la fecha del último cierre.
6. Si hay que rehacerlo: **Eliminar cierre** → corregir → **Realizar cierre**.

---

## 10. Errores frecuentes

| Mensaje | Causa / solución |
|---|---|
| *No puede cerrar la sociedad '…' por no ser interventor de la misma* | El usuario no es interventor de la sociedad. |
| *…ha de ser posterior a la fecha de su último cierre…* / *…por ser posterior al día de hoy* | *Cerrar hasta* fuera de rango. |
| *No hay preasientos contables de la sociedad '…' para traspasar en el ejercicio…* | No hay pendientes hasta esa fecha. |
| *…tiene preasientos pendientes con fecha contable anterior al ejercicio…* | Cierra antes el ejercicio anterior, o da por contabilizados esos asientos. |
| *El ejercicio 'X' no puede ser mayor… / menor dos años…* | Solo se pueden cerrar el ejercicio actual y el anterior. |
| *En la contabilidad Ncs las cuentas son de '11' dígitos* / *En la contabilidad ContaSol los dígitos… entre 4 y 10* | Dígitos no válidos para la contabilidad elegida. |
| *Ha de indicar en qué contabilidad se contabiliza la sociedad* | Elige NCS o ContaSol en *Contabilizar en*. |
| *No se puede anular el cierre de la sociedad '…' de fecha '…' por tener cierres posteriores* | Elimina antes los cierres posteriores. |
| *El preasiento '…' está descuadrado* (ContaSol) | Regenera el preasiento. |
| *Ha de configurar el parámetro 'SPR_Contabilizar_En' para la sociedad…* | Falta la configuración de la sociedad. Al realizar el cierre se crea con la contabilidad elegida. |
