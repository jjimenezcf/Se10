# Permisos de un usuario

Lista de los **permisos efectivos de un usuario**: la unión de los permisos de los roles de sus puestos y de los permisos directos de esos
puestos, más los permisos sobre elementos concretos que se le hayan dado. Se abre desde el usuario (filtro **Usuario**). Es de consulta:
para cambiarlos, modifique sus puestos o los roles de éstos.

## ¿Cómo hago...?

### ...para comprobar si un usuario tiene un permiso?

Filtro **Permiso** (por nombre, p. ej. `VISTA: Pedidos` o `NEGOCIO (Gestor): Pedidos`).

---

#### Detalle técnico

- Controlador: `PermisosDeUnUsuarioController` (Controllers/Entorno). Vista: `Views/Entorno/CrudPermisosDeUnUsuario.cshtml`.
  Descriptor: `DescriptorDePermisosDeUnUsuario`. Permisos efectivos: vista `ENTORNO.USUARIO_PERMISO`.
