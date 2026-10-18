# Cómo implementar el Cuadro de Mandos (esquema INDICADOR)

Este manual describe el diseño y los pasos para añadir a Se un **cuadro de mandos de indicadores**:

- El **administrador** define **grupos de indicadores**. Cada grupo lleva un permiso de consulta que se asigna a roles o puestos.
- Cada **indicador** pertenece a un grupo, se agrupa por una **dimensión** (provincia, municipio, calle, centro gestor…) y referencia una **acción** (`ENTORNO.ACCION`: programa, bloque SQL o procedimiento almacenado) que hace el cálculo.
- Un **trabajo sometido nocturno** recorre los indicadores activos a los que les toca ejecutarse y guarda en `INDICADOR.VALOR` un valor por **fecha** y **agrupador**, ya coloreado en **verde, naranja o rojo** según los umbrales del indicador.
- Una **página nueva "Cuadro de mandos"** muestra al usuario los grupos para los que tiene permiso.

> **Regla de oro:** el cuadro de mandos **nunca calcula en línea**. Solo lee `INDICADOR.VALOR`. Todo el coste de las consultas se paga de noche, en el trabajo sometido.

---

## 0. Antes de empezar

| Concepto | Valor |
|---|---|
| Esquema nuevo | `INDICADOR` |
| Carpeta de Dtm | `ServicioDeDatos\Indicadores\` |
| Carpeta de Dto | `ModeloDeDto\Indicadores\` |
| Carpeta de gestores | `GestoresDeNegocio\Indicadores\` |
| Área MVC / TS | `Indicadores` (`Controllers\Indicadores\`, `Views\Indicadores\`, `wwwroot\ts\Indicadores\`) |
| Clase de permiso nueva | `enumClaseDePermiso.Indicador` |
| Negocios nuevos (`enumNegocio`) | `GrupoDeIndicadores`, `DimensionDeIndicador`, `Indicador`, `ValorDeIndicador` |

Orden recomendado de implementación (cada fase compila y se puede probar por separado):

1. Modelo de datos y migración (§1).
2. Gestores y seguridad (§2).
3. Ejecución de acciones PA/SQL (§3).
4. Trabajo nocturno (§4).
5. Mantenimientos CRUD (§5.1).
6. Página "Cuadro de mandos" (§5.2).

---

## 1. Modelo de datos

### 1.1 Diagrama

```
SEGURIDAD.PERMISO ◄── INDICADOR.GRUPO_INDICADOR
                              ▲
                              │ ID_GRUPO
ENTORNO.ACCION ◄── ID_ACCION ─ INDICADOR.INDICADOR ── ID_DIMENSION ──► INDICADOR.DIMENSION_INDICADOR ── ID_NEGOCIO? ──► NEGOCIO.NEGOCIO
                              ▲
                              │ ID_INDICADOR
                     INDICADOR.VALOR_INDICADOR   (FECHA, ID_AGRUPADOR, AGRUPADOR, VALOR, COLOR)
```

> Los nombres de tabla llevan el sufijo `_INDICADOR` para no chocar con constantes que ya existen en `Metadatos.cs` (por ejemplo `VALOR` como nombre de campo).

### 1.2 Metadatos

Fichero: `ServicioDeDatos\_Elemento\Metadatos.cs`

```csharp
// clase Esquemas
public const string INDICADOR = nameof(INDICADOR);

// clase Tablas
public const string GRUPO_INDICADOR = nameof(GRUPO_INDICADOR);
public const string DIMENSION_INDICADOR = nameof(DIMENSION_INDICADOR);
public const string INDICADOR = nameof(INDICADOR);
public const string VALOR_INDICADOR = nameof(VALOR_INDICADOR);

