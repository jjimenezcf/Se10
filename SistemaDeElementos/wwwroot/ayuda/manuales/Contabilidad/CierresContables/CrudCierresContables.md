# Cierres contables

Un cierre contable **traspasa a la contabilidad** los preasientos pendientes de una sociedad hasta una fecha: genera el **lote contable**
(el fichero para el programa de contabilidad, NCS o ContaSol) y deja la sociedad **cerrada hasta esa fecha**. Lo anterior a la fecha de
cierre ya no se puede modificar (ni sus preasientos, ni los documentos que los generaron).

## ¿Cómo hago...?

### ...para cerrar un periodo?

Menú → **Realizar cierre** (gestor):

1. Elija la **Sociedad** (sólo las gestionadas, que no sean de un autónomo y de las que sea interventor). Se muestran su **último cierre** y
   su contabilidad.
2. **Cerrar hasta**: por defecto hoy. Ha de ser **posterior al último cierre y no posterior a hoy**.
3. **Contabilizar en** (NCS o ContaSol) y **Dígitos** de las cuentas auxiliares (NCS: 11, fijo; ContaSol: los de la sociedad, 8 si no
   tiene).

El cierre se ejecuta como **trabajo en segundo plano**: traspasa los preasientos pendientes con fecha contable desde el último cierre
hasta la fecha de cierre, crea el lote contable y registra el cierre. Se avisa si no hay preasientos que traspasar.

Reglas:

- No se puede cerrar un ejercicio si quedan preasientos pendientes con fecha contable de **ejercicios anteriores**.
- Mientras se cierra, la sociedad no se puede modificar y no se pueden crear ni cancelar sus preasientos anteriores a la fecha de cierre.

### ...para deshacer un cierre?

Seleccione el cierre y **Eliminar cierre**. Sólo se puede el **último cierre** de la sociedad. Se anula su lote contable y sus preasientos
vuelven a pendientes.

### ...para buscar cierres?

Filtros: **Lote** (nombre o referencia del lote contable), **Sociedad** y **Cerrada el** (entre fechas). Desde el grid se navega a la
sociedad y al lote contable del cierre.

---

#### Detalle técnico

- Controlador: `CierresContablesController` (Controllers/Contabilidad). Vista: `Views/Contabilidad/CrudCierresContables.cshtml`.
- Descriptor: `DescriptorDeCierresContables`: modal *Realizar cierre* (`CrearCierreContableDto`), *Eliminar cierre*, filtros.
- Reglas: `GestorDeCierresContables` (los cierres sólo se persisten desde la cola de trabajos: crear y anular lote contable; semáforo
  sobre la sociedad; sólo se anula el último).
- ts: `wwwroot/ts/Contabilidad/CierresContables.ts`: propone fechas, valida *Cerrar hasta*, ajusta contabilidad y dígitos según la
  sociedad y activa *Eliminar cierre* sólo en el último cierre.
