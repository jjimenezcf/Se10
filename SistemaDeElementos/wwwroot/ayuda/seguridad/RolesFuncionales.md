# Roles funcionales (AFB)

Los roles **AFB** (*acceso funcional básico*) dan acceso a las **opciones de menú**: cada uno contiene los permisos `VISTA: ...` de las opciones que cuelgan de un punto del menú. Sólo dan **seguridad funcional** (ver la opción y abrir el crud): para ver o gestionar datos, el puesto necesita además los permisos de datos (negocio, tipo, centro gestor, estado, transición), como se explica en [ModeloDeSeguridad.md](ModeloDeSeguridad.md).

## Cómo se nombran y cuándo se crean

- Nombre: `AFB: <ruta del menú>`, con los menús de la ruta separados por un punto. Ejemplo: `AFB: Módulo de Logística.Configuración.Pedidos`.
- Hay un rol por cada menú del que cuelgan opciones que muestran un **crud**. Las opciones que lanzan un proceso (inicializar entorno o BD, aplicar seguridad, definir o inicializar maestros, someter trabajos, importar el callejero) **no** se incluyen: quedan para el administrador.
- Cada rol incluye también las **vistas secundarias** que se abren desde sus cruds (las carpetas de un archivador, los barrios de un municipio, los lotes de un contrato...): no salen en el menú, pero exigen su propio permiso de vista. En las tablas aparecen tras *secundaria(s):*.
- Quedan fuera las vistas secundarias que no son de un módulo concreto: *Auditoría de elementos* (se abre desde cualquier crud), *Errores de un trabajo* y *Traza de un trabajo* (desde *Trabajos de usuario*, que no está en ningún rol AFB), y las del panel personal del usuario (*Eventos de mis agendas*, *Mi correo*).
- Los crea `InzRolesFuncionales.DefinirRoles` (`Inicializador/Negocios/InzRolesFuncionales.cs`) cada vez que se lanza **Inicializar entorno**, después de crear negocios, vistas y menús.
- **Si el rol ya existe no se toca**: ni se vuelve a crear ni se le añaden o quitan permisos, para respetar los ajustes hechos a mano. Si se añade una opción nueva a un menú y se quiere en el rol, hay que añadirla al rol a mano (o borrar el rol y volver a inicializar el entorno).
- Los nombres de los menús salen de los literales de `InzMenus`, los de las vistas de `enumVistas` y los de los menús de configuración de cada negocio del propio negocio, para que el rol siempre coincida con el menú.

Los permisos `VISTA: Estados` y `VISTA: Transiciones` son las vistas genéricas de estados y transiciones, que todos los negocios con flujo comparten: la pantalla filtra por el negocio del menú.

La columna **Nombre del permiso** es el nombre del permiso que se añade al rol: `VISTA: <nombre de la vista>`. Una excepción: el permiso de la vista *Actividades Formativas* se llama `VISTA: Acciones Formativas`.

## Configuración del entorno

