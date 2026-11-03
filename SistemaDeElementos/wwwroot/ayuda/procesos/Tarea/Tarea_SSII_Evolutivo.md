# Flujo del negocio 'Tareas' para el tipo 'SSII: Evolutivo'

Una tarea de control del tipo evolutivo (hijo de «SSII: Estatum») que se planifica, se asigna a un responsable, se resuelve y se valida. El estado inicial del tipo es **EVO: Inicial**.

Todas las transiciones de este flujo las aplica el usuario desde la tarea.

## Flujo

```mermaid
stateDiagram-v2
    [*] --> Inicial
    Inicial --> Asignada: Asignar
    Inicial --> Cancelada: Cancelar
    Asignada --> Inicial: Desasignar
    Asignada --> EnResolucion: Iniciar
    EnResolucion --> Asignada: Devolver a asignada
    EnResolucion --> Parada: Parar
    EnResolucion --> Validacion: Finalizar
    Parada --> EnResolucion: Reiniciar
    Validacion --> EnResolucion: Devolver a iniciada
    Validacion --> Terminada: Terminar
```

En el diagrama, *Inicial* es **EVO: Inicial**, *EnResolucion* es **EVO: En Resolución** y *Validacion* es **EVO: En validación**.

| Estado | Etapa | Observaciones |
|---|---|---|
| EVO: Inicial | `TAR_Etapa_Inicial` | Estado inicial. Sin responsable asignado. |
| EVO: Asignada | `TAR_Etapa_Asignada` | Tiene responsable y planificación. |
| EVO: En Resolución | `TAR_Etapa_En_Resolucion` | El responsable está trabajando en ella. |
| EVO: Parada | `TAR_Etapa_En_Espera` | Detenida temporalmente. |
| EVO: En validación | `TAR_Etapa_Validacion` | Finalizada por el responsable; pendiente de que se acepte. |
| EVO: Terminada | `TAR_Etapa_Terminada` | Estado terminado: aceptada. |
| EVO: Cancelada | `TAR_Etapa_Cancelado` | Estado final: la tarea no se realizó. |

## Transiciones

### EVO: Inicial ---> EVO: Asignar --> EVO: Asignada
Asigna la tarea a su responsable.
- Si la tarea tiene planificación, exige que tenga **responsable** y fechas de planificación inicial y final.
- Tras transitar envía un **correo al responsable** (si no es quien asigna) con un enlace a la tarea y crea el **evento de planificación** en su agenda.

### EVO: Inicial ---> EVO: Cancelar --> EVO: Cancelada
Anula una tarea que aún no se ha asignado. Pide el **motivo de cancelación**. No ejecuta ninguna comprobación ni cambio propio: solo cambia de estado.

### EVO: Asignada ---> EVO: Desasignar --> EVO: Inicial
Quita la asignación. Pide el **motivo de desasignar**.
- No deja si la planificación ya tiene datos de inicio, fin, duración o unidad de medida: hay que blanquearlos antes.
- Tras transitar envía un correo al responsable avisando de que ya no tiene que realizarla, **quita el evento** de su agenda y deja la tarea sin responsable.

### EVO: Asignada ---> EVO: Iniciar --> EVO: En Resolución
El responsable empieza a trabajar. Si la planificación no tiene fecha de inicio, anota la de ahora.

### EVO: En Resolución ---> EVO: Devolver a asignada --> EVO: Asignada
Deshace el inicio de la tarea. Pide el **motivo de devolución a asignada**. Borra la fecha de inicio de la planificación.

### EVO: En Resolución ---> EVO: Parar --> EVO: Parada
Detiene temporalmente la tarea. Pide el **motivo de parar la tarea**. No ejecuta ninguna comprobación ni cambio propio: solo cambia de estado.

### EVO: Parada ---> EVO: Reiniciar --> EVO: En Resolución
Reanuda una tarea parada. No ejecuta ninguna comprobación ni cambio propio: solo cambia de estado.

### EVO: En Resolución ---> EVO: Finalizar --> EVO: En validación
El responsable da la tarea por realizada y la deja pendiente de validar.
- La fecha de fin no puede ser posterior a hoy ni anterior al inicio; si faltan la fecha de fin, la duración o su unidad de medida, se validan los datos de durabilidad.
- Tras transitar anota la fecha de fin (la de ahora, si no consta) y, si no se indicó la **duración**, la calcula, siempre que el parámetro `TAR_Proponer_Valoracion_Al_Finalizar` esté activo; si no, exige indicarla.

### EVO: En validación ---> EVO: Devolver a iniciada --> EVO: En Resolución
Quien valida no acepta la finalización. Pide el **motivo de devolución a iniciada**.
- Borra la fecha de fin y, si `TAR_Eliminar_Valoracio_Tras_Rechazar` está activo, también la duración y su unidad, dejando una traza con los datos suprimidos.

### EVO: En validación ---> EVO: Terminar --> EVO: Terminada
Quien valida acepta la tarea.
- Si el tipo de tarea define **clases** y la tarea no tiene ninguna indicada, no deja terminarla.

## Comportamiento común a todas las transiciones
- Cada transición queda anotada en el historial (hitos) de la tarea.
- Solo pueden aplicarla los usuarios con permiso sobre la transición.
- Una tarea incluida en una factura emitida solo puede transitar mientras la factura es una prefactura.
- Además, pueden existir **acciones de transición** definidas por el parametrizador (se ven al pinchar la flecha en el visor de flujo del estado), que se ejecutan antes o después de las anteriores.
