# Diseño de plantillas de impresión (Word)

Esta guía explica cómo funciona el mecanismo de impresión por plantillas de Se10 (un `.docx` con
etiquetas `{{{...}}}` que el sistema sustituye por datos reales) y cómo diseñar una plantilla nueva de
principio a fin: desde darla de alta en la aplicación hasta escribir el procedimiento almacenado (PA)
que le da de comer datos. Se basa en el código real del motor
(`ServicioDeReportes/Base/ApiDePlantillas.cs`, `GestoresDeNegocio/SistemaDocumental/ServicioDeImpresion.cs`,
`ServicioDeDatos/_Elemento/PaSql.cs`) y en los ejemplos reales del repositorio (`ServicioDeDatos/Ventas/Factura/Plt_Emision_De_Factura.sql`
y `Plt_Por_Tipo_Anexo_de_factura_de_desarrollo.sql`, además de los ficheros de prueba
`SistemaDeElementos/Documentación/Plantillas/Test_FacturasPlt.docx` y `Test_FacturasPlt_2.docx`). Al
final hay un apéndice con un caso real completo: la plantilla de factura de Un Abogado.

## 1. Dos clases de plantilla, y cuándo usar cada una

| Clase | Se define en | Se aplica a | Nombre de PA que genera el sistema |
|---|---|---|---|
| **Plantilla de negocio** (`PlantillaDeNegocioDtm`) | Ficha "Plantillas" del propio negocio (p.ej. Facturas emitidas) | Cualquier elemento del negocio, sin distinguir tipo | `Plt_De_Negocio_<Nombre normalizado>` |
| **Plantilla por tipo** (`PlantillaPorTipoDtm`) | Ficha del Tipo concreto dentro del negocio | Solo elementos de ese Tipo | `Plt_Por_Tipo_<Nombre normalizado>` |

El nombre del PA no se escribe a mano: sale de `NombrePa => $"Plt_De_Negocio_{Nombre.NormalizarFichero()}"`
(`ServicioDeDatos/Negocio/NegocioDtm.cs:56`) o `Plt_Por_Tipo_...` (`ServicioDeDatos/_Elemento/TipoDeElementoDtm.cs:455`),
donde `NormalizarFichero()` cambia espacios y comas por `_` (`Ayudas/Extensiones/Extensiones.cs:372`). Es
decir: si al crear la plantilla le pones de Nombre "Factura de Word", el PA se llamará
`Plt_De_Negocio_Factura_de_Word`, sin que tengas que decidirlo tú.

Usa "Plantilla por tipo" cuando el mismo negocio tiene variantes que necesitan un documento distinto
(p.ej. una "Factura" normal y un "Anexo" de desarrollo de horas, como en
`Plt_Por_Tipo_Anexo_de_factura_de_desarrollo.sql`). Usa "Plantilla de negocio" para el caso general (una
factura, un pedido, un contrato...).

## 2. Cómo se imprime — flujo completo

1. El usuario, con un elemento abierto, pide imprimir. Si hay varias plantillas aplicables se le muestra
   un diálogo para elegir; si solo hay una, se usa directamente.
2. El cliente llama a `BaseController.epImprimir` (`SistemaDeElementos/Controllers/BaseController.cs:497`),
   que delega en `Imprimir(idNegocio, parametros)` (línea 528) con el `IdPlantilla`, `idElemento` y la
   `Clase` de plantilla elegidos.
3. Se instancia `new ServicioDeImpresion(Contexto, negocio, idElemento, idPlantilla).Imprimir(clase)`
   (`GestoresDeNegocio/SistemaDocumental/ServicioDeImpresion.cs:46-96`), que hace en orden:
   1. Localiza la plantilla (`PlantillaDeNegocioDtm`/`PlantillaPorTipoDtm`) y descarga su `.docx`.
   2. Lee la `AccionDtm` asociada (`accion.Esquema` + `accion.Pa`) — debe ser de clase `PA`.
   3. Ejecuta el PA (`ExtensorDePlantillas.LeerDatos`) y parsea su primer result-set, el **descriptor**
      (ver punto 3).
   4. Inyecta automáticamente en `datosDelObjeto` los datos **del propio elemento** (bajo la clave del
      nombre del Dtm sin sufijo, p.ej. `FacturaEmt`) y de sus **ampliaciones** (`VerifactuDtm`,
      `IrpfEmtDtm`, `PeriodoEmtDtm`... cada una bajo su propio nombre) — sin que el PA tenga que
      devolver nada de esto (ver punto 4.a).
   5. Calcula los **detalles** del elemento (`DetallesDelObjeto`): Hitos, Observaciones, Direcciones y
      los "tipos de detalle" propios del negocio (ver punto 4.c).
   6. Copia la plantilla al fichero de salida y llama a `GenerarDocumentoDocx`, que abre el `.docx` con
      `DocumentFormat.OpenXml` y aplica, sobre el cuerpo y cada pie de página, en este orden:
      `ProcesarParte` (etiquetas del PA + etiquetas del Dto + fórmulas) → `ProcesarMapeosDeTablasDelPa`
      (tablas del PA) → `ProcesarMapeosDeDetalles` / `ProcesarMapeosDeExtensiones` (tablas de detalle:
      Detalles, Hitos, Observaciones, Direcciones).

