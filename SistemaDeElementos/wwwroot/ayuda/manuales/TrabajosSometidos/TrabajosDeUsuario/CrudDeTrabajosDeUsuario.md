# Trabajos de usuario

Cada vez que algo se ejecuta **en segundo plano** (importar un recuento, crear facturas con IA, exportar un archivador, un cierre contable,
los trabajos diarios...) se crea un **trabajo de usuario**: qué trabajo, quién lo sometió y quién lo ejecuta, su **estado**, cuándo **entró**,
cuándo está **planificado**, cuándo se **inició** y **terminó**, sus **parámetros** y su **periodicidad** (los periódicos se vuelven a someter
solos). En la edición se ven sus **errores** y su **traza**.

## ¿Cómo hago...?

### ...para saber cómo acabó un trabajo que lancé?

Búsquelo (filtros **Ejecutado entre**, **Planificado**) y ábralo: su estado y, en los apartados de **traza** y **errores**, el detalle. Muchas
operaciones dejan además el enlace al trabajo en las observaciones del elemento.

### ...para volver a lanzar un trabajo que falló?

**Resometer**. **Ejecutar trabajo** lo ejecuta ya.

### ...para que un trabajo no se ejecute?

**Bloquear** (y **Desbloquear** para reactivarlo).

---

#### Detalle técnico

- Controlador: `TrabajosDeUsuarioController` (Controllers/TrabajosSometidos). Vista: `Views/TrabajosSometido/CrudDeTrabajosDeUsuario.cshtml`.
  Dto: `TrabajoDeUsuarioDto`. Descriptor: `DescriptorDeTrabajosDeUsuario` (errores, traza, *Bloquear*, *Desbloquear*, *Ejecutar trabajo*,
  *Resometer*).
