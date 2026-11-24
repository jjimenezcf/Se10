# Gestión de negocios

Un **negocio** es cada tipo de elemento que gestiona la aplicación (facturas emitidas, pedidos, expedientes, tareas...). Aquí se define su
configuración general: nombre, **enumerado**, Dto y Dtm, **icono**, los **permisos** de administración, gestión y consulta del negocio, y si
está **activo**, **usa seguridad**, **usa centro gestor** o **es de parametrización**.

Cada negocio con flujo tiene sus **estados**, **transiciones** y **parámetros** (ver esas vistas) y su ayuda de proceso
(`ayuda/procesos/<negocio>`), que se ve con el **?** de su ficha en el panel de control.

## ¿Cómo hago...?

### ...para dar acceso a un negocio?

Asigne a un rol los permisos del negocio (*Administrador*, *Gestión*, *Consulta*): `NEGOCIO (<modo>): <negocio>`.

### ...para desactivar un negocio?

Desmarque **Negocio activo**: no se podrán crear, editar ni borrar sus elementos.

Los negocios los crea **Inicializar entorno**.

---

#### Detalle técnico

- Controlador: `NegocioController` (Controllers/Negocio). Vista: `Views/Negocio/CrudDeNegocios.cshtml`. Dto: `NegocioDto`.
- Descriptor: `DescriptorDeNegocio`.