No hay ningún motor de plantillas de terceros (nada de Mustache/Handlebars/RazorEngine): es una
implementación propia, basada en buscar/reemplazar sobre los `Descendants<Text>()` del documento OOXML.
El delimitador `{{{ }}}` son literalmente las constantes `Simbolos.PltInicio`/`PltCierre`
(`Ayudas/Extensiones/ClasesComunes.cs:67,69`), no un motor de secciones/helpers.

## 3. El contrato del procedimiento almacenado (PA)

Todo PA de plantilla recibe siempre `@IdNegocio INT, @IdElemento INT` y su **primer result-set** es un
descriptor de 4 filas fijas `(orden, tipo, dato)`:

| `tipo` | `dato` | Significado |
|---|---|---|
| `pltFormulasDePlantilla` | `-1` (sin fórmulas propias) o lista de claves | Actualmente la única fórmula soportada es `impresaEl` → se sustituye por `DateTime.Now` formateado `dd-MM-yyyy` |
| `pltDatosDePlantilla` | `'1:alias1,2:alias2,...'` | Qué **result-set** (por posición, contando desde el siguiente al descriptor) alimenta cada **alias escalar** — una sola fila de datos |
| `pltFilasDeTabla` | `'3:nombreTabla1,7:NombreTabla2'` | Qué result-set alimenta cada **tabla repetida** — puede haber varias filas de datos |
| `pltMapeosDeTabla` | `'nombreTabla1:col0=Columna,col1=OtraColumna\|nombreTabla2:...'` | Para cada tabla, qué columna real del SELECT corresponde a cada placeholder `colN` del Word |

**El índice es la posición secuencial del SELECT dentro del PA**, da igual si es escalar o de tabla — los
result-sets se numeran 1, 2, 3... en el orden en que aparecen las consultas después del descriptor,
mezclando escalares y tablas según haga falta. Ejemplo real
(`ServicioDeDatos/Ventas/Factura/Plt_Emision_De_Factura.sql:8-21`):

```sql
select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
union
select 1 as orden, 'pltDatosDePlantilla' as tipo, '1:factura,2:cliente,3:sociedad, 6:documento, 7:cobrado' as dato
union
select 2 as orden, 'pltFilasDeTabla' as tipo, '4:lineasdefactura,5:Cobros' as dato
union
select 3 as orden, 'pltMapeosDeTabla' as tipo,
       'lineasdefactura:col0=Concepto,col1=Cantidad,col2=Unidad,col3=Precio,col4=Importe,col5=Iva,col6=Total' + '|' +
       'cobros:col0=Fecha,col1=Importe,col2=Pendiente,col3=Resto'
       as dato
```

Aquí el result-set 1 es "factura" (escalar), 2 "cliente" (escalar), 3 "sociedad" (escalar), el 4
"lineasdefactura" (tabla), el 5 "cobros" (tabla), el 6 "documento" (escalar) y el 7 "cobrado" (escalar) —
en ese orden exacto tienen que ir los `SELECT` en el cuerpo del PA, cada uno precedido de un comentario
que diga qué es (por legibilidad, no lo lee el motor).

### 3.1 Alta automática del PA (no lo creas tú a mano)

Cuando das de alta una plantilla nueva desde la aplicación (`GestorDePlantillasDeNegocio.AntesDePersistir`,
`GestoresDeNegocio/Negocio/GestorDePlantillasDeNegocio.cs:48-77`), el sistema:

1. Crea automáticamente el PA con `Contexto.CrearPAParaDatosPlantilla(esquema, plantilla.NombrePa)`
   (`ServicioDeDatos/_Elemento/PaSql.cs:57-104`), con este esqueleto de ejemplo (6 result-sets: 3
   "fuentes" escalares, "documento", y dos tablas):

   ```sql
   CREATE PROCEDURE <esquema>.<NombrePa>
     @IdNegocio INT,
     @IdElemento INT
   AS
   BEGIN
       -- Descriptor de información para una plantilla
       select tipo, dato
       from (
             select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
             union
             select 1 as orden, 'pltDatosDePlantilla' as tipo, '1:fuente_1, 2:fuente_2, 3:fuente_3, 4:documento' as dato
             union
             select 2 as orden, 'pltFilasDeTabla' as tipo, '5:tabla_1,6:Tabla_2' as dato
             union
             select 3 as orden, 'pltMapeosDeTabla' as tipo,
                 'tabla_1:col0=columna_1,col1=columna_2,col2=columna_2,col3=column_4' + '|' +
                 'tabla_2:col0=columna_1,col1=columna_2,col2=columna_2,col3=column_4'
                 as dato)
          as descriptor
       order by orden

       -- Consulta para obtener los datos de la fuente_1
       select 'campo1' as campo1,'campo2' as campo2
       -- Consulta para obtener los datos de la fuente_2
       select 'campo1' as campo1,'campo2' as campo2
       -- Consulta para obtener los datos de la fuente_3
       select 'campo1' as campo1,'campo2' as campo2
       -- Consulta para obtener los datos del documento
       select 'titulo del documento' as titulo, 'DateTime.Now' as impresaEl
       -- Consulta para obtener tablas: Tabla_1
       select 'columna 1' as columna_1, 'columna 2' as columna_2, 'columna 3' as columna_3, 'columna 4' as columna_4
       -- Consulta para obtener tablas: Tabla_2
       select 'columna 1' as columna_1, 'columna 2' as columna_2, 'columna 3' as columna_3, 'columna 4' as columna_4
   END
   ```

   Este esqueleto es solo un punto de partida: **tu trabajo es editarlo por completo** — decidir cuántos
   alias/tablas necesitas de verdad (pueden ser menos, más o ninguno, si te basta con las etiquetas
   automáticas del punto 4.a), ajustar el descriptor a esa cantidad exacta, y sustituir cada `SELECT` de
   ejemplo por la consulta real. Esto se edita directamente en la base de datos (SSMS), no desde la
   aplicación.
