# Auditoría de elementos

Registro de **los cambios hechos a los elementos**: qué elemento, de qué negocio, qué usuario y cuándo, con los datos modificados. Se abre
desde cualquier mantenimiento (auditoría del elemento seleccionado) y es de consulta.

## ¿Cómo hago...?

### ...para saber quién cambió algo?

Filtre por **Negocio**, **Elemento** y **Usuario**. Cada fila es una modificación con su fecha.

---

#### Detalle técnico

- Controlador: `AuditoriaController` (Controllers/Negocio). Vista: `Views/Negocio/CrudDeAuditoria.cshtml`. Descriptor:
  `DescriptorDeAuditoria`.
