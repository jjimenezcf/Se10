## La creación de un elemento

Se abre con **Crear** desde el menú de la vista. Muestra los datos que hay que indicar para dar de alta el elemento; el resto (detalle,
ampliaciones, archivos) se completa después en la edición.

### ¿Cómo creo un elemento?

1. Pulse **Crear**.
2. Rellene los datos. Los obligatorios están marcados; las listas permiten buscar escribiendo parte del texto.
3. Pulse **Crear** (o Aceptar) en la modal. Si falta algo o algún dato no es válido, se indica en el pie de página y la modal sigue abierta.
4. Al crearlo, según la vista, se abre su edición para completar el resto o se queda en el grid.

### ¿Cómo evito escribir siempre los mismos datos?

- **Guardar datos por defecto**: guarda lo que tiene escrito para que la próxima creación empiece con esos valores.
- **Guardar plantilla**: lo guarda con un nombre como plantilla de creación. Las plantillas aparecen en el desplegable
  *Opciones de creación* del menú de la vista; al elegir una, la creación empieza con sus valores.

### ¿Cómo cierro sin crear?

Pulse la × de la modal o Escape. No se graba nada.

---

#### Detalle técnico de la creación

- Servidor: `DescriptorDeCreacion<T>` (lo crea `DescriptorDeCrud<T>` como `Creador`), con los controles de los atributos del dto
  marcados para la creación. Las plantillas son `PlantillaDeCreacionDtm` por negocio, usuario y vista
  (`DescriptorDeMantenimiento.CrearMenuDeCreacionDeUsuario`).
- Cliente: `Crud.CrudCreacion` (ts/_Crud/CrudCreacion.ts). Cada vista puede heredarla para fijar valores iniciales, bloquear o
  desbloquear campos según lo que se va eligiendo o validar antes de enviar.
