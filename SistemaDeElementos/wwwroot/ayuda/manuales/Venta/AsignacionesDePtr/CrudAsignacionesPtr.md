# Gestión de asignaciones (de partes de trabajo)

Lista de las **asignaciones de partes de trabajo a trabajadores**: qué parte tiene que hacer cada trabajador, cuándo está planificado
(*Cuando empezar* / *Cuando terminar*) y cuándo se ha hecho (*Empezó* / *Terminó* / *Tiempo dedicado*). Sirve para organizar el trabajo
del día y para dar por realizadas las asignaciones, una a una o varias a la vez. Las asignaciones se crean desde el apartado
*Trabajadores* del parte de trabajo.

La edición es de **sólo consulta**: los datos de ejecución se ponen con las opciones del menú.

## ¿Cómo hago...?

### ...para ver lo que tiene que hacer un trabajador?

Escriba en el filtro **Trabajador** (busca por apellido, correo o NIF). En **Filtros de fechas** acote por *Inicio planificado* y
marque **Partes no iniciados** para ver sólo lo pendiente (o **Partes ejecutados** para lo ya hecho).

### ...para dar por realizadas varias asignaciones?

Selecciónelas en el grid (Ctrl / Mayúsculas + clic) y, en el menú individual (permiso de gestor):

- **Aplicar fecha de planificación**: les pone como inicio y fin de ejecución los planificados (si ya tenían inicio, lo respeta).
- **Dar por realizadas hoy**: fin = ahora; si no tenían inicio, inicio = ahora menos las horas de una jornada.
- **Aignar datos de ejecución**: pide en la modal *Fechas de ejecución* el inicio (y, opcionalmente, el fin) y se lo aplica a todas.
  El inicio ha de ser anterior al fin.

Las que ya estaban realizadas no se tocan. El resultado de cada una (actualizada, ya realizada o con error) se ve en la consola de
mensajes.

Después, en el parte de trabajo, aplique **Realizar** (exige que los datos de ejecución de sus asignaciones sean válidos).

### ...para ver el detalle de lo que hay que hacer?

Abra la asignación: se muestran el parte y sus **líneas** (concepto, cantidad, unidad, precio...).

## Filtros propios

- **Filtros de fechas**: *Inicio planificado* y *Inicio ejecución* entre fechas; *Mostra fechas plan* / *Mostra fechas ejecución*
  (muestran esas columnas); *Partes no iniciados* / *Partes ejecutados*.
- **Elementos relacionados**: *Parte de trabajo*, *Cliente*, *Unitario*, *Contrato* y *Presupuesto* (los dos últimos con la opción
  *con / sin* relación).

Sólo se ven las asignaciones de los partes a los que tiene acceso.

---

#### Detalle técnico

- Controlador: `AsignacionesDePtrController` (Controllers/Venta), que procesa las opciones del menú. Vista:
  `Views/Venta/CrudAsignacionesPtr.cshtml`. Dto: `AsignacionDePtrDto`.
- Descriptor: `DescriptorDeAsignacionesPtr`: edición no editable, filtro de nombre renombrado a *Trabajador*, menú individual
  multiselección y modal *Fechas de ejecución*.
- Reglas: `GestorDeAsignacionesDePtr` (`DarPorRealizadasHoy`, `DarPorRealizadasSegunPlan`, `AplicarDatosDeEjecucion`; horas de
  jornada en la variable de partes `HorasDeJornada`).
- ts: `wwwroot/ts/Venta/AsignacionesPtr.ts`: abre la modal de fechas y envía los ids seleccionados a `AplicarDatosDeEjecucion`.