Para administradores funcionales del sistema.

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Configuración` | Correos de usuario, Trabajos sometidos, Acciones del sistema | `VISTA: Correos de usuario`, `VISTA: Trabajos sometidos`, `VISTA: Acciones del sistema` |
| `AFB: Configuración.Funcionalidad` | Vistas, Variables, Menús, Árbol de menús, Agendas, Certificados; *secundaria:* Visor de agenda | `VISTA: Vistas del sistema`, `VISTA: Variables de entorno`, `VISTA: Menú definidos`, `VISTA: Árbol de menús`, `VISTA: Agendas del entorno`, `VISTA: Certificados`, `VISTA: Visor de agenda` |
| `AFB: Configuración.Accesos` | Usuarios, Puestos, Roles, Permisos, Seguridad; *secundarias:* clases de permisos y las relaciones entre usuarios, puestos, roles y permisos | `VISTA: Usuarios del sistema`, `VISTA: Puestos de trabajo`, `VISTA: Roles`, `VISTA: Permisos`, `VISTA: Seguridad de usuarios`, `VISTA: Clases de permisos`, `VISTA: Puestos de trabajo de un usuario`, `VISTA: Roles de un puesto`, `VISTA: Permisos de un rol`, `VISTA: Permisos de un usuario`, `VISTA: Roles de un permiso`, `VISTA: Puestos de un rol`, `VISTA: Usuarios de un puesto`, `VISTA: Permisos de un puesto de trabajo` |
| `AFB: Configuración.Negocios` | Negocios de SE, Acciones de relación; *secundarias:* Parámetros de negocio, Plantillas de exportación | `VISTA: Negocios de SE`, `VISTA: Acciones de relación`, `VISTA: Parámetros de Negocio`, `VISTA: Plantillas de exportación` |

## Maestros

Datos comunes que necesitan casi todos los módulos (al menos en consulta).

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Maestros` | Centros gestores | `VISTA: Centros gestores` |
| `AFB: Maestros.Callejero` | Tipos de vía, Países, Provincias, Municipios, Calles, Códigos postales; *secundarias:* Cps de una provincia y de un municipio, Barrios, Zonas y sus calles | `VISTA: Tipos de vía`, `VISTA: Paises`, `VISTA: Provincias`, `VISTA: Municipios`, `VISTA: Calles`, `VISTA: Codigos postales`, `VISTA: Cps de una provincia`, `VISTA: Cps de un municipio`, `VISTA: Barrios`, `VISTA: Calles de un barrio`, `VISTA: Zonas`, `VISTA: Calles de una Zona` |
| `AFB: Maestros.Terceros` | Personas, Sociedades, Interlocutores, Bancos | `VISTA: Personas`, `VISTA: Sociedades`, `VISTA: Interlocutores`, `VISTA: Bancos` |
| `AFB: Maestros.Logística` | Proveedores, Clientes | `VISTA: Proveedores`, `VISTA: Clientes` |
| `AFB: Maestros.Jurídico` | Clases de juzgado, Juzgados, Procuradores, Abogados | `VISTA: Clases de juzgado`, `VISTA: Juzgados`, `VISTA: Procuradores`, `VISTA: Abogados` |

## Maestros técnicos

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo Mt` | Unitarios | `VISTA: Unitarios` |
| `AFB: Módulo Mt.Configuración` | Unidades, Naturalezas | `VISTA: Unidades`, `VISTA: Naturalezas` |

## Sistema documental

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Sistema Documental` | Archivadores, Circuitos documentales; *secundaria:* Carpetas | `VISTA: Archivadores`, `VISTA: Circuitos documentales`, `VISTA: Carpetas` |
| `AFB: Sistema Documental.Configuración` | Series documentales | `VISTA: Tipos de archivadores` |
| `AFB: Sistema Documental.Configuración.Circuitos documentales` | Estados, Transiciones, Tipos de circuito documental | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de circuito documental` |

## Contabilidad

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo Contable` | Cierres contables, Preasientos, Estimaciones, Lotes contables | `VISTA: Cierres contables`, `VISTA: Preasientos`, `VISTA: Estimaciones`, `VISTA: Lotes Contables` |
| `AFB: Módulo Contable.Configuración` | Cuentas, Iva soportado, Irpf, Iva repercutido | `VISTA: Cuentas`, `VISTA: Ivas soportado`, `VISTA: Irpfs`, `VISTA: Ivas repercutido` |
| `AFB: Módulo Contable.Configuración.Preasientos` | Estados, Transiciones, Tipos de preasiento | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de preasiento` |

## Gestión administrativa

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Gestión Administrativa` | Registros de E/S, Tareas, Expedientes, Actividades; *secundaria:* Actividades formativas | `VISTA: Registros de E/S`, `VISTA: Tareas`, `VISTA: Expedientes`, `VISTA: Actividades`, `VISTA: Acciones Formativas` |
| `AFB: Gestión Administrativa.Configuración.Registro` | Estados, Transiciones, Tipos de registros | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de registros` |
| `AFB: Gestión Administrativa.Configuración.Tarea` | Estados, Transiciones, Tipos de tareas | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de tareas` |
| `AFB: Gestión Administrativa.Configuración.Expediente` | Estados, Transiciones, Tipos de expedientes | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de expedientes` |

## Recursos humanos

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo RRHH` | Trabajadores, Registro de fichadas | `VISTA: Trabajadores`, `VISTA: Fichadas` |

