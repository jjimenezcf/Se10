# Permisos heredados (de un puesto)

Lista de **todos los permisos que le llegan a un puesto de trabajo**: los de sus roles y sus permisos directos. Se abre desde el puesto y
está filtrada por él (filtro **Puesto**). Es de consulta: para cambiarlos, modifique los roles del puesto o sus permisos directos.

## ¿Cómo hago...?

### ...para comprobar si un puesto tiene un permiso?

Filtro **Permiso** (por nombre, p. ej. `VISTA: Pedidos`).

---

#### Detalle técnico

- Controlador: `PermisosHeredadosController` (Controllers/Seguridad). Vista: `Views/Seguridad/CrudPermisosHeredados.cshtml`. Descriptor:
  `DescriptorDePermisosHeredados`.
