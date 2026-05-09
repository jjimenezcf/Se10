using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Juridico
{
    public static class ltrDeUnContrato
    {
        public static string FiltroPorResponsable = nameof(FiltroPorResponsable);
        public static string FiltroPorConOSinResponsable => nameof(FiltroPorConOSinResponsable);
        public static string ConOSinResponsable = nameof(ConOSinResponsable);
        public static string ConResponsable = nameof(ConResponsable);
        public static string SinResponsable = nameof(SinResponsable);

        public static string FiltrosPorEvolucion = nameof(FiltrosPorEvolucion);

        public const string IdExpediente = nameof(ContratoDtm.IdExpediente);
        public const string IdUnitario = nameof(IdUnitario);
        public const string IdPlfDeVenta = nameof(IdPlfDeVenta);
        public const string IdFacturaEmt = nameof(IdFacturaEmt);
        public const string IdParteTr = nameof(IdParteTr);


        public const string FiltroPorConOSinPlfDeVenta = nameof(FiltroPorConOSinPlfDeVenta);
        public const string FiltroPorConOSinFacturaEmt = nameof(FiltroPorConOSinFacturaEmt);
        public const string FiltroPorConOSinParteTr = nameof(FiltroPorConOSinParteTr);
        public const string FiltroPorConOSinExpediente = nameof(FiltroPorConOSinExpediente);

        public const string SelectorParaUnaPlvDeVenta = nameof(SelectorParaUnaPlvDeVenta);
        public const string SelectorParaUnParteTr = nameof(SelectorParaUnParteTr);
        public const string SelectorParaUnaAsignacionDeParte = nameof(SelectorParaUnaAsignacionDeParte);
        public const string SelectorParaFiltratFacturasEmt = nameof(SelectorParaFiltratFacturasEmt);
        public const string SelectorParaFiltrarFacturasRec = nameof(SelectorParaFiltrarFacturasRec);
        public const string SelectorParaUnaFacturaRec = nameof(SelectorParaUnaFacturaRec);
        public const string SelectorParaUnaFacturaEmt = nameof(SelectorParaUnaFacturaEmt);
        public const string SelectorParaUnPedido = nameof(SelectorParaUnPedido);

        public const string FiltroPorEtapas = nameof(FiltroPorEtapas);
        public const string FiltroPorEtapaDePlfsDeVenta = nameof(FiltroPorEtapaDePlfsDeVenta);
        public const string FiltroPorEtapaDePartesTr = nameof(FiltroPorEtapaDePartesTr);

        public const string NecesitaFiltrarPorClase = nameof(NecesitaFiltrarPorClase);
    }

    [Table(Tablas.CONTRATO, Schema = Esquemas.JURIDICO)]
    public class ContratoDtm : ElementoDeProcesoDtm, IPuedeUsarResponsable, IUsaAmpliaciones, IUsaDetalles, IUsaExpediente, IUsaDirecciones
    {
        public enumClaseDeContrato ClaseDeContrato { get; set; }
        public int? IdResponsable { get; set; }
        public new TipoDeContratoDtm Tipo { get; set; }
        public new EstadoDeUnContratoDtm Estado { get; set; }
        public UsuarioDtm Responsable { get ; set ; }
        public int IdAgenda { get; set; }
        public int? IdExpediente { get; set; }
        public ExpedienteDtm Expediente { get; set; }
        public DatosDelContratoDtm Datos { get; set; }
        public SaldosDelContratoDtm Saldos { get; set; }
        public AvalSolicitadoDtm Aval { get; set; }
        public ProrrogaDtm Prorroga { get; set; }

        public AvanceDtm Avance { get; set; }
        public MatriculaDeGuarderiaDtm MatriculaDeGuarderia{ get; set; }
        public string MiAgenda => $"Agenda del Contrato: {Referencia}";

        public string LaClaseDelContrato => ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia ? "la matrícula" : "el contrato";
        public string DeLaClaseDeContrato => ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia ? "de la matrícula" : "del contrato";


    }


    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.JURIDICO)]
    public class AuditoriaDeUnContratoDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.JURIDICO)]
    public class ArchivosDeUnContratoDtm : VinculoDtm
    {
        public ContratoDtm Contrato { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.JURIDICO)]
    public class AgendaDeUnContratoDtm : VinculoDtm
    {
        public ContratoDtm Contrato { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.JURIDICO)]
    public class ObservacionesDeUnContratoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.JURIDICO)]
    public class PermisoDelContratoDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.JURIDICO)]
    public class TrazasDeUnContratoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.JURIDICO)]
    public class DireccionDeUnContratoDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.JURIDICO)]
    public class HitosDeUnContratoDtm : HitoDtm
    {

    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.JURIDICO)]
    public class ArchivadoresDeUnContratoDtm : VinculoDtm
    {
        public ContratoDtm Contrato { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.INTERLOCUTOR), Schema = Esquemas.JURIDICO)]
    public class InterlocutoresDeUnContratoDtm : VinculoDtm
    {
        public ContratoDtm Contrato { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }


    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.REGISTRO), Schema = Esquemas.JURIDICO)]
    public class RegistrosDeUnContratoDtm : VinculoDtm
    {
        public ContratoDtm Contrato { get; set; }
        public RegistroEsDtm Registro { get; set; }
    }


    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.TAREA), Schema = Esquemas.JURIDICO)]
    public class TareasDeUnContratoDtm : VinculoDtm
    {
        public ContratoDtm Contrato { get; set; }
        public TareaDtm Tarea { get; set; }
    }

    public static partial class ModeloDeContrato
    {

        public static void Contrato(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ContratoDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeElementoDtm.DefinirCampoCg<ContratoDtm>(modelBuilder, nameof(ContratoDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<ContratoDtm>(modelBuilder, nameof(ContratoDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<ContratoDtm>(modelBuilder, nameof(ContratoDtm.Estado));
            ApiDeUsuarioDtm.DefinirResponsable<ContratoDtm>(modelBuilder, false);
            ModeloDeExpediente.DefinirExpediente<ContratoDtm>(modelBuilder, false);

            modelBuilder.Entity<ContratoDtm>().Property(p => p.IdAgenda).HasColumnName(ICampos.ID_AGENDA).HasColumnType(IDominio.INT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<ContratoDtm, AgendaDtm>(modelBuilder, nameof(ContratoDtm.IdAgenda), ICampos.ID_AGENDA, unico: true);

            modelBuilder.Entity<ContratoDtm>().Property(nameof(ContratoDtm.ClaseDeContrato)).HasColumnName(ICampos.CLASE_CONTRATO).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            //ApiDeRegistroDtm.DefinirFkSobreUnaAmpliacion<ContratoDtm, DatosDelContratoDtm>(modelBuilder, nameof(ContratoDtm.Datos));

            modelBuilder.Entity<ContratoDtm>().Ignore(x => x.Datos);
            modelBuilder.Entity<ContratoDtm>().Ignore(x => x.Saldos);
            modelBuilder.Entity<ContratoDtm>().Ignore(x => x.Aval);
            modelBuilder.Entity<ContratoDtm>().Ignore(x => x.Prorroga);
            modelBuilder.Entity<ContratoDtm>().Ignore(x => x.Avance);
            modelBuilder.Entity<ContratoDtm>().Ignore(x => x.MatriculaDeGuarderia);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnContratoDtm, ContratoDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnContratoDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnContratoDtm>(modelBuilder, nameof(ArchivosDeUnContratoDtm.Contrato), nameof(ArchivosDeUnContratoDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnContratoDtm>(modelBuilder, nameof(AgendaDeUnContratoDtm.Contrato), nameof(AgendaDeUnContratoDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnContratoDtm, ContratoDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelContratoDtm, ContratoDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnContratoDtm, ContratoDtm>(modelBuilder);            
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnContratoDtm, ContratoDtm, EstadoDeUnContratoDtm, TransicionesDeUnContratoDtm, ObservacionesDeUnContratoDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnContratoDtm>(modelBuilder, nameof(ArchivadoresDeUnContratoDtm.Contrato), nameof(ArchivadoresDeUnContratoDtm.Archivador));
        }
        internal static void Interlocutores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<InterlocutoresDeUnContratoDtm>(modelBuilder, nameof(InterlocutoresDeUnContratoDtm.Contrato), nameof(InterlocutoresDeUnContratoDtm.Interlocutor));

        }

        internal static void Tareas(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<TareasDeUnContratoDtm>(modelBuilder, nameof(TareasDeUnContratoDtm.Contrato), nameof(TareasDeUnContratoDtm.Tarea));
        }

        internal static void Registros(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<RegistrosDeUnContratoDtm>(modelBuilder, nameof(RegistrosDeUnContratoDtm.Contrato), nameof(RegistrosDeUnContratoDtm.Registro));
        }
    }
}