## Guarderías

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo de Guarderías` | Aulas, Niños, Cursos, Matrículas (vista de contratos); *secundaria:* Niños de un curso | `VISTA: Aulas`, `VISTA: Niños de la guarderia`, `VISTA: Cursos`, `VISTA: Contratos`, `VISTA: Niños de la guarderia de un curso de guardería` |

## Jurídico

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo Jurídico` | Procedimientos (vista de expedientes), Pleitos, Contratos de venta y de compra (vista de contratos); *secundarias:* Lotes de un contrato, Unitarios de un lote, Planificador de ventas | `VISTA: Expedientes`, `VISTA: Pleitos`, `VISTA: Contratos`, `VISTA: Lotes de un contrato`, `VISTA: Unitarios de un lote`, `VISTA: Planificador de ventas` |
| `AFB: Módulo Jurídico.Configuración.Pleitos` | Estados, Transiciones, Tipos de pleito | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de pleito` |
| `AFB: Módulo Jurídico.Configuración.Contratos` | Estados, Transiciones, Tipos de contratos | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de contratos` |

## Ventas

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo de Ventas` | Presupuestos, Planificaciones de venta, Partes de trabajo, Asignaciones de parte, Facturas emitidas, Remesas de facturas, Consultar en AEAT; *secundarias:* Facturas de una remesa, Planificador de ventas | `VISTA: Presupuestos`, `VISTA: Planificaciones de venta`, `VISTA: Partes de trabajo`, `VISTA: Asignaciones de parte`, `VISTA: Facturas emitida`, `VISTA: Remesas de facturas`, `VISTA: Consultar en AEAT`, `VISTA: Facturas emitida de una remesa de facturas`, `VISTA: Planificador de ventas` |
| `AFB: Módulo de Ventas.Configuración.Presupuesto` | Estados, Transiciones, Tipos de presupuesto | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de presupuesto` |
| `AFB: Módulo de Ventas.Configuración.Planificaciones de venta` | Estados, Transiciones, Tipos de planificación de venta | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de planificacion de venta` |
| `AFB: Módulo de Ventas.Configuración.Partes de trabajo` | Estados, Transiciones, Tipos de parte de trabajo | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de parte de trabajo` |
| `AFB: Módulo de Ventas.Configuración.Facturas emitida` | Estados, Transiciones, Tipos de factura emitida | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de factura emitida` |
| `AFB: Módulo de Ventas.Configuración.Remesas de facturas` | Estados, Transiciones, Tipos de remesa de facturas | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de remesa de facturas` |

## Logística

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo de Logística` | Almacenes, Pedidos, Documentos de venta, Devoluciones de venta, Regularizaciones, Consultar movimientos, Consultar inventario | `VISTA: Almacenes`, `VISTA: Pedidos`, `VISTA: Documentos de venta`, `VISTA: Devoluciones de venta`, `VISTA: Regularizaciones`, `VISTA: Consultar movimientos`, `VISTA: Consultar inventario` |
| `AFB: Módulo de Logística.Configuración.Almacenes` | Estados, Transiciones, Tipos de almacén | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de almacén` |
| `AFB: Módulo de Logística.Configuración.Pedidos` | Estados, Transiciones, Tipos de pedido | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de pedido` |
| `AFB: Módulo de Logística.Configuración.Documentos de venta` | Estados, Transiciones, Tipos de documento de venta | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de documento de venta` |
| `AFB: Módulo de Logística.Configuración.Devoluciones de venta` | Estados, Transiciones, Tipos de devolución de venta | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de devolución de venta` |
| `AFB: Módulo de Logística.Configuración.Regularizaciones` | Estados, Transiciones, Tipos de regularización | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de regularización` |

