# Flujo del negocio 'Tareas' para el tipo 'SSII: Estatum'

**SSII: Estatum** es el tipo raíz de las tareas de sistemas de información (SSII). Agrupa a los tipos hijos *SSII: Evolutivo* y *SSII: Correctivo*, que son los que tienen flujo. El estado inicial del tipo es **SSII: Sin Flujo**.

Este tipo **no tiene transiciones definidas**: el estado *SSII: Sin Flujo* es a la vez inicial y terminado, y la tarea no cambia de estado.

## Flujo

```mermaid
flowchart TD
    S([SSII: Sin Flujo])
```

| Estado | Etapa | Observaciones |
|---|---|---|
| SSII: Sin Flujo | (sin etapa definida) | Estado inicial y terminado. |

## Transiciones

Ninguna. Para las tareas con flujo véanse los tipos hijos:
- *SSII: Evolutivo* (estados `EVO`).
- *SSII: Correctivo* (estados `SPC`).

## Comportamiento común
- Es un tipo de tarea de control con planificación, que sirve de padre en la jerarquía de tipos.
