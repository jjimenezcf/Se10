using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.RegistroEs
{
    [Table(Tablas.REGISTRO, Schema = Esquemas.REGISTRO)]
    public class RegistroEsDtm : ElementoDeProcesoDtm, IUsaSolicitante, IUsaDirecciones
    {
      
        public string ClaseDeRegistro { get; set; }
        public int IdSolicitante { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }
        public int? IdArchivadorDeEntrada { get; set; }
        public int? IdArchivadorDeSalida { get; set; }
        public int? IdArchivadorInterno { get; set; }
        public new TipoDeRegistroEsDtm Tipo { get; set; }
        public new EstadoDeUnRegistroEsDtm Estado { get; set; }
        public InterlocutorDtm Solicitante { get; set; }
        public ArchivadorDtm ArchivadorDeEntrada { get; set; }
        public ArchivadorDtm ArchivadorDeSalida { get; set; }
        public ArchivadorDtm ArchivadorInterno { get; set; }       
    }

    [Table(Tablas.REGISTRO + "_" + Sufijo.ESTADO, Schema = Esquemas.REGISTRO)]
    public class EstadoDeUnRegistroEsDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Registro;
    }

    [Table(Tablas.REGISTRO + "_" + Sufijo.TRANSICION, Schema = Esquemas.REGISTRO)]
    public class TransicionesDeUnRegistroEsDtm : TransicionDtm
    {
    }

    [Table(Tablas.REGISTRO + "_" + Sufijo.ACCION, Schema = Esquemas.REGISTRO)]
    public class AccionesDeUnRegistroEsDtm : AccionesDeTrnDtm
    {
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.REGISTRO)]
    public class AuditoriaDeUnRegistroEsDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.REGISTRO)]
    public class ArchivosDeUnRegistroEsDtm : VinculoDtm
    {
        public RegistroEsDtm Registro { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.REGISTRO)]
    public class AgendaDeUnRegistroEsDtm : VinculoDtm
    {
        public RegistroEsDtm Registro { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.REGISTRO)]
    public class ObservacionesDeUnRegistroEsDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Registro;
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.REGISTRO)]
    public class PermisoDelRegistroEsDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.REGISTRO)]
    public class TrazasDeUnRegistroEsDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Registro;
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.REGISTRO)]
    public class DireccionDeUnRegistroEsDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Registro;
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.REGISTRO)]
    public class HitosDeUnRegistroEsDtm : HitoDtm
    {

    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.REGISTRO)]
    public class ArchivadoresDeUnRegistroEsDtm : VinculoDtm
    {
        public RegistroEsDtm Registro { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.INTERLOCUTOR), Schema = Esquemas.REGISTRO)]
    public class InterlocutoresDeUnRegistroDtm : VinculoDtm
    {
        public RegistroEsDtm Registro { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }

    [Table(Tablas.REGISTRO + "_" + nameof(Sufijo.TAREA), Schema = Esquemas.REGISTRO)]
    public class TareasDeUnRegistroDtm : VinculoDtm
    {
        public RegistroEsDtm Registro { get; set; }
        public TareaDtm Tarea { get; set; }
    }


    public static partial class ModeloDeRegistroEs
    {
        internal static void EstadosDeUnRegistroEs(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnRegistroEsDtm>(modelBuilder);
        }
        internal static void TransicionesDeUnRegistroEs(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnRegistroEsDtm, EstadoDeUnRegistroEsDtm>(modelBuilder);
        }
        internal static void AccionesDeUnRegistroEs(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnRegistroEsDtm, TransicionesDeUnRegistroEsDtm>(modelBuilder);
        }

        public static void RegistroEs(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<RegistroEsDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<RegistroEsDtm>(modelBuilder, nameof(RegistroEsDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<RegistroEsDtm>(modelBuilder, nameof(RegistroEsDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<RegistroEsDtm>(modelBuilder, nameof(RegistroEsDtm.Estado));
            ApiDeElementoDtm.DefinirSolicitante<RegistroEsDtm>(modelBuilder);
            modelBuilder.Entity<RegistroEsDtm>().Property(p => p.ClaseDeRegistro).HasColumnName(ICampos.CLASE_ES).HasColumnType(IDominio.VARCHAR_1).IsRequired();
                      
            ApiDeElementoDtm.DefinirDependenciaDe<RegistroEsDtm>(modelBuilder, nameof(RegistroEsDtm.ArchivadorDeEntrada), nameof(RegistroEsDtm.IdArchivadorDeEntrada), ICampos.ID_ARCHIVADOR_ENTRADA, requerido: false, unico:true);
            ApiDeElementoDtm.DefinirDependenciaDe<RegistroEsDtm>(modelBuilder, nameof(RegistroEsDtm.ArchivadorDeSalida), nameof(RegistroEsDtm.IdArchivadorDeSalida), ICampos.ID_ARCHIVADOR_SALIDA, requerido: false, unico: true);
            ApiDeElementoDtm.DefinirDependenciaDe<RegistroEsDtm>(modelBuilder, nameof(RegistroEsDtm.ArchivadorInterno), nameof(RegistroEsDtm.IdArchivadorInterno), ICampos.ID_ARCHIVADOR_INTERNO, requerido: false, unico: true);
        }


        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnRegistroEsDtm, RegistroEsDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnRegistroEsDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnRegistroEsDtm>(modelBuilder, nameof(ArchivosDeUnRegistroEsDtm.Registro), nameof(ArchivosDeUnRegistroEsDtm.Archivo));

        }

        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnRegistroEsDtm>(modelBuilder, nameof(AgendaDeUnRegistroEsDtm.Registro), nameof(AgendaDeUnRegistroEsDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnRegistroEsDtm, RegistroEsDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelRegistroEsDtm, RegistroEsDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnRegistroEsDtm, RegistroEsDtm>(modelBuilder);            
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnRegistroEsDtm, RegistroEsDtm, EstadoDeUnRegistroEsDtm, TransicionesDeUnRegistroEsDtm, ObservacionesDeUnRegistroEsDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnRegistroEsDtm>(modelBuilder, nameof(ArchivadoresDeUnRegistroEsDtm.Registro), nameof(ArchivadoresDeUnRegistroEsDtm.Archivador));

        }
        internal static void Interlocutores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<InterlocutoresDeUnRegistroDtm>(modelBuilder, nameof(InterlocutoresDeUnRegistroDtm.Registro), nameof(InterlocutoresDeUnRegistroDtm.Interlocutor));

        }
        internal static void Tareas(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<TareasDeUnRegistroDtm>(modelBuilder, nameof(TareasDeUnRegistroDtm.Registro), nameof(TareasDeUnRegistroDtm.Tarea));
        }
    }
}
