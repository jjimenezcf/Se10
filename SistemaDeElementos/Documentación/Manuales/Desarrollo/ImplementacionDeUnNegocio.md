# Cómo dar de alta un negocio nuevo — plantilla genérica

Esta guía es una **plantilla parametrizada**, no la crónica de un caso concreto. Describe, en un orden
de ejecución fijo, todos los ficheros que hay que tocar para dar de alta un negocio nuevo del sistema
(modelo de datos, Dto, flujo, gestores, CRUD básico, vistas, menús y alta del negocio), y cómo rellenar
cada uno a partir de un puñado de datos de entrada. Se ha extraído generalizando el alta real de dos
negocios (**Pedido**, preexistente, usado como referencia constante; y **Almacén**, dado de alta
siguiendo esta misma plantilla), así que cualquier fichero de Pedidos o de Almacenes sirve como ejemplo
concreto ya funcionando de cualquier paso de aquí.

Con los datos de entrada del punto 0 rellenos, todo lo que sigue es mecánico: cada paso dice
exactamente qué fichero tocar y con qué contenido, sustituyendo los placeholders por los datos de
entrada. Esto es lo que permite generar un negocio nuevo de una sola vez a partir de la información de
entrada, sin tener que redescubrir el patrón cada vez.

## 0. Datos de entrada

Este es el formato en el que hay que dar la información para poder generar un negocio nuevo de un
tirón. Cuanto más completo esté, menos huecos hay que rellenar a mano después.

```
Nombre del negocio   : <Nombre en castellano, con tilde si corresponde — p.ej. "Almacén">
Área / módulo        : <Logística | Ventas | Gastos | Jurídico | ... — determina namespaces, carpetas
                        y menú>
Prefijo               : <3-4 letras mayúsculas para estados/transiciones/parámetros — p.ej. "ALM">

Capacidades del elemento (qué interfaces implementa, de InterfacesDtm.cs):
- <p.ej. IPuedeUsarProveedor / IUsaProveedor>
- <p.ej. IUsaArchivo>
- <p.ej. IUsaDirecciones>
- <p.ej. IUsaExpediente, IUsaPresupuesto, IPuedeUsarResponsable, IUsaSolicitante, IPrioridad...>

¿El tipo usa permiso de interventor (IPermisoDeInterventor)?  <sí/no>

Campos particulares del Tipo (Dtm), aparte de los heredados de TipoDeElementoDtm/TipoConFlujoDtm:
- <Nombre>: <tipo .NET — si es un enumerado nuevo, se indica aquí con sus valores y Description>

Campos particulares del Elemento (Dtm), aparte de los heredados de ElementoDeProcesoDtm y de las
capacidades anteriores:
- <Nombre>: <tipo .NET>

Estados (nombre + marca inicial/terminado/cancelado si aplica):
- e1 (inicial)
- e2
- e3 (terminado)
- e4 (cancelado)
- ...

Transiciones:
- e1 --(t1)--> e2
- e2 --(t2)--> e1
- ...  (si una transición es de cancelación, indicarlo para pedir un "asunto"/motivo)

Etapas del flujo (una entrada de enum por cada agrupación de estados que el resto del sistema necesite
consultar de forma genérica; lo habitual es una etapa por estado relevante):
public enum enumEtapasDeXxx
{
    etapa1, etapa2, ...
}

Parámetros de negocio propios (aparte de las etapas), o "ninguno":
public enum enumParametrosDeXxx
{
    ...
}

Tipo(s) por defecto a crear: <nombre, sigla, clase de libro (POR_TIPO/POR_CG/POR_CG_TIPO), y valor de
cada campo particular del Tipo>

Orden en el menú: <por delante/detrás de qué otro negocio del mismo módulo, y por qué (p.ej. "hueco
para un negocio futuro")>
```

### Leyenda de los placeholders usados en esta guía

| Placeholder | Significado | Ejemplo (Almacén) |
|---|---|---|
| `Xxx` | Nombre del negocio, PascalCase, sin acentos, para nombres de clase/fichero | `Almacen` |
| `Xxxs` | Plural en castellano, tal cual se usa en textos/menús (normalmente `enumNegocio.Xxx.Plural()`) | `Almacenes` |
| `XXX` | Prefijo corto para estados/transiciones/parámetros/etapas | `ALM` |
| `<Área>` | Módulo/área de negocio (namespace, carpeta, menú) | `Logistica` |

Cuando un paso dice "cada capacidad añade lo suyo", significa que las clases satélite, las llamadas de
`ModeloDeXxx` y las propiedades del Dto a crear dependen de qué interfaces de la lista de capacidades
se hayan indicado en el punto 0 — **no se crea nada que no esté respaldado por una capacidad o un campo
particular indicado explícitamente** (evita clases/columnas que luego no se usan).

## 1. Analizar un negocio existente parecido como patrón

Antes de generar nada, localizar un negocio ya existente con una forma parecida al nuevo y usarlo como
plantilla literal para copiar estructura de clases, `using`, naming y estilo de los atributos
`IUPropiedad`. Esta propia guía usa **Pedido** (`ServicioDeDatos/Logistica/Pedidos`) como referencia
para todo lo que tiene flujo con tipo, y **Naturaleza** (`ServicioDeDatos/MaestrosTecnico`,
`NaturalezasController`/`DescriptorDeNaturalezas`/`Naturalezas.ts`) como referencia del CRUD más simple
posible, sin tipo ni flujo. Copiar un ejemplo afín es más seguro que empezar de cero, porque respeta
las convenciones del resto del sistema (nombres de tablas, sufijos, interfaces, etc.).

## 2. Modelo de datos (capa `ServicioDeDatos`)

Fichero: `ServicioDeDatos/<Área>/Xxxs/XxxDtm.cs` (+ `TipoDeXxxDtm.cs` si el negocio tiene tipo/flujo).

Un negocio "de proceso" (con tipo, estado y flujo) se compone siempre de dos piezas:

