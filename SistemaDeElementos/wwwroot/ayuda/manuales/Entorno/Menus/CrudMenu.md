# Menús definidos

Las **opciones del menú** de la aplicación: menú **padre**, nombre, **orden**, **icono**, descripción, **parámetros** y la **vista** a la que
lleva, con su **permiso de acceso**. Un usuario sólo ve las opciones cuya vista tiene permiso.

## ¿Cómo hago...?

### ...para reorganizar el menú?

Cambie el **Menu padre** y el **Orden** de la opción. Para verlo en árbol use *Árbol de menús*.

### ...para filtrar?

Por **Menu padre** y **Mostrar las activas**.

Los menús los crea **Inicializar entorno**; los roles AFB se basan en su estructura (ver *Roles funcionales*).

---

#### Detalle técnico

- Controlador: `MenusController` (Controllers/Entorno). Vista: `Views/Entorno/CrudMenu.cshtml`. Dto: `MenuDto`.
- Descriptor: `DescriptorDeMenu`. Alta: `Inicializador/Negocios/InzMenus.cs`.
