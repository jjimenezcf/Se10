using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.VENTA + "_" + Sufijo.ESTADO, Schema = Esquemas.LOGISTICA)]
    public class EstadoDeUnaVentaDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Venta;
    }

    [Table(Tablas.VENTA + "_" + Sufijo.TRANSICION, Schema = Esquemas.LOGISTICA)]
    public class TransicionesDeUnaVentaDtm : TransicionDtm
    {
    }

    [Table(Tablas.VENTA + "_" + Sufijo.ACCION, Schema = Esquemas.LOGISTICA)]
    public class AccionesDeUnaVentaDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.VENTA + "_" + Sufijo.TIPO, Schema = Esquemas.LOGISTICA)]
    public class TipoDeVentaDtm : TipoConFlujoDtm
    {
        public TipoDeVentaDtm Padre { get; set; }
        public new EstadoDeUnaVentaDtm Estado { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Venta;
    }

    public static partial class ModeloDeVenta
    {
        internal static void EstadosDeUnaVenta(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaVentaDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaVenta(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaVentaDtm, EstadoDeUnaVentaDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaVenta(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaVentaDtm, TransicionesDeUnaVentaDtm>(modelBuilder);
        }

        internal static void TipoDeVenta(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeVentaDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeVentaDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeVentaDtm>(modelBuilder, nameof(TipoDeVentaDtm.Padre), nameof(TipoDeVentaDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeVentaDtm>(modelBuilder, nameof(TipoDeVentaDtm.Estado), nameof(TipoDeVentaDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