2. Crea una `AccionDtm` (`Nombre = "Plantilla: <nombre>"`, `ClaseDeAccion = PA`) que enlaza la plantilla
   con ese PA.
3. Si no subiste tú un `.docx`, crea uno **vacío** (`ApiDePlantillas.CrearLaPlantilla`) para que lo
   rellenes y lo vuelvas a subir; si subiste uno, lo usa tal cual.
4. Da de alta el permiso de uso de la plantilla (clase `Plantilla`).

Si renombras la plantilla, el sistema renombra el PA y el archivo a la vez (`RenombrarPa`,
`GestorDePlantillasDeNegocio.cs:108`); si la borras, borra el PA, la Acción y el archivo. **No renombres
el PA a mano en la base de datos**: hazlo cambiando el Nombre de la plantilla en la aplicación.

## 4. De dónde salen las etiquetas del Word

Hay varios orígenes de etiquetas, y no se excluyen entre sí — se pueden combinar libremente en el mismo
documento.

### 4.a. Automáticas del elemento y sus ampliaciones — `{{{Prefijo.Campo}}}`

Al imprimir, el sistema vuelca **todas** las propiedades del Dto del propio elemento (p.ej.
`FacturaEmtDto`) bajo la clave `FacturaEmt` (el nombre del Dtm sin el sufijo `Dtm`), y hace lo mismo con
cada **ampliación** que tenga el negocio (`VerifactuDtm` → `{{{Verifactu.Campo}}}`, `IrpfEmtDtm` →
`{{{IrpfEmtDto.Campo}}}`...). No hace falta que el PA devuelva nada de esto: es automático, por
reflexión sobre el Dto (`ServicioDeImpresion.cs:65-81`).

**Para saber qué etiquetas tienes disponibles de este tipo, no las adivines**: en la ficha de la
plantilla (`ModeloDeDto/Negocio/PlantillaDeNegocioDto.cs:107-120`) hay un enlace **"Etiquetas
disponibles"** que descarga un `.docx` con el listado completo y actualizado de etiquetas del negocio (el
propio elemento, sus ampliaciones, sus tablas de detalle, y desde ahora también los datos maestros del
punto 4.d) — lo genera `ApiDeEtiquetas.CrearFicheroDeEtiquetas` (`ServicioDeReportes/Base/ApiDeEtiquetas.cs`).
Es el fichero que hay que mirar primero siempre, antes de escribir una sola etiqueta a mano.

Formato de sustitución según el tipo .NET de la propiedad (`ApiDePlantillas.cs:431-449`):

| Tipo .NET | Cómo se imprime |
|---|---|
| `string` / `null` | Tal cual |
| `int` | `.ToString()` |
| `decimal` | `.ToString()` — **sin separador de miles ni símbolo de moneda**, con la coma/punto decimal de la cultura del servidor |
| `DateTime` | `.ToString()` **con la cultura del servidor** (normalmente incluye la hora, no es solo `dd-MM-yyyy`) |
| `bool` | `"Si"` / `"No"` |
| `enum` | `.Descripcion()` (el atributo `[Description]` del valor) |

> Si necesitas un formato concreto (fecha corta sin hora, decimales con dos cifras, moneda con símbolo,
> etc.), **no uses la etiqueta automática**: trae ese dato ya formateado como texto desde el PA (ver
> 4.b), con `FORMAT(...)`/`CONVERT(...)` en SQL — así es como lo hacen los dos PA reales del repositorio
> (`FORMAT(FACTURADA_EL, 'dd-MM-yyyy')`, `FORMAT(..., 'N2')`).

### 4.b. Explícitas del PA — `{{{alias.campo}}}`

