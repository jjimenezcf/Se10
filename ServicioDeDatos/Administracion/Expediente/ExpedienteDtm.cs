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

namespace ServicioDeDatos.Expediente
{
    public static class ltrDeUnExpediente
    {
        public const string IdExpediente = nameof(IdExpediente);
        public const string IdCliente = nameof(ExpedienteDtm.IdSolicitante);
        public const string IdResponsable = nameof(ExpedienteDtm.IdResponsable);
        public const string IdTarea = nameof(IdTarea);
        public const string IdPresupuesto = nameof(IdPresupuesto);
        public const string IdContrato = nameof(IdContrato);
        public const string DatosContacto = nameof(DatosContacto);
        public const string ValoradoEn = nameof(ExpedienteDtm.ValoradoEn);
        public const string PresupuestoHijo = nameof(PresupuestoHijo);
        public const string ExpedientesConPpts = nameof(ExpedientesConPpts);
        public const string ExpedientesConTareas = nameof(ExpedientesConTareas);
        public const string ExpedienteConValoracion = nameof(ExpedienteConValoracion);
        public const string ExpedienteConDatosJuridicos = nameof(ExpedienteConDatosJuridicos);
        public const string ExpedienteConContrato = nameof(ExpedienteConContrato);
        public const string TiposDePptsAsociados = nameof(TiposDePptsAsociados);
        public const string ExpedientePadre = nameof(ExpedientePadre);
        public const string AsociarTareasDelExpediente = nameof(AsociarTareasDelExpediente);

        public const string SelectorParaUnaFacturaRec = nameof(SelectorParaUnaFacturaRec);
        public const string SelectorParaFiltrarFacturasRec = nameof(SelectorParaFiltrarFacturasRec);
        public const string SelectorParaUnPedido = nameof(SelectorParaUnPedido);

        public const string ExpedieteNoJuridico = nameof(ExpedieteNoJuridico);
    }


    [Table(Tablas.EXPEDIENTE, Schema = Esquemas.EXPEDIENTE)]
    public class ExpedienteDtm : ElementoDeProcesoDtm, IUsaSolicitante, IPuedeUsarResponsable, IUsaAmpliaciones, IUsaDirecciones
    {
        public enumClaseDeExpediente ClaseDeExpediente { get; set; }
        public int IdSolicitante { get; set; }
        public int? IdResponsable { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }
        public new TipoDeExpedienteDtm Tipo { get; set; }
        public new EstadoDeUnExpedienteDtm Estado { get; set; }
        public InterlocutorDtm Solicitante { get; set; }
        public UsuarioDtm Responsable { get; set; }

