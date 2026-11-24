# Mi correo

Los **correos recibidos en los buzones** a los que tiene acceso (buzón, fecha, emisor, destinatario, asunto, cuerpo y adjuntos). Desde aquí
se **incorporan a la aplicación**: cada correo se archiva como el elemento que corresponda, con sus adjuntos.

## ¿Cómo hago...?

### ...para buscar un correo?

Filtro **Correo**: busca por el asunto (contenido).

### ...para archivar un correo en la aplicación?

Seleccione el correo y elija **cómo archivarlo** (*Seleccionar como archivar el correo*):

- **Como factura** (*Archivar correo como factura*): crea la factura recibida con el adjunto.
- **Como registro de E/S** (*Archivar correo como registro de E/S*).
- **Como tarea** (*Archivar correo como tarea*).
- **Como archivador** (*Archivar correo como archivador*).

O **incorporarlo a un elemento existente** (*Seleccionar como incorporar el correo*).

---

#### Detalle técnico

- Controlador: `MiCorreoController` (Controllers/Entorno). Vista: `Views/Entorno/CrudDeMiCorreo.cshtml`. Dto: `MiCorreoDto`.
- Descriptor: `DescriptorDeMiCorreo`: modales de archivar como factura, registro de E/S, tarea o archivador, y de incorporar.