- **El Tipo** (`TipoDeXxxDtm`), hereda de `TipoConFlujoDtm` (o de `TipoDeElementoDtm` si no tiene
  flujo), y de `IPermisoDeInterventor` si el punto 0 lo indica. Lleva `Padre`, `Estado` (si tiene
  flujo), el override de `iEstado`/`Negocio`, y una propiedad por cada "campo particular del Tipo" del
  punto 0 (si ese campo es un enumerado nuevo, se define aquí mismo, con `[Description(...)]` en cada
  valor).
- **El Elemento** (`XxxDtm`), hereda de `ElementoDeProcesoDtm` (o `ElementoDtm`/`ElementoConCgDtm` si
  no usa flujo) y de una interfaz por cada capacidad del punto 0. Cada capacidad añade sus propias
  propiedades (`IPuedeUsarProveedor`/`IUsaProveedor` → `Contacto`, `IdProveedor`, `Proveedor`, más
  `Telefono`/`eMail` heredados de `IDatosDeContacto`; `IUsaArchivo` → `IdArchivo`, `Archivo`;
  `IUsaDirecciones` no añade propiedades, es un marcador). Después se añade una propiedad por cada
  "campo particular del Elemento" del punto 0.

Junto a estas dos clases principales se definen sus clases "satélite" — **solo las que respalda alguna
capacidad o el propio flujo**, no todas por sistema:

| Clase satélite | Hereda de | Cuándo hace falta |
|---|---|---|
| `EstadoDeUnXxxDtm` | `EstadoDtm`, `IInstanciaEstado` | Si el negocio tiene flujo |
| `TransicionesDeUnXxxDtm` | `TransicionDtm` | Si el negocio tiene flujo |
| `AccionesDeUnXxxDtm` | `AccionesDeTrnDtm` | Si el negocio tiene flujo |
| `AuditoriaDeUnXxxDtm` | `AuditoriaDtm` | Siempre (todo elemento se audita) |
| `ObservacionesDeUnXxxDtm` | `ObservacionDtm` | Siempre |
| `TrazasDeUnXxxDtm` | `TrazaDtm` | Siempre |
| `PermisoDelXxxDtm` | `PermisosDelElementoDtm` | Siempre |
| `DireccionDeUnXxxDtm` | `DireccionDtm` | Si implementa `IUsaDirecciones` |
| `HitosDeUnXxxDtm` | `HitoDtm` | Si tiene flujo |
| `ArchivosDeUnXxxDtm` / `ArchivadoresDeUnXxxDtm` | `VinculoDtm` | Siempre (ficheros/archivadores vinculados; independiente de si además hay un `IUsaArchivo` de fichero único) |
| `AgendaDeUnXxxDtm` | `VinculoDtm` | Solo si implementa `IPuedeUsarAgenda`/`IUsaAgenda` |

Todas se decoran con `[Table(Tablas.XXX + "_" + Sufijo.YYY, Schema = Esquemas.<Área>)]`, la tabla
principal con `[Table(Tablas.XXX, Schema = Esquemas.<Área>)]`. La constante `Tablas.XXX` se da de alta
en `ServicioDeDatos/_Elemento/Metadatos.cs` (clase `Tablas`) y **su nombre debe coincidir exactamente
con el valor del enumerado `enumNegocio`** (sin tildes), porque `ApiTipoDeElementoDtm.Negocio(Type)`
deduce el negocio a partir del nombre de la tabla de tipo.

Al final del fichero (o de los dos, uno por partial) se añade una clase estática `partial class
ModeloDeXxx` con un método `ModelBuilder` por cada pieza: `Xxx`, `EstadosDeUnXxx`,
`TransicionesDeUnXxx`, `AccionesDeUnXxx`, `TipoDeXxx`, `Trazas`, `Auditoria`, `Archivos`,
`Observaciones`, `Permisos`, `Direcciones` (solo si hay `IUsaDirecciones`), `Historia`, `Archivadores`,
`Agenda` (solo si aplica). Estos métodos se invocan desde el `DbContext` en el paso 6.

**Regla de oro de esta capa**: cada llamada de `ModeloDeXxx.Xxx(modelBuilder)` que dependa de una
capacidad (`DefinirProveedor<XxxDtm>`, `DefinirCampoArchivo<XxxDtm>`...) tiene que corresponder a una
interfaz que `XxxDtm` implemente de verdad. Si se quita una capacidad, hay que quitar también su
llamada de mapeo — si no, EF Core falla al construir el modelo intentando mapear una propiedad o
navegación que ya no existe en la clase (pasó al retirar Proveedor/Archivo de `AlmacenDtm`).

Cada "campo particular del Elemento" del punto 0 se mapea igual: propiedad en `XxxDtm` +
`modelBuilder.Entity<XxxDtm>().Property(x => x.Campo).HasColumnName(ICampos.CAMPO).HasColumnType(IDominio.Xxx).IsRequired(bool)`
dentro de `ModeloDeXxx.Xxx(...)` — añadiendo la constante a `ICampos` si no existe ya una reutilizable.

### 2.1 `VariablesDeXxx.cs` — etapas y parámetros del negocio

Mismo directorio, fichero `VariablesDeXxx.cs`, con:

- **`enumEtapasDeXxx`**: tal cual la trae el punto 0, cada valor con `[Description("...")]`. Los ids de
  estado concretos de cada etapa no se codifican aquí: se resuelven en caliente vía parámetro de
  negocio (`enumNegocio.Xxx.Parametro(etapa)`), así que se pueden reconfigurar sin tocar código.
