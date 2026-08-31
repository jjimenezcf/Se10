# Cómo crear un CRUD de solo visualización de una tabla dependiente — plantilla genérica

Esta guía es una **plantilla parametrizada**, hermana de
[`ImplementacionDeUnNegocio.md`](ImplementacionDeUnNegocio.md) pero para un caso mucho más ligero: una
tabla que **cuelga de un elemento de una entidad de negocio ya existente** (una FK a `XxxDtm`, no un
negocio nuevo con tipo/estado/flujo propio) y cuya interfaz solo necesita **listar/consultar** filas —
sin alta, edición ni borrado manual, porque las filas las genera otro proceso del sistema (una
recepción de pedido, una venta, una regularización...). Se ha extraído generalizando el alta real de
**Movimientos de almacén** (`MovimientoDeAlmacenDtm`, cuelga de `AlmacenDtm`), así que cualquier fichero
de ese negocio sirve como ejemplo concreto ya funcionando de cualquier paso de aquí.

Si la tabla nueva necesita alta/edición manual, o tiene su propio flujo de estados, **no es este caso**:
usa `ImplementacionDeUnNegocio.md`. Esta plantilla es específicamente para el patrón "rejilla de solo
lectura, alimentada por otros procesos".

## 0. Datos de entrada

```
Nombre de la tabla   : <Nombre en castellano — p.ej. "Movimiento de almacén">
Área / módulo        : <Logística | Ventas | Gastos | ... — determina namespaces, carpetas y menú>
Entidad de la que depende : <XxxDtm ya existente — p.ej. AlmacenDtm — la FK obligatoria de la tabla>

Otras referencias (FK) que lleva la fila, con si son obligatorias u opcionales:
- <p.ej. IdUnitario -> UnitarioDtm (obligatoria)>
- <p.ej. IdPreasiento -> PreasientoDtm (opcional)>
- <p.ej. IdLineaAlbaran -> int simple, sin Dtm todavía (opcional, "documento origen/destino")>

¿Necesita un catálogo de parametrización propio (p.ej. "tipo de movimiento")? <sí/no>
  Si sí: nombre de la tabla de catálogo, campos propios, y la lista de valores por defecto a sembrar
  (nombre + valor de cada campo propio de cada fila).

Campos propios de la fila (aparte de las FK anteriores):
- <Nombre>: <tipo .NET>

Columnas del grid (en orden), y cuáles son calculadas en vez de mapeadas 1:1 desde el Dtm:
- <Columna>: <propiedad del Dtm | calculada en DespuesDeMapearElElemento, y con qué lógica>

Orden en el menú: <dentro de qué módulo, por delante/detrás de qué otra entrada>
Icono: <reutiliza uno existente, o "nuevo SVG con tal descripción">
```

### Leyenda de los placeholders

| Placeholder | Significado | Ejemplo (Movimientos de almacén) |
|---|---|---|
| `Xxx` | Nombre de la fila, PascalCase | `MovimientoDeAlmacen` |
| `Xxxs` | Plural en castellano usado en textos/menú | `Movimientos de almacén` |
| `<Área>` | Módulo/área (namespace, carpeta, menú) | `Logistica` |
| `Padre` | La entidad de negocio de la que depende (punto 0) | `Almacen` |

## 1. Analizar un ejemplo existente parecido como patrón

Antes de generar nada, localizar una tabla dependiente ya existente con forma parecida. Esta guía usa
**`LineaDeUnPedidoDtm`** (`ServicioDeDatos/Logistica/Pedidos`) como referencia de "registro plano con
auditoría, no elemento de proceso" (`RegistroDtm, IAuditoria`, sin `Tipo`/`Estado` propios), y
**`FacturaAeatDto`** (`ModeloDeDto/Ventas/FacturaEmt`) como referencia del atributo `[IUDto(SoloGrid =
true, ...)]` que convierte un Dto normal en uno de solo lectura. `MovimientoDeAlmacenDtm` combina ambos
patrones.

**Diferencia clave con un negocio completo**: la fila **no** hereda de `ElementoDtm`/
`ElementoDeProcesoDtm` (no tiene `Tipo`, `Estado`, `Cg`, flujo, ni las clases satélite de
Auditoría/Traza/Observación/Permiso/Historia que trae un elemento completo) — hereda directamente de
`RegistroDtm` más las interfaces que necesite (`IAuditoria` casi siempre, `IUsaReferencia` si aplica).
Esto simplifica el modelo de datos y el gestor drásticamente frente al paso 2 de
`ImplementacionDeUnNegocio.md`.

## 2. Modelo de datos (capa `ServicioDeDatos`)

Fichero: `ServicioDeDatos/<Área>/<Padres>/XxxDtm.cs` (en la misma carpeta del `Padre`, no en una nueva).

