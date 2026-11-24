# Gestión de trabajadores

Un trabajador es una persona de la organización que **realiza trabajos**: se le asignan partes de trabajo, ficha su jornada y se le pueden
hacer pagos. Tiene su **CG**, el **Usuario asociado** (su usuario de la aplicación), correo, teléfono y **cuenta** contable.

## ¿Cómo hago...?

### ...para dar de alta un trabajador?

Desde la **persona** (*Crear trabajador*) o **Crear** aquí: CG y usuario asociado.

### ...para ver lo que tiene asignado?

Apartado de **asignaciones** de la edición (partes de trabajo asignados) o *Gestión de asignaciones* filtrando por el trabajador.

### ...para pagarle?

Añada su **cuenta bancaria** en el apartado de cuentas.

### ...para dar de baja un trabajador?

Marque **Baja**.

---

#### Detalle técnico

- Controlador: `TrabajadoresController` (Controllers/Terceros). Vista: `Views/Terceros/CrudTrabajadores.cshtml`. Dto: `TrabajadorDto`.
- Descriptor: `DescriptorDeTrabajadores`: asignaciones y cuentas. ts: `wwwroot/ts/Terceros/Trabajadores.ts`.