// clase ICampos (añadir solo los que no existan ya)
ID_PERMISO_CONSULTA, ID_GRUPO, ID_DIMENSION, ID_ACCION, ID_INDICADOR, ID_AGRUPADOR, AGRUPADOR,
UNIDAD, DECIMALES, SENTIDO, UMBRAL_VERDE, UMBRAL_ROJO, PERIODICIDAD, DIA_PERIODO,
ACTIVO, ORDEN, FECHA_ULTIMO_CALCULO, ULTIMO_ERROR, COLOR, CALCULADO_EL
```

### 1.3 Tablas

#### `INDICADOR.GRUPO_INDICADOR`: `GrupoDeIndicadoresDtm : ElementoDtm`

| Columna | Tipo (`IDominio`) | Nulo | Notas |
|---|---|---|---|
| ID, NOMBRE, auditoría, ROWVERSION | heredados de `ElementoDtm` | | NOMBRE único |
| DESCRIPCION | `VARCHAR_2000` | sí | |
| ORDEN | `INT` | no | Orden de las pestañas en el cuadro |
| ID_PERMISO_CONSULTA | `INT` | no | FK → `SEGURIDAD.PERMISO`, **autocreado** (§2) |

#### `INDICADOR.DIMENSION_INDICADOR`: `DimensionDeIndicadorDtm : ElementoDtm`

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| ID, NOMBRE, … | heredados | | Provincia, Municipio, Calle, Centro gestor, *Sin agrupar*… |
| DESCRIPCION | `VARCHAR_2000` | sí | |
| ID_NEGOCIO | `INT` | sí | FK → `NEGOCIO.NEGOCIO`. Si se informa, `ID_AGRUPADOR` es el Id de un elemento de ese negocio y el cuadro puede enlazar con su ficha |

> La dimensión ***Sin agrupar*** la crea el inicializador. Sus indicadores devuelven una sola fila con `ID_AGRUPADOR = 0` (el total).

#### `INDICADOR.INDICADOR`: `IndicadorDtm : ElementoDtm`

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| ID, NOMBRE, … | heredados | | NOMBRE único |
| DESCRIPCION | `VARCHAR_2000` | sí | Qué mide y cómo se interpreta |
| ID_GRUPO | `INT` | no | FK → `GRUPO_INDICADOR` |
| ID_DIMENSION | `INT` | no | FK → `DIMENSION_INDICADOR` |
| ID_ACCION | `INT` | no | FK → `ENTORNO.ACCION` |
| UNIDAD | `VARCHAR_10` | sí | `%`, `€`, `nº`, `días`… |
| DECIMALES | `INT` | no | Por defecto 0 |
| SENTIDO | `CHAR_1` | no | `A` = mayor es mejor; `D` = menor es mejor |
| UMBRAL_VERDE | `DECIMAL` (18,6) | no | |
| UMBRAL_ROJO | `DECIMAL` (18,6) | no | |
| PERIODICIDAD | `CHAR_1` | no | `D` diaria, `S` semanal, `M` mensual |
| DIA_PERIODO | `INT` | sí | Si es `S`: 1 = lunes … 7 = domingo. Si es `M`: 1 … 31 (si el mes es más corto, el último día). Si es `D`: null |
| ACTIVO | `BIT` | no | Por defecto 1 |
| ORDEN | `INT` | no | Orden dentro del grupo |
| FECHA_ULTIMO_CALCULO | `DATETIME_2` | sí | La escribe el trabajo |
| ULTIMO_ERROR | `VARCHAR_2000` | sí | La escribe el trabajo; null si fue bien |

Índices:
- `I_INDICADOR_NOMBRE` (único);
- `I_INDICADOR_ID_GRUPO_ORDEN`;
- FKs con `DeleteBehavior.Restrict`.

Se recomienda crear también la tabla `INDICADOR_AUDITORIA` (patrón `AuditoriaDtm`, como `ProvinciaDtm`), porque los umbrales cambian con el tiempo y conviene saber quién los tocó.

#### `INDICADOR.VALOR_INDICADOR`: `ValorDeIndicadorDtm : RegistroDtm`

Hereda solo de `RegistroDtm`, sin auditoría ni `RowVersion`, por el **volumen**: un indicador por calle puede generar miles de filas al día.

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| ID | `INT` identity | no | |
| ID_INDICADOR | `INT` | no | FK → `INDICADOR` con `Cascade` (borrar el indicador borra sus valores) |
| FECHA | `DATE` | no | **Fecha del dato**, no de la ejecución |
| ID_AGRUPADOR | `INT` | no | 0 = total o sin agrupar |
| AGRUPADOR | `VARCHAR_250` | no | Texto **congelado** en la fecha del cálculo (si la calle se renombra, el histórico no cambia) |
| VALOR | `DECIMAL` (18,6) | no | |
| COLOR | `CHAR_1` | no | `V`, `N` o `R`, calculado con los umbrales **vigentes al calcular** |
| CALCULADO_EL | `DATETIME_2` | no | |

Índices:
- `I_VALOR_INDICADOR_ID_INDICADOR_FECHA_ID_AGRUPADOR` (**único**): garantiza la idempotencia del trabajo.
- `I_VALOR_INDICADOR_ID_INDICADOR_FECHA` con `INCLUDE (AGRUPADOR, VALOR, COLOR)`: la consulta principal del cuadro.

> **Por qué se congela el color:** si mañana el administrador cambia los umbrales, el histórico refleja cómo estaba el semáforo aquel día. Si se quiere recolorear el pasado, es una acción explícita (ver *Pendientes*).

### 1.4 Ejemplo de Dtm y configuración

Fichero: `ServicioDeDatos\Indicadores\IndicadorDtm.cs`. Sigue el patrón de `ServicioDeDatos\Callejero\ProvinciaDtm.cs`.

```csharp
public static class SentidoDeIndicador { public const string Ascendente = "A"; public const string Descendente = "D"; }
public static class PeriodicidadDeIndicador { public const string Diaria = "D"; public const string Semanal = "S"; public const string Mensual = "M"; }
public static class ColorDeIndicador { public const string Verde = "V"; public const string Naranja = "N"; public const string Rojo = "R"; }

[Table(Tablas.INDICADOR, Schema = Esquemas.INDICADOR)]
public class IndicadorDtm : ElementoDtm
{
    public string Descripcion { get; set; }
    public int IdGrupo { get; set; }
    public GrupoDeIndicadoresDtm Grupo { get; set; }
    public int IdDimension { get; set; }
    public DimensionDeIndicadorDtm Dimension { get; set; }
    public int IdAccion { get; set; }
    public AccionDtm Accion { get; set; }
    public string Unidad { get; set; }
    public int Decimales { get; set; }
    public string Sentido { get; set; }
    public decimal UmbralVerde { get; set; }
    public decimal UmbralRojo { get; set; }
    public string Periodicidad { get; set; }
    public int? DiaPeriodo { get; set; }
    public bool Activo { get; set; }
    public int Orden { get; set; }
    public DateTime? FechaUltimoCalculo { get; set; }
    public string UltimoError { get; set; }
}