```csharp
[Table(Tablas.XXX, Schema = Esquemas.<Área>)]
public class XxxDtm : RegistroDtm, IAuditoria /*, IUsaReferencia si aplica */
{
    public int IdPadre { get; set; }
    public PadreDtm Padre { get; set; }

    // una propiedad (Id + navegación) por cada FK obligatoria u opcional del punto 0;
    // las FK "sin Dtm todavía" (documentos futuros) se dejan como int? simple, sin navegación

    // campos propios del punto 0

    public int IdUsuaCrea { get; set; }
    public int? IdUsuaModi { get; set; }
    public DateTime FechaCreacion { get; set; }
    public UsuarioDtm UsuarioCreador { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public UsuarioDtm UsuarioModificador { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

Si el punto 0 pide un catálogo de parametrización propio (p.ej. "tipo de movimiento"), se define en el
mismo fichero, **antes** de `XxxDtm`:

```csharp
[Table(Tablas.XXX_TIPO, Schema = Esquemas.<Área>)]
public class TipoDeXxxDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
{
    // campos propios del catálogo (p.ej. un enum de clase)
}
```

`IRegistroDeParametrizacion` es solo un marcador (sin miembros); no aporta comportamiento por sí
mismo, pero documenta la intención y es el mismo patrón que `IvaRepercutidoDtm`.

Al final del fichero, una clase estática `partial class ModeloDe<Padres>` (la misma que ya define el
modelo del `Padre` — se amplía, no se crea una nueva) con un método por pieza:

```csharp
public static partial class ModeloDe<Padres>
{
    public static void TipoDeXxx(ModelBuilder modelBuilder)   // solo si hay catálogo
    {
        ApiDeRegistroDtm.DefinirCampoIdDtm<TipoDeXxxDtm>(modelBuilder);
        ApiDeNombreDtm.DefinirCampoNombreDtm<TipoDeXxxDtm>(modelBuilder, unico: true);
        // .Property(...) de cada campo propio del catálogo
    }

    public static void Xxx(ModelBuilder modelBuilder)
    {
        ApiDeRegistroDtm.DefinirCampoFk<XxxDtm>(modelBuilder, nameof(XxxDtm.Padre), nameof(XxxDtm.IdPadre), ICampos.ID_PADRE, requerida: true, unico: false);
        // una DefinirCampoFk por cada otra FK con navegación (requerida: false si es opcional)
        // .Property(...) de cada campo propio, con HasColumnName/HasColumnType/IsRequired
        // .Property(...) sin FK de cada "documento futuro" (int? simple)

        ApiDeElementoDtm.DefinirCamposDeAuditoria<XxxDtm>(modelBuilder);
    }
}
```

**Regla de oro**: `RegistroDtm` (a diferencia de `ElementoDtm`) no tiene los métodos de conveniencia que
resuelven solos las capacidades (`DefinirCampoReferencia`, `DefinirPreasiento`... exigen `where T :
ElementoDtm`) — cada FK y cada campo se mapea aquí a mano, uno a uno, con `ApiDeRegistroDtm.DefinirCampoFk`
para las FK y `.Property(...)` directo para el resto. `ApiDeElementoDtm.DefinirCamposDeAuditoria<T>` sí
acepta cualquier `RegistroDtm` que implemente `IAuditoria` (lo usa igual `LineaDeUnPedidoDtm`).

**Antes de escribir un nombre de propiedad/columna nuevo**, comprobar si ya existe una constante
reutilizable en `ICampos` (p.ej. `ID_UNITARIO`, `CANTIDAD`, `PRECIO`, `VALOR`, `REFERENCIA`,
`ID_PREASIENTO` ya existen). Solo añadir una constante nueva a `ServicioDeDatos/_Elemento/Metadatos.cs`
(clase `ICampos`) si de verdad no hay ninguna equivalente, y una constante de tabla nueva a la misma
clase `Tablas` (`XXX`, y `XXX_TIPO` si hay catálogo) con el nombre exacto de tabla del punto 0.

## 3. Enganchar el modelo en el `DbContext`

Fichero: `ServicioDeDatos/CreadorDelMd.cs`. Dentro del método `DefinirTablasDe<Padres>` que ya existe
para el `Padre` (no se crea uno nuevo), añadir las llamadas nuevas en este orden:

```csharp
private void DefinirTablasDe<Padres>(ModelBuilder modelBuilder)
{
    ...                                        // lo que ya hubiera del Padre
    ModeloDe<Padres>.TipoDeXxx(modelBuilder);   // solo si hay catálogo — antes de Padre
    ModeloDe<Padres>.Padre(modelBuilder);       // ya existente
    ModeloDe<Padres>.Xxx(modelBuilder);         // la tabla nueva, después del Padre
}
```

## 4. Modelo de Dto (capa `ModeloDeDto`)

Fichero: `ModeloDeDto/<Área>/<Padres>/XxxDto.cs`.

```csharp
[IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(XxxDto.Columna1),
       SoloGrid = true, OpcionDeEnviar = false, OpcionDeTransitar = false)]
