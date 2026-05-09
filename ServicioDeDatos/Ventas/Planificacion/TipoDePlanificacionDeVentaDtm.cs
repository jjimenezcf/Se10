using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.PLANIFICACION_VENTA + "_" + Sufijo.ESTADO, Schema = Esquemas.VENTA)]
    public class EstadoDeUnaPlanificacionDeVentaDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.PlanificacionDeVenta;
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + Sufijo.TRANSICION, Schema = Esquemas.VENTA)]
    public class TransicionesDeUnaPlanificacionDeVentaDtm : TransicionDtm
    {
    }

    [Table(Tablas.PLANIFICACION_VENTA + "_" + Sufijo.ACCION, Schema = Esquemas.VENTA)]
    public class AccionesDeUnaPlanificacionDeVentaDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.PLANIFICACION_VENTA + "_" + Sufijo.TIPO, Schema = Esquemas.VENTA)]
    public class TipoDePlanificacionDeVentaDtm : TipoConFlujoDtm
    {
        public TipoDePlanificacionDeVentaDtm Padre { get; set; }
        public new EstadoDeUnaPlanificacionDeVentaDtm Estado { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.PlanificacionDeVenta;
    }

    public static partial class ModeloDePlanificacionDeVenta
    {
        internal static void EstadosDeUnaPlanificacionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaPlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaPlanificacionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaPlanificacionDeVentaDtm, EstadoDeUnaPlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaPlanificacionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaPlanificacionDeVentaDtm, TransicionesDeUnaPlanificacionDeVentaDtm>(modelBuilder);
        }

        internal static void TipoDePlanificacionDeVenta(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDePlanificacionDeVentaDtm>(modelBuilder);

            modelBuilder.Entity<TipoDePlanificacionDeVentaDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDePlanificacionDeVentaDtm>(modelBuilder, nameof(TipoDePlanificacionDeVentaDtm.Padre), nameof(TipoDePlanificacionDeVentaDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDePlanificacionDeVentaDtm>(modelBuilder, nameof(TipoDePlanificacionDeVentaDtm.Estado), nameof(TipoDePlanificacionDeVentaDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
