# Gestión de pleitos

Un pleito recoge un **procedimiento judicial**: quién lo solicita (**Solicitante**, con contacto, teléfono y correo), el **responsable**
interno, la parte contraria (**Abogado** y **Procurador** contrarios) y el **Juzgado**. Además lleva la información de la **deuda** que se
reclama (apartado de recobro, con lo abonado y lo pendiente), la **minuta** y los **registros de entrada/salida** asociados.

## ¿Cómo hago...?

### ...para dar de alta un pleito?

**Crear**: solicitante, responsable, abogado y procurador contrarios y juzgado. Después complete en la edición:

- **Recobro** (*información sobre datos de la deuda*): conceptos con su *Valor*, *Abonado* y *Pendiente*.
- **Minuta**: información jurídica.

### ...para buscar pleitos?

En **Filtros de pleitos**: por *Abogado* contrario, *Procurador* o *Juzgado*.

### ...para vincular documentación recibida o enviada?

Menú individual → **Asociar registro de E/S** (gestor): asocia un registro de entrada/salida; se ven en el apartado de registros (*Registro*,
*Estado*).

### ...para gestionar las personas del pleito?

Menú individual → **Gestionar interlocutores**.

---

#### Detalle técnico

- Controlador: `PleitosController` (Controllers/Juridico). Vista: `Views/Juridico/CrudPleitos.cshtml`. Dto: `PleitoDto`.
- Descriptor: `DescriptorDePleitos`: ampliaciones de recobro, minuta y registros de E/S; bloque *Filtros de pleitos*; opciones *Asociar
  registro de E/S* y *Gestionar interlocutores*.
- ts: `wwwroot/ts/Juridico/Pleitos.ts` (sin comportamiento propio relevante).
