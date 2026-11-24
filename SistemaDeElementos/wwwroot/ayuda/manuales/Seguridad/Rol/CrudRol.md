# Gestión de roles

Un **rol** agrupa **permisos que van juntos** (p. ej. *Gestor de pedidos*) para asignarlos de una vez a los puestos de trabajo. Un permiso
puede estar en varios roles y un rol en varios puestos.

Hay dos familias:

- **Roles AFB** (*acceso funcional básico*, `AFB: <ruta del menú>`): dan acceso a las opciones de un punto del menú (permisos `VISTA: ...`).
  Los crea **Inicializar entorno** y, si ya existen, no se tocan (se respetan los ajustes a mano). Detalle en *Roles funcionales*
  (`ayuda/seguridad/RolesFuncionales.md`).
- **Roles de datos**: permisos de negocio, tipo, centro gestor, estado o transición en un modo (*Consultor*, *Gestor*, *Interventor*,
  *Administrador*).

## ¿Cómo hago...?

### ...para crear un rol?

**Crear** el rol y añadirle permisos en *Permisos de un rol*. Después asígnelo a los puestos (*Puestos de un rol*).

### ...para saber quién tiene un rol?

Filtros **Puesto** y **Usuario**; o desde el rol, *Puestos de un rol*. Filtro **Permisos asociado** para buscar los roles que contienen un permiso.

---

#### Detalle técnico

- Controlador: `RolController` (Controllers/Seguridad). Vista: `Views/Seguridad/CrudRol.cshtml`. Descriptor: `DescriptorDeRol`.
- Roles AFB: `Inicializador/Negocios/InzRolesFuncionales.cs`.