public class XxxDto : ElmentoAuditadoDto
{
    // una propiedad [IUPropiedad(..., VisibleEnGrid = true, Obligatorio = false)] por cada
    // columna del grid del punto 0 que venga directa de una FK/campo del Dtm
}
```

Puntos clave de este atributo, tomados de `FacturaAeatDto` (el ejemplo de referencia de un Dto de solo
lectura):

- **`SoloGrid = true`** es lo que anula automáticamente las opciones de crear/editar/borrar
  (`OpcionDeCrear`/`OpcionDeEditar`/`OpcionDeBorrar` devuelven `false` sin poder activarse aunque se
  fuerce el setter) — el `DescriptorDeCrud` correspondiente detecta esto solo y no genera los paneles de
  creación/edición/borrado.
- `ElmentoAuditadoDto` (en vez de `ElementoDto` a secas) trae ya `CreadoEl`, `ModificadoEl`, `Creador`,
  `Modificador` — los "campos de auditoría" del punto 0 vienen gratis con solo heredar de esta clase, no
  hace falta declararlos.
- Las columnas **calculadas** del punto 0 (que no vienen 1:1 de una propiedad del Dtm) se declaran aquí
  como propiedades normales (`public string Origen { get; set; }`) pero **se rellenan en el gestor**
  (paso 5), no aquí — el Dto solo define la forma, no el cálculo.

## 5. Gestor de negocio

Fichero: `GestoresDeNegocio/<Área>/<Padres>/GestorDeXxxs.cs`.

```csharp
public class GestorDeXxxs : GestorDeElementos<ContextoSe, XxxDtm, XxxDto>
{
    public class MapearXxxs : Profile
    {
        public MapearXxxs()
        {
            CreateMap<XxxDtm, XxxDto>()
            .ForMember(dto => dto.Columna1, x => x.MapFrom(dtm => dtm.Fk1.Nombre)); // o .Expresion

            CreateMap<XxxDto, XxxDtm>()
            .ForMember(dtm => dtm.Padre, dto => dto.Ignore())
            .ForMember(dtm => dtm.Fk1, dto => dto.Ignore());
        }
    }

    public override enumNegocio Negocio => enumNegocio.No_Definido;

    public GestorDeXxxs(ContextoSe contexto, IMapper mapeador) : base(contexto, mapeador) { }
    public static GestorDeXxxs Gestor(ContextoSe contexto, IMapper mapeador) => new GestorDeXxxs(contexto, mapeador);