public static partial class ModeloDeIndicadores
{
    public static void Indicador(ModelBuilder modelBuilder)
    {
        ApiDeElementoDtm.DefinirCamposDelElementoDtm<IndicadorDtm>(modelBuilder);

        modelBuilder.Entity<IndicadorDtm>().Property(p => p.Sentido).HasColumnName(ICampos.SENTIDO).HasColumnType(IDominio.CHAR_1).IsRequired();
        modelBuilder.Entity<IndicadorDtm>().Property(p => p.UmbralVerde).HasColumnName(ICampos.UMBRAL_VERDE).HasColumnType(IDominio.DECIMAL).IsRequired();
        // ... resto de columnas igual

        modelBuilder.Entity<IndicadorDtm>()
            .HasOne(p => p.Accion).WithMany().HasForeignKey(p => p.IdAccion)
            .HasConstraintName($"FK_{Tablas.INDICADOR}_{ICampos.ID_ACCION}")
            .OnDelete(DeleteBehavior.Restrict);
        // ... FKs a Grupo y Dimension igual

        modelBuilder.Entity<IndicadorDtm>().HasIndex(p => new { p.IdGrupo, p.Orden })
            .HasDatabaseName($"I_{Tablas.INDICADOR}_{ICampos.ID_GRUPO}_{ICampos.ORDEN}");
    }
}
```

### 1.5 Registro en el contexto y migración

Fichero: `ServicioDeDatos\CreadorDelMd.cs`

```csharp
// en OnModelCreating, tras DefinirTablasDeGuarderias(modelBuilder);
DefinirTablasDeIndicadores(modelBuilder);

private void DefinirTablasDeIndicadores(ModelBuilder modelBuilder)
{
    ModeloDeIndicadores.GrupoDeIndicadores(modelBuilder);
    ModeloDeIndicadores.Dimension(modelBuilder);
    ModeloDeIndicadores.Indicador(modelBuilder);
    ModeloDeIndicadores.IndicadorAudt(modelBuilder);
    ModeloDeIndicadores.Valor(modelBuilder);
}
```

Migración, desde el proyecto `Migraciones`:

```powershell
dotnet ef migrations add "crear esquema INDICADOR" --project Migraciones
```

Revisar en el `.cs` generado que aparece `migrationBuilder.EnsureSchema("INDICADOR")` antes de los `CreateTable`.

Añadir el DDL de referencia en `ModeloDeDatos\BD\INDICADOR\Tables\*.sql` e incluirlo en `BD.sqlproj`.

---

## 2. Gestores y seguridad

### 2.1 Gestores

Fichero: `GestoresDeNegocio\Indicadores\GestorDe*.cs`. Sigue el patrón de `GestoresDeNegocio\Callejero\GestorDeProvincias.cs`: un `Profile` anidado de AutoMapper, `AplicarJoins` y un `AntesDePersistir`.

| Gestor | Responsabilidades especiales |
|---|---|
| `GestorDeGruposDeIndicadores` | Crear o renombrar el permiso de consulta (§2.2). En `AplicarFiltros`, filtrar por permiso (§2.3) |
| `GestorDeDimensionesDeIndicador` | Nada especial |
| `GestorDeIndicadores` | Validaciones (abajo). `AplicarJoins` con `Grupo`, `Dimension` y `Accion`. Filtrar por permiso del grupo |
| `GestorDeValoresDeIndicador` | Solo lectura desde la interfaz: insertar, modificar y borrar lo hace únicamente el trabajo. Filtrar por permiso del grupo del indicador |

Validaciones de `GestorDeIndicadores.AntesDePersistir`:

```csharp
if (r.Sentido == SentidoDeIndicador.Ascendente && r.UmbralVerde <= r.UmbralRojo)
    GestorDeErrores.Emitir("Si mayor es mejor, el umbral verde debe ser mayor que el rojo");
if (r.Sentido == SentidoDeIndicador.Descendente && r.UmbralVerde >= r.UmbralRojo)
    GestorDeErrores.Emitir("Si menor es mejor, el umbral verde debe ser menor que el rojo");
if (r.Periodicidad == PeriodicidadDeIndicador.Diaria && r.DiaPeriodo != null) r.DiaPeriodo = null;
if (r.Periodicidad == PeriodicidadDeIndicador.Semanal && (r.DiaPeriodo is null or < 1 or > 7))
    GestorDeErrores.Emitir("Indique el día de la semana (1 = lunes … 7 = domingo)");
if (r.Periodicidad == PeriodicidadDeIndicador.Mensual && (r.DiaPeriodo is null or < 1 or > 31))
    GestorDeErrores.Emitir("Indique el día del mes (1 … 31)");
