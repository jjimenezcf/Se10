# Gestión de permisos

Un **permiso** es la llave que protege **un objeto** en **un modo de acceso**. Los crea el sistema automáticamente al crear el objeto
(vistas, negocios, tipos, centros gestores, estados, transiciones, elementos, agendas, certificados, buzones, plantillas, exportaciones): **no
se dan de alta a mano**. Aquí se consultan y se averigua quién los tiene.

- **Clase de permiso**: qué protege (*Vista*, *Negocio*, *Tipo*, *CentroGestor*, *Estado*, *Transicion*, *Elemento*...).
- **Tipo de permiso**: en qué modo (*Administrador*, *Interventor*, *Gestor*, *Consultor* para datos; *Acceso* para pantallas).

El nombre dice lo que protege, p. ej. `VISTA: Pedidos`, `NEGOCIO (Gestor): Clientes`, `PEDIDO.TIPO (Consultor): PED: General`,
`CENTROGESTOR (Gestor): Pedidos - 01: Murcia`. Detalle en *Modelo de seguridad* (`ayuda/seguridad/ModeloDeSeguridad.md`).

## ¿Cómo hago...?

### ...para saber por qué un usuario ve (o no ve) algo?

Filtre por **Usuario** (permisos efectivos), o por **Puesto** / **Rol**, y por **Clase de permiso**, **Tipo de permiso** o **C.G.**

### ...para dar un permiso?

Añádalo a un **rol** (*Roles de un permiso* / *Permisos de un rol*) o como permiso directo del **puesto**.

---

#### Detalle técnico

- Controlador: `PermisosController` (Controllers/Seguridad). Vista: `Views/Seguridad/CrudPermiso.cshtml`. Descriptor: `DescriptorDePermiso`
  (filtros C.G., clase, tipo, puesto, rol, usuario). Permisos efectivos: vista `ENTORNO.USUARIO_PERMISO`.
