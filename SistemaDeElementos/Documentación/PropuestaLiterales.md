# Propuesta: trocear y reorganizar `wwwroot/ts/_Literales/literales.ts`

> **Estado: EJECUTADA (fases 0 a 6), sin commitear, pendiente del visto bueno.** El resultado y la revisión final están en el
> apartado 7 al final del documento. Lo que sigue (apartados 0 a 6) es la propuesta tal como se aprobó.

> Documento para decidir. **No se ha tocado código.** El análisis corresponde a `literales.ts` de **main en ddb478f**
> (4.994 líneas, ya con `ltrIdsHtml`, `ltrHtml.Modales.Conteo/ComoSeHace`, `ltrCss.PanelDeControl.Conteo`, `ltrCss.Modal.FadeIn/BotonIa`,
> `ltrCss.crud.cabeceraFormulario`, `ltrMantenimiento.sufijoIdDelTitulo`, `ltrVistas.Entorno.PanelDeControl`, `Ajax.Param.Pregunta/Titulo`).
> Revisado tras sincronizar con main: el fichero es idéntico al analizado y `Startup.cs` / `_Layout.cshtml` no han cambiado.
>
> Compilación de referencia en ddb478f (`tsc -p tsconfig.json --noEmit`): **0 errores**.

---

## 0. Resumen

* El fichero tiene **92 objetos de primer nivel**. Cuatro concentran el 60 % de las líneas: `ltrPropiedades` (1.484), `Ajax` (547),
  `ltrCss` (471) y `ltrMenus` (397).
* La organización de primer nivel es **por tipo de literal** (css, atributos, propiedades, endpoints...) y la de segundo nivel **por área**
  (Venta, Gasto, Logística...). Esa convención funciona y la propuesta la respeta: **un fichero por tipo de literal**, no por área.
* La duplicación real no es tanta como parece: la mayoría de los "duplicados" son **alias** (`atControl.controlador = literal.controlador`)
  o el mismo valor en **ámbitos distintos** (eventos de cada modal). Lo que sí merece limpieza está en el apartado 2.3.
* El riesgo del troceo está **solo en el orden de carga**: hoy hay 29 objetos que leen a otro al definirse. Se propone un orden de
  11 ficheros que respeta todas esas dependencias, y una **comprobación automática** (instantánea de valores antes/después) que hace
  que el troceo sea verificable sin abrir el navegador.
* Además: las dos listas de scripts (`Startup.cs` y `_Layout.cshtml`) **ya no coinciden** (apartado 3.1). Conviene unificarlas antes de
  añadir 10 ficheros más a cada una.

---

## 1. Inventario de objetos de primer nivel

Columnas: líneas que ocupa, número de usos fuera de `literales.ts` (referencias `objeto.`), número de ficheros `.ts` que lo usan,
y de qué otros objetos depende **al definirse** (es lo que fija el orden de carga).

### 1.1 Los grandes

| Objeto | Líneas | Usos | Ficheros | Depende de | Para qué sirve |
|---|---:|---:|---:|---|---|
| `ltrPropiedades` | 1.484 | 2.359 | 96 | `literal`, `atGrid`, `atControl` | Nombres de propiedades de los DTO (en minúsculas tal como llegan en el json / atributo `propiedad`), por área: `Entorno`, `Maestros`, `Negocio`, `Callejero`, `Elemento`, `SisDoc`, `Terceros`, `Juridico`, `Logistica`, `Venta`, `Gasto`... más `Ia.Json` (campos del json que devuelve la IA). |
| `Ajax` | 547 | 849 | 81 | `ltrControladores`, `enumNegocio`, `atControl`, `literal` | Controladores, endpoints (`ep...`) y nombres de parámetros. Mezcla tres estructuras (ver 2.3.d) y alguna constante que no es de ajax (`eventoLoad`, `jsonResultOk`...). |
| `ltrCss` | 471 | 1.117 | 66 | `atGridDeDetalle` | Clases css: `crud.*` (cuerpo, grid, paneles de creación/edición, historial, modales), `PanelDeControl`, `Modal`, `Archivos`, `flujo`, `etapas`... |
| `ltrMenus` | 397 | 656 | 64 | `ltrEventos`, `ltrPropiedades` | Ids de menús, opciones del menú flotante (`eventosDeMf`, ~300 líneas), barra de menú, menú de filtrado. |
| `ltrEventos` | 291 | 269 | 19 | `atControl` | Nombres de evento que se pasan a los gestores (`Crud.EventosModalDe...`), agrupados por modal/panel. |
| `ltrHtml` | 148 | 8 | 3 | `ltrCss`, `ltrIdsHtml` | Plantillas html (modales de editar texto, opciones de pdf, nombre de archivo, Conteo, ComoSeHace, PreguntasIa). |
| `ltrEspanes` | 135 | 170 | 27 | `enumPostfijoControl` | Ids de los expansores (detalles/ampliaciones) por área. |
| `ltrValores` | 113 | 46 | 15 | `enumModoDePagoContado` | Valores de dominio que el ts compara (clases de cobro/pago, etc.). |
| `ltrControladores` | 96 | 125 | 47 | — | Nombres de controladores MVC por área. |

### 1.2 Atributos html (`at*`) — 22 objetos, ~420 líneas

`atControl` (75 líneas, 651 usos: el atributo genérico de cualquier control), `atGrid` (42), `atArchivo` (40), `atListasDinamicas` (28),
`atGridDeDetalle` (23), `atListas` (19), `atModal` (18), `atMantenimniento` (16), `atListasDeElemento` (15), `atSelectorDeFiltro` (14),
`atCriterio` (11), `atSelectorDeElementos` (10), `atEntreFechas` (10), `atOpcionDeMenu` (9), `atRestrictor` (9), `atNavegar` (8),
`atCheck` (7), `atModalParaImputar` (7), `atRef` (6), `atSelectorDeFecha` (6), `atMenu` (6), `atNombre` (5), `atEntreRangos` (5),
`atEntreImportes` (5), `atListasDeValores` (5), `atListasDinamicasDto` (5).

