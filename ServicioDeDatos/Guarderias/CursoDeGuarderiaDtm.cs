using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Guarderias
{
    public static class ltrDeCursosDeGuarderia
    {
        public static readonly string FiltrarPorActivo = nameof(FiltrarPorActivo);
        public static readonly string FiltrarPorNoActivo = nameof(FiltrarPorNoActivo);
        public static readonly string FiltrarPorPeriodo = nameof(FiltrarPorPeriodo);
        public static readonly string FiltrarPorAula = nameof(FiltrarPorAula);
        public static readonly string FiltrarPorFechaDeActivo = nameof(FiltrarPorFechaDeActivo);
        public static readonly string FiltrarPorInfante = nameof(FiltrarPorInfante);
        public const string FiltrarParaAsociarCurso = nameof(FiltrarParaAsociarCurso);
    }

    [Table(Tablas.CURSO, Schema = Esquemas.GUARDERIA)]
    public class CursoDeGuarderiaDtm : ElementoDtm, IUsaTraza, IUsaTrabajador, IUsaAgenda
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public int IdAula { get; set; }
        public AulaDeGuarderiaDtm Aula { get; set; }
        public int IdTrabajador { get; set; }
        public TrabajadorDtm Trabajador { get; set; }
        public int IdAgenda { get; set; }
        public AgendaDtm Agenda { get; set; }

        public int IdConsultor { get; set; }
        public PuestoDtm Consultor { get; set; }

        public int IdGestor { get; set; }
        public PuestoDtm Gestor { get; set; }

        public string NombreDeAgenda => $"Agenda del Curso: {Nombre}";
        public string NombrePt(enumModoDeAccesoDeDatos modo) => $"CURSO: ({modo.Nombre()}) {Nombre}";

    }

    [Table(Tablas.CURSO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.GUARDERIA)]
    public class AuditoriaDeUnCursoDeGuarderiaDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.CURSO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.GUARDERIA)]
    public class ArchivosDeUnCursoDeGuarderiaDtm : VinculoDtm
    {
        public CursoDeGuarderiaDtm CursoDeGuarderia { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.CURSO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.GUARDERIA)]
    public class ObservacionesDeUnCursoDeGuarderiaDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.CursoDeGuarderia;
    }

    [Table(Tablas.CURSO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.GUARDERIA)]
    public class TrazasDeUnCursoDeGuarderiaDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.CursoDeGuarderia;
    }

    [Table(Tablas.CURSO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.GUARDERIA)]
    public class ArchivadoresDeUnCursoDeGuarderiaDtm : VinculoDtm
    {
        public CursoDeGuarderiaDtm CursoDeGuarderia { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    [Table(Tablas.CURSO + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.GUARDERIA)]
    public class AgendaDeUnCursoDeGuarderiaDtm : VinculoDtm
    {
        public CursoDeGuarderiaDtm CursoDeGuarderia { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }


    public static partial class ModeloDeGuarderias
    {
        public static void CursoDeGuarderia(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<CursoDeGuarderiaDtm>(modelBuilder, indiceUnicoPorNombre: true);
            modelBuilder.Entity<CursoDeGuarderiaDtm>().Property(nameof(CursoDeGuarderiaDtm.Inicio)).HasColumnName(ICampos.INICIO).HasColumnType(IDominio.DATE).IsRequired(true);
            modelBuilder.Entity<CursoDeGuarderiaDtm>().Property(nameof(CursoDeGuarderiaDtm.Fin)).HasColumnName(ICampos.FIN).HasColumnType(IDominio.DATE).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<CursoDeGuarderiaDtm>(modelBuilder, nameof(CursoDeGuarderiaDtm.Aula), nameof(CursoDeGuarderiaDtm.IdAula), nameof(ICampos.ID_AULA), requerida: true, unico: false);

            ApiDeElementoDtm.DefinirTrabajador<CursoDeGuarderiaDtm>(modelBuilder);

            ApiDeElementoDtm.DefinirCampoAgenda<CursoDeGuarderiaDtm>(modelBuilder, obligatorio: true, unico: true);


            ApiDeRegistroDtm.DefinirCampoFk<CursoDeGuarderiaDtm>(modelBuilder, nameof(CursoDeGuarderiaDtm.Consultor), nameof(CursoDeGuarderiaDtm.IdConsultor), ICampos.ID_CONSULTOR, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<CursoDeGuarderiaDtm>(modelBuilder, nameof(CursoDeGuarderiaDtm.Gestor), nameof(CursoDeGuarderiaDtm.IdGestor), ICampos.ID_GESTOR, requerida: true, unico: true);

        }

        public static void AuditoriaDeUnCursoDeGuarderia(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnCursoDeGuarderiaDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnCursoDeGuarderia(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnCursoDeGuarderiaDtm>(modelBuilder, nameof(ArchivosDeUnCursoDeGuarderiaDtm.CursoDeGuarderia), nameof(ArchivosDeUnCursoDeGuarderiaDtm.Archivo));
        }

        internal static void ObservacionesDeUnCursoDeGuarderia(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnCursoDeGuarderiaDtm, CursoDeGuarderiaDtm>(modelBuilder);
        }

        internal static void ArchivadoresDeUnCursoDeGuarderia(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnCursoDeGuarderiaDtm>(modelBuilder, nameof(ArchivadoresDeUnCursoDeGuarderiaDtm.CursoDeGuarderia), nameof(ArchivadoresDeUnCursoDeGuarderiaDtm.Archivador));
        }

        internal static void TrazasDeUnCursoDeGuarderia(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnCursoDeGuarderiaDtm, CursoDeGuarderiaDtm>(modelBuilder);
        }
        internal static void AgendaDeUnCurso(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnCursoDeGuarderiaDtm>(modelBuilder, nameof(AgendaDeUnCursoDeGuarderiaDtm.CursoDeGuarderia), nameof(AgendaDeUnCursoDeGuarderiaDtm.Evento));
        }
    }
}