    protected override IQueryable<XxxDtm> AplicarJoins(IQueryable<XxxDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
    {
        consulta = base.AplicarJoins(consulta, filtros, parametros);
        consulta = consulta.Include(x => x.Padre);
        // .Include(x => x.Fk1) por cada FK con navegación que alimente una columna del grid
        return consulta;
    }

    protected override void DespuesDeMapearElElemento(XxxDtm registro, XxxDto elemento, ParametrosDeNegocio parametros)
    {
        base.DespuesDeMapearElElemento(registro, elemento, parametros);
        // aquí se rellenan las columnas calculadas del punto 0 (Origen/Destino, totales derivados...)
    }
}
```

Puntos clave, tomados de `GestorDeLineasDeUnPedido` (el ejemplo de referencia de un gestor sobre
`RegistroDtm`, no `ElementoDtm`):

- **`Negocio => enumNegocio.No_Definido`** — al no ser un elemento de un negocio propio (no tiene fila
  en `NegocioDtm`, ni menú de configuración, ni tipos/estados), no se asocia a ningún `enumNegocio`
  concreto. Esto es lo que distingue a este patrón de un negocio completo.
- No hay `GestorDeTipos`, ni overrides de `AntesDeTransitar`/`DespuesDeTransitar` (no hay flujo), ni
  `AplicarSeguridad` con filtrado por tipo/Cg — si el punto 0 no pide seguridad particular, se deja la
  de la base.
- El resto de overrides (`AplicarFiltros`, `AntesDePersistir`, `DespuesDePersistir`) se dejan con `//
  TODO` o llamando solo a la base, igual que en un negocio completo — la lógica concreta se completa
  cuando se conozca el detalle funcional de quién genera las filas.

Registrar el gestor en el contenedor de DI — `GestoresDeNegocio/ServiceExtensions.cs`, dentro del
`Configure<Área>` que ya registra el gestor del `Padre`:

```csharp
services.AddScoped<GestorDeXxxs>();
```

## 6. Controlador MVC, Descriptor de crud, vista y TypeScript

Cuatro piezas. La diferencia frente a un CRUD completo (`ImplementacionDeUnNegocio.md`, paso 9) es que,
al ser `SoloGrid`, no hay paneles de creación/edición/borrado que enganchar y el `modo` es
`ModoDescriptor.Consulta` en vez de `ModoDescriptor.Mantenimiento` — el resto de la fontanería
(controlador + descriptor) es exactamente la misma que usa cualquier CRUD del sistema, con **caché de
render por usuario**: la primera vez que un usuario abre el grid se construye el descriptor entero
(controles, filtros, columnas) y se guarda el HTML ya renderizado; en las visitas siguientes, mientras
no cambie nada, se reutiliza ese HTML tal cual, solo remendando los bloques que sí cambian siempre
("últimos menús accedidos", "últimos registros").

1. **Controlador** — `SistemaDeElementos/Controllers/<Área>/XxxsController.cs`, hereda de
   `EntidadController<ContextoSe, XxxDtm, XxxDto>`, recibe por constructor `GestorDeXxxs` y
   `GestorDeErrores`, y expone una única acción:
   ```csharp
   public IActionResult CrudXxxs()
   {
       ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

       var modo = ModoDescriptor.Consulta;
       var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeXxxs).FullName}";
       var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
       try
       {
           if (cache.ContainsKey(indice))
           {
               ViewBag.DatosDeConexion = DatosDeConexion;
               var destino = $"../{enumNameSpaceTs.<Área>}/{nameof(CrudXxxs)}";
               return base.View(destino, new DescriptorDeXxxs(Contexto, (string)cache[indice]));
           }
           else
           {
               var descriptor = DescriptorDeCrud<XxxDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeXxxs(Contexto, modo));
               return ViewCrud(descriptor);
           }
       }
       catch (Exception e)
       {
           return RenderizarErrorDe(indice, e);
       }
   }
   ```
   `indice` identifica de forma única "este usuario, este modo, este descriptor". Si ya hay HTML cacheado
   bajo esa clave, se reconstruye el descriptor con el constructor `(Contexto, renderCacheString)` (punto
   2) en vez de con `(Contexto, modo)`, y se devuelve la vista directamente con `base.View(...)` (sin pasar
   por `ViewCrud`, que es el que dispararía la reconstrucción completa). Si no hay nada cacheado,
   `DescriptorDeCrud<XxxDto>.CrearDescriptor(...)` construye el descriptor normal (con reintento automático
   si salta una excepción de "ya está asignado" por una carrera de ids) y `ViewCrud(descriptor)` es quien,
   al llamar a `RenderControl()`, guarda el HTML resultante en el caché para la próxima vez. Si algo falla,
   `RenderizarErrorDe(indice, e)` limpia esa entrada de caché (para no dejar cacheado un render roto) y
   devuelve el mensaje de error, en vez del `try/catch` con `RenderMensaje` a secas de un descriptor sin
   caché.

   ⚠️ **`typeof(DescriptorDeXxxs)` tiene que ser el tipo del descriptor de *esta* pantalla.** Es fácil de
   copiar mal al partir de otro controlador como plantilla (p.ej. dejar `typeof(DescriptorFacturasAeat)`
   al adaptar `FacturasEmtController.CrudFacturasAeat`): si dos pantallas distintas usan el mismo `modo` y
   el mismo tipo hardcodeado en `indice`, comparten la misma entrada de caché para el mismo usuario — la
   segunda pantalla que se abra puede acabar mostrando el HTML cacheado de la primera. La escritura interna
   del caché (dentro de `RenderControl()`, punto 2) sí usa `GetType().FullName` — dinámico y por tanto
   siempre correcto —, así que el único sitio donde se puede colar este error es aquí, en el controlador.
2. **Descriptor de crud** — `SistemaDeElementos/Descriptores/DescriptoresDe<Área>/DescriptorDeXxxs.cs`,
   hereda de `DescriptorDeCrud<XxxDto>` y necesita **dos constructores**:
   ```csharp
   public DescriptorDeXxxs(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
   {
   }

   public DescriptorDeXxxs(ContextoSe contexto, ModoDescriptor modo)
   : base(contexto, nameof(XxxsController), nameof(XxxsController.CrudXxxs), modo, rutaBase: enumNameSpaceTs.<Área>)
   {
       // filtros del Mnt.BloqueGeneral, si el punto 0 pide alguno (ver DescriptorFacturasAeat como referencia)
   }

   public override string RenderControl()
   {
       if (!_renderCache.IsNullOrEmpty())
           return _renderCache;

       var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
       if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
           return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];

       var render = base.RenderControl();
       render = render +
              $@"<script src=¨../../js/{RutaBase}/Xxxs.js?v={System.DateTime.Now.Ticks}¨></script>
                 <script>
                    try {{
                      {RutaBase}.CrearCrudDeXxxs('{Mnt.IdHtml}')
                    }}
                    catch(error) {{
                       MensajesSe.Error('Creando el crud', error.message);
                    }}
                 </script>
               ";
       ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
       return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
   }
   ```
   El constructor `(contexto, renderCache)` es el que usa el controlador cuando ya hay HTML cacheado: llama
   a la base, que remienda en el propio `renderCache` los bloques de "últimos menús"/"últimos registros"
   (que sí cambian por request aunque el resto del grid no) y lo deja en `_renderCache`; el `RenderControl()`
   de arriba, al ver `_renderCache` relleno, lo devuelve tal cual sin reconstruir nada más. La llamada de
   creación del CRUD en TypeScript **solo lleva el id del panel de mantenimiento** (no hay ids de
   creación/edición/borrado, porque no existen, al ser `SoloGrid`):
   ```csharp
   {RutaBase}.CrearCrudDeXxxs('{Mnt.IdHtml}')
   ```
   (compárese con un CRUD completo, que pasa cuatro ids: `CrearCrudDeXxxs(idMnt, idCreacion, idEdicion,
   idModalBorrar)`).
3. **Vista** — `SistemaDeElementos/Views/<Área>/CrudXxxs.cshtml`:
   ```cshtml
   @model MVCSistemaDeElementos.Descriptores.DescriptorDeXxxs

   @{
       ViewData["Title"] = $"{Model.Etiqueta}";
   }

   @Html.Raw(Model.RenderControl())
   ```
   **Este fichero es fácil de olvidar porque nada falla en tiempo de compilación si falta** — el error
   solo aparece en tiempo de ejecución, al navegar a la vista: *"La vista .../CrudXxxs.cshtml no está
   definida en el directorio de Views"*. El nombre del fichero `.cshtml` tiene que coincidir exactamente
   con el nombre de la acción del controlador (paso 1), porque `ViewCrud`/`View` resuelven la vista por
   convención de nombre.
4. **TypeScript** — `SistemaDeElementos/wwwroot/ts/<Área>/Xxxs.ts`, dentro del `namespace` del área. Al
   ser `SoloGrid`, la clase de creación/edición se dejan a `undefined` en vez de instanciarse (patrón
   calcado de `FacturasAeat.ts`):
   ```typescript
   namespace <Área> {
       export function CrearCrudDeXxxs(idPanelMnt: string) {
           Crud.crudMnt = new <Área>.CrudDeXxxs(idPanelMnt);
           window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);
           window.onbeforeunload = function () { Crud.crudMnt.AntesDeSalir(); };
       }

       export class CrudDeXxxs extends Crud.CrudMnt {
           constructor(idPanelMnt: string) {
               super(idPanelMnt, undefined);
               this.crudDeCreacion = undefined;
               this.crudDeEdicion = undefined;
           }
       }
   }
   ```

## 7. Constantes de vista y controlador

Fichero: `Ayudas/Extensiones/Controladores.cs`.

- Añadir `Xxxs` al enumerado `enumControladores<Área>` que ya existe (junto al del `Padre`).
- Añadir una constante `CrudXxxs = nameof(CrudXxxs)` a la clase estática `enumVistas<Área>` que ya
  existe.

## 8. Vista, menú e icono

Tres altas en `Inicializador/Negocios/`, mismo patrón que un negocio completo (paso 11 de
`ImplementacionDeUnNegocio.md`) pero sin submenú de configuración (no hay Estados/Transiciones/Tipos que
configurar):

1. **Vista** — `Inicializador/Negocios/InzVistas.cs`: una constante `readonly string` con el nombre
   visible (p.ej. `"Movimientos de almacén"`, no necesariamente `enumNegocio.Xxx.Plural()` porque no hay
   `enumNegocio` propio), y en `CrearVistasDelModuloDe<Área>`:
   ```csharp
   gestor.CrearVistaSiNoExiste(enumVistas.<Área>.Xxxs, enumControladores<Área>.Xxxs, enumVistas<Área>.CrudXxxs, false, typeof(XxxDto).FullName);
   ```
2. **Menú** — `Inicializador/Negocios/InzMenus.cs`, dentro del método del módulo, una llamada directa a
   `GestorDeMenus.CrearMenuSiNoExiste(...)` (no `MenusDeConfiguracionDeProceso`, que es solo para
   negocios con flujo) apuntando a la vista del punto anterior:
   ```csharp
   GestorDeMenus.CrearMenuSiNoExiste(gestor
       , nombre: enumVistas.<Área>.Xxxs
       , descripcion: "Consulta de Xxxs"
       , icono: "Xxxs.svg", padre: Modulo<Área>, vista: enumVistas.<Área>.Xxxs, orden: <n>);
   ```
   El **orden relativo** dentro del módulo lo dice el punto 0.
3. **Icono** — si se reutiliza uno existente (p.ej. `enumNegocio.Padre.Icono()`), no hace falta nada
   más. Si el punto 0 pide uno propio, crear
   `SistemaDeElementos/wwwroot/images/menu/Xxxs.svg` con ese nombre exacto y usarlo como cadena literal
   en el `icono:` de arriba (no hay `enumNegocio.Xxxs.Icono()` porque no hay `enumNegocio` propio). Un
   SVG simple con `viewBox="0 0 512.000000 512.000000"`, un `<g>` con `fill="none" stroke="#000000"
   stroke-width="26"` y unas pocas `<path>`/`<line>` es suficiente si no hay un icono de diseño ya hecho.

Nada de este paso llega a la base de datos por sí solo: se inserta la próxima vez que se ejecute la
inicialización general del sistema.

## 9. Semilla de un catálogo de parametrización propio (si el punto 0 lo pide)

Si `TipoDeXxxDtm` (paso 2) existe, sus valores por defecto se siembran desde
`Inicializador/Procesos/<Área>/Inz<Padres>.cs` (el inicializador del `Padre` que ya exista — no uno
nuevo), **no** con un método estático dentro del propio Dtm: el proyecto `ServicioDeDatos` no referencia
`GestorDeElementos`, así que no puede llamar a `.PersistirPorAk(...)` (la extensión de "insertar si no
existe por clave alterna") desde dentro de una clase `Dtm`. El sitio correcto es el método
`ModeloDe<Padres>(contexto)` que ya envuelve en una transacción la inicialización del `Padre` — se le
añade una llamada más, de modo que sembrar el catálogo siga disparándose desde la misma acción
"Inicializar maestros" del `Padre` que ya existe:

```csharp
public static void ModeloDe<Padres>(ContextoSe contexto)
{
    var tran = contexto.IniciarTransaccion();
    try
    {
        ...                    // lo que ya hubiera del Padre
        TiposDeXxx(contexto);  // la llamada nueva
        contexto.Commit(tran);
    }
    catch (Exception ex) { contexto.Rollback(tran, ex); throw; }
}