Cadena de dependencias interna: `literal → atControl → atMantenimniento → atGrid`, `atControl → atListas → atListas*`,
`atControl → atGridDeDetalle → atModal`.

### 1.3 Enumerados (`enum*` y similares) — 20 objetos, ~240 líneas

`enumNegocio` (77, es un `enum` de TS), `enumTipoDeModal` (27), `enumModoTrabajo` (13), `enumFormato` (12), `enumClaseDeContrato` (10),
`enumParaQueNavegar` (10), `enumModoDePresentacion` (10), `ttrModoPresentacion` (9), `enumCssOpcionMenu` (9, **es css**),
`enumPostfijoTipoModal` (8), `enumTipoDeTercero` (8), `enumCalificadorDireccion` (8), `enumOrigenDeNavegacion` (7),
`enumModoOrdenacion` (6), `enumModoDePagoContado` (6), `enumTipoControl` (6), `enumTipoDeLinea` (6, `enum`), `enumPostfijoControl` (5),
`enumIa` (5, `enum`), `ltrOrdenDePrioridades` (array).

### 1.4 Controles, espanes y modales — ~365 líneas

`ltrTipoControl` (57), `ltrEspanes` (135), `ltrModalDeCrearRelacion` (38, solo referencias a `ltrEspanes`), `ltrAmpliaciones` (32),
`ltrModal` (22), `ltrModalDeFiltrado` (20), `ltrCrud` (17), `ltrMantenimiento` (13), `ltrModalDeEditarRelacion` (9),
`ltrAccionesModal` (7), `ltrEdicion` (6), `ltrGridDeUnExpansor` (5), `ltrModalDeVincular` (4).

### 1.5 Navegación, urls y parámetros — ~200 líneas

`ltrUrls` (57, depende de `Ajax`), `ltrParametrosUrl` (42, depende de `ltrClaveDeEstado`), `ltrPaginas` (20), `ltrClaveDeEstado` (19),
`ltrOperacion` (12), `ltrParametrosNeg` (11), `ltrVistas` (11), `GestorDeEventos` (12, nombres de funciones js para los `onclick`).

### 1.6 Textos visibles — ~105 líneas

`ltrEtiquetas` (62), `ltrTitle` (20), `ltrNegocioSe` (25, nombres de negocio y enumerados).

### 1.7 Básicos — ~105 líneas

`literal` (45, 498 usos en 69 ficheros: mezcla de atributos genéricos, parámetros, `true/false/cero/uno/menos1/undefined` y criterios de
filtro), `ltrSimbolos` (18, separadores), `ltrStyle` (10), `ltrIconos` (9), `ltrVariables` (7), `ltrPrefijo` / `ltrPosfijo` (4 cada uno),
`TipoMensaje` (2), `TagName` (5), `newLine`, más `ltrDatosPropuestos` (11, depende de `ltrPropiedades`) e `ltrIdsHtml` (26).

### 1.8 Sin uso aparente

Objetos sin ninguna referencia fuera de `literales.ts`: **`TipoMensaje`** y **`TagName`**.

Además hay **~194 claves de primer nivel** que no aparecen como `objeto.clave` en ningún `.ts` ni `.cshtml`. Es un listado de
*candidatos*, no de borrables: algunas se usan por valor desde C# o por acceso dinámico (`enumNegocio[...]`, `Object.values(...)`).
Los más claros:

* `enumTipoDeModal`: 17 de sus 24 valores; `ltrTipoControl`: 22 de 57 (`Bloque`, `GridModal`, `pnlCreador`, `pnlEditor`, `ZonaDeMenu`...).
* `ltrCss`: `cabeceraModal`, `cuerpoModal`, `pieModal`, `propiedad`, `propiedadId`, `trPropiedad`, `divPropiedad`, `obligatorio`,
  `selectorElemento`, `tablaEdicion`, `anularFirma`, `tamanoImagen`, `imagenEnBoton`, `filaVisible`, `celdaVisible`, `controlNoVisible`.
* `ltrPropiedades.RegistroEs`, `ltrPropiedades.RecargarGrid`, `Ajax.EndPoint.MaestrosTecnicos` (existe también `MaestrosTecnico`),
  `Ajax.EndPoint.Administracion.Tarea.Renombrar` (vale `'epRenombarUnaFar'`, que es el endpoint de **facturas recibidas**: copia y pega,
  y no se usa), `ltrTitle.Venta`, `ltrEspanes.Tipos`, `ltrModalDeCrearRelacion.Tercero`, `ltrPaginas.Gastos`, `ltrCrud.Acciones`.
* `GestorDeEventos.deListaDinamica/paraTransitar/paraImprimir`, `Ajax.jsonResultOk/jsonUndefined/eventoProgreso`.

La lista completa se puede regenerar en la fase 4 con el script del apartado 5.

---

## 2. Duplicados

### 2.1 Alias (mismo valor por referencia) — inofensivos, pero hay demasiadas formas de decir lo mismo

| Concepto | Dónde está |
|---|---|
| `'controlador'` | `literal.controlador`, `atControl.controlador`, `atMantenimniento.controlador`, `atArchivo.controlador`, `atModal.controlador`, `atGridDeDetalle.controlador`, `atListasDeElemento.Controlador`, `ltrAmpliaciones.controlador` |
| `'accion'` | `literal.accion`, `atControl.accion` (sin uso), `atGridDeDetalle.Accion`, `ltrAmpliaciones.AccionTrasCrear`, `Ajax.Param.accion` (¡pero `atModal.accion` vale `'accion-controlador'`!) |
| `'id-negocio'` | `literal.idNegocio`, `atMantenimniento.idNegocio`, `atGridDeDetalle.idNegocio`, `atModal.idNegocio`, `atModalParaImputar.idNegocio`, y **escrito a mano** en `atListasDeElemento.idNegocio` |
| `'negocio'`, `'vista'`, `'id'` | `literal.*` reexportado en `atControl`, `atListas`, `Ajax.Param` (`nombreDeNegocio`, `id`) |
| `'-1'` | `literal.menos1`, `atListas.noSeleccionado`, `atRestrictor.noRestringido`, `ltrPropiedades...SinSeleccionar` (este último escrito a mano) |

