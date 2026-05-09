using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    [Table(Tablas.PLEITO, Schema = Esquemas.JURIDICO)]
    public class PleitoDtm : ElementoDeProcesoDtm, IUsaSolicitante, IPuedeUsarResponsable, IUsaAmpliaciones, IUsaDirecciones
    {
        public enumClaseDePleito ClaseDePleito { get; set; }
        public int IdSolicitante { get; set; }
        public int? IdResponsable { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }
        public new TipoDePleitoDtm Tipo { get; set; }
        public new EstadoDeUnPleitoDtm Estado { get; set; }
        public InterlocutorDtm Solicitante { get; set; }
        public UsuarioDtm Responsable { get ; set ; }
        public int? IdProcurador { get; set; }
        public int? IdAbogado { get; set; }
        public int? IdJuzgado { get; set; }
        public ProcuradorDtm Procurador { get; set; }
        public AbogadoDtm Abogado { get; set; }
        public JuzgadoDtm Juzgado { get; set; }

        public RecobroDtm Recobro { get; set; }
    }


    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.JURIDICO)]
    public class AuditoriaDeUnPleitoDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.JURIDICO)]
    public class ArchivosDeUnPleitoDtm : VinculoDtm
    {
        public PleitoDtm Pleito { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.JURIDICO)]
    public class AgendaDeUnPleitoDtm : VinculoDtm
    {
        public PleitoDtm Pleito { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.JURIDICO)]
    public class ObservacionesDeUnPleitoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pleito;
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.JURIDICO)]
    public class PermisoDelPleitoDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.JURIDICO)]
    public class TrazasDeUnPleitoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pleito;
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.JURIDICO)]
    public class DireccionDeUnPleitoDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pleito;
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.JURIDICO)]
    public class HitosDeUnPleitoDtm : HitoDtm
    {

    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.JURIDICO)]
    public class ArchivadoresDeUnPleitoDtm : VinculoDtm
    {
        public PleitoDtm Pleito { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.INTERLOCUTOR), Schema = Esquemas.JURIDICO)]
    public class InterlocutoresDeUnPleitoDtm : VinculoDtm
    {
        public PleitoDtm Pleito { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }


    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.REGISTRO), Schema = Esquemas.JURIDICO)]
    public class RegistrosDeUnPleitoDtm : VinculoDtm
    {
        public PleitoDtm Pleito { get; set; }
        public RegistroEsDtm Registro { get; set; }
    }


    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.TAREA), Schema = Esquemas.JURIDICO)]
    public class TareasDeUnPleitoDtm : VinculoDtm
    {
        public PleitoDtm Pleito { get; set; }
        public TareaDtm Tarea { get; set; }
    }

    public static partial class ModeloDePleito
    {

        public static void Pleito(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PleitoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<PleitoDtm>(modelBuilder, nameof(PleitoDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<PleitoDtm>(modelBuilder, nameof(PleitoDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<PleitoDtm>(modelBuilder, nameof(PleitoDtm.Estado));
            ApiDeElementoDtm.DefinirSolicitante<PleitoDtm>(modelBuilder);
            ApiDeUsuarioDtm.DefinirResponsable<PleitoDtm>(modelBuilder, false);
            modelBuilder.Entity<PleitoDtm>().Property(nameof(PleitoDtm.ClaseDePleito)).HasColumnName(ICampos.CLASE_PLEITO).HasColumnType(IDominio.VARCHAR_30).IsRequired();


            ApiDeElementoDtm.DefinirDependenciaDe<PleitoDtm>(modelBuilder, nameof(PleitoDtm.Procurador), nameof(PleitoDtm.IdProcurador), ICampos.ID_PROCURADOR, requerido:false, unico:false);
            ApiDeElementoDtm.DefinirDependenciaDe<PleitoDtm>(modelBuilder, nameof(PleitoDtm.Abogado), nameof(PleitoDtm.IdAbogado), ICampos.ID_ABOGADO, requerido: false, unico: false);
            ApiDeElementoDtm.DefinirDependenciaDe<PleitoDtm>(modelBuilder, nameof(PleitoDtm.Juzgado), nameof(PleitoDtm.IdJuzgado), ICampos.ID_JUZGADO, requerido: false, unico: false);

            modelBuilder.Entity<PleitoDtm>().Ignore(x => x.Recobro);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnPleitoDtm, PleitoDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnPleitoDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnPleitoDtm>(modelBuilder, nameof(ArchivosDeUnPleitoDtm.Pleito), nameof(ArchivosDeUnPleitoDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnPleitoDtm>(modelBuilder, nameof(AgendaDeUnPleitoDtm.Pleito), nameof(AgendaDeUnPleitoDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnPleitoDtm, PleitoDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelPleitoDtm, PleitoDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnPleitoDtm, PleitoDtm>(modelBuilder);            
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnPleitoDtm, PleitoDtm, EstadoDeUnPleitoDtm, TransicionesDeUnPleitoDtm, ObservacionesDeUnPleitoDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnPleitoDtm>(modelBuilder, nameof(ArchivadoresDeUnPleitoDtm.Pleito), nameof(ArchivadoresDeUnPleitoDtm.Archivador));
        }
        internal static void Interlocutores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<InterlocutoresDeUnPleitoDtm>(modelBuilder, nameof(InterlocutoresDeUnPleitoDtm.Pleito), nameof(InterlocutoresDeUnPleitoDtm.Interlocutor));

        }

        internal static void Tareas(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<TareasDeUnPleitoDtm>(modelBuilder, nameof(TareasDeUnPleitoDtm.Pleito), nameof(TareasDeUnPleitoDtm.Tarea));
        }

        internal static void Registros(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<RegistrosDeUnPleitoDtm>(modelBuilder, nameof(RegistrosDeUnPleitoDtm.Pleito), nameof(RegistrosDeUnPleitoDtm.Registro));
        }
    }
}
