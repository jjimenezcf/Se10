# Gestión de usuarios

Usuarios de la aplicación: login, apellidos, nombre, correo, fecha de alta, fotografía y si son **cliente** (usuario web de un cliente),
**administrador** (se salta toda la seguridad) o están **activos**. Lo que puede ver y hacer cada usuario le llega por sus **puestos de
trabajo** (ver *Gestión de puestos de trabajo* y el documento *Modelo de seguridad*).

## ¿Cómo hago...?

### ...para dar de alta un usuario?

**Crear**: login, nombre, apellidos y correo. Después asígnele **puestos de trabajo** (*Puestos de un usuario*).

### ...para saber qué puede hacer un usuario?

Filtros **Puesto de trabajo**, **Roles**, **Permiso** / **Permisos**; o desde el usuario, *Permisos de un usuario* (sus permisos efectivos).

### ...para que un usuario firme documentos?

Menú → **Añadir mi certificado**: el usuario sube su certificado digital.

### ...para impedir el acceso a un usuario?

Desmarque **Usuario activo**.

---

#### Detalle técnico

- Controlador: `UsuariosController` (Controllers/Entorno). Vista: `Views/Entorno/CrudUsuario.cshtml`. Dto: `UsuarioDto`.
- Descriptor: `DescriptorDeUsuario` (filtro *Usuario* por apellido y nombre; filtros de seguridad; *Añadir mi certificado*).