```

Registrar los gestores en `GestoresDeNegocio\ServiceExtensions.cs` (`AddScoped`) y los negocios en `Servicios\GestorDeElementos\NegociosDeSe.cs` (`CrearGestor`).

### 2.2 Permiso de consulta del grupo

Fichero: `ServicioDeDatos\Seguridad\Enumerados.cs`. Añadir `Indicador` al final de `enumClaseDePermiso`.

> No hace falta sembrar `SEGURIDAD.CLASE_PERMISO`: `GestorDePermisos.CrearPermisoFuncional` crea la clase si no existe (`GestorDeClaseDePermisos.Crear(clase)`).

Fichero: `GestoresDeNegocio\Indicadores\GestorDeGruposDeIndicadores.cs`. Se copia el patrón de `GestoresDeNegocio\Entorno\GestorDeVistasMvc.cs` (`AntesDePersistir` y `DespuesDePersistir`):

```csharp
protected override void AntesDePersistir(GrupoDeIndicadoresDtm grupo, ParametrosDeNegocio parametros)
{
    base.AntesDePersistir(grupo, parametros);
    if (parametros.Operacion == enumTipoOperacion.Insertar)
        grupo.IdPermisoConsulta = GestorDePermisos.CrearObtener(Contexto, grupo.Nombre, enumClaseDePermiso.Indicador).Id;
    if (parametros.Operacion == enumTipoOperacion.Modificar)
        grupo.IdPermisoConsulta = ((GrupoDeIndicadoresDtm)parametros.registroEnBd).IdPermisoConsulta;
}

protected override void DespuesDePersistir(GrupoDeIndicadoresDtm grupo, ParametrosDeNegocio parametros)
{
    base.DespuesDePersistir(grupo, parametros);
    if (parametros.Operacion == enumTipoOperacion.Modificar)
        GestorDePermisos.ModificarPermisoFuncional(Contexto, Mapeador, GestorDePermisos.LeerRegistroPorId(Contexto, grupo.IdPermisoConsulta), grupo.Nombre, enumClaseDePermiso.Indicador);
    if (parametros.Operacion == enumTipoOperacion.Eliminar)
        GestorDePermisos.Eliminar(Contexto, Mapeador, GestorDePermisos.LeerRegistroPorId(Contexto, grupo.IdPermisoConsulta));
}
```

El permiso queda con el nombre `INDICADOR: <nombre del grupo>`. Se asigna a roles o puestos desde los mantenimientos de seguridad que ya existen (rol → permisos, o permisos directos de un puesto).

### 2.3 Filtrado por permiso

Un usuario ve un grupo, sus indicadores y sus valores si:

```csharp
Contexto.DatosDeConexion.EsAdministrador || Contexto.TieneElPermiso(grupo.IdPermisoConsulta)
```

`TieneElPermiso` está en `Servicios\GestorDeElementos\ApiDePermisos.cs` y tiene caché sobre `UsuariosDeUnPermisoDtm`. Para listados conviene leer primero la lista de `IdPermisoConsulta` de todos los grupos, quedarse con los que el usuario tiene (`TieneAlgunPermiso` uno a uno o cacheado) y aplicar `consulta.Where(x => idsDeGruposVisibles.Contains(x.IdGrupo))`.

---

## 3. La acción de cálculo

### 3.1 Contrato

Cada indicador referencia una fila de `ENTORNO.ACCION` (`AccionDtm`, mantenida con `GestorDeAcciones`). El trabajo la ejecuta con el extensor que ya existe:

```csharp
var entrada = new Dictionary<string, object>
{
    { ltrDeIndicadores.IdIndicador, indicador.Id },
    { ltrDeIndicadores.Fecha, fechaDelDato },        // DateTime.Date
    { ltrDeIndicadores.IdDimension, indicador.IdDimension }
};
var salida = indicador.Accion.Ejecutar(contexto, indicador, enumNegocio.Indicador, entrada, parametros: null);
```

`ExtensorDeAcciones.Ejecutar` está en `Servicios\GestorDeElementos\Extensores\Entorno\ExtensorDeAcciones.cs`.

**Resultado esperado**, sea cual sea el tipo de acción: filas con estas tres columnas (los nombres son obligatorios):

| Columna | Tipo | Notas |
|---|---|---|
| `ID_AGRUPADOR` | int | 0 si la dimensión es *Sin agrupar* |
| `AGRUPADOR` | varchar(250) | Texto a mostrar |
| `VALOR` | decimal | |

| Clase de acción | Cómo se invoca | Dónde deja el resultado |
|---|---|---|
| **PA** | `EXEC <Esquema>.<Pa> @ID_INDICADOR, @FECHA, @ID_DIMENSION` | Primer result set → `Salida["Resultado"]` (`DataTable`) |
| **SQL** | El bloque `Sql`, con los parámetros `@ID_INDICADOR`, `@FECHA` e `@ID_DIMENSION` | Igual que PA |
| **DLL** | `public static void Metodo(EntornoDeUnaAccion entorno)` | El método deja `entorno.Salida["Resultado"] = List<ValorCalculado>` |

```csharp
public record ValorCalculado(int IdAgrupador, string Agrupador, decimal Valor);
```

> ⚠️ **SQL**: los parámetros **siempre** se pasan como `SqlParameter`; **nunca** se concatenan en el texto. Las acciones las define solo el administrador, pero aun así se recomienda **PA** (se versiona en `ModeloDeDatos\BD`, se prueba en SSMS y tiene plan de ejecución cacheado).

### 3.2 Implementar `EjecutarPa` y `EjecutarSql`

Fichero: `ServicioDeDatos\ContextoSe.cs`, clase `EntornoDeUnaAccion`. Hoy los dos métodos son **stubs** que devuelven `Salida` vacía. Hay que implementarlos de forma **genérica**, para que sirvan a cualquier acción y no solo a los indicadores. Se toma como base `PaSql.EjecutarPa` (`ServicioDeDatos\_Elemento\PaSql.cs`):

```csharp
public const string Resultado = nameof(Resultado);