Recomendación: no tocar los alias que ya existen (cambiarlos solo cambia el nombre, no el valor), pero fijar **una** regla para el
código nuevo: atributo html → `atControl`/`at*`; parámetro de endpoint → `Ajax.Param`; `literal` queda congelado (no se le añade nada).

### 2.2 `vistamvc` / `vistaMvc`: no es un error, son dos conceptos

* `atControl.vistaMvc = 'vistamvc'` es un **atributo html** (los navegadores pasan los nombres de atributo a minúsculas, así que en
  minúsculas es lo correcto).
* `Ajax.Param.Vista = 'vistaMvc'` es un **parámetro de endpoint** (el *model binding* de ASP.NET no distingue mayúsculas).
* `ltrPropiedades...id = 'idvistamvc'` es una propiedad del DTO.

Propuesta: dejar los valores y, como mucho, renombrar `Ajax.Param.Vista` → `Ajax.Param.VistaMvc` para que se lea igual que el atributo.
Lo mismo ocurre con `'guid'` (`atControl.Guid`, `Ajax.Param.guid`, `ltrClaveDeEstado.guid`, `ltrParametrosUrl.guid`): son atributo,
parámetro, clave de estado y parámetro de url; coinciden en el valor pero no son el mismo literal. Igual con `'titulo'`, que
`Ajax.Param.Titulo` (nuevo en ddb478f, parámetro de `epComoSeHace`) comparte con `ltrEspanes.Atributos.titulo` y una propiedad de
`ltrPropiedades`.

Lo añadido en ddb478f no crea duplicados nuevos salvo `ltrCss.PanelDeControl.Conteo.Botones = 'grafica-conteo-botones'`, que ya existía
como `ltrCss.crud.modal.pie` (recogido en 2.3.a).

### 2.3 Duplicados que sí conviene resolver

**a) Mismo valor escrito dos veces en el mismo objeto** (si uno cambia, el otro no):

* `atControl.menuFlotante` y `atControl.MenuFlotante` (`'menu-flotante'`).
* `atControl.soloConsulta` y `atControl.ampliacionSoloConsulta` (`'solo-consulta'`, además de `atModal.soloConsulta`).
* `atEntreFechas.idFechaHasta`/`idfechahasta` y `idHoraHasta`/`idhorahasta`.
* `atCriterio.igual` y `atCriterio.restringido`; y `literal.filtro.criterio.igual/diferente` repite `atCriterio.igual/diferente`.
* `ltrControladores.Comunes.Archivadores` y `ltrControladores.SisDoc.Archivador` (`'Archivadores'`).
* `ltrEventos`: `AnularOrden`/`AnularOrdenacion`, `Abrir`/`AbrirFiltro`, `AbrirModalParaCrearDetalle`/`AbrirModalDeDetalleParaCrear`,
  `CompartirElemento` dos veces.
* `ltrCss`: `crud.panelCreacion` y `crud.panelDeEdicion` repiten `VisorOculto`, `ContenedorVisor`, `ContenedorCabecera`,
  `ContenedorDeDatosDto` y `'visor-de-edicion'`; `'contenedor-modal'` (`contenedorModal` y `flujo.contenedorModal`),
  `'contenido-cuerpo'` (`cuerpoModal` y `Modal.ContenidoCuerpo`), `'contenedor-edicion-cuerpo'` (dos), `'control-oculto'`
  (`controlOculto` y `controlesDto.oculto`), `'archivos-en-consulta'` (dos), `'contenedor-de-visor'` (`contenedorVisor` y
  `Espan.cssVisorArchivos`), `'contenedor-de-archivos'` (dos), `'boton-modal btn btn-secondary'` (`Modal.BotonSecundario` y
  `flujo.botonRecargar`), `'grafica-conteo-botones'` (`crud.modal.pie` y `PanelDeControl.Conteo.Botones`).
* `ltrEspanes.Expedientes.ppts` y `.CrearValoracion`; `Juridico.Contratos.PlanificadorDeVentas` y `.CopiarPlfDeVenta`.

**b) Mismo valor en dos objetos, escrito a mano en los dos:**

* `'td-propiedad'`: `atNombre.tdPropiedad` y `ltrCss.tdPropiedad`.
* `'columna-accion'`: `atGridDeDetalle.EsAccion` y `ltrCss.columnaAccion` (y `ltrCss.Detalle.ColumnaAccion` referencia al primero).
* `'selector-de-elemento'`, `'selector-de-archivos'`: en `ltrTipoControl` y en `ltrCss`.
* `'restrictor-fijo'` (`atListas`, `atGridDeDetalle`), `'restringido-por'` (`atListas`, `atRestrictor`), `'es-fecha'` y `'formato'`
  (`atControl`, `atGrid`), `'tras-seleccionar'` (`atArchivo`, `atListas`), `'imagen-vinculada'` (`atControl.imagenVinculada`,
  `atArchivo.imagen`), `'cantidad-a-leer'` (`atControl.cantidadPorLeer`, `ltrMenus.CantidadALeeer`), `'cargando'` (tres `at*`).
* `'undefined'`: `literal.undefined` y `enumOrigenDeNavegacion.noDefinido`; `'0'`/`'1'`: `literal.cero/uno` y
  `ltrParametrosUrl.valorGuid/IndicePorContrato`.
