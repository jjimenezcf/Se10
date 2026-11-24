# Árbol de menús

El **menú de la aplicación en forma de árbol** ("Menus del SE"): permite ver y reorganizar las opciones jerárquicamente y comprobar quién
accede a cada una.

## ¿Cómo hago...?

### ...para ver qué opciones ve un usuario?

Filtre por **Usuario**, **Puesto de trabajo** o **Rol**: el árbol muestra las opciones a las que tiene acceso. Con **Vista** localiza las
opciones que abren una vista.

### ...para mover una opción?

Selecciónela en el árbol y cambie su padre u orden.

---

#### Detalle técnico

- Controlador: `JerarqiaMenusController` (Controllers/Entorno), vista `CrudJerarqiaMenus`: formulario en árbol con
  `DescriptorDeJerarqiaMenus`.
