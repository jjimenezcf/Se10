# Flujo del negocio 'Expedientes' para el tipo 'PIM: Impuestos'

Un expediente de impuestos lleva la presentación de impuestos de un cliente. Al crearlo se crean los **archivadores de impuestos** del año en curso y del anterior y un archivador del cliente. El estado inicial del tipo es **PIM: Abierto**.

Todas las transiciones de este flujo las aplica el usuario desde el expediente.

## Flujo

```mermaid
flowchart TD
    A([PIM: Abierto]) -->|PIM: Cerrar| C([PIM: Cerrado])
    A -->|PIM: Cancelar| X([PIM: Cancelado])
    C -->|PIM: Reabrir| A
```

| Estado | Etapa | Observaciones |
|---|---|---|
| PIM: Abierto | `EXP_Etapa_Inicial`, `EXP_Etapa_Asociar_Tareas`, `EXP_Etapa_Ejecucion` | Estado inicial. |
| PIM: Cerrado | `EXP_Etapa_Terminada` | Estado terminado. |
| PIM: Cancelado | `EXP_Etapa_Cancelada` | Estado final: expediente anulado. |

## Transiciones

### PIM: Abierto ---> PIM: Cerrar --> PIM: Cerrado
Cierra el expediente. Pide el **motivo de cierre**. Exige que no haya tareas en proceso ni presupuestos sin terminar.

### PIM: Abierto ---> PIM: Cancelar --> PIM: Cancelado
Cancela el expediente. Pide el **motivo de cancelación**. No deja si tiene tareas realizadas o en proceso, ni elementos dependientes.
- Antes de transitar **da de baja los archivadores** del proceso.

### PIM: Cerrado ---> PIM: Reabrir --> PIM: Abierto
Reabre el expediente. Pide el **motivo de reapertura**. No ejecuta ninguna comprobación ni cambio propio: solo cambia de estado.

## Comportamiento común a todas las transiciones
- Cada transición queda anotada en el historial (hitos) del expediente.
- Solo pueden aplicarla los usuarios con permiso sobre la transición.
- Un expediente de cliente no transita si el cliente está de baja.
- Además, pueden existir otras **acciones de transición** definidas por el parametrizador (se ven al pinchar la flecha en el visor de flujo del estado), que se ejecutan antes o después de las anteriores.