private static void TiposDeXxx(ContextoSe contexto)
{
    contexto.IniciarTraza("Tipos de Xxx");
    try
    {
        PersistirTipoDeXxx(contexto, "Nombre del valor 1", ...);
        PersistirTipoDeXxx(contexto, "Nombre del valor 2", ...);
        // uno por cada valor por defecto del punto 0
    }
    finally { contexto.CerrarTraza(); }
}

private static void PersistirTipoDeXxx(ContextoSe contexto, string nombre, ...)
{
    new TipoDeXxxDtm { Nombre = nombre, ... }.PersistirPorAk(contexto, nameof(TipoDeXxxDtm.Nombre));
}
```

`PersistirPorAk` (namespace `GestorDeElementos`, fichero `Servicios/GestorDeElementos/ApiParaPersistir.cs`)
inserta si no existe una fila con ese valor en la propiedad indicada, o actualiza los demás campos si ya
existía — idempotente, se puede ejecutar tantas veces como se quiera desde "Inicializar maestros".

## 10. Qué queda pendiente siempre (fuera del alcance de esta plantilla)

- **Migración de EF Core**: con el modelo ya enganchado al `DbContext` (paso 3), generar la migración
  (`dotnet ef migrations add ...`) y aplicarla — sin esto no existen ni la tabla nueva ni su catálogo,
  así que el alta de vista/menú del paso 8 no se puede probar de verdad hasta que se aplique. A
  diferencia de un negocio completo, aquí normalmente **no** hace falta tocar la migración a mano (no
  hay tabla `_OBSERVACION` con columna computada que dependa de una función `CC_*` — eso solo aparece en
  negocios con `ElementoDtm` completo).
- **Lógica de negocio real**: los `// TODO` del gestor (paso 5) — filtros propios, seguridad,
  validaciones de `AntesDePersistir` si en algún momento se persiste desde código en vez de solo
  leerse — y el cálculo real de las columnas derivadas de `DespuesDeMapearElElemento`, que solo se puede
  rellenar del todo cuando exista el Dtm de los documentos de origen/destino que hoy son solo un `int?`
  suelto.

