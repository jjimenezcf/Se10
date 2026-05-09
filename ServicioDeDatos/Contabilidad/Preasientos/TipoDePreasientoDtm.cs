using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Contabilidad
{

    [Table(Tablas.PREASIENTO + "_" + Sufijo.ESTADO, Schema = Esquemas.CONTABILIDAD)]
    public class EstadoDeUnPreasientoDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Preasiento;
    }

    [Table(Tablas.PREASIENTO + "_" + Sufijo.TRANSICION, Schema = Esquemas.CONTABILIDAD)]
    public class TransicionesDeUnPreasientoDtm : TransicionDtm
    {
    }

    [Table(Tablas.PREASIENTO + "_" + Sufijo.ACCION, Schema = Esquemas.CONTABILIDAD)]
    public class AccionesDeUnPreasientoDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.PREASIENTO + "_" + Sufijo.TIPO, Schema = Esquemas.CONTABILIDAD)]
    public class TipoDePreasientoDtm : TipoConFlujoDtm
    {
        public TipoDePreasientoDtm Padre { get; set; }
        public new EstadoDeUnPreasientoDtm Estado { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Preasiento;
    }

    public static partial class ModeloDePreasiento
    {
        internal static void EstadosDeUnPreasiento(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnPreasientoDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnPreasiento(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnPreasientoDtm, EstadoDeUnPreasientoDtm>(modelBuilder);
        }

        internal static void AccionesDeUnPreasiento(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnPreasientoDtm, TransicionesDeUnPreasientoDtm>(modelBuilder);
        }

        internal static void TipoDePreasiento(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDePreasientoDtm>(modelBuilder);

            modelBuilder.Entity<TipoDePreasientoDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDePreasientoDtm>(modelBuilder, nameof(TipoDePreasientoDtm.Padre), nameof(TipoDePreasientoDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDePreasientoDtm>(modelBuilder, nameof(TipoDePreasientoDtm.Estado), nameof(TipoDePreasientoDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