        public decimal ValoradoEn { get ;}
        public DatosJuridicosDtm DatosJuridicos { get; set; }
    }


    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.EXPEDIENTE)]
    public class AuditoriaDeUnExpedienteDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.EXPEDIENTE)]
    public class ArchivosDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.EXPEDIENTE), Schema = Esquemas.EXPEDIENTE)]
    public class ExpedientesDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public ExpedienteDtm Relacionado { get; set; }
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.EXPEDIENTE)]
    public class AgendaDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.EXPEDIENTE)]
    public class ObservacionesDeUnExpedienteDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Expediente;
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.EXPEDIENTE)]
    public class PermisoDelExpedienteDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.EXPEDIENTE)]
    public class TrazasDeUnExpedienteDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Expediente;
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.EXPEDIENTE)]
    public class DireccionDeUnExpedienteDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Expediente;
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.EXPEDIENTE)]
    public class HitosDeUnExpedienteDtm : HitoDtm
    {

    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.EXPEDIENTE)]
    public class ArchivadoresDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.INTERLOCUTOR), Schema = Esquemas.EXPEDIENTE)]
    public class InterlocutoresDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }


    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.REGISTRO), Schema = Esquemas.EXPEDIENTE)]
    public class RegistrosDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public RegistroEsDtm Registro { get; set; }
    }


    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.TAREA), Schema = Esquemas.EXPEDIENTE)]
    public class TareasDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public TareaDtm Tarea { get; set; }
    }


    [Table(Tablas.EXPEDIENTE + "_" + nameof(Sufijo.CIRCUITO_DOC), Schema = Esquemas.EXPEDIENTE)]
    public class CircuitoDocDeUnExpedienteDtm : VinculoDtm
    {
        public ExpedienteDtm Expediente { get; set; }
        public CircuitoDocDtm Circuito { get; set; }
    }


    public static partial class ModeloDeExpediente
    {

        public static void Expediente(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ExpedienteDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<ExpedienteDtm>(modelBuilder, nameof(ExpedienteDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<ExpedienteDtm>(modelBuilder, nameof(ExpedienteDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<ExpedienteDtm>(modelBuilder, nameof(ExpedienteDtm.Estado));
            ApiDeElementoDtm.DefinirSolicitante<ExpedienteDtm>(modelBuilder);
            ApiDeUsuarioDtm.DefinirResponsable<ExpedienteDtm>(modelBuilder, false);
            modelBuilder.Entity<ExpedienteDtm>().Property(nameof(ExpedienteDtm.ClaseDeExpediente)).HasColumnName(ICampos.CLASE_EXPEDIENTE).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            modelBuilder.Entity<ExpedienteDtm>().Property(x => x.ValoradoEn).HasColumnName(ICampos.VALORADO_EN).HasColumnType(IDominio.DECIMAL)
                .HasComputedColumnSql($"{Esquemas.EXPEDIENTE}.{Funciones.CC_VALORADO_EN}({ICampos.ID})");

            modelBuilder.Entity<ExpedienteDtm>().Ignore(x => x.DatosJuridicos);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnExpedienteDtm, ExpedienteDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnExpedienteDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnExpedienteDtm>(modelBuilder, nameof(ArchivosDeUnExpedienteDtm.Expediente), nameof(ArchivosDeUnExpedienteDtm.Archivo));

        }
        internal static void Expedientes(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ExpedientesDeUnExpedienteDtm>(modelBuilder, nameof(ExpedientesDeUnExpedienteDtm.Expediente), nameof(ExpedientesDeUnExpedienteDtm.Relacionado));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnExpedienteDtm>(modelBuilder, nameof(AgendaDeUnExpedienteDtm.Expediente), nameof(AgendaDeUnExpedienteDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnExpedienteDtm, ExpedienteDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelExpedienteDtm, ExpedienteDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnExpedienteDtm, ExpedienteDtm>(modelBuilder);            
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnExpedienteDtm, ExpedienteDtm, EstadoDeUnExpedienteDtm, TransicionesDeUnExpedienteDtm, ObservacionesDeUnExpedienteDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnExpedienteDtm>(modelBuilder, nameof(ArchivadoresDeUnExpedienteDtm.Expediente), nameof(ArchivadoresDeUnExpedienteDtm.Archivador));
        }
        internal static void Interlocutores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<InterlocutoresDeUnExpedienteDtm>(modelBuilder, nameof(InterlocutoresDeUnExpedienteDtm.Expediente), nameof(InterlocutoresDeUnExpedienteDtm.Interlocutor));

        }

        internal static void Tareas(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<TareasDeUnExpedienteDtm>(modelBuilder, nameof(TareasDeUnExpedienteDtm.Expediente), nameof(TareasDeUnExpedienteDtm.Tarea));
        }

        internal static void Registros(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<RegistrosDeUnExpedienteDtm>(modelBuilder, nameof(RegistrosDeUnExpedienteDtm.Expediente), nameof(RegistrosDeUnExpedienteDtm.Registro));
        }

        internal static void DefinirExpediente<T>(ModelBuilder modelBuilder, bool obligatorio) where T : ElementoDtm
        {
            modelBuilder.Entity<T>().Property(nameof(IUsaExpediente.IdExpediente)).HasColumnName(ICampos.ID_EXPEDIENTE).HasColumnType(IDominio.INT).IsRequired(obligatorio);
            ApiDeRegistroDtm.DefinirFk<T>(modelBuilder, nameof(IUsaExpediente.Expediente), nameof(IUsaExpediente.IdExpediente), ICampos.ID_EXPEDIENTE, unico: false);
        }


        internal static void CircuitosCadDeUnExpediente(ModelBuilder modelBuilder)
        {
           ApiDeVinculos.DefinirCampos<CircuitoDocDeUnExpedienteDtm>(modelBuilder, nameof(CircuitoDocDeUnExpedienteDtm.Expediente), nameof(CircuitoDocDeUnTrabajadorDtm.Circuito));
        }


    }
}
