# Manuales de las vistas (para quien los escribe)

Este fichero no se muestra a los usuarios: sólo explica cómo se organizan los manuales.

## Dónde va cada cosa

```
wwwroot/ayuda/manuales/
├── LEEME.md                         ← este fichero (no se fusiona)
├── Crud_1_Mantenimiento.md          ← común a todas las vistas: el grid, filtros, menús (Crud.CrudMnt)
├── Crud_2_Creacion.md               ← común: la modal/página de creación (Crud.CrudCreacion)
├── Crud_3_Edicion.md                ← común: la edición, ampliaciones, visor de archivos (Crud.CrudEdicion)
├── {módulo funcional}/{controlador}/{vista}.md
│   p. ej. Venta/FacturasEmt/CrudFacturasEmt.md
└── Entorno/Home/PanelDeControl.md   ← página que no sigue el estándar: el panel de control (ver más abajo)
```

- **Comunes**: los `Crud*.md` de esta carpeta, en orden alfabético. Describen lo que aportan los genéricos
  `DescriptorDeCrud<T>` / `DescriptorDeMantenimiento<T>` / `DescriptorDeCreacion<T>` / `DescriptorDeEdicion<T>` en el servidor
  y `Crud.CrudMnt` / `Crud.CrudCreacion` / `Crud.CrudEdicion` en el ts.
- **Específico**: `{módulo}/{controlador}/{vista}.md`.
  - `módulo` = carpeta del controlador en `Controllers/` (Venta, Compra, Juridico...). Sólo sirve para ordenar: el servidor busca la
    carpeta del controlador en cualquier módulo.
  - `controlador` = nombre del controlador sin `Controller` (`FacturasEmt`). Es el de la cabecera de la vista (el que se pasa al
    descriptor), que no siempre coincide con el fichero donde está la acción: p. ej. *Consultar en AEAT* es `FacturasEmt/CrudFacturasAeat.md`
    y *Gestión de actividades* es `Expedientes/CrudActividades.md`.
  - `vista` = la vista mvc del crud (`CrudFacturasEmt`, la de `Views/Venta/CrudFacturasEmt.cshtml`).
  - El servidor busca la carpeta del controlador sólo en el segundo nivel (`{módulo}/{controlador}`), así un controlador puede llamarse
    igual que un módulo (`Negocio/Negocio/CrudDeNegocios.md`).
  - Si un manual es largo se puede partir en `{vista}_*.md` (p. ej. `CrudFacturasEmt_Cobros.md`): se fusionan detrás de `{vista}.md`.

## Cómo se muestra

El `?` del pie de página (junto a la papelera de mensajes) llama a `ApiDeArchivos.MostrarManualDeLaVista()`, que lee el controlador y
la vista de la cabecera del mantenimiento de la página y pide `/{controlador}/epManualDeLaVista?vista=...`
(`BaseController`, usa `ApiDeManuales.ManualDeLaVista`). Se muestra el específico seguido de los comunes, en la modal de sólo consulta
de `ApiDeArchivos.AbrirModalDeEditarTexto`. Si la vista no tiene manual propio se avisa y se muestran sólo los comunes.

En la cabecera de la modal, el **robot de la IA** abre "¿Cómo se hace...?" (`ApiPreguntasIa.AbrirModalComoSeHace`): la pregunta va a
`Archivos/epComoSeHace`, que monta en el servidor el mismo manual fusionado y se lo pasa, con la pregunta, a la IA del usuario.

De dónde se saca la vista según el tipo de página:

| Página | Dónde están controlador y vista |
|---|---|
| **Crud** (`DescriptorDeCrud`) | cabecera del mantenimiento: `.cuerpo-cabecera[controlador][vistamvc]` |
| **Formulario** en árbol (`DescriptorDeFormulario`: centros gestores, carpetas, árbol de menús) | `.cuerpo-cabecera-formulario[controlador][vista]`; el título es el de la página |
| **Páginas que no siguen el estándar** | no tienen esa cabecera: se declaran en `paginasSinCabecera` de `ApiDeArchivos.MostrarManualDeLaVista` (selector que las reconoce, controlador, vista y título) |

### Páginas que no siguen el estándar

Hoy sólo el **panel de control** (el dashboard que se ve al entrar, `PanelDeControl.RenderPagina` + `wwwroot/ts/PanelDeControl.ts`):

- se reconoce por su contenedor de tarjetas `#panel-graficas`;
- se le pide el manual a `Home` con la vista `PanelDeControl`, así que su manual es `Entorno/Home/PanelDeControl.md`;
- **no se le añaden los comunes**: `ApiDeManuales.ManualDeLaVista` sólo fusiona los `Crud*.md` en las vistas cuyo nombre empieza por `Crud`.

Para añadir otra página así: una entrada más en `paginasSinCabecera` (con un selector que sólo exista en esa página, un controlador que
herede de `BaseController` y un nombre de vista que **no** empiece por `Crud`) y su `.md` en `{módulo}/{controlador}/{vista}.md`. Su manual
lo describe todo (no hay comunes): para qué sirve la página, sus zonas y botones, "¿Cómo hago...?" y detalle técnico (sobre todo el ts).

## Cómo escribir el específico

Pensando en que lo va a leer un usuario y también la IA para contestar "¿cómo hago ...?":

1. **Para qué sirve la vista**: qué elementos gestiona y en qué momento del trabajo se usa.
   Si la entidad tiene proceso (flujo de estados), un **resumen del ciclo de vida**: el diagrama simplificado, una tabla de estados con
   qué se puede hacer en cada uno, qué transiciones son manuales y qué hace y exige cada una. Sale de `wwwroot/ayuda/procesos/{negocio}`
   y de las reglas de etapa del `GestorDe...` y del `ExtensorDe...` de la entidad (ver el de `Venta/FacturasEmt`).
2. **¿Cómo hago...?**: una sección `###` por tarea, con los pasos y el nombre exacto de la opción de menú.
3. **Filtros propios**: los que añade el descriptor específico (`DescriptorDe...`) además de los comunes.
4. **Opciones propias** del menú de la vista, del menú individual, del de relaciones y de la edición (lo que añade el descriptor con
   `Mnt.IncluirMfContextual`, `Mnt.IncluirMfDeRelacion`, `Editor.IncluirMfIndividual`...).
5. **Ampliaciones** de la edición (los bloques que se despliegan debajo de los datos principales).
6. **Comportamiento particular** que aporta el ts de la vista (las clases que heredan de `Crud.CrudMnt`, `Crud.CrudCreacion` y
   `Crud.CrudEdicion`): qué se bloquea o desbloquea y cuándo, qué se calcula, qué se valida.
7. **Detalle técnico** al final (descriptor, ts, controlador, dto), para el mantenimiento del manual.

Usar los nombres que ve el usuario en pantalla (etiquetas de los campos y de las opciones), no los de las propiedades.