## 11. Checklist de ficheros (referencia rápida)

| # | Fichero | Qué se añade |
|---|---|---|
| 2 | `ServicioDeDatos/<Área>/<Padres>/XxxDtm.cs` | `TipoDeXxxDtm` (si aplica), `XxxDtm`, `ModeloDe<Padres>.TipoDeXxx/Xxx` |
| 2 | `ServicioDeDatos/_Elemento/Metadatos.cs` | `Tablas.XXX` (y `XXX_TIPO` si aplica), `ICampos.*` nuevos |
| 3 | `ServicioDeDatos/CreadorDelMd.cs` | Llamadas nuevas dentro de `DefinirTablasDe<Padres>` |
| 4 | `ModeloDeDto/<Área>/<Padres>/XxxDto.cs` | Dto `SoloGrid`, hereda `ElmentoAuditadoDto` |
| 5 | `GestoresDeNegocio/<Área>/<Padres>/GestorDeXxxs.cs` | Gestor (`Negocio => No_Definido`) |
| 5 | `GestoresDeNegocio/ServiceExtensions.cs` | `services.AddScoped<GestorDeXxxs>();` |
| 6 | `SistemaDeElementos/Controllers/<Área>/XxxsController.cs` | Controlador con una acción `CrudXxxs()` |
| 6 | `SistemaDeElementos/Descriptores/DescriptoresDe<Área>/DescriptorDeXxxs.cs` | Descriptor de crud |
| 6 | `SistemaDeElementos/Views/<Área>/CrudXxxs.cshtml` | Vista — **imprescindible, no falla en compilación si falta** |
| 6 | `SistemaDeElementos/wwwroot/ts/<Área>/Xxxs.ts` | TypeScript, sin creación/edición |
| 7 | `Ayudas/Extensiones/Controladores.cs` | Valor en `enumControladores<Área>` + constante en `enumVistas<Área>` |
| 8 | `Inicializador/Negocios/InzVistas.cs` | Constante + alta de vista |
| 8 | `Inicializador/Negocios/InzMenus.cs` | Alta de menú |
| 8 | `SistemaDeElementos/wwwroot/images/menu/Xxxs.svg` | Icono (solo si no se reutiliza uno existente) |
| 9 | `Inicializador/Procesos/<Área>/Inz<Padres>.cs` | Semilla del catálogo (solo si hay `TipoDeXxxDtm`) |
| 10 | `Migraciones/Migrations/<timestamp>_<nombre>.cs` | Migración de EF Core |

## Ejemplo de referencia: Movimientos de almacén

**Movimientos de almacén** (`MovimientoDeAlmacenDtm`, cuelga de `AlmacenDtm`, área Logística) está dado
de alta siguiendo esta plantilla al completo salvo el paso 10. Sirve como ejemplo real de cada paso:

- Entidad de la que depende: `AlmacenDtm` (`IdAlmacen`).
- Otras FK: `IdUnitario -> UnitarioDtm` (obligatoria), `IdTipoMovimiento -> TipoMovimientoDtm`
  (obligatoria), `IdPreasiento -> PreasientoDtm` (opcional); `IdMovimiento`/`IdLineaAlbaran`/
  `IdLineaDevolucion`/`IdLineaInventario` como `int?` sueltos, sin Dtm todavía (documentos de
  origen/destino futuros).
