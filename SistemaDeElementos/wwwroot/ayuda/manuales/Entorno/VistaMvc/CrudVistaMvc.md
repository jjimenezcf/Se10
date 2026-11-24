# Vistas del sistema

Registro de las **vistas** (pantallas) de la aplicación: nombre, **controlador**, **acción**, parámetros, si se **muestra en modal**, el
**elemento Dto** que gestiona y su **permiso** (`VISTA: <nombre>`). Las opciones de menú apuntan a estas vistas y el permiso de la vista es
lo que decide si un usuario puede abrirla (seguridad funcional).

## ¿Cómo hago...?

### ...para saber qué permiso hace falta para abrir una pantalla?

Busque la vista: su columna **Permiso**. Ese permiso se da con un rol (normalmente un rol **AFB**).

### ...para filtrar?

Por **Controlador**, y con **Mostrar solo las modales**.

Las vistas las crea **Inicializar entorno**; normalmente sólo se consultan.

---

#### Detalle técnico

- Controlador: `VistaMvcController` (Controllers/Entorno). Vista: `Views/Entorno/CrudVistaMvc.cshtml`. Dto: `VistaMvcDto`.
- Descriptor: `DescriptorDeVistaMvc`. Alta: `Inicializador/Negocios/InzVistas.cs`.
