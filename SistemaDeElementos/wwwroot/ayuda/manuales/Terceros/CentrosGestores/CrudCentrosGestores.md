# Centros de gestión

Árbol de **centros gestores (CG)** de las sociedades. El centro gestor es la unidad organizativa a la que pertenecen los elementos
(expedientes, facturas, pedidos, tareas...): determina su sociedad, su responsable y, con los puestos de trabajo, **quién tiene acceso**.
Cada CG tiene su **Sociedad**, su **CG Padre**, código, **sigla**, **responsable** (usuario), correo y logotipo.

## ¿Cómo hago...?

### ...para crear un centro gestor?

Seleccione el CG padre en el árbol y cree el nuevo dentro: sociedad, nombre, código, sigla y responsable.

### ...para dar acceso a un centro gestor?

En el apartado de **puestos de trabajo** del CG: los usuarios de esos puestos acceden a sus elementos (ver *Gestión de puestos de trabajo*).

### ...para dar de baja o reactivar un CG?

Menú → **Dar de baja** / **Dar de alta**.

---

#### Detalle técnico

- Controlador: `CentrosGestoresController` (Controllers/Terceros), vista `CrudCentrosGestores`: formulario en árbol con
  `DescriptorDeCentrosGestores` ("Centros de gestión"), con grid de puestos (`PuestoDeTrabajoController`).
- ts: `wwwroot/ts/Terceros/CentrosGestores.ts`.
