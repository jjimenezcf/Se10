using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.PLANIFICACION_VENTA, Schema = Esquemas.VENTA)]
    public class PlanificacionDeVentaDtm : ElementoDeProcesoDtm, IUsaCliente, IUsaDirecciones
    {
        public int IdCliente { get; set; }
        public ClienteDtm Cliente { get; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }

        public DateTime EjecutarEl { get; set; }

        public new TipoDePlanificacionDeVentaDtm Tipo { get; set; }
        public new EstadoDeUnaPlanificacionDeVentaDtm Estado { get; set; }

        public int? IdTipoDeFactura { get; set; }
        public int? IdTipoDeParte { get; set; }
        public TipoDeFacturaEmtDtm TipoDeFactura { get; set; }
        public TipoDeParteTrDtm TipoDeParte { get; set; }

        public int? IdContrato { get; set; }
        public static enumClaseDeContrato ClaseContrato => enumClaseDeContrato.Venta;
        public ContratoDtm Contrato { get; set; }

        public int? IdPlanificador { get; set; }
        public int? IdFacturaEmt { get; set; }
        public int? IdParteTr { get; set; }
        public PlanificadorDeVentaDtm Planificador { get; set; }
        public FacturaEmtDtm FacturaEmt { get; set; }
        public ParteTrDtm ParteTr { get; set; }
    }

    #region tablas del modelo de planificación
    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.VENTA)]
    public class AuditoriaDeUnaPlanificacionDeVentaDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.VENTA)]
    public class ArchivosDeUnaPlanificacionDeVentaDtm : VinculoDtm
    {
        public PlanificacionDeVentaDtm Planificacion { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.VENTA)]
    public class AgendaDeUnaPlanificacionDeVentaDtm : VinculoDtm
    {
        public PlanificacionDeVentaDtm Planificacion { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.VENTA)]
    public class ObservacionesDeUnaPlanificacionDeVentaDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.PlanificacionDeVenta;
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.VENTA)]
    public class PermisoDeLaPlanificacionDeVentaDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.VENTA)]
    public class TrazasDeUnaPlanificacionDeVentaDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.PlanificacionDeVenta;
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.VENTA)]
    public class DireccionDeUnaPlanificacionDeVentaDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.PlanificacionDeVenta;
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.VENTA)]
    public class HitosDeUnaPlanificacionDeVentaDtm : HitoDtm
    {
    }
    #endregion

    public static partial class ModeloDePlanificacionDeVenta
    {

        public static void PlanificacionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PlanificacionDeVentaDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<PlanificacionDeVentaDtm>(modelBuilder, nameof(PlanificacionDeVentaDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<PlanificacionDeVentaDtm>(modelBuilder, nameof(PlanificacionDeVentaDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<PlanificacionDeVentaDtm>(modelBuilder, nameof(PlanificacionDeVentaDtm.Estado));
            ApiDeElementoDtm.DefinirCliente<PlanificacionDeVentaDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirDependencia<PlanificacionDeVentaDtm, TipoDeFacturaEmtDtm>(modelBuilder, apuntadoPor: nameof(PlanificacionDeVentaDtm.IdTipoDeFactura), idCampo: ICampos.ID_TIPO_FACTURA, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<PlanificacionDeVentaDtm, TipoDeParteTrDtm>(modelBuilder, apuntadoPor: nameof(PlanificacionDeVentaDtm.IdTipoDeParte), idCampo: ICampos.ID_TIPO_PARTE, requerido: false);

            ApiDeRegistroDtm.DefinirDependencia<PlanificacionDeVentaDtm, ContratoDtm>(modelBuilder, apuntadoPor: nameof(PlanificacionDeVentaDtm.IdContrato), idCampo: ICampos.ID_CONTRATO, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<PlanificacionDeVentaDtm, PlanificadorDeVentaDtm>(modelBuilder, apuntadoPor: nameof(PlanificacionDeVentaDtm.IdPlanificador), idCampo: ICampos.ID_PLANIFICADOR, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<PlanificacionDeVentaDtm, FacturaEmtDtm>(modelBuilder, apuntadoPor: nameof(PlanificacionDeVentaDtm.IdFacturaEmt), idCampo: ICampos.ID_FACTURA_EMT, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<PlanificacionDeVentaDtm, ParteTrDtm>(modelBuilder, apuntadoPor: nameof(PlanificacionDeVentaDtm.IdParteTr), idCampo: ICampos.ID_PARTE_TR, requerido: false);


            modelBuilder.Entity<PlanificacionDeVentaDtm>().Property(p => p.EjecutarEl).HasColumnName(ICampos.EJECUTAR_EL).HasColumnType(IDominio.DATETIME_2).IsRequired();
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaPlanificacionDeVentaDtm, PlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaPlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaPlanificacionDeVentaDtm>(modelBuilder, nameof(ArchivosDeUnaPlanificacionDeVentaDtm.Planificacion), nameof(ArchivosDeUnaPlanificacionDeVentaDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnaPlanificacionDeVentaDtm>(modelBuilder, nameof(AgendaDeUnaPlanificacionDeVentaDtm.Planificacion), nameof(AgendaDeUnaPlanificacionDeVentaDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaPlanificacionDeVentaDtm, PlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaPlanificacionDeVentaDtm, PlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnaPlanificacionDeVentaDtm, PlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaPlanificacionDeVentaDtm, PlanificacionDeVentaDtm, EstadoDeUnaPlanificacionDeVentaDtm, TransicionesDeUnaPlanificacionDeVentaDtm, ObservacionesDeUnaPlanificacionDeVentaDtm>(modelBuilder);
        }



    }
}
