# Puestos de un usuario

Relación de los **puestos de trabajo asignados a un usuario**. Se abre desde el usuario y está filtrada por él (filtro **Usuario**). El
usuario recibe la unión de los permisos de todos estos puestos.

## ¿Cómo hago...?

### ...para dar un puesto al usuario?

**Crear** eligiendo el puesto. **Borrar** se lo quita.

### ...para buscar?

Filtro **Puesto**.

---

#### Detalle técnico

- Controlador: `PuestosDeUnUsuarioController` (Controllers/Seguridad). Vista: `Views/Seguridad/CrudPuestosDeUnUsuario.cshtml`.
  Descriptor: `DescriptorDePuestosDeUnUsuario`.
