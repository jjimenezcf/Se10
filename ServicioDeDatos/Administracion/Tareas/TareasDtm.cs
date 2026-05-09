using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ServicioDeDatos.Tarea
{

    [Table(Tablas.TAREA, Schema = Esquemas.TAREA)]
    public class TareaDtm : ElementoDeProcesoDtm, IUsaSolicitante, IPuedeUsarResponsable, IUsaCalseDeElemento, IUsaDirecciones
    {
        public enumClaseDeTarea ClaseDeTarea { get; set; }
        public int IdSolicitante { get; set; }
        public int? IdResponsable { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }
        public int? IdArchivador { get; set; }
        public int? IdFacturaEmt { get; set; }

        public new TipoDeTareaDtm Tipo { get; set; }
        public new EstadoDeUnaTareaDtm Estado { get; set; }
        public InterlocutorDtm Solicitante { get; set; }
        public ArchivadorDtm Archivador { get; set; }
        public UsuarioDtm Responsable { get; set; }
        public PlfDeTareaDtm Planificacion { get; set; }
        public FacturaEmtDtm FacturaEmt { get; set; }
        public int? IdClaseDeElemento { get; set; }
        public ClaseDelNegocioDtm ClaseDeElemento { get; set; }
    }

    [Table(Tablas.TAREA + "_" + Sufijo.ESTADO, Schema = Esquemas.TAREA)]
    public class EstadoDeUnaTareaDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Tarea;
    }

    [Table(Tablas.TAREA + "_" + Sufijo.TRANSICION, Schema = Esquemas.TAREA)]
    public class TransicionesDeUnaTareaDtm : TransicionDtm
    {
    }

    [Table(Tablas.TAREA + "_" + Sufijo.ACCION, Schema = Esquemas.TAREA)]
    public class AccionesDeUnaTareaDtm : AccionesDeTrnDtm
    {
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TAREA)]
    public class AuditoriaDeUnaTareaDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TAREA)]
    public class ArchivosDeUnaTareaDtm : VinculoDtm
    {
        public TareaDtm Tarea { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TAREA)]
    public class ObservacionesDeUnaTareaDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Tarea;
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.TAREA)]
    public class PermisoDeLaTareaDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TAREA)]
    public class TrazasDeUnaTareaDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Tarea;
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TAREA)]
    public class DireccionDeUnaTareaDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Tarea;
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.TAREA)]
    public class HitosDeUnaTareaDtm : HitoDtm
    {

    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.TAREA)]
    public class ArchivadoresDeUnaTareaDtm : VinculoDtm
    {
        public TareaDtm Tarea { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.INTERLOCUTOR), Schema = Esquemas.TAREA)]
    public class InterlocutoresDeUnaTareaDtm : VinculoDtm
    {
        public TareaDtm Tarea { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }

    [Table(Tablas.TAREA + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.TAREA)]
    public class AgendaDeUnaTareaDtm : VinculoDtm
    {
        public TareaDtm Tarea { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    public static partial class ModeloDeTarea
    {
        internal static void EstadosDeUnaTarea(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaTareaDtm>(modelBuilder);
        }
        internal static void TransicionesDeUnaTarea(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaTareaDtm, EstadoDeUnaTareaDtm>(modelBuilder);
        }
        internal static void AccionesDeUnaTarea(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaTareaDtm, TransicionesDeUnaTareaDtm>(modelBuilder);
        }

        public static void Tarea(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TareaDtm>().Ignore(x => x.Planificacion);

            ApiDeElementoDtm.DefinirCamposDelElementoDtm<TareaDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<TareaDtm>(modelBuilder, nameof(TareaDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<TareaDtm>(modelBuilder, nameof(TareaDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<TareaDtm>(modelBuilder, nameof(TareaDtm.Estado));
            ApiDeElementoDtm.DefinirSolicitante<TareaDtm>(modelBuilder);
            ApiDeUsuarioDtm.DefinirResponsable<TareaDtm>(modelBuilder, false);
            DefinirClaseDeTarea<TareaDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirDependenciaDe<TareaDtm>(modelBuilder, nameof(TareaDtm.Archivador), nameof(TareaDtm.IdArchivador), ICampos.ID_ARCHIVADOR, requerido: false, unico: false);
            ApiDeElementoDtm.DefinirDependenciaDe<TareaDtm>(modelBuilder, nameof(TareaDtm.FacturaEmt), nameof(TareaDtm.IdFacturaEmt), ICampos.ID_FACTURA_EMT, requerido: false, unico: false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaTareaDtm, TareaDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaTareaDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaTareaDtm>(modelBuilder, nameof(ArchivosDeUnaTareaDtm.Tarea), nameof(ArchivosDeUnaTareaDtm.Archivo));

        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaTareaDtm, TareaDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaTareaDtm, TareaDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnaTareaDtm, TareaDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaTareaDtm, TareaDtm, EstadoDeUnaTareaDtm, TransicionesDeUnaTareaDtm, ObservacionesDeUnaTareaDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaTareaDtm>(modelBuilder, nameof(ArchivadoresDeUnaTareaDtm.Tarea), nameof(ArchivadoresDeUnaTareaDtm.Archivador));
        }
        internal static void Interlocutores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<InterlocutoresDeUnaTareaDtm>(modelBuilder, nameof(InterlocutoresDeUnaTareaDtm.Tarea), nameof(InterlocutoresDeUnaTareaDtm.Interlocutor));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnaTareaDtm>(modelBuilder, nameof(AgendaDeUnaTareaDtm.Tarea), nameof(AgendaDeUnaTareaDtm.Evento));
        }
    }

}
