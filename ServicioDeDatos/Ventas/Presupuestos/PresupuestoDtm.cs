using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Presupuesto
{

    [Table(Tablas.PRESUPUESTO, Schema = Esquemas.PRESUPUESTO)]
    public class PresupuestoDtm : ElementoDeProcesoDtm, IUsaSolicitante, IPuedeUsarResponsable, IUsaExpediente, IUsaAmpliaciones, IUsaDirecciones
    {
        public enumClaseDePresupuesto ClaseDePresupuesto { get; set; }
        public int IdSolicitante { get; set; }
        public int? IdResponsable { get; set; }
        public int? IdExpediente { get; set; }    
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }
        public new TipoDePresupuestoDtm Tipo { get; set; }
        public new EstadoDeUnPresupuestoDtm Estado { get; set; }
        public InterlocutorDtm Solicitante { get; set; }
        public UsuarioDtm Responsable { get ; set ; }
        public ExpedienteDtm Expediente { get; set; }
        public decimal Total { get; }

        public PptDeVentaDtm DatosPropuestos { get; set; }  
    }


    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.PRESUPUESTO)]
    public class AuditoriaDeUnPresupuestoDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.PRESUPUESTO)]
    public class ArchivosDeUnPresupuestoDtm : VinculoDtm
    {
        public PresupuestoDtm Presupuesto { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.PRESUPUESTO)]
    public class AgendaDeUnPresupuestoDtm : VinculoDtm
    {
        public PresupuestoDtm Presupuesto { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.PRESUPUESTO)]
    public class ObservacionesDeUnPresupuestoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Presupuesto;
    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.PRESUPUESTO)]
    public class PermisoDelPresupuestoDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.PRESUPUESTO)]
    public class TrazasDeUnPresupuestoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Presupuesto;
    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.PRESUPUESTO)]
    public class DireccionDeUnPresupuestoDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Presupuesto;
    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.PRESUPUESTO)]
    public class HitosDeUnPresupuestoDtm : HitoDtm
    {

    }

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.PRESUPUESTO)]
    public class ArchivadoresDeUnPresupuestoDtm : VinculoDtm
    {
        public PresupuestoDtm Presupuesto { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.TAREA), Schema = Esquemas.PRESUPUESTO)]
    public class TareasDeUnPresupuestoDtm : VinculoDtm
    {
        public PresupuestoDtm Presupuesto { get; set; }
        public TareaDtm Tarea { get; set; }
    }

    public static partial class ModeloDePresupuesto
    {

        public static void Presupuesto(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PresupuestoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<PresupuestoDtm>(modelBuilder, nameof(PresupuestoDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<PresupuestoDtm>(modelBuilder, nameof(PresupuestoDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<PresupuestoDtm>(modelBuilder, nameof(PresupuestoDtm.Estado));
            ApiDeElementoDtm.DefinirSolicitante<PresupuestoDtm>(modelBuilder);
            ApiDeUsuarioDtm.DefinirResponsable<PresupuestoDtm>(modelBuilder, false);
            ModeloDeExpediente.DefinirExpediente<PresupuestoDtm>(modelBuilder, false);
            modelBuilder.Entity<PresupuestoDtm>().Property(nameof(PresupuestoDtm.ClaseDePresupuesto)).HasColumnName(ICampos.CLASE_PPT).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            modelBuilder.Entity<PresupuestoDtm>().Property(x => x.Total).HasColumnName(ICampos.TOTAL).HasColumnType(IDominio.DECIMAL)
                .HasComputedColumnSql($"{Esquemas.PRESUPUESTO}.{Funciones.CC_TOTAL_PPT}({ICampos.ID})");
            
            modelBuilder.Entity<PresupuestoDtm>().Ignore(x => x.DatosPropuestos);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnPresupuestoDtm, PresupuestoDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnPresupuestoDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnPresupuestoDtm>(modelBuilder, nameof(ArchivosDeUnPresupuestoDtm.Presupuesto), nameof(ArchivosDeUnPresupuestoDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnPresupuestoDtm>(modelBuilder, nameof(AgendaDeUnPresupuestoDtm.Presupuesto), nameof(AgendaDeUnPresupuestoDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnPresupuestoDtm, PresupuestoDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelPresupuestoDtm, PresupuestoDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnPresupuestoDtm, PresupuestoDtm>(modelBuilder);            
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnPresupuestoDtm, PresupuestoDtm, EstadoDeUnPresupuestoDtm, TransicionesDeUnPresupuestoDtm, ObservacionesDeUnPresupuestoDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnPresupuestoDtm>(modelBuilder, nameof(ArchivadoresDeUnPresupuestoDtm.Presupuesto), nameof(ArchivadoresDeUnPresupuestoDtm.Archivador));
        }

        internal static void Tareas(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<TareasDeUnPresupuestoDtm>(modelBuilder, nameof(TareasDeUnPresupuestoDtm.Presupuesto), nameof(TareasDeUnPresupuestoDtm.Tarea));
        }

    }
}