* `ltrNegocioSe.Enumerado` repite valores de `enumNegocio` (`No_Definido`, `Contrato`, `Presupuesto`) y `ltrNegocioSe.Nombre` repite
  nombres de `ltrControladores` (`Sociedades`, `Archivadores`, `Contratos`, `Presupuestos`).
* `ltrTitle.Venta.FacturaEmt` = `ltrEtiquetas.Venta.FacturaEmt` (`Cobrado`, `Abonado`); `ltrTitle.Venta` no se usa.
* `'pasar-ocr'`, `'convertir-a-pdf'`, `'editar-texto'`: en `ltrCss.crud.panelDeEdicion` y en `ltrEventos` (probablemente a propósito:
  la clase del botón es también el nombre del evento; si es así, que `ltrEventos` referencie a `ltrCss`).

**c) Dos listas para lo mismo:**

* `enumTipoDeModal` (`'modal-de-crear-relacion'`) y `enumPostfijoTipoModal` (`'modaldecrearrelacion'`): dos catálogos de tipos de modal
  con distinta grafía.
* `ltrPaginas` (`crud_personadto_mantenimiento`...) y `ltrUrls` (`Personas/CrudPersonas`...): las dos apuntan a los mantenimientos.
* `ltrEspanes` y `ltrModalDeCrearRelacion` (la segunda solo contiene referencias a la primera).

**d) `Ajax`: tres estructuras en el mismo objeto**

1. `Ajax.<Area>.{controlador, accion|endpoints}` — p. ej. `Ajax.Entorno.MiCorreo.DescargarAdjunto`, `Ajax.Archivos.accion.*`.
2. `Ajax.EndPoint.<Area>.<Negocio>.*` — p. ej. `Ajax.EndPoint.Entorno.MiCorreo.DescargarAdjunto` (**el mismo endpoint que el anterior**;
   `MiCorreo.ts` usa uno en la línea 293 y el otro en la 735).
3. Parámetros: `Ajax.Param`, `Ajax.Parametros`, `Ajax.Archivos.parametro`, `Ajax.TrabajosSometidos.parametro`, más `ltrParametrosNeg`
   y `ltrParametrosUrl` fuera de `Ajax`.

Endpoints repetidos en varias áreas: `epObtenerUrlAlExpediente` (3), `epImportarCatalogo` (3), `epImportarTarifa` (2),
`epCambiarDatos` (2), `epCancelarDevolucion` (2), `epDescargarPlantilla` (2), `epLeerCertificados` (`Archivos` y `Usuarios`). Estos son
legítimos (cada uno es de un controlador distinto), salvo `Administracion.Tarea.Renombrar` (ver 1.8).

**e) El mismo nombre de objeto en varios sitios** (es la convención "tipo arriba, área abajo", no un error):
`PanelDeControl` está en `ltrCss`, `ltrModal` y `ltrMenus`; `Venta`/`Gasto`/`Logistica` están en `ltrPropiedades`, `ltrEspanes`,
`ltrControladores`, `ltrUrls`, `Ajax.EndPoint`, `ltrValores`, `ltrEtiquetas`, `ltrParametrosUrl`... No se propone cambiarlo.

### 2.4 Erratas en nombres (no en valores)

`atMantenimniento`, `ttrModoPresentacion`, `ltrMenus.CantidadALeeer`, `ltrMenus.IdsDeOpcinesMf/OpcinesMf`, `Ajax.persisitencia`,
`Ajax.Entorno.Ia.PregutaConConteo`, `atSelectorDeFiltro.popiedadBuscar`, `ltrCss.crud.grid.filto`, `ltrCss.Espan.animarBarrra`.
Ojo con los **valores** con errata (`atGrid.accion.siguiente = 'sigiente'`, `epRenombarUnaFar`): esos tienen pareja en C# y no se tocan
sin cambiar los dos lados.

---

## 3. Estructura de ficheros propuesta

### 3.1 Cómo se cargan hoy (y un problema previo)

* TypeScript sin módulos: cada `.ts` compila a un `.js` suelto con `const`/`enum` globales. Los `const` de distintos `<script>` comparten
  el ámbito global, así que un fichero puede usar los de otro **si ya se ha ejecutado**; si no, `ReferenceError` (TDZ).
* `tsc` **no avisa** de un orden de carga incorrecto entre ficheros (solo lo haría con `outFile`): el fallo aparece en el navegador.
  Es lo que pasó con `ApiPreguntasIa.SvgRobot` leído desde `PanelDeControl` antes de cargarse.
* Hay **dos listas** que mantener a mano: `Startup.cs` (líneas 124-177, para `bundle.js`, se usa con `UsarBundle=true`) y
  `Views/Shared/_Layout.cshtml` (líneas 57-110, en local). **Ya no coinciden**:
  * `ApiFlujo.js`: en el layout va detrás de `ApiDePeticiones.js`; en Startup, detrás de `ApiDeDireccion.js`.
  * `PanelDeControl.js`: en el layout es el último; en Startup va detrás de `EntornoSe.js`, **antes** de `_Crud/ApiPreguntasIa.js`
    (por eso solo fallaba en producción; hoy no falla porque `PanelDeControl` lee `SvgRobot` de forma perezosa).
* El bundle referencia `Literales.js` con L mayúscula y el fichero es `literales.ts`: funciona en Windows/IIS, fallaría en un servidor
  con sistema de ficheros sensible a mayúsculas.
* Los descriptores del callejero cargan además `js/Callejero/literales.js` (namespace `Callejero`, 54 líneas, usa
  `ltrControladores`): es el precedente de literales por módulo y no se ve afectado si `ltrControladores` se sigue cargando antes.

**Fase 0 recomendada** (ver 4): una única lista en C# (p. ej. `ScriptsDelEntorno.Lista`) que usen `Startup` para el bundle y
`_Layout.cshtml` para pintar los `<script>`. Así los 11 ficheros nuevos se añaden en un solo sitio y en el mismo orden.