private static Dictionary<string, object> EjecutarPa(EntornoDeUnaAccion entorno)
{
    entorno.Contexto.ValidarExistePa(entorno.Accion.Esquema, entorno.Accion.Pa);
    var parametros = ParametrosSql(entorno.Entrada);
    var texto = $"{entorno.Accion.Esquema}.{entorno.Accion.Pa} {string.Join(", ", parametros.Select(p => p.ParameterName))}";
    return Rellenar(entorno, texto, parametros);
}

private static Dictionary<string, object> EjecutarSql(EntornoDeUnaAccion entorno)
    => Rellenar(entorno, entorno.Accion.Sql, ParametrosSql(entorno.Entrada));

// IdIndicador -> @ID_INDICADOR (PascalCase a MAYUSCULAS_CON_GUION)
private static List<SqlParameter> ParametrosSql(Dictionary<string, object> entrada) => ...;

private static Dictionary<string, object> Rellenar(EntornoDeUnaAccion entorno, string texto, List<SqlParameter> parametros)
{
    using var command = entorno.Contexto.Database.GetDbConnection().CreateCommand();
    command.Transaction = entorno.Contexto.Database.CurrentTransaction?.GetDbTransaction();
    command.CommandText = texto;
    command.Parameters.AddRange(parametros.ToArray());
    var tabla = new DataTable();
    entorno.Contexto.IniciarTraza($"Accion_{entorno.Accion.Nombre}");
    try { new SqlDataAdapter((SqlCommand)command).Fill(tabla); }
    catch (Exception ex) { entorno.Contexto.AnotarExcepcion(ex); throw; }
    finally { entorno.Contexto.CerrarTraza(); }
    entorno.Salida[Resultado] = tabla;
    return entorno.Salida;
}
```

> El PA recibe **todos** los parámetros de `Entrada`, en el orden del diccionario. Por eso el contrato de §3.1 fija el orden: `@ID_INDICADOR`, `@FECHA`, `@ID_DIMENSION`.

### 3.3 Ejemplo: "Expedientes abiertos por provincia"

```sql
CREATE OR ALTER PROCEDURE INDICADOR.PA_EXPEDIENTES_ABIERTOS_POR_PROVINCIA
    @ID_INDICADOR INT,
    @FECHA DATE,
    @ID_DIMENSION INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  P.ID                   AS ID_AGRUPADOR,
            P.NOMBRE               AS AGRUPADOR,
            CAST(COUNT(E.ID) AS DECIMAL(18,6)) AS VALOR
    FROM    CALLEJERO.PROVINCIA P
    LEFT JOIN EXPEDIENTE.EXPEDIENTE E          -- ajustar a la tabla/columna real
           ON E.ID_PROVINCIA = P.ID
          AND E.FECHA_APERTURA <= @FECHA
          AND (E.FECHA_CIERRE IS NULL OR E.FECHA_CIERRE > @FECHA)
    GROUP BY P.ID, P.NOMBRE;
END
```

Pasos para dar de alta este indicador:

1. Crear el PA en BD y guardarlo en `ModeloDeDatos\BD\INDICADOR\StoredProcedures\`.
2. En *Acciones*, crear una acción de clase **PA**, con esquema `INDICADOR` y PA `PA_EXPEDIENTES_ABIERTOS_POR_PROVINCIA`. `GestorDeAcciones` valida que el PA exista.
3. Crear el indicador con la dimensión *Provincia*, sentido `D` (menos expedientes abiertos es mejor) y umbrales verde 50 y rojo 200.

> **Fecha del dato:** el PA debe calcular **la foto a `@FECHA`**, no "lo de hoy". Así se puede recalcular cualquier día pasado y el resultado es reproducible.

---

## 4. Trabajo nocturno

Fichero: `GestoresDeNegocio\Indicadores\TrabajosDeIndicadores.cs`. Sigue el patrón de `GestoresDeNegocio\Guarderias\TrabajosDeGuarderia.cs`.

```csharp
public enum enumTrabajosDeIndicadores
{
    [Description("Calcular indicadores del cuadro de mandos")]
    CalcularIndicadores
}

