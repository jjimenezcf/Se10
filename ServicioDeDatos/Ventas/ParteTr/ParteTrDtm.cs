using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.PARTE_TR, Schema = Esquemas.VENTA)]
    public class ParteTrDtm : ElementoDeProcesoDtm, IUsaCliente, IUsaDirecciones
    {
        public int IdCliente { get; set; }
        public ClienteDtm Cliente { get; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }

        public new TipoDeParteTrDtm Tipo { get; set; }
        public new EstadoDeUnParteTrDtm Estado { get; set; }

        public decimal Total { get; }
        public int? IdPresupuesto { get; set; }
        public PresupuestoDtm Presupuesto {get; set;}

        public int? IdFacturaEmt { get; set; }
        public FacturaEmtDtm FacturaEmt { get; set; }

        public int? IdContrato { get; set; }
        public ContratoDtm Contrato { get; set; }
    }


    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.VENTA)]
    public class AuditoriaDeUnParteTrDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.VENTA)]
    public class ArchivosDeUnParteTrDtm : VinculoDtm
    {
        public ParteTrDtm ParteTr { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.VENTA)]
    public class AgendaDeUnParteTrDtm : VinculoDtm
    {
        public ParteTrDtm ParteTr { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.VENTA)]
    public class ObservacionesDeUnParteTrDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.ParteDeTrabajo;
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.VENTA)]
    public class PermisoDelParteTrDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.VENTA)]
    public class TrazasDeUnParteTrDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.ParteDeTrabajo;
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.VENTA)]
    public class DireccionDeUnParteTrDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.ParteDeTrabajo;
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.VENTA)]
    public class HitosDeUnParteTrDtm : HitoDtm
    {
    }

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.VENTA)]
    public class ArchivadoresDeUnParteTrDtm : VinculoDtm
    {
        public ParteTrDtm ParteTr { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.TAREA), Schema = Esquemas.VENTA)]
    public class TareasDeUnParteTrDtm : VinculoDtm
    {
        public ParteTrDtm ParteTr { get; set; }
        public TareaDtm Tarea { get; set; }
    }

    public static partial class ModeloDeParteTr
    {

        public static void ParteTr(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ParteTrDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<ParteTrDtm>(modelBuilder, nameof(ParteTrDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<ParteTrDtm>(modelBuilder, nameof(ParteTrDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<ParteTrDtm>(modelBuilder, nameof(ParteTrDtm.Estado));
            ApiDeElementoDtm.DefinirCliente<ParteTrDtm>(modelBuilder);

            ApiDeRegistroDtm.DefinirDependencia<ParteTrDtm, ContratoDtm>(modelBuilder, apuntadoPor: nameof(ParteTrDtm.IdContrato), idCampo: ICampos.ID_CONTRATO, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<ParteTrDtm, PresupuestoDtm>(modelBuilder, apuntadoPor: nameof(ParteTrDtm.IdPresupuesto), idCampo: ICampos.ID_PRESUPUESTO, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<ParteTrDtm, FacturaEmtDtm>(modelBuilder, apuntadoPor: nameof(ParteTrDtm.IdFacturaEmt), idCampo: ICampos.ID_FACTURA_EMT, requerido: false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnParteTrDtm, ParteTrDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnParteTrDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnParteTrDtm>(modelBuilder, nameof(ArchivosDeUnParteTrDtm.ParteTr), nameof(ArchivosDeUnParteTrDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnParteTrDtm>(modelBuilder, nameof(AgendaDeUnParteTrDtm.ParteTr), nameof(AgendaDeUnParteTrDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnParteTrDtm, ParteTrDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelParteTrDtm, ParteTrDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnParteTrDtm, ParteTrDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnParteTrDtm, ParteTrDtm, EstadoDeUnParteTrDtm, TransicionesDeUnParteTrDtm, ObservacionesDeUnParteTrDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnParteTrDtm>(modelBuilder, nameof(ArchivadoresDeUnParteTrDtm.ParteTr), nameof(ArchivadoresDeUnParteTrDtm.Archivador));
        }

        internal static void Tareas(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<TareasDeUnParteTrDtm>(modelBuilder, nameof(TareasDeUnParteTrDtm.ParteTr), nameof(TareasDeUnParteTrDtm.Tarea));
        }

    }
}
