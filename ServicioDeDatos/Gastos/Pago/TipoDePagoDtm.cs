using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    [Table(Tablas.PAGO + "_" + Sufijo.ESTADO, Schema = Esquemas.GASTO)]
    public class EstadoDeUnPagoDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Pago;
    }

    [Table(Tablas.PAGO + "_" + Sufijo.TRANSICION, Schema = Esquemas.GASTO)]
    public class TransicionesDeUnPagoDtm : TransicionDtm
    {
    }

    [Table(Tablas.PAGO + "_" + Sufijo.ACCION, Schema = Esquemas.GASTO)]
    public class AccionesDeUnPagoDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.PAGO + "_" + Sufijo.TIPO, Schema = Esquemas.GASTO)]
    public class TipoDePagoDtm : TipoConFlujoDtm
    {
        public TipoDePagoDtm Padre { get; set; }
        public new EstadoDeUnPagoDtm Estado { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Pago;
    }

    public static partial class ModeloDePago
    {
        internal static void EstadosDeUnPago(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnPagoDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnPago(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnPagoDtm, EstadoDeUnPagoDtm>(modelBuilder);
        }

        internal static void AccionesDeUnPago(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnPagoDtm, TransicionesDeUnPagoDtm>(modelBuilder);
        }

        internal static void TipoDePago(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDePagoDtm>(modelBuilder);

            modelBuilder.Entity<TipoDePagoDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDePagoDtm>(modelBuilder, nameof(TipoDePagoDtm.Padre), nameof(TipoDePagoDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDePagoDtm>(modelBuilder, nameof(TipoDePagoDtm.Estado), nameof(TipoDePagoDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
