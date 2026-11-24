# Transiciones (de un negocio)

Las **transiciones del flujo** de un negocio: de qué estado a qué estado, nombre, **permiso**, el **asunto** que se pide al aplicarla (p. ej.
*Motivo de anulación*), si lleva **observación**, si es **del sistema** (la aplica el propio sistema, no el usuario), si es la transición
**por defecto** (flechas de transitar / devolver), si **inicia** o **termina** el proceso, si **cancela** y si está **activa**. Vista común a
todos los negocios con flujo, filtrada por **Negocio** y **Estado**.

## ¿Cómo hago...?

### ...para que un usuario pueda aplicar una transición?

Dele el **permiso** de la transición en un rol.

### ...para dejar de usar una transición?

Desmarque **Está activa**.

### ...para que una transición haga algo más?

Añádale **acciones** (*Acciones de relación*), que se ejecutan antes o después de transitar.

### ...para saber qué hace una transición?

Su explicación está en la ayuda del proceso del negocio (*Ayuda* en *Estados*, o el **?** de la ficha del panel de control).

---

#### Detalle técnico

- Controlador: `TransicionesController` (Controllers/Negocio). Vista: `Views/Negocio/CrudDeTransiciones.cshtml`. Dto: `TransicionDto`.
- Descriptor: `DescriptorDeTransiciones` (filtros *Negocio* y *Estado*).