Cualquier alias que definas en `pltDatosDePlantilla` (ver punto 3) da acceso a `{{{alias.NombreDeColumna}}}`
para cada columna que devuelva su `SELECT`. Estos valores llegan **ya como texto** (tal como los
devuelve SQL Server) y se sustituyen literalmente vía expresión regular sobre el XML
(`ProcesarEtiquetasDeUnPa`, `ApiDePlantillas.cs:189-205`) — el motor no aplica ningún formato adicional,
así que el formateo (fechas, decimales, `N2`, moneda...) es responsabilidad tuya en el propio `SELECT`.

Usa este mecanismo para todo lo que **no** esté ya en el Dto del elemento ni en sus ampliaciones: datos
de otros terceros relacionados (cliente, sociedad/emisor...), agregados calculados, o cualquier dato que
necesites con un formato concreto.

### 4.c. Datos maestros — `{{{maestro.<clave>....}}}`

Todo lo que el punto 4.b resuelve escribiendo un `JOIN` a mano en el PA (cliente, sociedad emisora,
centro gestor...) tiene, para los casos más habituales, una alternativa que **no toca el PA en
absoluto**: el registro extensible `GestoresDeNegocio.SistemaDocumental.DefinicionesDeMaestros.Registro`
(`GestoresDeNegocio/SistemaDocumental/GestorDeMaestros.cs`). Al imprimir, `ServicioDeImpresion` instancia
`new GestorDeMaestros(Contexto, Elemento)` (igual que ya hacía con `DetallesDelObjeto`) y comprueba, para
cada entrada del registro, si el elemento implementa la interfaz de la que sale esa clave; si es así, cala
sus datos (`DatosPrincipales`, `Direcciones`, `CuentasBancarias`) sin que el PA tenga que pedirlos.

Los cinco maestros dados de alta hoy:

| Clave | Sale de... | Interfaz que lo activa | Tiene Direcciones/CuentasBancarias |
|---|---|---|---|
| `cliente` | `ClienteDto` | `IUsaCliente` / `IPuedeUsarCliente` | Sí |
| `proveedor` | `ProveedorDto` | `IUsaProveedor` / `IPuedeUsarProveedor` | Sí |
| `solicitante` | `InterlocutorDto` | `IUsaSolicitante` | Sí |
| `MiCg` | `CentroGestorDto` (el Centro Gestor del propio elemento) | `IUsaCg` | No (solo `DatosPrincipales`) |
| `MiSociedad` | `SociedadDto` (la Sociedad titular de ese Centro Gestor) | `IUsaCg` (vía `MiCg.IdSociedad`) | Sí |

`IUsaCg` la implementa cualquier elemento que herede de `ElementoConCgDtm`/`ElementoDeProcesoDtm`
(`ServicioDeDatos/_Elemento/ElementoDtm.cs`) — es decir, la mayoría de los negocios de proceso del
sistema, no solo facturas. Igual que en 4.a, no hace falta que el PA sepa nada de esto: si la interfaz
está implementada, la etiqueta funciona sola.

Gramática de la etiqueta, según qué parte del maestro se referencia:

```
{{{maestro.<clave>.<Campo>}}}
                                    -> propiedad directa de DatosPrincipales

{{{maestro.<clave>.direccion.<Campo>}}}
{{{maestro.<clave>.direccion.[<Calificador>].<Campo>}}}
                                    -> <Campo> (de DireccionDto) de la primera dirección de la lista,
                                       o de la primera cuya Calificador coincida (p.ej. "fiscal")

{{{maestro.<clave>.cuentabancaria.<Campo>}}}
{{{maestro.<clave>.cuentabancaria.[<Clase>].<Campo>}}}
                                    -> <Campo> (de CuentaDeXxxDto) de la primera cuenta de la lista,
                                       o de la primera cuya Clase coincida (p.ej. "Ingreso")
```

Ejemplos reales usados en `Plantilla_Factura_de_Word_III.docx`:
`{{{maestro.cliente.VAT}}}`, `{{{maestro.cliente.direccion.[fiscal].Expresion}}}`,
`{{{maestro.MiSociedad.Nombre}}}`, `{{{maestro.MiSociedad.cuentabancaria.[Ingreso].Cuenta}}}`,
`{{{maestro.MiCg.Nombre}}}`.

El motor que resuelve estas etiquetas (`ApiDePlantillas.ProcesarEtiquetasDeMaestros`,
`ApiDePlantillas.cs`) es **distinto** del de 4.a/4.b: en vez de recorrer un diccionario cerrado de claves
conocidas, recorre el texto del documento buscando cualquier `{{{maestro....}}}`, lo parsea con una
expresión regular y lo resuelve dinámicamente contra el registro — por eso admite el filtro `[...]`, que
no existe en ningún otro tipo de etiqueta del sistema.

**Añadir un maestro nuevo** (Iva, Irpf, Unitario, Juzgado...) es una entrada más en
`DefinicionesDeMaestros.Registro`, con su `Clave`, cómo sacar su Id a partir del elemento (`LeerId`) y
cómo cargar sus datos (`CargarDatosPrincipales`, y opcionalmente `CargarDirecciones`/`CargarCuentasBancarias`
si aplica) — el resto de `GestorDeMaestros` y de `ProcesarEtiquetasDeMaestros` no cambia.