### 3.2 Ficheros y orden de carga

Todo dentro de `wwwroot/ts/_Literales/`, con prefijo numérico para que el orden se vea en el explorador y coincida con el de carga:

| # | Fichero | Contenido | Depende de (fichero) | Líneas aprox. |
|---|---|---|---|---:|
| 1 | `01_Basicos.ts` | `newLine`, `TipoMensaje`, `ltrSimbolos`, `literal`, `ltrPrefijo`, `ltrPosfijo`, `ltrVariables`, `ltrStyle`, `TagName`, `ltrIconos` | — | 105 |
| 2 | `02_Enumerados.ts` | `enumNegocio`, `enumClaseDeContrato`, `enumModoDePagoContado`, `enumTipoDeTercero`, `enumPostfijoTipoModal`, `enumPostfijoControl`, `enumTipoDeModal`, `enumModoOrdenacion`, `enumFormato`, `enumTipoControl`, `enumParaQueNavegar`, `enumCalificadorDireccion`, `enumModoTrabajo`, `enumModoDePresentacion`, `ttrModoPresentacion`, `ltrOrdenDePrioridades`, `enumIa`, `enumTipoDeLinea`, `enumOrigenDeNavegacion` | — | 235 |
| 3 | `03_Controladores.ts` | `ltrControladores`, `ltrVistas`, `ltrPaginas` | — | 130 |
| 4 | `04_Atributos.ts` | todos los `at*` en este orden: `atMenu`, `atControl`, `atNombre`, `atRef`, `atCheck`, `atMantenimniento`, `atGrid`, `atArchivo`, `atOpcionDeMenu`, `atSelector*`, `atEntre*`, `atListas`, `atListasDeElemento`, `atListasDeValores`, `atListasDinamicas`, `atListasDinamicasDto`, `atRestrictor`, `atGridDeDetalle`, `atModal`, `atModalParaImputar`, `atCriterio`, `atNavegar` | 1 | 400 |
| 5 | `05_Controles.ts` | `ltrTipoControl`, `ltrEspanes`, `ltrGridDeUnExpansor`, `ltrModalDeVincular`, `ltrModalDeFiltrado`, `ltrModalDeEditarRelacion`, `ltrModalDeCrearRelacion`, `ltrAmpliaciones`, `ltrCrud`, `ltrEdicion`, `ltrMantenimiento`, `ltrModal`, `ltrAccionesModal` | 1, 2 | 365 |
| 6 | `06_Css.ts` | `ltrCss`, `enumCssOpcionMenu` | 4 (`atGridDeDetalle`) | 480 |
| 7 | `07_Ajax.ts` | `Ajax`, `ltrUrls`, `ltrParametrosNeg`, `ltrOperacion`, `ltrClaveDeEstado`, `ltrParametrosUrl` | 1, 2, 3, 4 | 690 |
| 8 | `08_Eventos.ts` | `ltrEventos`, `GestorDeEventos` | 4 | 305 |
| 9 | `09_Propiedades.ts` | `ltrPropiedades`, `ltrDatosPropuestos`, `ltrValores`, `ltrNegocioSe` | 1, 2, 4 | 1.635 |
| 10 | `10_Menus.ts` | `ltrMenus`, `ltrEtiquetas`, `ltrTitle` | 8, 9 | 480 |
| 11 | `11_Html.ts` | `ltrIdsHtml`, `ltrHtml` | 6 | 175 |

Comprobación de las 29 dependencias actuales contra este orden: todas apuntan a un fichero anterior o a un objeto anterior del mismo
fichero (`atControl → atMantenimniento → atGrid → atGridDeDetalle → atModal`, `ltrClaveDeEstado → ltrParametrosUrl`,
`ltrEspanes → ltrModalDeCrearRelacion`, `ltrTipoControl → ltrGridDeUnExpansor`, `ltrIdsHtml → ltrHtml`).

Regla para el futuro (ponerla como comentario de cabecera en cada fichero): *un fichero solo puede leer, al definirse, objetos de
ficheros con número menor*. Leer dentro de una función o de una plantilla que se evalúa después no cuenta.

### 3.3 Partir `ltrPropiedades` y `Ajax.EndPoint` por área (opcional, fase 5)

Aun troceado, `09_Propiedades.ts` tendría 1.635 líneas. Un `const` objeto no se puede repartir entre ficheros, pero un **`namespace` sí**:
TypeScript fusiona los `namespace` con el mismo nombre declarados en varios ficheros sin módulos, y el acceso sigue siendo
`ltrPropiedades.Venta.Precio`, así que **no cambia ninguna línea de los 96 ficheros que lo usan**:

```ts
// _Literales/Propiedades/Venta.ts
namespace ltrPropiedades {
    export const Venta = {
        Presupuesto: { ... },
        ...
    };
}
```

Compila a `var ltrPropiedades; (function (ltrPropiedades) { ltrPropiedades.Venta = {...}; })(ltrPropiedades || (ltrPropiedades = {}));`.

Condiciones y riesgos:
* Hay que convertir **todo** el objeto (un `const ltrPropiedades` y un `namespace ltrPropiedades` no pueden convivir).
* Las claves sueltas de primer nivel (`baja`, `DelSistema`, `RecargarGrid`...) pasan a `export const baja = 'baja'` en un
  `Propiedades/Comunes.ts`. El tipo pasa de `string` a literal `'baja'`; no debería romper nada (con `let` se ensancha), lo dirá `tsc`.
* No vale para `literal` (tiene claves `true`, `false`, `undefined`, que no pueden ser nombres de `export const`).
* Ventaja extra: cada área podría vivir junto a su código (`ts/Venta/...`), pero **no se recomienda**: las áreas de `ltrPropiedades`
  `Elemento`, `Entorno`, `Negocio`, `SisDoc`, `Callejero` y `Gasto` se usan desde ficheros comunes del bundle (`_Api`, `_Crud`), y los
  `.ts` de cada área solo se cargan en sus páginas. Se quedan todas en `_Literales/Propiedades/` y en el bundle.