## Gastos

| Rol | Opciones (vistas) | Nombre del permiso |
|---|---|---|
| `AFB: Módulo de Gastos` | Facturas recibidas, Pagos, Remesas de pagos; *secundaria:* Pagos de una remesa | `VISTA: Facturas recibidas`, `VISTA: Pagos`, `VISTA: Remesas de pagos`, `VISTA: Pagos de una remesa de pagos` |
| `AFB: Módulo de Gastos.Configuración.Facturas recibidas` | Estados, Transiciones, Tipos de factura recibida | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de factura recibida` |
| `AFB: Módulo de Gastos.Configuración.Pagos` | Estados, Transiciones, Tipos de pago | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de pago` |
| `AFB: Módulo de Gastos.Configuración.Remesas de pagos` | Estados, Transiciones, Tipos de remesa de pagos | `VISTA: Estados`, `VISTA: Transiciones`, `VISTA: Tipos de remesa de pagos` |

## Cómo usarlos al montar puestos

- Un **usuario de un módulo** necesita el rol AFB del módulo (`AFB: Módulo de Logística`) y los AFB de los maestros que consulta (`AFB: Maestros.Logística`, `AFB: Maestros`...).
- Un **parametrizador del módulo** añade los AFB de su configuración (`AFB: Módulo de Logística.Configuración.Pedidos`...).
- Los AFB no bastan solos: el puesto necesita también los permisos de datos de cada negocio (ver *Guía para diseñar puestos de trabajo* en [ModeloDeSeguridad.md](ModeloDeSeguridad.md)).

## Roles que no son por menú

- `AFB: Entorno` (lo crea `InzSeguridadComun`): herramientas de cualquier usuario. Vistas: Usuarios del sistema, Trabajos de usuario, Correos de usuario, Visor de agenda, Agendas del entorno, Eventos de mis agendas, y las secundarias que no son de ningún módulo: Auditoría de elementos, Errores de un trabajo, Traza de un trabajo, Visor de la bandeja de entrada, Mi correo (Imap), Mi correo (ApiKey).
- `Archivador: Básico` (rol de datos, lo crea `InzSeguridadComun`): gestor del tipo de archivador básico y consultor del de exportación.

Los antiguos `AFB: Callejero`, `AFB: Terceros básicos`, `AFB: Sistema documental`, `AFB: Registro de E/S`, `AFB: Tareas`, `AFB: Expedientes` y `AFB: Seguridad de acceso` ya no existen. Se sustituyeron en los puestos con `Documentación/ScriptSql/SEG_SustituirRolesAfbAntiguos.SQL`:

| Rol antiguo | Roles que lo sustituyen |
|---|---|
| `AFB: Terceros básicos` | `AFB: Maestros`, `AFB: Maestros.Terceros` |
| `AFB: Seguridad de acceso` | `AFB: Configuración.Accesos` |
| `AFB: Callejero` | `AFB: Maestros.Callejero` |
| `AFB: Registro de E/S` | `AFB: Gestión Administrativa`, `AFB: Gestión Administrativa.Configuración.Registro` |
| `AFB: Tareas` | `AFB: Gestión Administrativa`, `AFB: Gestión Administrativa.Configuración.Tarea` |
| `AFB: Expedientes` | `AFB: Gestión Administrativa`, `AFB: Gestión Administrativa.Configuración.Expediente` |
| `AFB: Sistema documental` | `AFB: Sistema Documental`, `AFB: Sistema Documental.Configuración` y `Archivador: Básico` (el mismo rol renombrado, sólo con sus permisos de datos) |

Los demás roles `AFB: ...` que haya en una BD (creados a mano para una instalación) no se tocan.
