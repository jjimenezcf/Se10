using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.ABOGADO, Schema = Esquemas.TERCEROS)]
    public class AbogadoDtm : ElementoDtm, IUsaTraza, IUsaBaja, IDatosDeContacto, IEsInterlocutor, IUsaDirecciones
    {
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }

    [Table(Tablas.ABOGADO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnAbogadoDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.ABOGADO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnAbogadoDtm : VinculoDtm
    {
        public AbogadoDtm Abogado { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.ABOGADO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnAbogadoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Abogado;
    }

    [Table(Tablas.ABOGADO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnAbogadoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Abogado;
    }


    [Table(Tablas.ABOGADO + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeAbogadoDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Abogado;
    }

    public static partial class ModeloDeTerceros
    {
        public static void Abogado(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<AbogadoDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeRegistroDtm.DefinirDatosDeContacto<AbogadoDtm>(modelBuilder);

            modelBuilder.Entity<AbogadoDtm>().Property(x => x.IdInterlocutor).HasColumnName(ICampos.ID_INTERLOCUTOR).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirCampoFk<AbogadoDtm>(modelBuilder
                , nameof(AbogadoDtm.Interlocutor)
                , nameof(AbogadoDtm.IdInterlocutor)
                , nameof(ICampos.ID_INTERLOCUTOR)
                , requerida:true
                , unico: true);
        }

        public static void AbogadoAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnAbogadoDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnAbogado(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnAbogadoDtm>(modelBuilder, nameof(ArchivosDeUnAbogadoDtm.Abogado), nameof(ArchivosDeUnAbogadoDtm.Archivo));
        }

        internal static void ObservacionesDeUnAbogado(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnAbogadoDtm, AbogadoDtm>(modelBuilder);
        }

        internal static void DireccionesDeUnAbogado(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeAbogadoDtm, AbogadoDtm>(modelBuilder);
        }
        internal static void TrazasDeUnAbogado(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnAbogadoDtm, AbogadoDtm>(modelBuilder);
        }


    }

}