### 4.d. PA vs maestro: qué alternativa usar para traer datos de terceros

Desde el punto 4.c hay dos formas legítimas de traer datos de un tercero relacionado (cliente, sociedad,
centro gestor...) a la plantilla — no son excluyentes, se puede usar una para unos datos y otra para
otros en el mismo documento:

| | **PA** (4.b) | **Maestro** (4.c) |
|---|---|---|
| Trabajo por plantilla | Escribir y mantener el `JOIN`/`SELECT` en cada PA que lo necesite | Ninguno — ya está resuelto una vez para todo el sistema |
| Formateo (fechas, decimales, moneda) | Total control con `FORMAT`/`CONVERT` en SQL | El de 4.a (tipo .NET → texto), sin control fino |
| Filtrado de listas (direcciones/cuentas) | Cualquier condición SQL | Solo "primera de la lista" o "primera con un campo = valor" (`[filtro]`) |
| Alcance | Cualquier dato alcanzable por SQL | Solo lo que ya esté en el registro (hoy: cliente, proveedor, solicitante, MiCg, MiSociedad) |
| Reutilización | Ninguna — cada PA repite su propio join | Automática en cualquier plantilla de cualquier negocio que tenga la interfaz correspondiente |

**Regla práctica**: si lo que necesitas ya está en el registro de maestros y te vale el formateo por
defecto, usa el maestro — es menos código y no hay que tocar el PA. Si necesitas un dato que el registro
no cubre todavía, un formato concreto, o filtrar una lista con una condición que `[filtro]` no permite
expresar, tráelo por el PA (4.b) como hasta ahora. Ver el apéndice para un caso real que usa las dos
alternativas para el mismo dato en distintas versiones de la misma plantilla.

### 4.e. Tablas repetidas — dos mecanismos distintos, no los confundas

**Tablas que vienen del PA** (`pltFilasDeTabla` + `pltMapeosDeTabla`): la fila de datos usa placeholders
**genéricos** `{{{col0}}}`, `{{{col1}}}`... — sin prefijo de tabla — que se mapean a la columna real
mediante el propio `pltMapeosDeTabla` (`nombreTabla:col0=Concepto,col1=Importe`).

**Tablas de "detalle"** (`Hitos`, `Observaciones`, `Direcciones`, o cualquier "tipo de detalle" propio
del negocio — p.ej. líneas de factura si estuvieran modeladas como detalle en vez de venir del PA): la
fila de datos usa **el nombre de la propiedad .NET tal cual**, sin `colN` (`{{{Concepto}}}`,
`{{{Fecha}}}`...) — se resuelven por reflexión sobre el objeto (`CrearFilasEnLaTabla`,
`ApiDePlantillas.cs:360-395`), no por un mapeo explícito. No hace falta tocar el PA para estas: se
inyectan solas si el negocio usa flujo/observaciones/direcciones o tiene ese tipo de detalle definido.

### 4.f. Estructura de la tabla en Word — el patrón de 3 filas

En ambos mecanismos, la tabla del Word tiene que tener exactamente esta forma (verificado directamente
sobre el código de `CrearLineasEnLaTabla`/`CrearFilasEnLaTabla`, `ApiDePlantillas.cs:360-421`):

1. **Fila marcadora**: una celda cuyo contenido sea exactamente `{{{nombreTabla}}}` (o `{{{Hitos}}}`,
   `{{{Observaciones}}}`... para las de detalle) — el resto de celdas de esa fila pueden ir vacías. Esta
   fila se localiza buscando el texto exacto en todo el documento, y se borra al procesar.
2. **Fila de cabecera** (opcional pero recomendada): los rótulos de columna en texto normal, sin
   etiquetas — es la que ve el usuario final como cabecera de la tabla impresa.
3. **Fila plantilla** (tiene que ser la **última** fila de la tabla): una celda por columna, cada una con
   **únicamente** el placeholder — `{{{col0}}}`, `{{{col1}}}`... o `{{{NombrePropiedad}}}` según el
   mecanismo —, sin texto adicional en la misma celda. El motor clona esta fila una vez por cada
   registro devuelto, sustituye los placeholders, inserta las copias justo antes de la fila plantilla, y
   al final borra la fila plantilla original (que ya no tiene utilidad) junto con la fila marcadora.

Ejemplo real tal cual está en `SistemaDeElementos/Documentación/Plantillas/Test_FacturasPlt.docx`:

```
{{{lineasdefactura}}}
Concepto    Cantidad    Unidad    Precio    Importe    IVA    Total
{{{col0}}}  {{{col1}}}  {{{col2}}} {{{col3}}} {{{col4}}} {{{col5}}} {{{col6}}}
```

### 4.g. Etiquetas que no se resuelven