Lo mismo serviría para `Ajax.EndPoint` (`namespace Ajax.EndPoint { export const Venta = {...} }`), pero solo si antes se ordena `Ajax`
(apartado 2.3.d).

---

## 4. Plan por fases (de menor a mayor riesgo)

Cada fase es un commit independiente que se puede publicar y revertir por separado. Ninguna fase cambia **valores**, salvo la 4, que se
revisa caso a caso.

### Fase 0 — Preparación (sin cambios funcionales)
1. Partir de main al día (los cambios de `ltrIdsHtml` y compañía ya están en ddb478f).
2. Unificar las listas de scripts de `Startup.cs` y `_Layout.cshtml` en una sola lista en C# (y decidir qué orden manda: el del layout,
   con `PanelDeControl.js` al final, es el más seguro).
3. Añadir el script de instantánea (apartado 5) y guardar la instantánea de referencia de los valores actuales.

**Comprobar:** `tsc --noEmit` sin errores; arrancar en local con `UsarBundle=false` y con `UsarBundle=true`, consola del navegador sin
errores en la página de inicio, un mantenimiento y una modal.

### Fase 1 — Trocear sin cambiar nada (riesgo bajo)
Mover los objetos, **tal cual**, a los 11 ficheros del apartado 3.2 y borrar `literales.ts`. Añadir los 11 `.js` a la lista de scripts
(en lugar de `Literales.js`). Corregir de paso la mayúscula de `Literales.js`.

**Comprobar:**
* `tsc --noEmit` sin errores (detecta nombres que se pierdan, no el orden).
* Script de instantánea: carga los 11 `.js` en orden en node y compara con la referencia → tiene que salir **idéntico** y sin
  `ReferenceError` (esto sí detecta el orden).
* Navegador con y sin bundle, igual que en la fase 0.
* Publicar primero en preproducción (`desplieguePreProductivo.exe`).

### Fase 2 — Recolocar lo que está en el sitio equivocado (riesgo bajo)
Sin renombrar nada: `enumCssOpcionMenu` a Css (ya en la fase 1), constantes no ajax de `Ajax` (`eventoLoad`, `eventoError`,
`eventoProgreso`, `jsonResult*`) se quedan pero con comentario; quitar `TipoMensaje` y `TagName` si se confirma que no se usan.

**Comprobar:** `tsc`, instantánea (solo deben desaparecer los objetos borrados).

### Fase 3 — Resolver duplicados por referencia (riesgo bajo-medio)
Para cada pareja del apartado 2.3.a/b, dejar un único valor escrito y que el otro **lo referencie**
(p. ej. `ltrCss.tdPropiedad = atNombre.tdPropiedad`, `atListasDeElemento.idNegocio = literal.idNegocio`,
`ltrEventos.PasarOcr = ltrCss.crud.panelDeEdicion.PasarOcr`). Los nombres no cambian, así que el resto del código no se toca.
Cuidado con el orden: la referencia tiene que ir del fichero de número mayor al menor.

**Comprobar:** `tsc` + instantánea idéntica (los valores no cambian, solo de dónde salen).

### Fase 4 — Eliminar claves duplicadas o sin uso (riesgo medio)
Borrar uno de cada par repetido (`atControl.MenuFlotante`, `atEntreFechas.idfechahasta`...), sustituyendo sus usos, y las claves sin uso
del apartado 1.8 tras comprobar que tampoco se usan desde C#/cshtml por valor. Incluye `Ajax.EndPoint.Administracion.Tarea.Renombrar`
y `Ajax.EndPoint.MaestrosTecnicos`. Unificar `Ajax.Entorno.MiCorreo.DescargarAdjunto` con su gemelo de `Ajax.EndPoint`.

**Comprobar:** `tsc` (cualquier uso olvidado da error de compilación, porque las propiedades de objetos literales están tipadas);
instantánea (solo desaparecen las claves borradas); prueba manual de las pantallas afectadas.

### Fase 5 — `namespace` para `ltrPropiedades` (y quizá `Ajax.EndPoint`) (riesgo medio)
Apartado 3.3: `_Literales/Propiedades/Comunes.ts`, `Entorno.ts`, `Maestros.ts`, `Negocio.ts`, `Callejero.ts`, `Elemento.ts`,
`SisDoc.ts`, `Terceros.ts`, `Juridico.ts`, `Logistica.ts`, `Venta.ts`, `Gasto.ts`... todos cargados justo donde estaba
`09_Propiedades.ts` y antes de `ltrDatosPropuestos` (que lee `ltrPropiedades.Elemento.Descripcion`).

**Comprobar:** `tsc` + instantánea idéntica + navegador con y sin bundle.

### Fase 6 — Renombrados (riesgo alto por volumen, opcional)
Erratas de nombre del 2.4 y unificar alias (`literal` → `atControl`/`Ajax.Param`). Son cambios en muchos ficheros (`literal` tiene 498 usos
en 69 ficheros); conviene hacerlos con "renombrar símbolo" del IDE, uno por commit, o no hacerlos y limitarse a no usar los nombres viejos
en código nuevo.

---

## 5. Herramienta de comprobación: instantánea de los literales

`tsc` no ve el orden de carga y el navegador solo lo ve si se abre la página adecuada. Como los literales son datos puros (no usan el DOM),
se pueden ejecutar en node y comparar. Propuesta de script (por ejemplo `Documentación/ScriptCmd/InstantaneaDeLiterales.js`, no se ha
creado todavía):

