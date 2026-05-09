using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.TRABAJADOR, Schema = Esquemas.TERCEROS)]
    public class TrabajadorDtm : ElementoDtm, IUsaTraza, IUsaBaja, IDatosDeContacto, IUsaCg, IPermisosPorCg, IEsInterlocutor, IUsaDirecciones
    {
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
        public int IdCuenta { get; set; }
        public int? IdUsuario { get; set; }
        public UsuarioDtm Usuario { get; set; }

        public CuentaDtm Cuenta { get; set; }
        public int IdCg { get; set; }

        public CentroGestorDtm Cg { get; set; }
    }

    [Table(Tablas.TRABAJADOR + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnTrabajadorDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.TRABAJADOR + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnTrabajadorDtm : VinculoDtm
    {
        public TrabajadorDtm Trabajador { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.TRABAJADOR + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnTrabajadorDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Trabajador;
    }

    [Table(Tablas.TRABAJADOR + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnTrabajadorDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Trabajador;
    }


    [Table(Tablas.TRABAJADOR + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeTrabajadorDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Trabajador;
    }


    [Table(Tablas.TRABAJADOR + "_" + nameof(Sufijo.CIRCUITO_DOC), Schema = Esquemas.TERCEROS)]
    public class CircuitoDocDeUnTrabajadorDtm : VinculoDtm
    {
        public TrabajadorDtm Trabajador { get; set; }
        public CircuitoDocDtm Circuito { get; set; }
    }


    public static partial class ModeloDeTerceros
    {
        public static void Trabajador(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<TrabajadorDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeRegistroDtm.DefinirDatosDeContacto<TrabajadorDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirCampoFk<TrabajadorDtm>(modelBuilder, nameof(TrabajadorDtm.Interlocutor), nameof(TrabajadorDtm.IdInterlocutor), ICampos.ID_INTERLOCUTOR, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TrabajadorDtm>(modelBuilder, nameof(TrabajadorDtm.Cuenta), nameof(TrabajadorDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);

            ApiDeElementoDtm.DefinirCampoCg<TrabajadorDtm>(modelBuilder, nameof(TrabajadorDtm.Cg));

            ApiDeRegistroDtm.DefinirCampoFk<TrabajadorDtm>(modelBuilder, nameof(TrabajadorDtm.Usuario), nameof(TrabajadorDtm.IdUsuario), ICampos.ID_USUARIO, requerida: false, unico: true);
        }

        public static void TrabajadorAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnTrabajadorDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnTrabajador(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnTrabajadorDtm>(modelBuilder, nameof(ArchivosDeUnTrabajadorDtm.Trabajador), nameof(ArchivosDeUnTrabajadorDtm.Archivo));
        }

        internal static void ObservacionesDeUnTrabajador(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnTrabajadorDtm, TrabajadorDtm>(modelBuilder);
        }

        internal static void DireccionesDeUnTrabajador(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeTrabajadorDtm, TrabajadorDtm>(modelBuilder);
        }
        internal static void TrazasDeUnTrabajador(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnTrabajadorDtm, TrabajadorDtm>(modelBuilder);
        }

        internal static void CircuitosCadDeUnTrabajador(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<CircuitoDocDeUnTrabajadorDtm>(modelBuilder, nameof(CircuitoDocDeUnTrabajadorDtm.Trabajador), nameof(CircuitoDocDeUnTrabajadorDtm.Circuito));
        }


    }

}

