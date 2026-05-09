using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.PEDIDO + "_" + Sufijo.ESTADO, Schema = Esquemas.LOGISTICA)]
    public class EstadoDeUnPedidoDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.FacturaEmitida;
    }

    [Table(Tablas.PEDIDO + "_" + Sufijo.TRANSICION, Schema = Esquemas.LOGISTICA)]
    public class TransicionesDeUnPedidoDtm : TransicionDtm
    {
    }

    [Table(Tablas.PEDIDO + "_" + Sufijo.ACCION, Schema = Esquemas.LOGISTICA)]
    public class AccionesDeUnPedidoDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.PEDIDO + "_" + Sufijo.TIPO, Schema = Esquemas.LOGISTICA)]
    public class TipoDePedidoDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDePedidoDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public new EstadoDeUnPedidoDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Pedido;        
    }

    public static partial class ModeloDePedido
    {
        internal static void EstadosDeUnPedido(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnPedidoDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnPedido(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnPedidoDtm, EstadoDeUnPedidoDtm>(modelBuilder);
        }

        internal static void AccionesDeUnPedido(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnPedidoDtm, TransicionesDeUnPedidoDtm>(modelBuilder);
        }

        internal static void TipoDePedido(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDePedidoDtm>(modelBuilder);

            modelBuilder.Entity<TipoDePedidoDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDePedidoDtm>(modelBuilder, nameof(TipoDePedidoDtm.Padre), nameof(TipoDePedidoDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDePedidoDtm>(modelBuilder, nameof(TipoDePedidoDtm.Estado), nameof(TipoDePedidoDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
