using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{
    public class ltrDeUnCircuito
    {
        public const string IdTipoCircuito = nameof(IdTipoCircuito);
        public const string SeleccionarParaFiltrarPorLoteContable = nameof(SeleccionarParaFiltrarPorLoteContable);
        public const string SeleccionarParaFiltrarPorEstimacion = nameof(SeleccionarParaFiltrarPorEstimacion);
        public const string SeleccionarParaFiltrarPorFichada = nameof(SeleccionarParaFiltrarPorFichada);
        public const string SeleccionarParaFiltrarPorActividad = nameof(SeleccionarParaFiltrarPorActividad);
        public const string IdExpedientePadre = nameof(IdExpedientePadre);
        public const string AsociadosAUnExpediente = nameof(AsociadosAUnExpediente);
    }

    public class ltrDeUnaEstimacion
    {
        public const string EtiquetaEstimacionDirecta = "Estimación directa";
        public const string VinculosAUnaEstimacion = nameof(VinculosAUnaEstimacion);
        public const string FacturasRecVinculadas = nameof(FacturasRecVinculadas);
        public const string FacturasEmtVinculadas = nameof(FacturasEmtVinculadas);
        public const string PagosVinculados = nameof(PagosVinculados);
        public const string CobrosVinculados = nameof(CobrosVinculados);
        public const string Mensaje_FaltaConfigurarParametro = "Ha de conficurar el parámetro '{0}' que define como aplicar las reglas de fichada";
    }

    public class ltrDeUnLoteContable
    {
        public const string EtiquetaLoteContable = "Lote Contable";
        public const string VinculosAUnLote = nameof(VinculosAUnLote);
        public const string BuscarPorLotePreasiento = nameof(BuscarPorLotePreasiento);
        public const string IdLoteContable = nameof(IdLoteContable);
    }

    public class ltrTrazasDeUnLoteContable
    {
        public const string PreasientoQuitadoDelLote = "Preasiento quitado del lote";
        public const string RegenerarlLote = "Lote contable Regenerado";
    }

    [Table(Tablas.CIRCUITO_DOC, Schema = Esquemas.SISDOC)]
    public class CircuitoDocDtm : ElementoDeProcesoDtm
    {
        public new TipoDeCircuitoDocDtm Tipo { get; set; }
        public new EstadoDeUnCircuitoDocDtm Estado { get; set; }

        public DatosDeActividadFormativaDtm DatosActividadFormativa { get; set;  }
    }


    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.SISDOC)]
    public class AuditoriaDeUnCircuitoDocDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.SISDOC)]
    public class ArchivosDeUnCircuitoDocDtm : VinculoDtm
    {
        public CircuitoDocDtm CircuitoDoc { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.SISDOC)]
    public class AgendaDeUnCircuitoDocDtm : VinculoDtm
    {
        public CircuitoDocDtm CircuitoDoc { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.SISDOC)]
    public class ObservacionesDeUnCircuitoDocDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.CircuitoDoc;
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.SISDOC)]
    public class PermisoDelCircuitoDocDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.SISDOC)]
    public class TrazasDeUnCircuitoDocDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.CircuitoDoc;
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.SISDOC)]
    public class HitosDeUnCircuitoDocDtm : HitoDtm
    {

    }

    [Table(Tablas.CIRCUITO_DOC + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.SISDOC)]
    public class ArchivadoresDeUnCircuitoDocDtm : VinculoDtm
    {
        public CircuitoDocDtm CircuitoDoc { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeCircuitoDoc
    {

        public static void CircuitoDoc(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CircuitoDocDtm>().Ignore(x => x.DatosActividadFormativa);
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<CircuitoDocDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<CircuitoDocDtm>(modelBuilder, nameof(CircuitoDocDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<CircuitoDocDtm>(modelBuilder, nameof(CircuitoDocDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<CircuitoDocDtm>(modelBuilder, nameof(CircuitoDocDtm.Estado));
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnCircuitoDocDtm, CircuitoDocDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnCircuitoDocDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnCircuitoDocDtm>(modelBuilder, nameof(ArchivosDeUnCircuitoDocDtm.CircuitoDoc), nameof(ArchivosDeUnCircuitoDocDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnCircuitoDocDtm>(modelBuilder, nameof(AgendaDeUnCircuitoDocDtm.CircuitoDoc), nameof(AgendaDeUnCircuitoDocDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnCircuitoDocDtm, CircuitoDocDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelCircuitoDocDtm, CircuitoDocDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnCircuitoDocDtm, CircuitoDocDtm, EstadoDeUnCircuitoDocDtm, TransicionesDeUnCircuitoDocDtm, ObservacionesDeUnCircuitoDocDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnCircuitoDocDtm>(modelBuilder, nameof(ArchivadoresDeUnCircuitoDocDtm.CircuitoDoc), nameof(ArchivadoresDeUnCircuitoDocDtm.Archivador));
        }

    }
}
