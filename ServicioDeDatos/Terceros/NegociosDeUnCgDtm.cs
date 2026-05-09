using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.CENTRO_GESTOR_NEGOCIO, Schema = Esquemas.TERCEROS)]
    public class NegociosDeUnCgDtm : RegistroDtm, ICamposCG, ITieneCampoNegocio , ITienePermisoDeConsultor, ITienePermisoDeGestor
    {
        public int IdCg { get; set; }
        public int IdNegocio { get; set; }
        public int IdConsultor { get; set; }
        public int IdGestor { get; set; }

        public CentroGestorDtm CentroGestor { get; }
        public NegocioDtm Negocio { get;  }

        public PermisoDtm Consultor { get; }
        public PermisoDtm Gestor { get;  }

    }

    public static partial class ModeloDeTerceros
    {
        internal static string TablaNegociosDeUnCg => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(NegociosDeUnCgDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(NegociosDeUnCgDtm))}";
        public static void NegociosDeUnCentroGestor(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<NegociosDeUnCgDtm>(modelBuilder);

            ApiDeRegistroDtm.DefinirCampoFk<NegociosDeUnCgDtm>(modelBuilder, nameof(NegociosDeUnCgDtm.Consultor), nameof(NegociosDeUnCgDtm.IdConsultor), ICampos.ID_CONSULTOR, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<NegociosDeUnCgDtm>(modelBuilder, nameof(NegociosDeUnCgDtm.Gestor), nameof(NegociosDeUnCgDtm.IdGestor), ICampos.ID_GESTOR, requerida: true, unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<NegociosDeUnCgDtm>(modelBuilder, nameof(NegociosDeUnCgDtm.CentroGestor), nameof(NegociosDeUnCgDtm.IdCg), ICamposCG.ID_CG, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<NegociosDeUnCgDtm>(modelBuilder, nameof(NegociosDeUnCgDtm.Negocio), nameof(NegociosDeUnCgDtm.IdNegocio), ICampos.ID_NEGOCIO, requerida: true, unico: false);
        }
    }
}