- **`enumParametrosDeXxx`**: tal cual el punto 0 (vacío si no hay ninguno).
- **`VariableDeXxx`** (clase estática): expone, para `XxxDtm`, `Estados(etapa)`, `Lista(etapa)`,
  `EstaEnLaEtapa(...)`, `EstaEnAlgunaDeLasEtapa(...)`, `Etapas()`, `Etapa()`, `CadenaDeEtapas()` y
  `Nombre(etapa)` — calcados de `VariableDePedidos`, cambiando el enumerado de etapas y el tipo del
  elemento; un `case` del `switch` de `Estados(etapa)` por cada valor de `enumEtapasDeXxx`, y un `if`
  por cada uno en `Etapas(XxxDtm)`.

## 3. Modelo de Dto (capa `ModeloDeDto`)

Fichero: `ModeloDeDto/<Área>/Xxxs/XxxDto.cs` (+ `TipoDeXxxDto.cs`).

- `TipoDeXxxDto : TipoDeElementoDto[, IPermisoDeInterventorDto]` — solo las propiedades propias del
  tipo que no estén ya en `TipoDeElementoDto` (nombre, sigla, permisos, libro de registro... ya están
  en la base): `Estado`/`IdEstado` si tiene flujo, `IdPermisoDeInterventor`/`PermisoDeInterventor` si
  aplica, y una propiedad por cada "campo particular del Tipo" del punto 0 (si es un enum, propiedad
  del tipo del enum directamente, `TipoDeControl = enumTipoControl.Enumerado`).
- `XxxDto : ElementoDeUnProcesoDto` — solo las propiedades propias del elemento que no estén ya en
  `ElementoDeUnProcesoDto` (Cg, Tipo, Estado, Nombre, Descripción, Referencia... ya están en la base):
  una propiedad (o grupo de propiedades) por cada capacidad indicada en el punto 0 que tenga reflejo en
  el Dtm, más una por cada "campo particular del Elemento".

Cada propiedad se decora con `[IUPropiedad(...)]`, que es lo que construye el formulario y la rejilla
de forma automática (`Fila`/`Columna`, `TipoDeControl`, visibilidad en alta/edición/grid, controlador y
vista a la que navegar en listas dinámicas...). La forma más fiable de rellenar estos atributos es
copiar los de la propiedad equivalente de un negocio con la misma capacidad (p.ej. copiar el bloque de
Proveedor de `PedidoDto` si el nuevo negocio también implementa `IPuedeUsarProveedor`) y ajustar solo lo
que cambie (etiquetas, controlador, negocio, obligatoriedad — recordando que una capacidad "puede usar"
(`IPuedeUsar...`) es opcional, mientras que "usa" (`IUsa...`) es obligatoria).

## 4. Registrar el negocio en `enumNegocio`

Fichero: `Ayudas/Extensiones/Negocios.cs`.

1. Añadir el valor **al final** del enumerado `enumNegocio` (nunca en medio: si el valor se persiste
   por ordinal en algún sitio, insertarlo en medio desplazaría los siguientes), con
   `[Description("Nombre del negocio")]` en castellano y con tilde si corresponde.
2. Completar (solo si el caso genérico por defecto no es correcto) los métodos de extensión:
   - `Plural` — plural correcto, si `Descripcion() + "s"` no vale.
   - `ConArticulo` — si el género/artículo no sigue la regla por defecto (`Singular(true).EndsWith("a") ? "de la" : "del"`).
   - `Controlador` — `case enumNegocio.Xxx: return enumControladores<Área>.Xxxs.ToString();`; si el
     enumerado de controladores del área todavía no tiene el valor, se añade también ahí
     (`Ayudas/Extensiones/Controladores.cs`).
   - `Icono` — por defecto devuelve `"{negocio}.svg"`; basta con añadir el fichero
     `SistemaDeElementos/wwwroot/images/menu/Xxx.svg` con ese nombre exacto (un SVG simple, un único
     `<path>` con `fill-rule="evenodd"` y sub-trazados como "agujeros" es más que suficiente si no hay
     un icono de diseño).

## 5. Enchufar el negocio en los "Extensores" de parametrización

Solo si el negocio tiene flujo. Los extensores permiten operar de forma genérica (por `enumNegocio`)
sobre estados, transiciones y acciones sin un `switch` distinto en cada sitio del sistema. Añadir el
caso del nuevo negocio en:

- `Servicios/GestorDeElementos/Extensores/Parametrizacion/ExtensorDeEstados.cs`
  - `Negocio(Type tipo)` — mapea `EstadoDeUnXxxDtm` → `enumNegocio.Xxx`.
  - `Estados(this ContextoSe contexto, enumNegocio negocio)` — mapea `enumNegocio.Xxx` →
    `contexto.Set<EstadoDeUnXxxDtm>()`.
- `Servicios/GestorDeElementos/Extensores/Parametrizacion/ExtensorDeTransiciones.cs`
  - `Negocio(this Type tipo)` y `SetDeTransiciones(this ContextoSe, enumNegocio)`, análogos.
- `Servicios/GestorDeElementos/Extensores/Parametrizacion/ExtensorDeAccionesDeTrn.cs`
  - `Negocio(this Type tipo)`, análogo.
- `Servicios/GestorDeElementos/Extensores/Elementos/ExtensorDeHitos.cs`
  - `Negocio(Type tipo)` — mapea `HitosDeUnXxxDtm` → `enumNegocio.Xxx`.
  - `Hitos(this enumNegocio negocio, ContextoSe contexto)` — mapea `enumNegocio.Xxx` →
    `contexto.Set<HitosDeUnXxxDtm>().Cast<HitoDtm>()`. Es el mismo patrón de los tres anteriores
    (reverse-lookup por `Type` + forward-lookup por `enumNegocio`) pero para el histórico de hitos del
    flujo; se olvida fácilmente porque no está en el mismo fichero que Estados/Transiciones/Acciones —
    conviene añadir los cuatro extensores a la vez, no solo los tres primeros.

## 6. Enganchar el modelo en el `DbContext`

Fichero: `ServicioDeDatos/CreadorDelMd.cs` (`ContextoSe.OnModelCreating`).

