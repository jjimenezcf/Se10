using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{
    [Table("CARPETA", Schema = "SISDOC")]
    public class CarpetaDtm : RegistroConNombreDtm
    {
        public int IdArchivador { get; set; }
        public int? IdPadre { get; set; }
        public ArchivadorDtm Archivador { get; set; }
        public CarpetaDtm Padre { get; set; }

    }

    public class NodoDeCarpetaDtm : NodoDtm
    {
        public int NumeroDeArchivos { get; set; }

    }


    [Table("CARPETA_" + nameof(Sufijo.ARCHIVO), Schema = "SISDOC")]
    public class ArchivosDeUnaCarpetaDtm : VinculoDtm
    {
        public CarpetaDtm Carpeta { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }


    public class CarpetaSql
    {

        private static string _filtroPorIdArchivador = $@"T1.{ICampos.ID_ARCHIVADOR} = @{ICampos.ID_ARCHIVADOR}";

        private static string _filtroPorNombre = $"AND T1.{ICampos.NOMBRE} LIKE '%'+@{ICampos.NOMBRE}+'%'";

        public static string JerarquiaDeCarpeta = $@"
                              WITH Carpetas 
                              AS
                              (
                                  SELECT T1.{ICampos.ID} AS id
                              		   , 0 as {ICampos.BAJA}                  
                              		   ,T1.{ICampos.NOMBRE}                  
                              		   ,T1.{ICampos.ID_PADRE}               
                              		   ,T1.{ICampos.ID_ARCHIVADOR}  
                                  FROM  {ModeloDocumental.TablaCarpeta} T1 WITH(NOLOCK)
                                  UNION ALL
                                  --RECURSIVIDAD
                                  SELECT T2.{ICampos.ID} AS id
                              		   , 0 as {ICampos.BAJA}                 
                              		   ,T2.{ICampos.NOMBRE}                   
                              		   ,T2.{ICampos.ID_PADRE}          
                              		   ,T2.{ICampos.ID_ARCHIVADOR}  
                                  FROM  {ModeloDocumental.TablaCarpeta} AS T2 WITH(NOLOCK)
                              	  JOIN Carpetas AS TP ON T2.{ICampos.ID_PADRE} = TP.id
                              )    
                              SELECT DISTINCT
                                    T2.{ICampos.NOMBRE}                   AS Padre
                                   ,T1.{ICampos.ID}                       AS Id
                              	   ,case when T1.{ICampos.BAJA} = 1                
                                         then 0                          
                                         else 1 end                      AS Activo
                              	   ,T1.{ICampos.NOMBRE}                  AS Nombre
                                   ,(SELECT COUNT(*) FROM SISDOC.CARPETA_ARCHIVO CA WHERE CA.{ICampos.ID_ELEMENTO1} = T1.ID) AS {nameof(NodoDeCarpetaDtm.NumeroDeArchivos)}
								   ,T1.{ICampos.ID_PADRE}                AS IdPadre
                              	   ,'{typeof(CarpetaDtm).FullName}'      AS TipoDtm
                              FROM Carpetas T1
                              LEFT JOIN {ModeloDocumental.TablaCarpeta} T2 WITH(NOLOCK) ON T2.ID = T1.{ICampos.ID_PADRE}
                              WHERE [{nameof(_filtroPorIdArchivador)}]
                              [{nameof(_filtroPorNombre)}]
							  order by Padre, Nombre
                              ";

        public static string AplicarFiltros(string sentenciaSql, int idArchivador, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = filtrarPorArchivador(sentenciaSql, idArchivador, parametrosSql);
            sentenciaSql = filtrarPorNombre(sentenciaSql, filtros, parametrosSql);
            return sentenciaSql;
        }
        private static string filtrarPorArchivador(string sentenciaSql, int idArchivador, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorIdArchivador)}]", $"{_filtroPorIdArchivador}");
            parametrosSql.Add($"@{ICampos.ID_ARCHIVADOR}", idArchivador);
            return sentenciaSql;
        }
        private static string filtrarPorNombre(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (!filtros.ContainsKey(nameof(INombre.Nombre).ToLower()))
                filtros.Add(nameof(INombre.Nombre).ToLower(), "");

            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorNombre)}]", !filtros[nameof(INombre.Nombre).ToLower()].ToString().IsNullOrEmpty() ? $"{_filtroPorNombre}" : "");
            if (!filtros[nameof(INombre.Nombre).ToLower()].ToString().IsNullOrEmpty())
                parametrosSql.Add($"@{ICampos.NOMBRE}", filtros[nameof(INombre.Nombre).ToLower()].ToString());
            return sentenciaSql;
        }
    }


    public static partial class ModeloDocumental
    {
        internal static string TablaCarpeta => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(CarpetaDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(CarpetaDtm))}";

        public static void Carpeta(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<CarpetaDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<CarpetaDtm>(modelBuilder);

            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(CarpetaDtm));

            modelBuilder.Entity<CarpetaDtm>().Property(p => p.IdArchivador).HasColumnName(ICampos.ID_ARCHIVADOR).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<CarpetaDtm>().Property(p => p.IdPadre).HasColumnName(ICampos.ID_PADRE).HasColumnType("INT").IsRequired(false);

            modelBuilder.Entity<CarpetaDtm>().HasIndex(p => new { p.IdArchivador, p.IdPadre, p.Nombre })
                .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_ARCHIVADOR}_{ICampos.ID_PADRE}_{ICampos.NOMBRE}")
                .HasFilter(null)
                .IsUnique(true);

            ApiDeRegistroDtm.DefinirFk<CarpetaDtm>(modelBuilder, nameof(CarpetaDtm.Archivador), nameof(CarpetaDtm.IdArchivador), ICampos.ID_ARCHIVADOR, unico: false);
            ApiDeRegistroDtm.DefinirFk<CarpetaDtm>(modelBuilder, nameof(CarpetaDtm.Padre), nameof(CarpetaDtm.IdPadre), ICampos.ID_PADRE, unico: false);
        }

        internal static void ArchivosDeUnaCarpeta(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaCarpetaDtm>(modelBuilder, nameof(ArchivosDeUnaCarpetaDtm.Carpeta), nameof(ArchivosDeUnaCarpetaDtm.Archivo));
        }
    }


}