Si escribes una etiqueta `{{{alias.campo}}}`/`{{{Prefijo.Campo}}}` cuyo alias no está en el descriptor, o
cuyo campo no existe en el Dto/PA, **el motor la deja tal cual, sin avisar** — no da error, simplemente no
la toca. Es fácil de comprobar: en `SistemaDeElementos/Documentación/Plantillas/Docu_Facturas.pdf`
(resultado de una impresión de prueba) queda literalmente impreso `{{{nada.es}}}` en medio del texto,
porque ese alias no existe. **Revisa siempre el resultado impreso de una plantilla nueva buscando `{{{`
sobrante** — es la señal de que algo no se ha mapeado.

Las etiquetas `{{{maestro....}}}` (4.c) se comportan igual en cuanto a NO bloquear la generación del
documento — se dejan tal cual, igual que las demás —, pero además anotan un aviso en la traza del
contexto (`Contexto.AnotarTraza`, `ApiDePlantillas.ProcesarEtiquetasDeMaestros`) indicando exactamente qué
etiqueta no se pudo resolver (clave de maestro inexistente, campo inexistente, o filtro `[...]` sin
ninguna coincidencia en la lista). Sigue sin ser un error que detenga la impresión: es solo un rastro
adicional para depurar, que las demás etiquetas del sistema no dejan.

## 5. Checklist antes de subir una plantilla a producción

1. Descargar el `.docx` de "Etiquetas disponibles" del negocio y comprobar qué etiquetas automáticas
   (4.a) y qué datos maestros (4.c) cubren ya lo que necesitas, antes de tocar el PA.
2. Para lo que no cubran 4.a/4.c, decidir el descriptor del PA (4.d ayuda a decidir cuándo toca PA en vez
   de maestro): cuántos alias escalares (4.b) y cuántas tablas (4.e) hacen falta de verdad, y con qué
   índices de result-set.
3. Reescribir el PA generado por el sistema (3.1): un `SELECT` por índice, en el orden exacto del
   descriptor, formateando en SQL (`FORMAT`/`CONVERT`) todo lo que necesite un formato concreto. Si al
   final no queda ningún alias/tabla propio del negocio, el PA puede quedar reducido al descriptor +
   boilerplate genérico (ver v3 del apéndice).
4. Diseñar el `.docx` con las clases de etiquetas que hagan falta, respetando el patrón de 3 filas (4.f)
   en cualquier tabla repetida.
5. Imprimir sobre un elemento real y revisar el documento resultado buscando `{{{` sobrante (4.g) y
   comprobando formatos de fecha/decimales.
6. Repetir el punto 5 con un elemento "límite" (sin datos opcionales, con varias líneas, con importes a
   cero...) antes de darlo por bueno.

---

## Apéndice — Caso real: plantilla de factura de Un Abogado

Este caso tiene tres versiones sucesivas de la misma plantilla, y es el mejor ejemplo del punto 4.d (PA
vs maestro) porque el mismo dato (cliente, sociedad emisora) se resuelve de las dos formas en distintas
versiones: la v1 lo trae todo por PA (4.b, la única alternativa que había en ese momento), la v2 migra el
cliente a maestro (4.c) en cuanto ese mecanismo existió, y la v3 migra también la sociedad y elimina del
PA hasta la última línea de SQL específica de factura.

### v1 — todo por PA

Contexto: un despacho de abogacía (una base de datos de un cliente real) ya tenía dado de alta, desde la
aplicación, una plantilla de negocio llamada "Factura de Word" sobre el negocio de Facturas emitidas —
lo que había generado automáticamente el PA esqueleto `NEGOCIO.Plt_De_Negocio_Factura_de_Word` (punto
3.1) con las 3 fuentes/documento/2 tablas de ejemplo. El encargo fue: a partir de una factura real en
PDF (la del cliente Carmen María Mercader Conesa, nº 39/2026) y del listado de etiquetas descargado desde
la ficha de la plantilla (`Etiquetas_de_FacturaEmt...docx`), sustituir cada dato concreto de esa factura
por la etiqueta correspondiente, terminando el PA para que aporte lo que falte.

#### Lectura de la factura de ejemplo

La factura tenía: cabecera del despacho (nombre, "Abogado", CIF, dirección, email, teléfono), nº de
factura y fecha, datos del cliente (nombre, NIF, dirección), una tabla de un solo concepto con su base
imponible, el desglose de totales (base/IVA/total), la forma de pago y un IBAN, y un pie con la cláusula
de protección de datos y la firma.

#### Qué salió del listado de etiquetas sin tocar el PA (4.a)

El listado de etiquetas mostraba que `FacturaEmtDto` ya trae, entre otras, `NumeroFactura`,
`FacturadaEl`, `Cliente`, `TotalSinIva`, `TotalIva` y `APagar` — así que nº de factura, fecha, nombre del
cliente y los tres totales se resolvieron directamente con `{{{FacturaEmt.NumeroFactura}}}`,
`{{{FacturaEmt.FacturadaEl}}}`, `{{{FacturaEmt.Cliente}}}`, `{{{FacturaEmt.TotalSinIva}}}`,
`{{{FacturaEmt.TotalIva}}}`, `{{{FacturaEmt.APagar}}}`, sin escribir ni una línea de SQL para ellos.

#### Qué hubo que traer explícitamente en el PA (4.b)

