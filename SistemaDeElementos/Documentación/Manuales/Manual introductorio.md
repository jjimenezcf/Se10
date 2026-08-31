# Manual introductorio de SE10

> Primera versión. Este documento se irá ampliando con capturas, ejemplos de uso y detalle por módulo a medida que avancemos.

## 1. Qué es SE10

SE10 no es una aplicación de gestión "cerrada" para un sector concreto: es un **motor de gestión de negocio parametrizable**. Todos los módulos verticales del sistema (tareas, expedientes, facturas, contratos, pleitos, pedidos, presupuestos, partes de trabajo, remesas, registros de entrada/salida...) están construidos sobre un mismo esqueleto genérico:

- **Tipos** de elemento configurables por negocio (p. ej. tipos de tarea, tipos de factura, tipos de contrato).
- **Estados y transiciones** configurables por negocio, con historial de hitos (cuándo entró cada elemento en cada estado, quién hizo la transición, cuánto tiempo permaneció).
- **Centros gestores** y **sociedades**, para repartir la actividad entre distintas unidades de negocio o empresas del grupo.
- **Etapas**, un concepto de conveniencia por encima del estado bruto, pensado para que un usuario de negocio pregunte "¿qué está pagado?" sin tener que conocer el identificador interno del estado.

Esto es lo que hace que el mismo sistema sirva, sin tocar código, para una asesoría, una constructora, una guardería o un despacho jurídico: lo que cambia entre implantaciones es la **parametrización** (tipos, estados, transiciones, plantillas, permisos), no el motor.

La definición de todo esto se gestiona desde **Configuración → Negocios**, donde se declaran los negocios del sistema (`enumNegocio`) y las acciones que se disparan al relacionar dos elementos entre sí.

## 2. Interfaz homogénea

Cada negocio parametrizable expone siempre el mismo patrón de menús y de pantallas, lo que reduce drásticamente la curva de aprendizaje al pasar de un módulo a otro:

- **Configuración del negocio**: Estados, Transiciones, Tipos, e inicialización de maestros.
- **Gestión del negocio**: un grid de datos con filtros, exportación, y un formulario de alta/edición con el mismo lenguaje visual en todos los módulos (Ventas, Gastos, Logística, Jurídico, Administración, Contabilidad...).
- Los mismos conceptos transversales aparecen siempre en el mismo sitio: archivador documental asociado, circuito documental, direcciones, historial de estados, permisos.

Esta homogeneidad es la que permite que un usuario que ya sabe manejar "Facturas emitidas" entienda de inmediato cómo se gestionan "Pedidos" o "Pleitos": el esqueleto es idéntico, solo cambian los campos propios de cada negocio.

## 3. Mapa de módulos

### Configuración
Administración del propio sistema: vistas, variables, menús, agendas, certificados, usuarios, puestos de trabajo, roles y permisos, catálogo de negocios del sistema, correos, trabajos sometidos e inicializadores de base de datos y maestros.

### Maestros
Datos base compartidos por el resto de módulos:
- **Callejero**: tipos de vía, países, provincias, municipios, calles y códigos postales, con **importación masiva** de callejero oficial. Toda dirección del sistema (fiscal, de obra, de entrega, de contacto...) se apoya en esta base normalizada, en vez de en texto libre.
- **Terceros**: personas, sociedades, interlocutores, bancos, proveedores, clientes.
- **Jurídico**: clases de juzgado, juzgados, procuradores, abogados.
- **Centros gestores** de cada sociedad.

### Módulo Mt (Maestros técnicos)
Unidades de medida, naturalezas contables y unitarios (partidas técnicas reutilizables en presupuestos, mediciones, etc.).

### Sistema Documental
El gestor documental transversal de toda la aplicación:
- **Archivadores** y **series documentales** tipificadas, donde cuelgan los documentos de cualquier expediente, factura, contrato, tarea, etc.
- **Circuitos documentales** (`CircuitoDoc`): flujos de aprobación/tramitación de documentos, reutilizables por distintos negocios (facturas recibidas, pagos, gastos, trabajadores, facturas emitidas ya lo usan como vínculo).

### Módulo Contable
Cuentas contables, IVA soportado/repercutido, IRPF, preasientos parametrizables por tipo/estado/transición, estimaciones directas y lotes contables.

### Gestión Administrativa
El núcleo de gestión "de propósito general": Registro de entrada/salida, **Tareas** (con tipos, estados y transiciones propias), **Expedientes**, y actividades formativas.

### Módulo RRHH
Trabajadores y **fichadas** (registro de entradas/salidas del personal).

### Módulo de Guarderías
Un ejemplo de cómo el motor genérico se adapta a un sector distinto: aulas, infantes, cursos y matrículas (implementadas como contratos de una clase específica).

### Módulo Jurídico
Procedimientos judiciales (expedientes de clase jurídica), pleitos, y contratos de compra/venta, cada uno con su propia configuración de tipos/estados/transiciones.

### Módulo de Ventas
Presupuestos, planificaciones de venta, partes de trabajo, facturas emitidas, remesas de facturas y **facturas registradas en la AEAT con Veri*Factu**.

### Módulo de Gastos
Facturas recibidas, pagos y remesas de pago.

### Módulo de Logística
Pedidos, con su propia configuración de tipos/estados/transiciones.

## 4. Inteligencia Artificial integrada

