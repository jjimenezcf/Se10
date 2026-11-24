# Correos de usuario

Los **correos que envía la aplicación** (al proveedor con su pedido, avisos de trabajos, envíos de elementos...): quién lo creó, emisor,
**receptores**, asunto, cuerpo, los **elementos** y **archivos** que lleva, cuándo se creó y cuándo se **envió**.

## ¿Cómo hago...?

### ...para saber si se envió un correo?

Filtros **Mostrar los enviados** / **Mostrar los no enviados**, **Enviado entre**, **creado entre**, **Receptor** y **Usuario**.

### ...para volver a enviar un correo?

Selecciónelo y **Enviar o reenviar**.

---

#### Detalle técnico

- Controlador: `CorreosController` (Controllers/TrabajosSometidos). Vista: `Views/TrabajosSometido/CrudDeCorreos.cshtml`. Dto: `CorreoDto`.
  Descriptor: `DescriptorDeCorreos`.
