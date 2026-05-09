using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{
    public class ltrDeUnArchivo
    {
        public const string IdsDeArchivos = nameof(IdsDeArchivos);
        public const string NombreDeArchivo = nameof(NombreDeArchivo);
        public const string CorreoImportado = "Correo importado";
        public const string Migrado = "Migrado";
        public const string Exportacion = "Exportacion de datos";
        public const string ZipGenerado = "Zip generado";
        public const string FacturaEmitida = "SIF generada";
        public const string PendienteProcesar = "(pendiente)";
        public const string Posicion = nameof(Posicion);
        public static readonly string Sincronizar = nameof(Sincronizar);
        public static readonly string rutaDelArchivo = nameof(rutaDelArchivo);

        public static readonly string QuitarTodosLosAnexados = nameof(QuitarTodosLosAnexados);

        public static readonly string Accion_Marcar_Cancelacion = nameof(Accion_Marcar_Cancelacion);
        public static readonly string Accion_Anular_Cancelacion = nameof(Accion_Anular_Cancelacion);
        public static readonly string Accion_Permitir_Modificar_Nombre = nameof(Accion_Permitir_Modificar_Nombre);

    }

    public class ExtensionDeArchivo
    {
        public int IdArchivo { get; set; }
        public string Expresion { get; set; }
        public enumNegocio Negocio { get; set; }
        public int IdElemento { get; set; }
        public int? IdArchivador { get; set; }
        public int? IdCarpeta { get; set; }
        public CarpetaDtm Carpeta { get; set; }
        public ArchivoDtm Archivo { get; set; }
        public IElementoDtm Elemento { get; set; }

        public bool EstaAnexadoAUnArchivador => IdArchivador is not null;
        public bool EstaAnexadoAUnaCarpeta => IdCarpeta is not null;
    }

    [Table(Tablas.ARCHIVO, Schema = Esquemas.SISDOC)]
    public class ArchivoDtm : ElementoDtm
    {
        public string AlmacenadoEn { get; set; }
        public bool AnexadoAUnArchivador { get; set; }

        public string SeDecargado(string ruta) => System.IO.Path.Combine(ruta, $"{Id}.{enumExtensiones.se}");

    }
    [Table(Tablas.ARCHIVO_SINCRONIZADO, Schema = Esquemas.SISDOC)]
    public class ArchivoSincronizadoDtm : RegistroDtm
    {
        public int IdArchivo { get; set; }
        public string Ruta { get; set; }
        public string Propietario { get; set; }
        public DateTime CreadoEl { get; set; }
        public DateTime ModificadoEl { get; set; }
        public DateTime SincronizadoEl { get; set; }
        public long Longitud { get; set; }
    }
    public class ArchivoSincronizadoSql
    {
        private static string _crear = $@"
        insert into [esquema].[tabla] ({ICampos.ID_ARCHIVO}
                                     , {ICampos.RUTA}
                                     , {ICampos.PROPIETARIO}
                                     , {ICampos.CREADO_EL}
                                     , {ICampos.MODIFICADO_EL}
                                     , {ICampos.SINCRONIZADO_EL}
                                     , {ICampos.LONGITUD}) 
        values(@{ICampos.ID_ARCHIVO}
             ,  @{ICampos.RUTA}
             ,  @{ICampos.PROPIETARIO}
             ,  @{ICampos.CREADO_EL} 
             ,  @{ICampos.MODIFICADO_EL} 
             ,  @{ICampos.SINCRONIZADO_EL} 
             ,  @{ICampos.LONGITUD})
        ";

        private static string _quitar = $@"
        delete from [esquema].[tabla] 
        where {ICampos.ID_ARCHIVO} = @{ICampos.ID_ARCHIVO}
        ";

        private static string _actualizar = $@"
        update [esquema].[tabla] 
        set {ICampos.RUTA} = @{ICampos.RUTA}
          , {ICampos.SINCRONIZADO_EL} = @{ICampos.SINCRONIZADO_EL} 
        where {ICampos.ID_ARCHIVO} = @{ICampos.ID_ARCHIVO}
        ";

        private static string _leer = $@"
        select {ICampos.ID} as {nameof(ArchivoSincronizadoDtm.Id)}
        , {ICampos.ID_ARCHIVO} as {nameof(ArchivoSincronizadoDtm.IdArchivo)}
        , {ICampos.RUTA} as {nameof(ArchivoSincronizadoDtm.Ruta)}
        , {ICampos.PROPIETARIO} as {nameof(ArchivoSincronizadoDtm.Propietario)}
        , {ICampos.CREADO_EL} as {nameof(ArchivoSincronizadoDtm.CreadoEl)}
        , {ICampos.MODIFICADO_EL} as {nameof(ArchivoSincronizadoDtm.ModificadoEl)}
        , {ICampos.SINCRONIZADO_EL} as {nameof(ArchivoSincronizadoDtm.SincronizadoEl)}
        , {ICampos.LONGITUD} as {nameof(ArchivoSincronizadoDtm.Longitud)}
        from [esquema].[tabla] 
        where {ICampos.ID_ARCHIVO} = @{ICampos.ID_ARCHIVO}
        ";

        public static int Crear(ContextoSe contexto, ArchivoSincronizadoDtm info)
        {
            var parametrosSql = new Dictionary<string, object>();

            var sentenciaSql = _crear.Replace("[esquema].[tabla]", $"{ModeloDocumental.TablaArchivoSincronizado}");
            parametrosSql.Add($"@{ICampos.ID_ARCHIVO}", info.IdArchivo);
            parametrosSql.Add($"@{ICampos.RUTA}", info.Ruta);
            parametrosSql.Add($"@{ICampos.PROPIETARIO}", info.Propietario);
            parametrosSql.Add($"@{ICampos.CREADO_EL}", info.CreadoEl);
            parametrosSql.Add($"@{ICampos.MODIFICADO_EL}", info.ModificadoEl);
            parametrosSql.Add($"@{ICampos.SINCRONIZADO_EL}", info.CreadoEl);
            parametrosSql.Add($"@{ICampos.LONGITUD}", info.Longitud);

            var consulta = new ConsultaSql<ArchivoSincronizadoDtm>(contexto, sentenciaSql);
            return consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static int Quitar(ContextoSe contexto, int idArchivo)
        {
            var parametrosSql = new Dictionary<string, object>();

            var sentenciaSql = _quitar.Replace("[esquema].[tabla]", $"{ModeloDocumental.TablaArchivoSincronizado}");
            parametrosSql.Add($"@{ICampos.ID_ARCHIVO}", idArchivo);

            var consulta = new ConsultaSql<ArchivoSincronizadoDtm>(contexto, sentenciaSql);
            return consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }
        public static int Actualizar(ContextoSe contexto, int idArchivo, string ruta)
        {
            var parametrosSql = new Dictionary<string, object>();

            var sentenciaSql = _actualizar.Replace("[esquema].[tabla]", $"{ModeloDocumental.TablaArchivoSincronizado}");
            parametrosSql.Add($"@{ICampos.ID_ARCHIVO}", idArchivo);
            parametrosSql.Add($"@{ICampos.RUTA}", ruta);
            parametrosSql.Add($"@{ICampos.SINCRONIZADO_EL}", DateTime.UtcNow);

            var consulta = new ConsultaSql<ArchivoSincronizadoDtm>(contexto, sentenciaSql);
            return consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static ArchivoSincronizadoDtm Leer(ContextoSe contexto, int idArchivo, bool errorSiNoHay = true)
        {
            var parametrosSql = new Dictionary<string, object>();

            var sentenciaSql = _leer.Replace("[esquema].[tabla]", $"{ModeloDocumental.TablaArchivoSincronizado}");
            parametrosSql.Add($"@{ICampos.ID_ARCHIVO}", idArchivo);

            var consulta = new ConsultaSql<ArchivoSincronizadoDtm>(contexto, sentenciaSql);
            var leidos = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            if (leidos.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No hay registro de sincronización con el archivo: {idArchivo}");
            return leidos.Count == 0 ? null : leidos[0];
        }
    }

    public static partial class ModeloDocumental
    {
        public static string TablaArchivoSincronizado => ApiDeRegistroDtm.EsquemaTabla(typeof(ArchivoSincronizadoDtm));

        public static void Archivo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArchivoDtm>().Ignore(x => x.AnexadoAUnArchivador);

            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ArchivoDtm>(modelBuilder);

            modelBuilder.Entity<ArchivoDtm>().Property(x => x.AlmacenadoEn).HasColumnName(ICampos.RUTA).HasColumnType(IDominio.VARCHAR_2000).IsRequired();
        }

        public static void ArchivoSincronizado(ModelBuilder modelBuilder)
        {

            ApiDeRegistroDtm.DefinirCampoIdDtm<ArchivoSincronizadoDtm>(modelBuilder);

            modelBuilder.Entity<ArchivoSincronizadoDtm>().Property(x => x.IdArchivo).HasColumnName(ICampos.ID_ARCHIVO).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<ArchivoSincronizadoDtm>().Property(x => x.Ruta).HasColumnName(ICampos.RUTA).HasColumnType(IDominio.VARCHAR_MAX).IsRequired();
            modelBuilder.Entity<ArchivoSincronizadoDtm>().Property(x => x.Propietario).HasColumnName(ICampos.PROPIETARIO).HasColumnType(IDominio.VARCHAR_50).IsRequired();
            modelBuilder.Entity<ArchivoSincronizadoDtm>().Property(x => x.CreadoEl).HasColumnName(ICampos.CREADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<ArchivoSincronizadoDtm>().Property(x => x.ModificadoEl).HasColumnName(ICampos.MODIFICADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<ArchivoSincronizadoDtm>().Property(x => x.SincronizadoEl).HasColumnName(ICampos.SINCRONIZADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<ArchivoSincronizadoDtm>().Property(x => x.Longitud).HasColumnName(ICampos.LONGITUD).HasColumnType(IDominio.BIGINT).IsRequired();

            ApiDeRegistroDtm.DefinirFk<ArchivoSincronizadoDtm, ArchivoDtm>(modelBuilder, nameof(ArchivoSincronizadoDtm.IdArchivo), ICampos.ID_ARCHIVO, true);

        }
    }


}
