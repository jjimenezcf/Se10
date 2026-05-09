using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.PROCURADOR, Schema = Esquemas.TERCEROS)]
    public class ProcuradorDtm : ElementoDtm, IUsaTraza, IUsaBaja, IDatosDeContacto, IEsInterlocutor, IUsaDirecciones
    {
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }

    [Table(Tablas.PROCURADOR + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnProcuradorDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.PROCURADOR + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnProcuradorDtm : VinculoDtm
    {
        public ProcuradorDtm Procurador { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PROCURADOR + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnProcuradorDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Procurador;
    }

    [Table(Tablas.PROCURADOR + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnProcuradorDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Procurador;
    }


    [Table(Tablas.PROCURADOR + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeProcuradorDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Procurador;
    }

    public static partial class ModeloDeTerceros
    {
        public static void Procurador(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ProcuradorDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeRegistroDtm.DefinirDatosDeContacto<ProcuradorDtm>(modelBuilder);

            modelBuilder.Entity<ProcuradorDtm>().Property(x => x.IdInterlocutor).HasColumnName(ICampos.ID_INTERLOCUTOR).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirCampoFk<ProcuradorDtm>(modelBuilder
                , nameof(ProcuradorDtm.Interlocutor)
                , nameof(ProcuradorDtm.IdInterlocutor)
                , nameof(ICampos.ID_INTERLOCUTOR)
                , requerida:true
                , unico: true);
        }

        public static void ProcuradorAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnProcuradorDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnProcurador(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnProcuradorDtm>(modelBuilder, nameof(ArchivosDeUnProcuradorDtm.Procurador), nameof(ArchivosDeUnProcuradorDtm.Archivo));
        }

        internal static void ObservacionesDeUnProcurador(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnProcuradorDtm, ProcuradorDtm>(modelBuilder);
        }

        internal static void DireccionesDeUnProcurador(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeProcuradorDtm, ProcuradorDtm>(modelBuilder);
        }
        internal static void TrazasDeUnProcurador(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnProcuradorDtm, ProcuradorDtm>(modelBuilder);
        }


    }

}