Un método privado `DefinirTablasDeXxx(ModelBuilder modelBuilder)` que invoca, en este orden, los
métodos de `ModeloDeXxx` del paso 2 — primero tipo/flujo, luego el elemento y sus satélites —, y una
llamada a él desde `OnModelCreating`, junto a las de los demás negocios del área:

```csharp
private void DefinirTablasDeXxx(ModelBuilder modelBuilder)
{
    ModeloDeXxx.EstadosDeUnXxx(modelBuilder);      // solo si tiene flujo
    ModeloDeXxx.TransicionesDeUnXxx(modelBuilder);  // solo si tiene flujo
    ModeloDeXxx.AccionesDeUnXxx(modelBuilder);      // solo si tiene flujo
    ModeloDeXxx.TipoDeXxx(modelBuilder);            // solo si tiene tipo

    ModeloDeXxx.Xxx(modelBuilder);
    ModeloDeXxx.Trazas(modelBuilder);
    ModeloDeXxx.Auditoria(modelBuilder);
    ModeloDeXxx.Archivos(modelBuilder);
    ModeloDeXxx.Observaciones(modelBuilder);
    ModeloDeXxx.Permisos(modelBuilder);
    ModeloDeXxx.Direcciones(modelBuilder);          // solo si implementa IUsaDirecciones
    ModeloDeXxx.Historia(modelBuilder);             // solo si tiene flujo
    ModeloDeXxx.Archivadores(modelBuilder);
}
```

Solo se incluyen las llamadas de las clases satélite que realmente existan (regla de oro del paso 2).
**Conviene compilar antes de generar la migración** (paso 14) para detectar cuanto antes referencias a
propiedades/navegaciones que ya no existan.

## 7. Metadatos del negocio (`MetadatosDelNegocio`)

Fichero: `Servicios/GestorDeElementos/MetadatosDelNegocio.cs`.

La clase `Metadatos` reúne, para cada `enumNegocio`, los `Type` de sus clases satélite y algunos
delegados de etapas/prioridad; la usan de forma genérica varias partes del sistema (hitos, seguridad,
plantillas...). Añadir:

```csharp
public static Metadatos MetadatosDeXxxs() => new Metadatos
{
    TipoDto = typeof(TipoDeXxxDto),                 // solo si tiene tipo
    TipoDtm = typeof(TipoDeXxxDtm),                 // solo si tiene tipo
    EstadoDtm = typeof(EstadoDeUnXxxDtm),           // solo si tiene flujo
    HitosDtm = typeof(HitosDeUnXxxDtm),             // solo si tiene flujo
    ObservacionesDtm = typeof(ObservacionesDeUnXxxDtm),
    TrazaDtm = typeof(TrazasDeUnXxxDtm),
    ArchivadoresDtm = typeof(ArchivadoresDeUnXxxDtm),
    ArchivosDtm = typeof(ArchivosDeUnXxxDtm),
    DireccionesDtm = typeof(DireccionDeUnXxxDtm),   // solo si implementa IUsaDirecciones
    TipoParametros = typeof(enumParametrosDeXxx),
    TipoEtapas = typeof(enumEtapasDeXxx),
    EstadosDeLaEtapa = etapa => VariableDeXxx.Lista((enumEtapasDeXxx)etapa),
    PlantillasPorTipoDtm = null
};
```

y el `case enumNegocio.Xxx: metadatos = MetadatosDeXxxs(); break;` dentro del `switch` de
`ObtenerMetadatos`. Si el fichero `VariablesDeXxx.cs` del paso 2.1 todavía no existe cuando se escribe
esto, se dejan `TipoParametros`/`TipoEtapas`/`EstadosDeLaEtapa` a `null` y se vuelve aquí en cuanto se
cree — pero como el punto 0 ya trae las etapas y los parámetros, lo normal es poder rellenarlo todo a
la primera.

## 8. Crear los gestores de negocio

Carpeta: `GestoresDeNegocio/<Área>/Xxxs/`.

- `GestorDeTiposDeXxx : GestorDeTiposDeElemento<ContextoSe, TipoDeXxxDtm, TipoDeXxxDto>` — perfil de
  AutoMapper `MapearTipoDeXxx` (heredando de `MapearTipoDeElemento`), constructor que pasa
  `enumNegocio.Xxx` a la base, factory estático `Gestor(...)`, sobrecargas mínimas (`AplicarJoins`,
  `ValidarNoHayElementos`, `AntesDePersistir`), y **`PersistirTipo(contexto, nombre, idEstado, clsLibro,
  sigla, permiteCrear, ...campos particulares del Tipo con su valor por defecto)`** — inserta el tipo
  si no existe por nombre, o actualiza sus campos si ya existía y cambiaron; lo usa el inicializador del
  paso 10. Este gestor **no** se registra en el contenedor de DI: se instancia siempre vía su factory
  estático (lo usa `GestorDeXxx.GestorDeTipos`).
- `GestorDeXxx : GestorDeElementos<ContextoSe, XxxDtm, XxxDto>` — perfil de AutoMapper `MapearXxx` (un
  `.ForMember` de mapeo por cada capacidad con reflejo en el Dto), propiedades `Negocio` y
  `GestorDeTipos`, constructor, factory estático `Gestor(...)`, y sobrecargas de `AplicarJoins`
  (`.Include(x => x.Proveedor)` si aplica...), `AplicarFiltros`, `AplicarOrden`, `AplicarSeguridad`
  (filtrado por tipo/Cg estándar), `AntesDePersistir`, `DespuesDePersistir`, `AntesDeTransitar`,
  `DespuesDeTransitar`. En esta fase los métodos se dejan con un `// TODO` o llamando solo a la base: la
  lógica de negocio concreta (filtros propios, validaciones, qué pasa antes/después de persistir o de
  transitar) se completa en una fase posterior, con el detalle funcional ya decidido.