- Catálogo propio: sí — `TipoMovimientoDtm` (`RegistroConNombreDtm, IRegistroDeParametrizacion`), con
  el campo `ClaseMovimiento` (`enumClaseDeMovimiento`: `Entrada`, `Salida`, `Ajuste`, `Incremento`,
  `Decremento`, `Nulo`), sembrado desde `InzAlmacenes.TiposDeMovimiento` (11 valores por defecto: Compra
  de materiales, Venta de materiales, Consumo, Devolución de compra/consumo/venta, Incremento/Decremento
  de stock, Ajuste de precio, Reserva/Cancelación de reserva de stock), invocada desde
  `InzAlmacenes.ModeloDeAlmacenes` — la misma acción "Inicializar maestros de almacenes" que ya existía.
- Campos propios: `Cantidad`, `Stock`, `Precio`, `Valor`, `RealizadoEl`.
- Columnas del grid: Tipo de movimiento, Unitario, Cantidad, Precio, Stock, Valor, Origen y Destino
  (calculadas en `GestorDeMovimientosDeAlmacen.DespuesDeMapearElElemento` según la `ClaseMovimiento` del
  tipo — para una `Entrada` el origen es el documento y el destino el almacén, para una `Salida` al
  revés), Realizado el, y los campos de auditoría heredados de `ElmentoAuditadoDto`.
- Orden en el menú: dentro de "Logística", justo detrás de "Almacenes" y "Pedidos".
- Icono propio: `MovimientosDeAlmacen.svg` — una casa en contorno (sin relleno) atravesada por tres
  líneas horizontales que simulan el stock, en vez de reutilizar el icono sólido de `Almacen.svg`.

## Apéndice: filtros personalizados en el bloque general del descriptor

Todo lo dicho hasta aquí construye el grid con sus columnas, pero un grid de solo lectura casi siempre
necesita además una zona de filtros por encima (por almacén, por tipo, por unitario, por fecha...). Los
filtros se añaden en el **constructor `(ContextoSe contexto, ModoDescriptor modo)`** del descriptor
(paso 6.2), **después** de la llamada al constructor de la base — nunca en `RenderControl()`, que es
solo quien pinta lo que ya está montado.

El punto de enganche siempre es el mismo: `Mnt.BloqueGeneral` — una `BloqueDeFitro<TElemento>` que la
base ya crea (junto con la zona de filtros completa) antes de que el constructor del descriptor
concreto llegue a ejecutarse. Cada filtro nuevo es un `new AlgúnTipoDeControlFiltro<XxxDto>(Mnt.BloqueGeneral, ...)` — el propio constructor del control se encarga de registrarse solo dentro del bloque
(no hace falta ni se puede hacer `Mnt.BloqueGeneral.Add(...)` a mano).

### Quitar el filtro de "Nombre" que añade la base por defecto

La base (`ZonaDeFiltro<TElemento>`) añade **siempre**, sin condición, un cuadro de texto libre
"Nombre" a `Mnt.BloqueGeneral` — pensado para los Dtos que sí tienen un `Nombre` por el que buscar
(la mayoría de negocios). Un Dto de solo visualización como `MovimientoDeAlmacenDto` no tiene ese campo,
así que ese filtro no tiene contra qué buscar y sobra; se quita con una línea, **antes** de añadir los
filtros propios:

```csharp
Mnt.BloqueGeneral.QuitarControl(nameof(INombre.Nombre));
```

`QuitarControl` busca el control ya añadido por su `propiedad` (aquí `"Nombre"`) y lo retira de la tabla
de controles del bloque; si el Dto sí tuviera un filtrado por nombre útil, simplemente no se llama y se
deja el de la base.

### 1. Lista de valores obtenida de una tabla — `ListaDeValores<TElemento>`

Para un desplegable simple "elige uno de esta tabla", cuando las opciones se conocen leyendo filas de
una tabla en el propio constructor (no hace falta que sea una `IUPropiedad` con `ListaDeElemento` — es
un control solo de filtrado, no de edición). Dos variantes, según de dónde salgan las opciones:

**a) Consultando el gestor genérico (`contexto.SeleccionarTodos<T>`)** — cuando la entidad tiene su
propio gestor y conviene respetar sus reglas (por ejemplo, no traer elementos cancelados/terminados si
no interesa):

```csharp
var almacenes = contexto.SeleccionarTodos<AlmacenDtm>(
    filtros: new Dictionary<string, object> { },
    parametros: new Dictionary<string, object> { { ltrParametrosNeg.ExcluirTerminados, false } });

var opciones = new Dictionary<string, string>();
foreach (AlmacenDtm almacen in almacenes)
    opciones.Add(almacen.Id.ToString(), almacen.Expresion);

new ListaDeValores<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
    etiqueta: "Almacén",
    ayuda: "Seleccione el almacén",
    opciones: opciones,
    filtraPor: nameof(ltrDeUnMovimientoDeAlmacen.FiltroPorAlmacen),
    posicion: new Posicion() { fila = 0, columna = 0 });
```

**b) Consultando el `DbSet` directamente (`contexto.Set<T>()`)** — más directo para un catálogo simple
de parametrización (sin flujo ni permisos que filtrar), como el propio `TipoMovimientoDtm`:

