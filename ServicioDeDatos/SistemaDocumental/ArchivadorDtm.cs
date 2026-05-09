using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{
    [Table(Tablas.ARCHIVADOR, Schema = Esquemas.SISDOC)]
    public class ArchivadorDtm : ElementoConCgDtm, IUsaBaja, IUsaObservacion, IUsaBloqueo
    {
        public bool Baja { get; set; }
        public string SincronizarCon { get; set; }
        public new TipoDeArchivadorDtm Tipo { get; set; }
        public bool Bloqueado { get; set; }
    }

    [Table(Tablas.ARCHIVADOR + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.SISDOC)]
    public class AuditoriaDeUnArchivadorDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.ARCHIVADOR + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.SISDOC)]
    public class ArchivosDeUnArchivadorDtm : VinculoDtm
    {
        public ArchivadorDtm Archivador { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.ARCHIVADOR + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.SISDOC)]
    public class ObservacionesDeUnArchivadorDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Archivador;
    }

    [Table(Tablas.ARCHIVADOR + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.SISDOC)]
    public class PermisoDelArchivadorDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.ARCHIVADOR + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.SISDOC)]
    public class TrazasDeUnArchivadorDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Archivador;
    }



    public static partial class ModeloDocumental
    {

        internal static string TablaArchivador => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(ArchivadorDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(ArchivadorDtm))}";

        public static void Archivador(ModelBuilder modelBuilder)
        {

            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ArchivadorDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<ArchivadorDtm>(modelBuilder, nameof(ArchivadorDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<ArchivadorDtm>(modelBuilder, nameof(ArchivadorDtm.Tipo));


            modelBuilder.Entity<ArchivadorDtm>().Property(p => p.SincronizarCon).HasColumnName(ICampos.SINCRONIZAR_CON).HasColumnType($"VARCHAR({250})").IsRequired(false);

        }

        internal static void AuditoriaDeUnArchivador(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnArchivadorDtm>(modelBuilder);
        }
        internal static void ArchivosDeUnArchivador(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnArchivadorDtm>(modelBuilder, nameof(ArchivosDeUnArchivadorDtm.Archivador), nameof(ArchivosDeUnArchivadorDtm.Archivo));
        }

        internal static void ObservacionesDeUnArchivador(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnArchivadorDtm, ArchivadorDtm>(modelBuilder);
        }

        internal static void PermisosPorArchivador(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelArchivadorDtm, ArchivadorDtm>(modelBuilder);
        }
        internal static void TrazasDeUnArchivador(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnArchivadorDtm, ArchivadorDtm>(modelBuilder);
        }

    }
}