- Por último, **el gestor del elemento sí hay que registrarlo** en el contenedor de DI —
  `GestoresDeNegocio/ServiceExtensions.cs`, dentro del método `Configure<Área>` correspondiente,
  `services.AddScoped<GestorDeXxx>();` (el de tipos no se registra, ver arriba).

## 9. Controlador MVC, Descriptor de crud, vista y TypeScript del CRUD básico

Cuatro piezas, todas calcadas del ejemplo más simple posible (`NaturalezasController`/
`DescriptorDeNaturalezas`/`Naturalezas.ts` — un CRUD sin particularidades de filtros ni de menú), no de
uno con mucha lógica añadida como Pedidos, salvo que el punto 0 indique particularidades desde el
principio:

1. **Controlador** — `SistemaDeElementos/Controllers/<Área>/XxxsController.cs`, hereda de
   `EntidadController<ContextoSe, XxxDtm, XxxDto>`, recibe por constructor `GestorDeXxx` (el mismo del
   paso 8) y `GestorDeErrores`, y expone `CrudXxxs()`: `ApiController.CumplimentarDatosDeUsuarioDeConexion(...)` y devuelve `ViewCrud(new DescriptorDeXxx(Contexto, ModoDescriptor.Mantenimiento))` en un
   `try/catch` que llama a `RenderMensaje(e.Message)` si falla.
2. **Descriptor de crud** — `SistemaDeElementos/Descriptores/DescriptoresDe<Área>/DescriptorDeXxx.cs`,
   hereda de `DescriptorDeCrud<XxxDto>`. El constructor solo llama al de la base pasando
   `nameof(XxxsController)`, `nameof(XxxsController.CrudXxxs)`, `modo` y `rutaBase` (el
   `enumNameSpaceTs` del área). Si no hay particularidades el cuerpo se deja vacío; solo se sobrescribe
   `RenderControl()` para inyectar el `<script>` que carga `Xxx.js` y llama a
   `RutaBase.CrearCrudDeXxxs(...)` (patrón calcado de `DescriptorDeNaturalezas`).
3. **Vista** — `SistemaDeElementos/Views/<Área>/CrudXxxs.cshtml`: `@model MVCSistemaDeElementos.Descriptores.DescriptorDeXxx` + `@Html.Raw(Model.RenderControl())`. El nombre del fichero tiene que
   coincidir con el nombre de la acción del controlador, porque `ViewCrud`/`View` resuelven la vista por
   convención.
4. **TypeScript** — `SistemaDeElementos/wwwroot/ts/<Área>/Xxx.ts`, dentro del `namespace` del área
   (compartido con los demás ficheros del área). Sin particularidades, mínimo indispensable —
   `CrearCrudDeXxxs(...)`, `class CrudDeXxxs extends Crud.CrudMnt`, `class CrudCreacionXxx extends
   Crud.CrudCreacion`, `class CrudEdicionXxx extends Crud.CrudEdicion` — el constructor de cada una solo
   llama a `super(...)`, sin overrides. No hace falta registrar el fichero en ningún sitio:
   `tsconfig.json` incluye `wwwroot/ts/**/*`.
5. **JSON de layout — `SistemaDeElementos/wwwroot/Json/XxxDto.json` y `TipoDeXxxDto.json`**
   (imprescindible, no opcional pese a que su ausencia no lanza ningún error al arrancar): en tiempo de
   ejecución, `ApiClasesComunes.ObtenerAtributosJson` busca por convención un fichero
   `wwwroot/Json/{NombreDelTipoDto}.json` y, si existe, sobreescribe con su contenido los atributos
   `[IUPropiedad]` de las propiedades que liste (posición en el formulario, orden del grid,
   `SeleccionarDe`/`Negocio`/`VistaDondeNavegar`/`RestrictorFijo` de las listas dinámicas...). Si el
   fichero no existe, `ObtenerAtributosJson` no falla (devuelve una lista vacía), **pero la propiedad
   `Tipo` heredada de `ElementoDeUnProcesoDto` no trae fijados `SeleccionarDe`/`Negocio`/
   `VistaDondeNavegar` en su atributo base** (son genéricos, iguales para cualquier negocio con tipo) —
   sin el override del JSON, el control `ListaDinamica` de "Tipo" no sabe de dónde traer los tipos ni a
   qué vista navegar, y falla al renderizarse con un error tipo
   `Fallo al renderizar el control de 'tipo' del tipo ListaDinamica con id table-xxxdto-nuevo-tipo`.
   Como mínimo, `XxxDto.json` necesita la entrada de `Tipo`:
   ```json
   [
     {
       "propiedad": "Tipo",
       "SeleccionarDe": "ModeloDeDto.<Área>.TipoDeXxxDto",
       "VistaDondeNavegar": "TiposDeXxx",
       "RestrictorFijo": "Negocio;Xxx;Gestor",
       "Negocio": "Xxx",
       "AutoSpan": true
     }
   ]
   ```
   El resto de entradas (posición de `CG`, `Referencia`, `Nombre`, `Estado`... en `XxxDto.json`; posición
   de `Nombre`, `Sigla`, `ClaseDeLibro`, `Estado`, permisos, campos particulares del Tipo... en
   `TipoDeXxxDto.json`) son de maquetación pura: se copian del `PedidoDto.json`/`TipoDePedidoDto.json` de
   referencia y se ajustan a las propiedades que realmente tenga el Dto nuevo.

## 10. Inicializador del flujo y del tipo por defecto (`InzXxx`)

Fichero: `Inicializador/Procesos/<Área>/InzXxx.cs` (namespace `Inicializador.<Área>`). Da de alta,
dentro de una única transacción, los estados/transiciones/tipo por defecto — materializa en base de
datos el flujo del punto 0:

- Constantes `readonly string` con el nombre de cada estado/transición/tipo, prefijadas con `XXX`.
- `Estados(contexto)`: un `GestorDeEstados.PersistirEstado(contexto, enumNegocio.Xxx, nombre, inicial:,
  terminado:, cancelado:, orden:)` por cada estado del punto 0.