public class TrabajosDeIndicadores
{
    public static TrabajoDeUsuarioDtm SometerCalcularIndicadores(ContextoSe contexto)
    {
        var dll = Assembly.GetExecutingAssembly().GetName().Name;
        var clase = typeof(TrabajosDeIndicadores).FullName;
        var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeIndicadores.CalcularIndicadores.Descripcion(), dll, clase,
                                                       nameof(CalcularIndicadores), comunicarFin: false);
        var datosDeCreacion = new Dictionary<string, object>
        {
            { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Today.AddDays(1).AddHours(2) },   // 02:00
            { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400 }                                    // segundos
        };
        return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
    }

    public static void CalcularIndicadores(EntornoDeTrabajo entorno)
    {
        var contexto = entorno.contextoDelProceso;
        var parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
        var fechaDelDato = parametros.ContainsKey(ltrDeIndicadores.Fecha)
                         ? parametros.LeerValor<DateTime>(ltrDeIndicadores.Fecha).Date
                         : DateTime.Today.AddDays(-1);          // por defecto: foto al cierre de ayer

        var indicadores = contexto.Set<IndicadorDtm>().Include(i => i.Accion)
                                  .Where(i => i.Activo).OrderBy(i => i.IdGrupo).ThenBy(i => i.Orden).ToList();
        int ok = 0, ko = 0, omitidos = 0;
        var traza = entorno.CrearTraza($"Calculando indicadores a fecha {fechaDelDato:dd/MM/yyyy}");

        foreach (var indicador in indicadores)
        {
            if (!LeTocaHoy(indicador, fechaDelDato)) { omitidos++; continue; }
            entorno.ActualizarTraza(traza, $"Indicador {indicador.Nombre}");
            var tran = contexto.IniciarTransaccion();
            try
            {
                BorrarValores(contexto, indicador.Id, fechaDelDato);          // idempotente
                var filas = EjecutarAccion(contexto, indicador, fechaDelDato); // List<ValorCalculado>
                InsertarValores(contexto, indicador, fechaDelDato, filas);     // con el color calculado
                MarcarCalculado(contexto, indicador, error: null);
                contexto.Commit(tran);
                ok++;
            }
            catch (Exception exc)
            {
                contexto.Rollback(tran);
                entorno.AnotarError($"Indicador {indicador.Nombre}", exc);
                MarcarCalculado(contexto, indicador, error: exc.Message);     // fuera de la transacción revertida
                ko++;
            }
        }
        entorno.CrearTraza($"Calculados {ok}, con error {ko}, no tocaba {omitidos}");
    }
}
```

### 4.1 Reglas

**`LeTocaHoy(indicador, fecha)`:**

- `D`: siempre.
- `S`: si el día de la semana ISO de `fecha` (lunes = 1 … domingo = 7) es igual a `DiaPeriodo`.
- `M`: si `fecha.Day == Math.Min(DiaPeriodo, DateTime.DaysInMonth(fecha.Year, fecha.Month))`.

**`EjecutarAccion`:** si `Salida["Resultado"]` es un `DataTable`, se convierte a `List<ValorCalculado>` leyendo las columnas `ID_AGRUPADOR`, `AGRUPADOR` y `VALOR`. Si falta alguna, se lanza un error claro: *"La acción X no devuelve la columna ID_AGRUPADOR"*. Si ya es un `List<ValorCalculado>` (acción DLL), se usa tal cual.

**Color:**

```csharp
public static string Color(IndicadorDtm i, decimal valor) => i.Sentido == SentidoDeIndicador.Ascendente
    ? (valor >= i.UmbralVerde ? ColorDeIndicador.Verde : valor <= i.UmbralRojo ? ColorDeIndicador.Rojo : ColorDeIndicador.Naranja)
    : (valor <= i.UmbralVerde ? ColorDeIndicador.Verde : valor >= i.UmbralRojo ? ColorDeIndicador.Rojo : ColorDeIndicador.Naranja);
