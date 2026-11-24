# Mi correo (carga asíncrona)

Variante de **Mi correo** que carga los correos de los buzones de forma asíncrona. Funciona igual que *Mi correo*: se buscan por asunto y
se archivan como factura, registro de E/S, tarea o archivador, o se incorporan a un elemento existente.

---

#### Detalle técnico

- Controlador: `MiCorreoController`. Vista: `Views/Entorno/CrudDeMiCorreoAsync.cshtml`, con el mismo descriptor `DescriptorDeMiCorreo`.
