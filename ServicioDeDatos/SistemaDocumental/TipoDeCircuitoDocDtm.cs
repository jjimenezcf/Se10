using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{

    [Table(Tablas.CIRCUITO_DOC + "_" + Sufijo.ESTADO, Schema = Esquemas.SISDOC)]
    public class EstadoDeUnCircuitoDocDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.CircuitoDoc;
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + Sufijo.TRANSICION, Schema = Esquemas.SISDOC)]
    public class TransicionesDeUnCircuitoDocDtm : TransicionDtm
    {
    }

    [Table(Tablas.CIRCUITO_DOC + "_" + Sufijo.ACCION, Schema = Esquemas.SISDOC)]
    public class AccionesDeUnCircuitoDocDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.CIRCUITO_DOC + "_" + Sufijo.TIPO, Schema = Esquemas.SISDOC)]
    public class TipoDeCircuitoDocDtm : TipoConFlujoDtm
    {
        public TipoDeCircuitoDocDtm Padre { get; set; }
        public new EstadoDeUnCircuitoDocDtm Estado { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.CircuitoDoc;
    }

    public static partial class ModeloDeCircuitoDoc
    {
        internal static void EstadosDeUnCircuitoDoc(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnCircuitoDocDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnCircuitoDoc(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnCircuitoDocDtm, EstadoDeUnCircuitoDocDtm>(modelBuilder);
        }

        internal static void AccionesDeUnCircuitoDoc(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnCircuitoDocDtm, TransicionesDeUnCircuitoDocDtm>(modelBuilder);
        }

        internal static void TipoDeCircuitoDoc(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeCircuitoDocDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeCircuitoDocDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeCircuitoDocDtm>(modelBuilder, nameof(TipoDeCircuitoDocDtm.Padre), nameof(TipoDeCircuitoDocDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeCircuitoDocDtm>(modelBuilder, nameof(TipoDeCircuitoDocDtm.Estado), nameof(TipoDeCircuitoDocDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