```csharp
var tipos = contexto.Set<TipoMovimientoDtm>();

var tiposDeMovimiento = new Dictionary<string, string>();
foreach (TipoMovimientoDtm tipo in tipos)
    tiposDeMovimiento.Add(tipo.Id.ToString(), tipo.Nombre);

new ListaDeValores<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
    etiqueta: "Tipo",
    ayuda: "Seleccione el tipo de movimiento",
    opciones: tiposDeMovimiento,
    filtraPor: nameof(ltrDeUnMovimientoDeAlmacen.FiltroPorTipoMovimiento),
    posicion: new Posicion() { fila = 0, columna = 1 });
```

En ambos casos, `opciones` es un `Dictionary<idComoTexto, etiquetaVisible>` construido a mano, y
`filtraPor` es el nombre de clave que llegará al gestor dentro de la `ClausulaDeFiltrado` cuando el
usuario elija una opción.

### 2. Selector de elemento / lista dinámica — `ListasDinamicas<TElemento>`

Para cuando las opciones **no** se pueden precargar en un desplegable (hay demasiadas, o se busca por
texto con autocompletado) — es el mismo control que usa un `IUPropiedad` con
`TipoDeControl = enumTipoControl.ListaDinamica`, pero puesto directamente como filtro:

```csharp
new ListasDinamicas<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
    etiqueta: enumNegocio.Unitario.Singular(),
    filtrarPor: nameof(UnitarioDto.Expresion),
    ayuda: "seleccione el unitario",
    seleccionarDe: nameof(UnitarioDto),
    buscarPor: ltrDeUnMovimientoDeAlmacen.FiltroPorUnitario,
    mostrarExpresion: nameof(UnitarioDto.Expresion),
    criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
    posicion: new Posicion(1, 0),
    controlador: nameof(UnitariosController),
    navegarA: nameof(UnitariosController.CrudUnitarios),
    restringirPor: "",
    alSeleccionarBlanquearControl: "");
```

Los parámetros que hay que fijar con cuidado:

- `seleccionarDe` / `controlador` / `navegarA`: de qué Dto viene la lista, y a qué controlador/acción
  navegar si el usuario pincha en "ver" — el mismo trío que usaría un `IUPropiedad` de tipo
  `ListaDinamica` sobre esa misma entidad (`UnitarioDto` / `UnitariosController` /
  `UnitariosController.CrudUnitarios`).
- `buscarPor` / `mostrarExpresion`: por qué propiedad del Dto referenciado se busca al escribir
  (`Expresion`, casi siempre) y qué se muestra en el resultado.
- `filtrarPor`: la clave del filtro que le llega al gestor — no tiene por qué coincidir con `buscarPor`
  (aquí ambos son `Expresion`/`FiltroPorUnitario` respectivamente en el ejemplo, pero son conceptos
  distintos: uno es "cómo busca el combo", el otro es "con qué nombre se manda el filtro elegido").
- `restringirPor` / `alSeleccionarBlanquearControl`: se dejan en `""` cuando el filtro es independiente;
  se usan cuando un filtro depende de otro ya seleccionado (p.ej. "unitarios de este almacén") o cuando
  seleccionar este control debe vaciar otro dependiente.

### 3. Rango de fechas — `FiltroEntreFechas<TElemento>`

Para un intervalo "desde/hasta" sobre una propiedad de fecha:

```csharp
new FiltroEntreFechas<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
    etiqueta: "Realizado",
    ayuda: "Seleccione el rango de fechas de movimiento",
    propiedad: ltrDeUnMovimientoDeAlmacen.FiltroPorRealizadoEl,
    posicion: new Posicion() { fila = 1, columna = 1 });
```

Es el control más simple de los tres: solo etiqueta, ayuda, la clave de filtro (`propiedad`) y la
posición — sin fuente de datos que preparar, porque no es un desplegable.

### Los nombres de los filtros: una clase `ltrDeUnXxx` junto al Dtm

Las cadenas usadas en `filtraPor`/`buscarPor`/`propiedad` (`FiltroPorAlmacen`, `FiltroPorTipoMovimiento`,
`FiltroPorUnitario`, `FiltroPorRealizadoEl` en el ejemplo) conviene centralizarlas como constantes en vez
de repetir el literal, para poder referenciarlas también desde `AplicarFiltros` en el gestor sin
duplicar el string. El sitio natural es una clase estática pequeña junto al propio `XxxDtm.cs`:

```csharp
public class ltrDeUnXxx
{
    public static string FiltroPorAlgo => nameof(FiltroPorAlgo);
    // una propiedad por cada filtro que se añada al descriptor
}
```

**Importante — esto es solo la mitad del trabajo.** Estos controles solo construyen la *interfaz* del
filtro; que el filtro **funcione de verdad** depende de que `GestorDeXxxs.AplicarFiltros` (paso 5)
reconozca cada una de estas claves dentro de la lista de `ClausulaDeFiltrado` que le llega y traduzca su
valor a una condición `Where(...)` sobre la consulta — mientras ese método siga con el `// TODO` del
paso 5, los controles se ven y se pueden rellenar, pero no filtran nada todavía.