```

**Inserción:** para volúmenes grandes (calles), usar `SqlBulkCopy` sobre la conexión y la transacción del contexto, o `AddRange` + `SaveChanges` si son pocos cientos. Hay que evitar insertar fila a fila con el gestor genérico.

**Recalcular un día concreto:** como el trabajo lee el parámetro `Fecha`, basta con encolarlo a mano desde *Trabajos de usuario* con `Parametros = {"Fecha":"2026-10-15"}`.

### 4.2 Sometimiento inicial

Fichero: `Inicializador\Negocios\InzTrabajos.cs`

```csharp
public static void SometerTrabajos(ContextoSe contexto)
{
    ...
    TrabajosDeIndicadores.SometerCalcularIndicadores(contexto);
}
```

En el mismo inicializador, crear la dimensión *Sin agrupar*, y si se quiere también *Provincia* y *Municipio* con su `ID_NEGOCIO`.

---

## 5. Interfaz

### 5.1 Mantenimientos (administrador)

Menú nuevo **Indicadores**, colgando de *Configuración* (`Inicializador\Negocios\InzMenus.cs`), con estas opciones:

| Opción | Tipo | Notas |
|---|---|---|
| Grupos de indicadores | CRUD con alta/edición | Muestra el permiso generado (solo lectura) |
| Dimensiones | CRUD con alta/edición | Lista de valores de negocio para `ID_NEGOCIO` |
| Indicadores | CRUD con alta/edición | Listas de valores de grupo, dimensión y acción. Columnas del grid: grupo, nombre, periodicidad, último cálculo y **último error** (resaltado si no es null) |
| Valores de indicadores | CRUD de **visualización** | Filtros: indicador (lista), `FiltroEntreFechas` sobre FECHA y color. Columna de color con círculo |

Para cada uno se sigue la receta de `ImplementacionDeUnNegocio.md` §9 (o `ImplementacionDeUnCrudDeVisualizacion.md` para *Valores*):

- controlador `EntidadController<ContextoSe, XxxDtm, XxxDto>`;
- descriptor `DescriptorDeCrud<XxxDto>`;
- vista `CrudXxx.cshtml`;
- TS `Crud.CrudMnt`;
- `enumControladoresIndicadores` / `enumVistasIndicadores` en `Ayudas\Extensiones\Controladores.cs`;
- `InzVistas.CrearVistaSiNoExiste`;
- `InzMenus.CrearMenuSiNoExiste`;
- icono en `wwwroot\images\menu\`.

El permiso de cada vista se crea solo (`GestorDeVistasMvc.AntesDePersistir`). Se asigna únicamente a los roles de administración.

En el Dto de indicador, `Sentido` y `Periodicidad` se muestran como listas de valores fijas (Mayor es mejor / Menor es mejor; Diaria / Semanal / Mensual) y `DiaPeriodo` como numérico, visible según la periodicidad.

### 5.2 Página "Cuadro de mandos" (usuarios)

| Pieza | Fichero |
|---|---|
| Controlador | `SistemaDeElementos\Controllers\Indicadores\CuadroDeMandosController.cs` (hereda de `BaseController`) |
| Vista | `SistemaDeElementos\Views\Indicadores\CuadroDeMandos.cshtml` |
| Front | `SistemaDeElementos\wwwroot\ts\Indicadores\CuadroDeMandos.ts` (`namespace Indicadores`) |
| Servicio de lectura | `GestoresDeNegocio\Indicadores\ApiDelCuadroDeMandos.cs` (consultas Dapper o LINQ de solo lectura, con el filtro de §2.3) |
| Vista y menú | `InzVistas` + `InzMenus` (menú de primer nivel o bajo *Inicio*). Permiso de vista asignado a todos los roles que deban ver algún grupo |

**Endpoints (JSON):**

| Acción | Devuelve |
|---|---|
| `ObtenerGrupos()` | Grupos visibles: `[{id, nombre}]` ordenados por `ORDEN` |
| `ObtenerResumen(idGrupo, fecha?)` | Por cada indicador del grupo: `{id, nombre, unidad, decimales, fecha, verdes, naranjas, rojos, total, ultimoError}`. Si no se pasa fecha, se usa la **última fecha calculada de cada indicador** (`MAX(FECHA)`) |
| `ObtenerValores(idIndicador, fecha?)` | `{indicador, fecha, fechasDisponibles[], filas:[{idAgrupador, agrupador, valor, color}]}` |

Todos comprueban el permiso del grupo antes de devolver nada. Si el usuario no lo tiene, emiten `GestorDeErrores.Emitir("No tiene permiso…")`.

**Diseño de pantalla:**

```
┌ Cuadro de mandos ─────────────────────────────────────────────┐
│ [Ventas] [Logística] [Expedientes]            Fecha: [▼ 17/10] │
├───────────────────────────────────────────────────────────────┤
│ ┌ Expedientes abiertos ┐ ┌ Plazo medio (días) ┐ ┌ ... ┐        │
│ │ ●12  ●5  ●2   (19)   │ │ ●40  ●3  ●0        │ │     │        │
│ │ ▓▓▓▓▓▓▓▓▓░░░░▒▒      │ │ ▓▓▓▓▓▓▓▓▓▓▓▓░      │ │     │        │
│ └──────────────────────┘ └────────────────────┘ └─────┘        │
├───────────────────────────────────────────────────────────────┤
│ Expedientes abiertos · por Provincia · 17/10/2026              │
│ Agrupador ▲        Valor   Estado                              │
│ Madrid             215     ● rojo                              │
│ Toledo             120     ● naranja                           │
│ Cuenca              12     ● verde                             │
└───────────────────────────────────────────────────────────────┘
```

- Hay una pestaña por grupo y una tarjeta por indicador. La tarjeta muestra el recuento V/N/R y una barra apilada en SVG.
- Al pulsar una tarjeta se carga su tabla de agrupadores, ordenable por columna, con filtro de texto y semáforo.
- Si la dimensión tiene `ID_NEGOCIO`, el agrupador es un enlace a la ficha del elemento.
- Una tarjeta con `ultimoError` muestra un aviso ⚠️ con el texto del error, pero solo si el usuario es administrador.
- **Sin librerías nuevas:** se dibuja en SVG/HTML a mano, como `wwwroot\ts\PanelDeControl.ts`, que ya pinta barras y tartas.
- Colores: reutilizar las variables CSS del tema si existen. Si no, verde `#2e7d32`, naranja `#ef6c00` y rojo `#c62828`. El color se acompaña siempre del texto (verde/naranja/rojo) por accesibilidad.

---

## 6. Checklist de ficheros