```js
// node InstantaneaDeLiterales.js <ficheros .js en orden de carga...> > instantanea.json
const fs = require('fs'), vm = require('vm');
const ficheros = process.argv.slice(2);
const nombres = [/* los 92 objetos del inventario: 'literal', 'ltrCss', 'Ajax', ... */];
const codigo = ficheros.map(f => fs.readFileSync(f, 'utf8')).join('\n;\n')
    + `\n;globalThis.__instantanea = { ${nombres.map(n => `${n}: typeof ${n} === 'undefined' ? '<<no definido>>' : ${n}`).join(', ')} };`;
const contexto = vm.createContext({});
vm.runInContext(codigo, contexto);          // un ReferenceError aquí = orden de carga incorrecto
console.log(JSON.stringify(contexto.__instantanea, Object.keys, 2)); // ordenar claves para poder comparar
```

Uso: antes de la fase 1 se genera con `wwwroot/js/_Literales/literales.js`; después de cada fase, con los ficheros nuevos en el orden de la
lista de scripts, y se comparan con `fc` o `git diff --no-index`. Con node de Visual Studio:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Microsoft\VisualStudio\NodeJs\node.exe" Documentación\ScriptCmd\InstantaneaDeLiterales.js wwwroot\js\_Literales\literales.js > $env:TEMP\literales-antes.json
```

(Para generar los `.js` hace falta compilar sin `--noEmit`, o compilar desde Visual Studio.)

---

## 6. Decisiones que necesito

1. ¿Te vale la división **por tipo** en 11 ficheros (3.2), o prefieres menos ficheros (p. ej. juntar 1-3 y 5)?
2. ¿Hacemos la **fase 0** (una sola lista de scripts para `Startup` y `_Layout`)? Es lo que más reduce el riesgo de que se repita el
   fallo de producción.
3. ¿Llegamos hasta la fase 3 (sin cambiar nombres) o también 4 y 5? La 6 la dejaría para cuando se toque cada zona.
4. Prefijo numérico en los nombres de fichero (`01_Basicos.ts`) ¿sí o no?

---

## 7. Resultado de la ejecución y revisión final

Decisiones del usuario: 11 ficheros por tipo, una sola lista de scripts, todas las fases, ficheros `literales_<lo que corresponda>.ts`
sin prefijo numérico. Todo está en el worktree, **sin commitear**.

### 7.1 Qué se ha hecho

| Fase | Cambio | Comprobación |
|---|---|---|
| 0 | `ListaDeScripts.cs` (nuevo): única lista de scripts, con el orden del layout (`PanelDeControl.js` el último). `Startup.cs` genera el bundle con ella y `_Layout.cshtml` pinta un `<script>` por elemento. En el `.csproj`, `Content Remove` pasa a `wwwroot\ts\_Literales\**\*.ts`. | `dotnet build` Release: 0 errores. |
| 0 | `Documentación/ScriptCmd/InstantaneaDeLiterales.js` (nuevo): carga los literales en node en el orden de la lista y escribe "ruta = valor". | Probado: un orden incorrecto da `ReferenceError`. |
| 1 | `literales.ts` → 11 ficheros `literales_*.ts`, con el texto movido tal cual, y se borra `literales.ts`. | Instantánea **idéntica** (3.343 valores). |
| 2 | Borrados `TipoMensaje` y `TagName` (sin uso). Comentario en las constantes de `Ajax` que no son de ajax. | Solo desaparecen esos 5 valores. |
| 3 | 29 valores escritos dos veces pasan a ser referencias al literal que ya los tenía. Por ejemplo, `ltrCss.tdPropiedad = atNombre.tdPropiedad`, `ltrEventos.Edicion.PasarOcr = ltrCss.crud.panelDeEdicion.PasarOcr`, `atListasDeElemento.idNegocio = literal.idNegocio` y `ltrNegocioSe.Enumerado.* = enumNegocio.*`. | Instantánea sin cambios de valor. |
| 4 | Se unifican 8 pares de claves duplicadas, sustituyendo sus usos en los `.ts`: `atControl.menuFlotante`→`MenuFlotante`, `atEntreFechas.idfechahasta/idhorahasta`→`idFechaHasta/idHoraHasta`, `atCriterio.restringido`→`igual`, `ltrControladores.Comunes.Archivadores`→`SisDoc.Archivador`, `ltrEspanes.Expedientes.CrearValoracion`→`ppts`, `ltrEspanes.Juridico.Contratos.CopiarPlfDeVenta`→`PlanificadorDeVentas` y `Ajax.Entorno.MiCorreo.DescargarAdjunto`→`Ajax.EndPoint.Entorno.MiCorreo.DescargarAdjunto`. Se borran **296 claves sin uso** en ningún `.ts`, `.cs` ni `.cshtml`, entre ellas `Ajax.EndPoint.Administracion.Tarea.Renombrar` (la que apuntaba a `epRenombarUnaFar`) y `Ajax.EndPoint.MaestrosTecnicos`. No se tocan los catálogos que reflejan enumerados de C#: `enum*`, `enumNegocio`, `ltrTipoControl`, `atCriterio`, `ltrValores`, `ltrSimbolos`, `ltrOperacion` y `ltrClaveDeEstado`. | Instantánea: desaparecen exactamente las rutas de la lista y ningún valor cambia. `tsc` sin errores. |
| 5 | `ltrPropiedades` pasa a ser un `namespace` repartido en `literales_Propiedades.ts` (claves comunes, `ltrDatosPropuestos`, `ltrValores` y `ltrNegocioSe`) y 7 ficheros por área: `literales_Propiedades_Entorno` (con Maestros, Negocio y Callejero), `_SisDoc` (con Tarea, Expediente y Contabilidad), `_Terceros`, `_Juridico` (con Guarderias), `_Logistica`, `_Venta` y `_Gasto`. El acceso no cambia (`ltrPropiedades.Venta...`), así que no se ha tocado ningún fichero que lo use. | Instantánea idéntica a la de la fase 4. |
| 6 | Renombrados: `atMantenimniento`→`atMantenimiento`, `ttrModoPresentacion`→`ltrModoPresentacion`, `ltrMenus...CantidadALeeer`→`CantidadALeer`, `IdsDeOpcinesMf`→`IdsDeOpcionesMf`, `OpcinesMf`→`OpcionesMf`, `Ajax.persisitencia`→`persistencia`, `Ajax.Entorno.Ia.PregutaConConteo`→`PreguntaConConteo`, `atSelectorDeFiltro.popiedadBuscar`→`propiedadBuscar`, `ltrCss.crud.grid.filto`→`filtro`, `ltrCss.Espan.animarBarrra`→`animarBarra` y `Ajax.Param.Vista`→`VistaMvc`. Son 84 sustituciones en 20 ficheros `.ts`. | Instantánea = la de la fase 5 con los nombres nuevos y los mismos valores. |

Además:
* `wwwroot/Acceso/Conectar.html` (la página de login, `LoginPath`) y `NuevaContrasena.html` cargaban `/js/_Literales/literales.js`.
  Ahora cargan los 18 `literales_*.js` en el orden de `ListaDeScripts.cs`. **Sin este cambio no se podría entrar en la aplicación.**
  Como son html estáticos, repiten la lista a mano; lo indica un aviso en `ListaDeScripts.cs`.
* El `dotnet build` reescribió con otros finales de línea `wwwroot/lib/leaflet/dist/leaflet.js` y `wwwroot/lib/types/googlemaps/index.d.ts`
  (restauración de libman). Se han devuelto a su estado original.

### 7.2 Revisión final

1. **Valores:** la instantánea final es exactamente la inicial **menos las 301 rutas borradas** (296 claves y los 5 valores de
   `TipoMensaje`/`TagName`) **con 11 rutas renombradas**. Los otros 3.042 valores son idénticos y se cargan sin error en el orden de la lista.
2. **Sustituciones y renombrados:** `tsc --noEmit` no da errores. En los literales no hay accesos dinámicos (`obj[...]`), así que
   cualquier uso olvidado de una clave borrada o renombrada habría dado error de compilación. No queda ningún nombre antiguo en `.ts`, `.cs`,
   `.cshtml`, `.html` ni `.json`, salvo `wwwroot/Procesar.html` (ver 7.3).
3. **C#:** `dotnet build SistemaDeElementos.csproj -c Release` no da errores. Para compilar desde el worktree hubo que crear un enlace
   temporal, porque `ServicioXml.csproj` referencia VeriFactu con una ruta relativa que solo resuelve desde la copia principal; el enlace
   ya está quitado.
4. **Cadenas sueltas:** hay 126 valores (288 apariciones) escritos a mano en los `.ts` que ya existen en los literales. Más otros 26
   valores (349 apariciones) que coinciden con palabras de JS/DOM y lo normal es dejarlos: `'load'` (112, el evento de `addEventListener`),
   `'undefined'`, `'none'`, `'true'`, `'string'`, `'href'`... Lista completa en
   `Documentación/LiteralesSueltos.txt`. Los más claros para pasar a literales:
   * `_Api/ApiDePeticiones.ts`: nombres de parámetros (`'idElemento'`, `'negocio'`, `'idNegocio'`, `'idArchivo'`, `'parametros'`), que ya están en `Ajax.Param`.
   * `TrabajosSometido/TrabajosDeUsuario.ts`: `'bloquear'`, `'desbloquear'`, `'ejecutar'`, `'resometer'`, `'editar'`, que ya están en `ltrMenus.eventosDeMf.Entorno.Trabajo`.
   * `Acceso/Conectar.ts`: `'login'`, `'password'`, `'codigo2F'`, `'guid'` (`Ajax.Param`).
   * `'fade-in'` (`ltrCss.Modal.FadeIn`) en `ApiDeArchivos`/`ApiVisorDeArchivos`, `'fila-pulsada'` en `GridDeDatos`, `'lineasdeunafar'` en `FacturasRec`.
   * `Callejero/literales.ts` repite `'idpais'`, `'idprovincia'`... de `ltrPropiedades.Callejero`.

   Ninguna de estas cadenas la ha introducido esta reestructuración: ya estaban así. **No se han cambiado**; es un trabajo aparte si se quiere.

### 7.3 Lo que no se ha hecho (y por qué)

* **Unificar `literal` con `atControl`/`Ajax.Param`** (parte de la fase 6). Son 498 usos en 69 ficheros con valores idénticos, así que solo
  cambiaría el nombre, y en cada uso hay que decidir si es atributo o parámetro. Queda `literal` congelado: no se le añade nada nuevo.
* **Duplicados dentro de `ltrCss` en los que se usan las dos claves.** Por ejemplo, `crud.panelCreacion` y `crud.panelDeEdicion`
  comparten valores pero son contextos distintos del DOM. Lo mismo con `atControl.ampliacionSoloConsulta` (alias documentado de `soloConsulta`).
* **Reordenar `Ajax`** (sus tres estructuras) y partir `Ajax.EndPoint` en `namespace`: la propuesta lo condicionaba a reordenar `Ajax`
  antes. Solo se ha unificado `DescargarAdjunto`.
* `enumTipoDeModal` frente a `enumPostfijoTipoModal`, y `ltrPaginas` frente a `ltrUrls`: siguen separados porque tienen valores distintos.
* `wwwroot/Procesar.html` ya estaba roto antes de este cambio (carga `/ts/Literales/Literales.js`, `/ts/_Api/ApiCrud.js`... que no existen).
  No se ha tocado.
* **A revisar:** `Startup.GenerarBundleManualmente` forma la ruta como `rutaBase + rutaAbsoluta`. Con `UsarBundle=true` parece que
  lanzaría "No existe el fichero". No se ha tocado; conviene confirmar qué configuración se usa en producción.
* No se ha probado en el navegador. Antes de publicar conviene abrir el login, la página de inicio, un mantenimiento y una modal en
  local (con `UsarBundle` a false y a true si se usa) y en preproducción.