- `Transiciones(contexto)`: un `GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Xxx,
  nombre, origen, destino, asunto:)` por cada transición del punto 0 (con `asunto` si es de
  cancelación).
- `DefinirEtapas(contexto)`: por cada valor de `enumEtapasDeXxx`, resuelve el id del estado
  correspondiente con `contexto.SeleccionarEstado<EstadoDeUnXxxDtm>(nombre)` y lo graba con
  `enumNegocio.Xxx.ResetearParametro(contexto, etapa, idEstado)`.
- `Tipos(contexto)`: `GestorDeTiposDeXxx.PersistirTipo(...)` con el tipo por defecto del punto 0.
- `ModeloDeXxx(contexto)`: envuelve las cuatro llamadas en `contexto.IniciarTransaccion()`/
  `Commit`/`Rollback`.

## 11. Vistas de negocio, menús y alta del negocio

Tres altas más en `Inicializador/Negocios/`, siempre con el mismo patrón "busca dónde está el negocio
de referencia del mismo módulo y añade la línea de Xxx justo al lado, con el orden relativo que
corresponda":

1. **Vista de "Tipos de Xxx"** (solo si tiene tipo) — `SistemaDeElementos/Controllers/Negocio/TiposDeElementoController.cs`: una acción trivial que delega en el método común:
   ```csharp
   public IActionResult TiposDeXxx() => TiposDeElemento(enumNegocio.Xxx, nameof(TiposDeXxx));
   ```
2. **Constantes de vista del área** — `Ayudas/Extensiones/Controladores.cs`, clase
   `enumVistas<Área>`: una constante `nameof(...)` por cada acción de controlador a registrar como
   vista (`TiposDeXxx`, `CrudXxxs`, y `MaestrosDeXxx` si tiene inicializador de flujo). Y el
   `case enumNegocio.Xxx: return enumVistas<Área>.TiposDeXxx;` dentro del `switch` de
   `enumVistasNegocio.CrudDeTipos(enumNegocio negocio)`.
3. **Vistas** — `Inicializador/Negocios/InzVistas.cs`: en la subclase del área dentro de `enumVistas`,
   una constante `readonly string` con el nombre visible de cada vista nueva, y en
   `CrearVistasDelModuloDe<Área>` una llamada `gestor.CrearVistaSiNoExiste(nombre, controlador, accion,
   modal, tipoDto.FullName)` por cada una.
4. **Menús** — `Inicializador/Negocios/InzMenus.cs`, dentro del método del módulo: una llamada a
   `MenusDeConfiguracionDeProceso(gestor, enumNegocio.Xxx, padre: $"{Modulo}.{Configuracion}",
   vistaDeTipo, vistaDeMaestros, orden)` (crea el submenú de configuración con
   Estados/Transiciones/Tipos/Maestros) y una llamada a `GestorDeMenus.CrearMenuSiNoExiste(...)` para el
   punto de menú de gestión del propio negocio (`icono: negocio.Icono()`, `vista` = la vista de crud).
   El **orden relativo** entre negocios del mismo módulo se controla con el parámetro `orden` de estas
   llamadas — el punto 0 dice por delante/detrás de qué otro negocio va, y con qué hueco.
5. **Alta del negocio** — `Inicializador/Negocios/InzNegocio.cs` (clase `InzNegocios`): una llamada a
   `GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Xxx, nombre, typeof(XxxDtm),
   typeof(XxxDto), icono, esDeParametrizacion:, usaCg:, usaSeguridad:)`, junto a la de los demás
   negocios del módulo y en la posición relativa que toque. Esta es la fila que hace que el negocio
   "exista" en la tabla de Negocios, algo independiente de sus vistas/menús.

