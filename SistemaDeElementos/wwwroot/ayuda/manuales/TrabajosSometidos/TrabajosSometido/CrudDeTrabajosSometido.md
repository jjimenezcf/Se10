# Trabajos sometidos

Catálogo de los **tipos de trabajo** que se pueden ejecutar en segundo plano: qué ejecutan (método de una **Dll** —*Clase*, *Método*—, un
**programa** o un **procedimiento almacenado** con su *Esquema*), si **comunican el fin** o los **errores** por correo, y el **usuario
ejecutor** y **puesto de trabajo** con que se ejecutan. Cada ejecución concreta es un *trabajo de usuario*.

La mayoría los da de alta el sistema la primera vez que se usan; normalmente sólo se consultan o se ajusta la comunicación.

---

#### Detalle técnico

- Controlador: `TrabajosSometidoController` (Controllers/TrabajosSometidos). Vista: `Views/TrabajosSometido/CrudDeTrabajosSometido.cshtml`.
  Dto: `TrabajoSometidoDto`. Descriptor: `DescriptorDeTrabajosSometido`. Alta: `GestorDeTrabajosSometido.CrearObtener`.
