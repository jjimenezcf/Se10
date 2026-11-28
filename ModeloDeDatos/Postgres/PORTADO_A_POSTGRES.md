# Versión PostgreSQL de Se10 — estado del port

Rama de trabajo: `ccr-e352e992-b669na` (parte de `main`; SQL Server sigue funcionando igual).

> **Importante:** este port se hizo en un entorno sin SDK de .NET (no se pudo compilar ni ejecutar C#).
> Los scripts SQL de `ModeloDeDatos/Postgres/` sí se han ejecutado contra PostgreSQL 16. El código C# está
> **sin compilar**: lo primero que hay que hacer es `dotnet build` y corregir lo que salga.

## Cómo se elige el proveedor

Por la cadena de conexión (`ConnectionStrings:CadenaDeConexion` en `appsettings.json`), sin ningún flag nuevo:

| Cadena | Proveedor |
|---|---|
| `Host=localhost;Port=5432;Database=se;Username=se;Password=...` | PostgreSQL (Npgsql) |
| `Server=...;Database=...;...` | SQL Server (como hasta ahora) |

Todo el código nuevo está en `ServicioDeDatos/ProveedorDeBd.cs`.

## Hecho

- **EF Core**: `UseNpgsql` / `UseSqlServer` según la cadena (`ContextoSe`, `Startup`, fábrica de diseño). Las migraciones de PostgreSQL van en el ensamblado nuevo `Migraciones.Postgres` (las de SQL Server no se tocan).
- **Modelo → PostgreSQL** (`ProveedorDeBd.AdaptarModeloAPostgres`, se llama al final de `OnModelCreating`): esquemas, tablas, vistas y columnas en **minúsculas** (así el SQL escrito a mano sin entrecomillar sigue valiendo) y traducción de tipos `BIT→boolean`, `DATETIME2→timestamp`, `VARCHAR(MAX)→text`, `varbinary(max)→bytea`, `UNIQUEIDENTIFIER→uuid`.
- **ConsultaSql / Dapper**: conexión y parámetros independientes del proveedor; traductor de los giros T-SQL más repetidos (`WITH(NOLOCK)`, `select top(1) 1`, CTE recursivas, `'%'+@x+'%'`).
- **PaSql, GestorDeMetadatos, BorrarBd, TrabajosDeAgenda, GestorDeMenus, MenuSql**: versión PostgreSQL.
- **Passwords cifradas**: `pgcrypto` en lugar de `ENCRYPTBYPASSPHRASE` (`ProveedorDeBd.SqlCifrar/SqlDescifrar`).
- **Clave duplicada**: se reconoce `23505` de PostgreSQL además de 2601/2627 de SQL Server.
- **Scripts** (`ModeloDeDatos/Postgres/`, probados en PG 16): `00_extensiones.sql`, `01_rowversion.sql` (RowVersion con trigger y secuencia), `02_menu.sql` (`ARBOL_MENU` y `MENU_SE`).

## Cómo crear la BD en PostgreSQL

```bash
# 1. appsettings.json con la cadena Host=...
# 2. generar la migración inicial de PostgreSQL (desde la carpeta del proyecto web, donde está appsettings.json)
cd SistemaDeElementos
dotnet ef migrations add InitialCreate -p ../Migraciones.Postgres -c ContextoSe
# 3. aplicar (lo hace Program.cs con AplicarMigraciones al arrancar) y después ejecutar, por este orden:
psql -f ../ModeloDeDatos/Postgres/00_extensiones.sql
psql -f ../ModeloDeDatos/Postgres/01_rowversion.sql
psql -f ../ModeloDeDatos/Postgres/02_menu.sql
```

## Pendiente (no se ha hecho; ordenado de más a menos bloqueante)

1. **Compilar y arreglar errores** (nadie lo ha compilado).
2. **Columnas calculadas con funciones** (`HasComputedColumnSql`): PostgreSQL no admite funciones de usuario ni subconsultas en columnas generadas. Afecta a ~13 ficheros: `InterlocutorDtm`, `PresupuestoDtm`, `UsuariosDeUnPermisoDtm`, `ModoDeAccesoAlNegocioDtm`, `ObservacionDtm`, `DireccionDtm`, `TrazaDtm`, `CalleDtm`, `CodigoPostalDtm`, `ExpedienteDtm`, `PuestoDtm`, `PermisosHeredadosDtm`, `PuestosDeUnUsuarioDtm`. Opciones: trigger que rellene la columna, o vista/consulta que la calcule.
3. **Funciones, vistas y PA en T-SQL** de `ModeloDeDatos/BD` (20 funciones, 5 vistas, 1 PA) y las llamadas `migrationBuilder.Sql(...)` de varios `*Dtm.cs`: hay que reescribirlas en PL/pgSQL.
4. **Mayúsculas/minúsculas y acentos**: SQL Server compara sin distinguir mayúsculas (`=`, `LIKE`); PostgreSQL sí las distingue. Los filtros y búsquedas del grid pueden cambiar de comportamiento. Solución habitual: collation ICU no determinista o `citext`, o `ILIKE` en el generador de filtros.
5. **PA de plantillas** (`PaSql.CrearPAParaDatosPlantilla`, `EjecutarPa`): devuelven varios conjuntos de resultados; en PostgreSQL requieren `refcursor`. Ahora lanzan un error claro en PostgreSQL.
6. **Resto de SQL a mano** con sintaxis T-SQL (`DECLARE @`, `TOP`, `OFFSET`, `DATEADD`, `STRING_AGG`...): `NegociosDeSe.cs`, `AuditoriaDtm.cs`, `VariableDtm.cs`, `LibroDtm.cs`, `TransicionDtm.cs`, `HitoDtm.cs`, `EstadoDtm.cs`, `AccionesDeTrnDtm.cs`, `TrabajoDeUsuarioDtm.cs`, `FiltrosDeParteTr.cs`, `ExtensorDeVinculos.cs` (estos dos usan el analizador T-SQL `ScriptDom`).
7. **Fechas**: `DateTimeOffset` se mapea a `timestamptz` y Npgsql solo acepta offset 0 (UTC). Revisar los puntos donde se guarden fechas con zona.
8. **Datos**: no hay migración de datos de SQL Server a PostgreSQL (solo esquema). Para pasar datos existentes usar `pgloader` o un volcado por tabla.
9. Los proyectos antiguos `GestorDeArchivos`, `GestorDeEntorno` y `GestorDeSeguridad` siguen con `EntityFrameworkCore.SqlServer 3.0.0` en su `.csproj`.