Nada de este paso llega a la base de datos por sí solo: son datos que se insertan la próxima vez que se
ejecute la inicialización general del sistema (las mismas opciones de "Inicializar entorno"/"Definir
maestros" del panel de administración que ya usan el resto de negocios).

## 12. Ejecutar el inicializador del flujo del negocio

Para que `InzXxx.ModeloDeXxx` del paso 10 se ejecute de verdad hace falta una acción en el controlador
del negocio, calcada de `PedidosController.MaestrosDePedidos`:

```csharp
public IActionResult MaestrosDeXxxs()
{
    var r = new Resultado();
    try
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
        if (!Contexto.SePuedeParametrizar())
            GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");
        InzXxx.ModeloDeXxx(Contexto);
        ViewBag.Mensaje = "Maestros inicializados";
        r.Estado = enumEstadoPeticion.Ok;
    }
    catch (Exception e)
    {
        return RenderMensaje($"No se ha podido inicializar los maestros.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
    }
    finally
    {
        ServicioDeCaches.EliminarTodas();
    }
    return VistaDelPanelDeControl(Contexto);
}
```

Esta acción es precisamente la que se registra como vista `MaestrosDeXxx` en el paso 11.3, así que
aparece en el menú de configuración creado en el paso 11.4.

## 13. Tras el `add-migration`: la función SQL `CC_XXX_NOMBRE`

Al generar la migración (paso 14) EF Core crea, para la tabla `Xxx_OBSERVACION` (y solo esa: es la
única clase satélite con columna `Elemento`), una columna computada así:

```csharp
ELEMENTO = table.Column<string>(type: "VARCHAR(255)", nullable: true,
    computedColumnSql: "<ESQUEMA>.CC_XXX_NOMBRE(ID_ELEMENTO)")
```

Esto sale de `ObservacionDtm.cs` (`ApiDeObservaciones.DefinirCampos<TEntity, TPadre>`), que construye
el nombre de la función **por convención**, a partir del esquema y el nombre de tabla del propio
`XxxDtm` (`{Esquema}.CC_{Tabla}_NOMBRE({ID_ELEMENTO})`) — no hay nada que tocar en C# para esto, la
migración ya la referencia sola. El problema es que **la función en sí no existe todavía en ninguna
base de datos**: EF Core no crea funciones SQL, solo las referencia. Sin ella, aplicar la migración
sobre una base de datos real falla al crear la columna computada.

La función tiene que crearla la propia migración — **no** algo que se ejecute a mano aparte, porque
`update-database`/`Update-Database` tiene que poder aplicarse solo, de un tirón, sobre cualquier base
de datos (la del desarrollador, la de pruebas, la de producción...). Hay que editar el fichero de
migración recién generado (`Migraciones/Migrations/<timestamp>_<nombre>.cs`) a mano:

1. En `Up(MigrationBuilder migrationBuilder)`, insertar un `migrationBuilder.Sql(@"...")` con el
   `CREATE FUNCTION` **justo antes** de la llamada `migrationBuilder.CreateTable(name: "XXX_OBSERVACION", ...)` (la función tiene que existir ya en el momento en que SQL Server intenta crear la
   columna computada que la usa):
   ```csharp
   migrationBuilder.Sql(@"
   CREATE FUNCTION [<ESQUEMA>].[CC_XXX_NOMBRE] (@id_elemento int)
   RETURNS VarChar(250)
   AS
   begin
     declare @resultado VARCHAR(250)

     select @resultado = NOMBRE from <ESQUEMA>.<TABLA> where id = @id_elemento
     return @resultado
   END
   ");

   migrationBuilder.CreateTable(
       name: "XXX_OBSERVACION",
       ...
   ```
   Importante: esta llamada a `Sql(...)` debe contener **únicamente** la sentencia `CREATE FUNCTION` (sin
   mezclarla con otras sentencias en el mismo `Sql(...)`), porque SQL Server exige que `CREATE FUNCTION`
   sea la única sentencia del lote — cada llamada a `migrationBuilder.Sql(...)`/`CreateTable(...)` ya se
   ejecuta como un lote independiente, así que basta con no juntarla con nada más.
2. En `Down(MigrationBuilder migrationBuilder)`, añadir el `DROP FUNCTION` simétrico justo después de
   `migrationBuilder.DropTable(name: "XXX_OBSERVACION", ...)`:
   ```csharp
   migrationBuilder.DropTable(
       name: "XXX_OBSERVACION",
       schema: "<ESQUEMA>");

   migrationBuilder.Sql("DROP FUNCTION [<ESQUEMA>].[CC_XXX_NOMBRE];");
   ```
3. Además, añadir la misma función a `Migraciones/Migrations/Región 1.txt`, junto a la del negocio de
   referencia del mismo esquema (p.ej. junto a `LOGISTICA.CC_PEDIDO_NOMBRE`). Ese fichero es el bloque
   de SQL a mano que se pegó en su día dentro de la migración `InitialCreate` (dentro de
   `#region region a incluir 1`), usado para poblar de golpe una base de datos completamente nueva; se
   mantiene como catálogo de referencia de todas las funciones `CC_*` del sistema para cuando algún día
   se regenere/unifique `InitialCreate` — no es la vía por la que la función llega a ninguna base de
   datos existente, solo documentación.

Compila `Migraciones` después de editar el `.cs` de la migración a mano, para asegurarte de que el SQL
va entre comillas correctamente y no has roto la sintaxis C#.

Para Almacenes se ha insertado `CREATE FUNCTION [LOGISTICA].[CC_ALMACEN_NOMBRE]` en
`20260827140112_almacenes.cs`, justo antes de `CreateTable("ALMACEN_OBSERVACION", ...)`, con su
`DROP FUNCTION` simétrico en `Down()`, y se ha añadido también a `Región 1.txt` junto a
`LOGISTICA.CC_PEDIDO_NOMBRE`.

## 14. Qué queda pendiente siempre (fuera del alcance de esta plantilla)

Ni siquiera siguiendo la plantilla al pie de la letra el negocio queda operativo de punta a punta. Dos
cosas no son mecánicas y quedan fuera:

- **Migración de EF Core**: con el modelo ya enganchado al `DbContext` (paso 6), generar la migración
  (`dotnet ef migrations add ...`), completar la función `CC_XXX_NOMBRE` del paso 13 y aplicarla — sin
  esto no existen ni la tabla del negocio ni sus satélites, así que el paso 12 (y por tanto el alta de
  vistas/menús/negocio del paso 11) no se puede ejecutar de verdad hasta que se aplique.
- **Lógica de negocio real**: los `// TODO` de los gestores (paso 8) y las particularidades del CRUD —
  filtros, opciones de menú, expansores de detalle... (pasos 3 y 9) — que solo se pueden rellenar una
  vez decidido el detalle funcional que no venga en el punto 0.

## 15. Checklist de ficheros (referencia rápida)

| # | Fichero | Qué se añade |
|---|---|---|
| 2 | `ServicioDeDatos/<Área>/Xxxs/XxxDtm.cs` | `XxxDtm`, satélites, `ModeloDeXxx.Xxx/Trazas/Auditoria/Archivos/Observaciones/Permisos/Direcciones/Archivadores` |
| 2 | `ServicioDeDatos/<Área>/Xxxs/TipoDeXxxDtm.cs` | Enum de campo particular del tipo (si aplica), `EstadoDeUnXxxDtm`, `TransicionesDeUnXxxDtm`, `AccionesDeUnXxxDtm`, `TipoDeXxxDtm`, `ModeloDeXxx.EstadosDeUnXxx/TransicionesDeUnXxx/AccionesDeUnXxx/TipoDeXxx` |
| 2.1 | `ServicioDeDatos/<Área>/Xxxs/VariablesDeXxx.cs` | `enumEtapasDeXxx`, `enumParametrosDeXxx`, `VariableDeXxx` |
| 2 | `ServicioDeDatos/_Elemento/Metadatos.cs` | `Tablas.XXX`, `ICampos.*` de los campos particulares |
| 3 | `ModeloDeDto/<Área>/Xxxs/XxxDto.cs` / `TipoDeXxxDto.cs` | Dtos |
| 4 | `Ayudas/Extensiones/Negocios.cs` | Valor de `enumNegocio`, `Plural`/`ConArticulo`/`Controlador` si hace falta |
| 4 | `Ayudas/Extensiones/Controladores.cs` | Valor en `enumControladores<Área>` si falta |
| 5 | `ExtensorDeEstados.cs` / `ExtensorDeTransiciones.cs` / `ExtensorDeAccionesDeTrn.cs` / `ExtensorDeHitos.cs` | Caso del negocio en cada `switch`/`if` |
| 6 | `ServicioDeDatos/CreadorDelMd.cs` | `DefinirTablasDeXxx` + llamada desde `OnModelCreating` |
| 7 | `Servicios/GestorDeElementos/MetadatosDelNegocio.cs` | `MetadatosDeXxxs()` + caso en el `switch` |
| 8 | `GestoresDeNegocio/<Área>/Xxxs/GestorDeTiposDeXxx.cs` / `GestorDeXxx.cs` | Gestores |
| 8 | `GestoresDeNegocio/ServiceExtensions.cs` | `services.AddScoped<GestorDeXxx>();` en `Configure<Área>` |
| 9 | `SistemaDeElementos/Controllers/<Área>/XxxsController.cs` | Controlador MVC |
| 9 | `SistemaDeElementos/Descriptores/DescriptoresDe<Área>/DescriptorDeXxx.cs` | Descriptor de crud |
| 9 | `SistemaDeElementos/Views/<Área>/CrudXxxs.cshtml` | Vista |
| 9 | `SistemaDeElementos/wwwroot/ts/<Área>/Xxx.ts` | TypeScript |
| 9 | `SistemaDeElementos/wwwroot/Json/XxxDto.json` / `TipoDeXxxDto.json` | Layout + override de `Tipo` (`SeleccionarDe`/`Negocio`/`VistaDondeNavegar`) — imprescindible |
| 4 | `SistemaDeElementos/wwwroot/images/menu/Xxx.svg` | Icono |
| 10 | `Inicializador/Procesos/<Área>/InzXxx.cs` | Estados/transiciones/tipo por defecto |
| 11 | `SistemaDeElementos/Controllers/Negocio/TiposDeElementoController.cs` | Acción `TiposDeXxx()` |
| 11 | `Inicializador/Negocios/InzVistas.cs` | Constantes + altas de vista |
| 11 | `Inicializador/Negocios/InzMenus.cs` | Menú de configuración + menú de gestión |
| 11 | `Inicializador/Negocios/InzNegocio.cs` | Alta en `NegocioDtm` |
| 12 | `SistemaDeElementos/Controllers/<Área>/XxxsController.cs` | Acción `MaestrosDeXxxs()` |
| 13 | `Migraciones/Migrations/<timestamp>_<nombre>.cs` | `Sql(CREATE FUNCTION ...)` antes de `XXX_OBSERVACION`, `Sql(DROP FUNCTION ...)` en `Down()` |
| 13 | `Migraciones/Migrations/Región 1.txt` | Catálogo: mismo `CREATE FUNCTION [<ESQUEMA>].[CC_XXX_NOMBRE]` |

## Ejemplo de referencia: Almacenes

El negocio de **Almacenes** (`ServicioDeDatos/Logistica/Almacenes`, área Logística, prefijo `ALM`) está
dado de alta siguiendo esta plantilla al completo salvo el paso 14. Sirve como ejemplo real de cada
paso:

- Capacidades: solo `IUsaDirecciones` (Proveedor y Archivo se probaron y se retiraron por no usarse
  todavía — ver la regla de oro del paso 2).
- Campo particular del Tipo: `Calculo` (`enumAlmacenCalculo`: `Fifo`, `Lifo`, `PMP`).
- Sin campos particulares del Elemento.
- Estados: `Abierto` (inicial), `En inventario`, `Cerrado` (terminado), `Cancelado` (cancelado).
- Transiciones: `Abierto --(Cerrar)--> Cerrado`, `Cerrado --(Reabrir)--> Abierto`,
  `Abierto --(Inventariar)--> En inventario`, `Abierto --(Cancelar)--> Cancelado` (con motivo).
- Etapas: `ALM_Etapa_Activo`, `ALM_Etapa_En_Inventario`, `ALM_Etapa_Cerrado`, `ALM_Etapa_Cancelado` —
  una por estado.
- Parámetros de negocio: ninguno (`enumParametrosDeAlmacenes` vacío).
- Tipo por defecto: `ALM: General`.
- Orden en el menú: por delante de Pedidos, dejando hueco detrás para un futuro negocio de Albaranes.
- Función `CC_XXX_NOMBRE`: `LOGISTICA.CC_ALMACEN_NOMBRE(@id_elemento)`, insertada dentro de
  `20260827140112_almacenes.cs` (antes de `CreateTable("ALMACEN_OBSERVACION")` en `Up()`, con su
  `DROP FUNCTION` en `Down()`), y añadida también a `Región 1.txt` junto a `LOGISTICA.CC_PEDIDO_NOMBRE`.
- JSON de layout: `AlmacenDto.json` y `TipoDeAlmacenDto.json` en `wwwroot/Json/`, con el override de
  `Tipo` (`SeleccionarDe: TipoDeAlmacenDto`, `Negocio: Almacen`, `VistaDondeNavegar: TiposDeAlmacen`) —
  sin ellos, el mantenimiento de Almacenes fallaba al renderizar el control de "Tipo".
- `ExtensorDeHitos.cs`: caso `HitosDeUnAlmacenDtm`/`enumNegocio.Almacen` añadido en `Negocio(Type)` y en
  `Hitos(this enumNegocio, ContextoSe)` — se había quedado fuera al hacer el paso 5 la primera vez.
