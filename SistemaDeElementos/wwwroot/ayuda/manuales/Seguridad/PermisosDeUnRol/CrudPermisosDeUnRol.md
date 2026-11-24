# Permisos de un rol

Relación de los **permisos que contiene un rol**. Se abre desde el rol y está filtrada por él (filtro **Rol**).

## ¿Cómo hago...?

### ...para añadir permisos al rol?

**Crear** eligiendo el permiso (búsquelo por su nombre, p. ej. `VISTA: Pedidos` o `NEGOCIO (Gestor): Pedidos`). **Borrar** lo quita del rol.
Lo reciben (o lo pierden) los usuarios de los puestos que tienen ese rol.

### ...para buscar?

Filtro **Permiso**.

---

#### Detalle técnico

- Controlador: `PermisosDeUnRolController` (Controllers/Seguridad). Vista: `Views/Seguridad/CrudPermisosDeUnRol.cshtml`. Descriptor:
  `DescriptorDePermisosDeUnRol` (restrictor por rol).