| # | Fichero | Nuevo / Mod. |
|---|---|---|
| 1 | `ServicioDeDatos\_Elemento\Metadatos.cs` (Esquemas, Tablas, ICampos) | Mod. |
| 2 | `ServicioDeDatos\Indicadores\GrupoDeIndicadoresDtm.cs` | Nuevo |
| 3 | `ServicioDeDatos\Indicadores\DimensionDeIndicadorDtm.cs` | Nuevo |
| 4 | `ServicioDeDatos\Indicadores\IndicadorDtm.cs` (+ auditoría y constantes) | Nuevo |
| 5 | `ServicioDeDatos\Indicadores\ValorDeIndicadorDtm.cs` | Nuevo |
| 6 | `ServicioDeDatos\CreadorDelMd.cs` (`DefinirTablasDeIndicadores`) | Mod. |
| 7 | `Migraciones\Migrations\<fecha>_crear esquema INDICADOR.cs` | Nuevo |
| 8 | `ModeloDeDatos\BD\INDICADOR\Tables\*.sql`, `BD.sqlproj` | Nuevo / Mod. |
| 9 | `ServicioDeDatos\Seguridad\Enumerados.cs` (`enumClaseDePermiso.Indicador`) | Mod. |
| 10 | `ServicioDeDatos\ContextoSe.cs` (`EntornoDeUnaAccion.EjecutarPa/EjecutarSql`) | Mod. |
| 11 | `ModeloDeDto\Indicadores\*Dto.cs` (4 Dtos) | Nuevo |
| 12 | `GestoresDeNegocio\Indicadores\GestorDe*.cs` (4 gestores) | Nuevo |
| 13 | `GestoresDeNegocio\Indicadores\TrabajosDeIndicadores.cs` | Nuevo |
| 14 | `GestoresDeNegocio\Indicadores\ApiDelCuadroDeMandos.cs` | Nuevo |
| 15 | `GestoresDeNegocio\ServiceExtensions.cs` | Mod. |
| 16 | `Servicios\GestorDeElementos\NegociosDeSe.cs` + `enumNegocio` | Mod. |
| 17 | `Inicializador\Negocios\InzTrabajos.cs`, `InzVistas.cs`, `InzMenus.cs` | Mod. |
| 18 | `Ayudas\Extensiones\Controladores.cs` | Mod. |
| 19 | `SistemaDeElementos\Controllers\Indicadores\*.cs` (4 CRUD + cuadro) | Nuevo |
| 20 | `SistemaDeElementos\Descriptores\DescriptoresDeIndicadores\*.cs` | Nuevo |
| 21 | `SistemaDeElementos\Views\Indicadores\*.cshtml` | Nuevo |
| 22 | `SistemaDeElementos\wwwroot\ts\Indicadores\*.ts` | Nuevo |
| 23 | `SistemaDeElementos\wwwroot\images\menu\Indicadores.svg`, `CuadroDeMandos.svg` | Nuevo |

---

## 7. Cómo probarlo

1. Aplicar la migración y comprobar que existe el esquema `INDICADOR` con sus 4 tablas (+ auditoría).
2. Crear un grupo y comprobar que aparece en `SEGURIDAD.PERMISO` un permiso `INDICADOR: <grupo>` de clase `Indicador`.
3. Crear el PA de ejemplo (§3.3), su acción y un indicador diario.
4. Encolar a mano el trabajo *Calcular indicadores del cuadro de mandos* con `Parametros = {"Fecha":"<ayer>"}`. Revisar sus trazas y que `VALOR_INDICADOR` tiene una fila por provincia con el color correcto.
5. Lanzarlo **otra vez** con la misma fecha: debe haber el mismo número de filas (idempotencia).
6. Poner a propósito un PA que devuelva otra columna: el indicador debe quedar con `ULTIMO_ERROR` y el resto calcularse igual.
7. Con un usuario sin el permiso del grupo, el cuadro no muestra la pestaña. Al asignarle el permiso a su rol, aparece (puede requerir vaciar la caché de permisos).
8. Probar un indicador semanal con `DIA_PERIODO` = día de la semana de la fecha pasada, y otro distinto: el segundo debe contar como "no tocaba".

---

## 8. Problemas frecuentes

| Síntoma | Causa |
|---|---|
| *"La acción X no devuelve la columna ID_AGRUPADOR"* | El PA/SQL usa alias distintos. Los nombres de §3.1 son obligatorios |
| El PA recibe parámetros desordenados | El orden de `Entrada` debe ser `IdIndicador`, `Fecha`, `IdDimension` (§3.2) |
| El indicador no sale en el cuadro | Nadie tiene el permiso `INDICADOR: <grupo>`, el indicador está inactivo o aún no tiene valores |
| El trabajo no se ejecuta | `Cola_Activa` está desactivado, o el trabajo no está sometido (§4.2) |
| Todo sale rojo o todo verde | Los umbrales están invertidos respecto a `SENTIDO`. La validación de §2.1 debería impedirlo |
| Valores duplicados | Falta el índice único `(ID_INDICADOR, FECHA, ID_AGRUPADOR)` o el borrado previo no está en la misma transacción |
| El trabajo tarda mucho | El PA calcula sin índices adecuados. Revisar el plan en SSMS. Para dimensiones grandes, usar `SqlBulkCopy` |

---

## 9. Pendientes (v2)

- **Recalcular desde el CRUD:** una acción "Recalcular" en *Indicadores* que encole el trabajo con `{IdIndicador, Fecha}` (el trabajo ya admite `Fecha`; habría que añadir el filtro `IdIndicador`).
- **Histórico de ejecuciones:** tabla `INDICADOR.EJECUCION` (indicador, fecha, inicio, fin, filas, estado, error), en lugar de solo `ULTIMO_ERROR`.
- **Evolución temporal:** una gráfica SVG de un agrupador en los últimos N cálculos.
- **Dimensiones jerárquicas** (calle → municipio → provincia) para el drill-down.
- **Umbrales por agrupador:** tabla `INDICADOR.UMBRAL` que sobrescribe los del indicador.
- **Recolorear el histórico** tras cambiar los umbrales.
- **Tarjetas en el Panel de Control** de inicio con los indicadores en rojo del usuario.
- **Purga:** conservar el detalle N meses y después solo los totales.