Lo que **no** estaba en el Dto ni en sus ampliaciones se resolvió con dos alias nuevos, siguiendo el
mismo patrón de joins que ya usan `Plt_Emision_De_Factura.sql` y
`Plt_Por_Tipo_Anexo_de_factura_de_desarrollo.sql` (`TERCEROS.CLIENTE`/`SOCIEDAD` + `TERCEROS.*_DIRECCION`
+ `CALLEJERO.*`):

- `{{{cliente.nif}}}` y `{{{cliente.direccion}}}` — NIF (`TERCEROS.CLIENTE.VAT`) y dirección fiscal del
  cliente que factura.
- `{{{sociedad.nombre}}}`, `{{{sociedad.nif}}}`, `{{{sociedad.direccion}}}`, `{{{sociedad.telefono}}}`,
  `{{{sociedad.email}}}` — datos del despacho emisor (`TERCEROS.SOCIEDAD`, vía `TERCEROS.CENTRO_GESTOR`
  para llegar desde la factura hasta su sociedad).
- `{{{sociedad.iban}}}` — la cuenta de cobro activa del despacho, uniendo `TERCEROS.SOCIEDAD_CUENTA` con
  `CONTABILIDAD.CUENTA_BANCARIA` y recomponiendo el IBAN con espacios cada 4 caracteres en el propio
  SQL.