SE10 no usa la IA como un añadido cosmético: la tiene integrada en el propio motor de consulta y en la digitalización documental, y es agnóstica de proveedor (Gemini, Mistral, Perplexity, OpenAI, DeepSeek, Claude, Apyhub — configurable por variable de sistema).

- **Preguntas en lenguaje natural sobre los datos**: desde cualquier pantalla de un negocio se puede preguntar en castellano ("cuántas tareas ha terminado cada responsable este mes", "facturas pendientes de cobro del centro gestor X", "tiempo medio hasta que un expediente se cierra"). La IA traduce la pregunta a filtros, agrupaciones y métricas reales sobre el modelo de datos del negocio en el que se está trabajando, respetando siempre la seguridad de datos del usuario.
- **Filtrado inteligente de listados**: la misma capacidad se usa para generar los filtros de un grid a partir de una frase, entendiendo estados actuales frente a históricos, etapas de negocio, fechas relativas, usuarios, etc.
- **Lectura automática de facturas**: sube una factura en PDF/imagen (OCR), o en formato **Facturae 3.2/3.2.1/3.2.2** o **UBL 2.1/2.5**, y la IA extrae proveedor, NIF, importes, líneas, IVA, IRPF y forma de pago, cuadrando automáticamente los totales.
- **Lectura de justificantes de pago**: extracción automática de ordenante, beneficiario, cuentas e importe a partir de un documento de pago.

## 5. Facturación electrónica y relación con Hacienda

- Soporte nativo de los esquemas **Facturae** (3.2, 3.2.1, 3.2.2) y **UBL**, tanto en emisión como en lectura/importación.
- Integración con **Veri*Factu**: generación de huella/hash de los registros de facturación, firma electrónica y comunicación con el sistema de facturación de la AEAT, con su propia sección de "Facturas registradas en la AEAT" dentro de Ventas.
- Códigos QR de factura (QRCoder) conforme a la normativa.

## 6. Remesas bancarias (SEPA)

Tanto en Ventas (remesas de facturas emitidas, cobro) como en Gastos (remesas de pago), el sistema genera ficheros **SEPA** normalizados (transferencias, adeudos, nóminas) listos para presentar en banca electrónica, y es capaz de leer remesas devueltas.

## 7. Exportación e informes

- **Exportación a Excel** con estilos (cabeceras, totales, formato de columnas) mediante un motor propio sobre EPPlus, disponible desde cualquier grid del sistema.
- **Plantillas de exportación parametrizables**: cada negocio puede tener registradas sus propias plantillas de exportación (a Excel u otros formatos), asociadas a un permiso concreto, de forma que un cliente puede tener exportaciones a medida sin tocar el núcleo de la aplicación.
- **Motor de plantillas de impresión** (sobre QuestPDF): presupuestos, facturas emitidas, facturas recibidas, pagos y partes de trabajo se imprimen mediante plantillas con cabecera, logo de la sociedad, pie de página y bloques de detalle reutilizables — así como etiquetas y documentos Word (.docx) generados por plantilla para casos concretos (por ejemplo, plantillas de facturas en Word).

## 8. Seguridad y adaptabilidad multiempresa

- Modelo de **usuarios, puestos de trabajo, roles y permisos**, con permisos tanto de acceso a pantallas/acciones como de acceso a datos (por ejemplo, quién puede usar una plantilla de exportación concreta).
- Todo el modelo de negocio cuelga de **Sociedad → Centro Gestor**, lo que permite operar con varias empresas o delegaciones dentro de la misma instalación, cada una con su propia numeración, plantillas y catálogos.
- La configuración de tipos/estados/transiciones/circuitos es específica de cada instalación: adaptar SE10 a una nueva empresa o sector es, ante todo, un ejercicio de parametrización, no de desarrollo.

## 9. Otras utilidades transversales

- **Trabajos sometidos**: tareas de fondo que el sistema ejecuta de forma asíncrona (envíos masivos, generación de remesas, exportaciones, análisis de correo/facturas...) con su propio catálogo y registro de ejecuciones por usuario, consultable desde Configuración.
- **Gestor de correos**: no es solo un registro pasivo — el sistema **envía** correo (SMTP) y también **lee buzones** (IMAP, y lectura de Gmail) para incorporar automáticamente correos entrantes y sus adjuntos al sistema documental, asociándolos al negocio que corresponda.
- **Almacén de certificados**: gestión centralizada de certificados digitales (X.509) usados para firma electrónica de documentos y para la comunicación con la AEAT (Veri*Factu).
- **Gestor de agendas**: cada agenda del sistema se publica como un fichero `.ics` en vivo al que se puede **suscribir** un cliente externo (Google Calendar, Outlook...), de forma que los eventos gestionados en SE10 aparecen automáticamente en el calendario habitual del usuario sin duplicar la introducción de datos. Además, cada evento puede enviarse como invitación `.ics` individual (`METHOD:REQUEST`) para convocar reuniones directamente por correo.
- **Doble factor de autenticación (2FA)**: envío de código de verificación por correo como segundo factor en el acceso al sistema.

## 10. Y esto es solo el principio...

Quedan por documentar en detalle, entre otros:
- El motor de circuitos documentales y su editor visual.
- El ciclo de vida completo de cada negocio (diagramas de estados/transiciones típicos).
- El catálogo de variables del sistema y su efecto.
- Ejemplos paso a paso de una implantación nueva (parametrizar un negocio desde cero).

*(Iremos ampliando este manual por secciones a medida que lo necesitemos.)*
