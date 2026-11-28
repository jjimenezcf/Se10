using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace ServicioDeDatos
{
    public enum TipoDeProveedor { SqlServer, PostgreSql }

    /// <summary>
    /// Punto único donde se decide si la aplicación habla con SQL Server o con PostgreSQL.
    /// El proveedor se deduce de la cadena de conexión:
    ///   - "Host=...;Database=...;Username=...;Password=..."  => PostgreSQL (Npgsql)
    ///   - "Server=...;Database=...;..."                      => SQL Server
    /// </summary>
    public static class ProveedorDeBd
    {
        public static TipoDeProveedor Detectar(string cadenaDeConexion)
        {
            if (string.IsNullOrWhiteSpace(cadenaDeConexion)) return TipoDeProveedor.SqlServer;

            var c = cadenaDeConexion.Replace(" ", "").ToLowerInvariant();
            return c.Contains("host=") || c.Contains("username=") || c.Contains("port=5432")
                 ? TipoDeProveedor.PostgreSql
                 : TipoDeProveedor.SqlServer;
        }

        /// <summary>Proveedor de la cadena de appsettings.json (para código estático sin contexto a mano)</summary>
        public static bool EsPostgresActual => _esPostgresActual.Value;
        private static readonly Lazy<bool> _esPostgresActual = new Lazy<bool>(() => EsPostgres(ContextoSe.ObtenerDatosDeConexion().CadenaConexion));

        /// <summary>
        /// Traduce a PostgreSQL los giros de T-SQL más repetidos en las sentencias que la aplicación compone a mano
        /// (MenuSql, filtros, etc.). Es deliberadamente corto: lo que no cubre se porta en su origen. Ver
        /// Documentacion/Postgres/PORTADO_A_POSTGRES.md
        /// </summary>
        public static string TraducirSql(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return sql;
            const RegexOptions o = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            sql = Regex.Replace(sql, @"\bWITH\s*\(\s*NOLOCK\s*\)", "", o);                                  // pistas de bloqueo
            sql = Regex.Replace(sql, @"\bselect\s+top\s*\(\s*1\s*\)\s+1\b", "select 1", o);              // exists (select top(1) 1 ...)
            sql = Regex.Replace(sql, @"^(\s*)WITH\s+(?!RECURSIVE\b)(\w+)\s+AS\b", "$1WITH RECURSIVE $2 AS", o | RegexOptions.Multiline); // CTE
            sql = Regex.Replace(sql, @"'%'\s*\+\s*(@\w+)\s*\+\s*'%'", "'%' || $1 || '%'", o);                // LIKE '%'+@x+'%'
            return sql;
        }

        public static bool EsPostgres(string cadenaDeConexion) => Detectar(cadenaDeConexion) == TipoDeProveedor.PostgreSql;

        /// <summary>
        /// Configura EF Core con el proveedor que corresponda a la cadena.
        /// Las migraciones de PostgreSQL viven en el ensamblado "Migraciones.Postgres".
        /// </summary>
        public static DbContextOptionsBuilder Usar(this DbContextOptionsBuilder opciones, string cadenaDeConexion, bool conMigraciones = false)
        {
            if (EsPostgres(cadenaDeConexion))
            {
                opciones.UseNpgsql(cadenaDeConexion, x =>
                {
                    if (conMigraciones)
                    {
                        x.MigrationsHistoryTable("__Migraciones", "ENTORNO");
                        x.MigrationsAssembly("Migraciones.Postgres");
                    }
                });
            }
            else
            {
                opciones.UseSqlServer(cadenaDeConexion, x =>
                {
                    if (conMigraciones)
                    {
                        x.MigrationsHistoryTable("__Migraciones", "ENTORNO");
                        x.MigrationsAssembly("Migraciones");
                    }
                });
            }
            return opciones;
        }

        // Cifrado de las passwords guardadas en la BD (usuario de conexión y certificados).
        // SQL Server: ENCRYPTBYPASSPHRASE / DECRYPTBYPASSPHRASE.  PostgreSQL: extensión pgcrypto (pgp_sym_encrypt / pgp_sym_decrypt),
        // guardado en base64 porque la columna es VARCHAR. Requiere: CREATE EXTENSION IF NOT EXISTS pgcrypto;
        public static string SqlCifrar(string expresionTexto)
            => EsPostgresActual
             ? $"encode(pgp_sym_encrypt({expresionTexto}, 'sistemaSe'), 'base64')"
             : $"ENCRYPTBYPASSPHRASE('sistemaSe', {expresionTexto})";

        public static string SqlDescifrar(string columna)
            => EsPostgresActual
             ? $"pgp_sym_decrypt(decode({columna}, 'base64'), 'sistemaSe')"
             : $"CONVERT(VARCHAR , DECRYPTBYPASSPHRASE('sistemaSe', {columna}))";

        // El resultado de cifrar es binario en SQL Server: para devolverlo en un SELECT a una propiedad string
        public static string SqlCifradoComoTexto(string expresionTexto)
            => EsPostgresActual
             ? SqlCifrar(expresionTexto)
             : $"CONVERT(VARCHAR , {SqlCifrar(expresionTexto)})";

        public static DbConnection CrearConexion(string cadenaDeConexion)
            => EsPostgres(cadenaDeConexion)
             ? new NpgsqlConnection(cadenaDeConexion)
             : new SqlConnection(cadenaDeConexion);

        public static DbParameter CrearParametro(string cadenaDeConexion, string nombre, object valor)
        {
            var nombreSinArroba = nombre.TrimStart('@');
            return EsPostgres(cadenaDeConexion)
                 ? new NpgsqlParameter(nombreSinArroba, valor ?? DBNull.Value)
                 : new SqlParameter("@" + nombreSinArroba, valor ?? DBNull.Value);
        }

        public static DbParameter CrearParametro(this DbContext contexto, string nombre, object valor)
            => CrearParametro(contexto.Database.GetConnectionString(), nombre, valor);

        public static bool EsPostgres(this DbContext contexto) => contexto.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

        /// <summary>
        /// Adapta el modelo (pensado para SQL Server) a PostgreSQL. Se llama al final de OnModelCreating, de modo que
        /// no hay que tocar los cientos de HasColumnName / HasColumnType del ServicioDeDatos:
        ///  1. Esquemas, tablas, vistas y columnas en minúsculas. El modelo los declara en mayúsculas (ENTORNO.USUARIO, ID, ...)
        ///     y todo el SQL escrito a mano de la aplicación los usa sin entrecomillar; PostgreSQL pliega a minúsculas lo no
        ///     entrecomillado, así que creando todo en minúsculas ese SQL sigue valiendo.
        ///  2. Tipos de SQL Server que no existen en PostgreSQL (BIT, DATETIME2, VARCHAR(MAX), ...).
        /// </summary>
        public static void AdaptarModeloAPostgres(ModelBuilder modelBuilder)
        {
            foreach (var entidad in modelBuilder.Model.GetEntityTypes())
            {
                var tabla = entidad.GetTableName();
                if (tabla != null)
                {
                    entidad.SetTableName(tabla.ToLowerInvariant());
                    var esquema = entidad.GetSchema();
                    if (esquema != null) entidad.SetSchema(esquema.ToLowerInvariant());
                }

                var vista = entidad.GetViewName();
                if (vista != null)
                {
                    entidad.SetViewName(vista.ToLowerInvariant());
                    var esquemaVista = entidad.GetViewSchema();
                    if (esquemaVista != null) entidad.SetViewSchema(esquemaVista.ToLowerInvariant());
                }

                foreach (var propiedad in entidad.GetDeclaredProperties())
                {
                    var nombre = propiedad.FindAnnotation(RelationalAnnotationNames.ColumnName)?.Value as string ?? propiedad.Name;
                    propiedad.SetColumnName(nombre.ToLowerInvariant());

                    var tipo = propiedad.FindAnnotation(RelationalAnnotationNames.ColumnType)?.Value as string;
                    if (tipo != null)
                        propiedad.SetColumnType(TraducirTipo(tipo));
                }
            }
        }

        public static string TraducirTipo(string tipo)
        {
            var t = tipo.Trim().ToUpperInvariant();
            if (t == "BIT") return "boolean";
            if (t.StartsWith("DATETIME2") || t == "DATETIME") return "timestamp without time zone";
            if (t == "VARCHAR(MAX)" || t == "NVARCHAR(MAX)") return "text";
            if (t == "VARBINARY(MAX)") return "bytea";
            if (t == "UNIQUEIDENTIFIER") return "uuid";
            if (t == "ROWVERSION" || t == "TIMESTAMP") return "bytea";
            return tipo;
        }
    }
}