- `{{{sociedad.piedefactura}}}` — el texto de aviso RGPD + despedida + firma, tomado de
  `TERCEROS.SOCIEDAD_PARAMETRO.PIE_DE_FACTURA` (el campo que la propia aplicación ya ofrece en "Mi
  sociedad" con la ayuda "Indique qué mostrar en la impresión del pie de una factura") — así el pie
  entero es un único tag, editable desde la aplicación sin volver a tocar la plantilla.

#### La tabla de conceptos (4.e/4.f)

Un único alias de tabla `lineasdefactura`, alimentado por `VENTA.FACTURA_EMT_LINEA`, con el patrón de 3
filas: marcadora `{{{lineasdefactura}}}`, cabecera "CONCEPTO / DESCRIPCIÓN" / "BASE IMPONIBLE", y fila
plantilla `{{{col0}}}` / `{{{col1}}}` mapeados en `pltMapeosDeTabla` a `Concepto`/`BaseImponible`.

#### Descriptor final de la v1

```
pltDatosDePlantilla : '1:cliente,2:sociedad,4:documento'
pltFilasDeTabla     : '3:lineasdefactura'
pltMapeosDeTabla    : 'lineasdefactura:col0=Concepto,col1=BaseImponible'
```

(El alias `documento`, índice 4, se dejó definido por convención aunque el cuerpo del documento no lo
usa — el rótulo "FACTURA" se dejó como texto fijo del Word en vez de `{{{documento.titulo}}}`, ya que no
cambia de una factura a otra.)

#### Limitaciones conocidas de la v1, pendientes de validar contra la base de datos real

- El `CALIFICADOR = 'fiscal'` de las direcciones, y que la cuenta bancaria de cobro esté marcada
  `ACTIVA = 1` con `CLASE in ('Ingreso','Ambas')`, son supuestos razonados por analogía con
  `Plt_Emision_De_Factura.sql`, no verificados contra esa base de datos (no hay acceso a esa base de
  datos desde este entorno).
- El "IVA (21 %)" del rótulo de totales quedó fijo como texto, porque no existe una etiqueta de
  porcentaje de IVA a nivel de factura — si en el futuro se necesita dinámico, habría que añadirlo como
  alias nuevo en el PA.
- `{{{FacturaEmt.FacturadaEl}}}` se dejó como etiqueta automática del Dto por simplicidad, pero según
  4.a eso imprime la fecha con el formato de cultura del servidor (probablemente con hora). Si al probar
  la impresión aparece con hora, hay que sustituirla por un alias `factura.fecha` calculado en el PA con
  `FORMAT(FACTURADA_EL, 'dd-MM-yyyy')`, como hace el ejemplo real del sistema.

Ficheros de la v1: `SistemaDeElementos/Documentación/Plantillas/Alberto/Plantilla_Factura_de_Word.docx`
y `Plt_De_Negocio_Factura_de_Word.sql`.

### v2 — el cliente migra a maestro, la sociedad se queda en el PA

En cuanto existió el registro de maestros (4.c), con las claves `cliente`/`proveedor`/`solicitante`, se
hizo una segunda versión que sustituye únicamente la parte de cliente: `FacturaEmtDtm` implementa
`IUsaCliente`, así que su NIF y dirección fiscal dejan de necesitar una consulta propia en el PA.

- `{{{cliente.nif}}}` → `{{{maestro.cliente.VAT}}}`
- `{{{cliente.direccion}}}` → `{{{maestro.cliente.direccion.[fiscal].Expresion}}}`

El PA pierde por completo la consulta "cliente" (nombre/NIF/dirección) y su entrada en el descriptor; la
sociedad emisora se queda exactamente igual que en la v1, porque en ese momento el registro de maestros
todavía no tenía ninguna clave para el centro gestor/sociedad (eso llegó con la v3).

Descriptor de la v2 (`sociedad` pasa a índice 1, la tabla a 2, `documento` a 3 — un índice menos que la
v1 porque ya no hay consulta de cliente):

```
pltDatosDePlantilla : '1:sociedad,3:documento'
pltFilasDeTabla     : '2:lineasdefactura'
pltMapeosDeTabla    : 'lineasdefactura:col0=Concepto,col1=BaseImponible'
```

Ficheros de la v2: `SistemaDeElementos/Documentación/Plantillas/Alberto/Plantilla_Factura_de_Word_II.docx`
y `Plt_De_Negocio_Factura_de_Word_II.sql`.

### v3 — sociedad y centro gestor también migran a maestro; el PA se queda sin SQL de negocio

Con las claves `MiCg`/`MiSociedad` ya en el registro (resolviendo la sociedad titular del centro gestor
del elemento sin que el PA tenga que hacer el `JOIN FACTURA_EMT → CENTRO_GESTOR → SOCIEDAD`), la v3
sustituye también la sociedad emisora, y de paso las líneas de factura pasan a salir de
`{{{LineaDeUnaFae}}}` como tabla de "detalle" automática (4.e) en vez de venir del PA — esto último no es
nuevo del punto 4.c, ya existía (`LineaDeUnaFaeDtm` ya era un `IDetalle` de `FacturaEmt`), simplemente no
se había usado en las versiones anteriores.

- `{{{sociedad.nombre}}}` / `.nif` / `.direccion` / `.telefono` / `.email` → `{{{maestro.MiSociedad.Nombre}}}` /
  `.Nif` / `.direccion.[fiscal].Expresion` / `.Telefono` / `.eMail`
- `{{{sociedad.iban}}}` → `{{{maestro.MiSociedad.cuentabancaria.[Ingreso].Cuenta}}}`
- `{{{lineasdefactura}}}` + `{{{col0}}}`/`{{{col1}}}` → `{{{LineaDeUnaFae}}}` + `{{{Concepto}}}`/`{{{BaseImponible}}}`
- Se añade además `{{{maestro.MiCg.Nombre}}}`/`{{{maestro.MiCg.Codigo}}}` como referencia interna (no
  estaba en la factura original, es una demostración de ese maestro).

Lo único que **no** migró: `{{{sociedad.piedefactura}}}` (el aviso RGPD + despedida + firma). Ese texto
sale de `TERCEROS.SOCIEDAD_PARAMETRO.PIE_DE_FACTURA`, que es una **ampliación** de `SociedadDtm`
(`ParametrosDeMiSociedadDtm : Ampliacion<SociedadDtm>`), no una propiedad directa de `SociedadDto` — el
maestro `MiSociedad` de hoy solo expone `DatosPrincipales`/`Direcciones`/`CuentasBancarias` de la sociedad,
no sus ampliaciones. En la v3 ese pie quedó como texto fijo en el Word en vez de una etiqueta; si se
quisiera mantener dinámico habría que traerlo todavía por PA, o ampliar el maestro el día que haga falta
en más de un sitio.

Descriptor de la v3 — sin ninguna línea de SQL específica de factura, solo boilerplate genérico (el
alias `documento`, idéntico en cualquier PA del sistema, y una tabla sin usar que existe solo porque el
descriptor exige al menos un bloque bien formado en `pltFilasDeTabla`/`pltMapeosDeTabla`, ver
`ExtensorDePlantillas.MapearCampoDatos`/`MapearCampoFilas`, que no admiten cadena vacía):

```
pltDatosDePlantilla : '1:documento'
pltFilasDeTabla     : '2:tablasinuso'
pltMapeosDeTabla    : 'tablasinuso:col0=col0'
```

Ficheros de la v3: `SistemaDeElementos/Documentación/Plantillas/Alberto/Plantilla_Factura_de_Word_III.docx`
y `Plt_De_Negocio_Factura_de_Word_III.sql`.

### Un bug real que salió al probar la v3

Al validar la v3 apareció un caso que la v1/v2 no ejercitaban: dos etiquetas `{{{maestro....}}}` en el
mismo run de texto de Word (p.ej. `"{{{maestro.MiSociedad.eMail}}} – {{{maestro.MiSociedad.Telefono}}}"`
escrito del tirón en una sola línea). La primera versión de `ProcesarEtiquetasDeMaestros` solo resolvía la
primera etiqueta de cada bloque de texto y avanzaba al siguiente sin volver a mirarlo, dejando la segunda
sin sustituir. Se corrigió reescribiendo el método para que recorra, en una sola pasada, **todas** las
ocurrencias `{{{...}}}` de un mismo bloque de texto (agrupando primero los `<w:t>` partidos por el
corrector ortográfico de Word, igual que ya hacía el resto del motor). Queda como aviso para quien añada
un maestro nuevo: probar siempre con al menos dos etiquetas `{{{maestro...}}}` en la misma línea del
Word, no solo una por párrafo.
