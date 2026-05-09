using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.PARTE_TR + "_" + Sufijo.ESTADO, Schema = Esquemas.VENTA)]
    public class EstadoDeUnParteTrDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.ParteDeTrabajo;
    }

    [Table(Tablas.PARTE_TR + "_" + Sufijo.TRANSICION, Schema = Esquemas.VENTA)]
    public class TransicionesDeUnParteTrDtm : TransicionDtm
    {
    }

    [Table(Tablas.PARTE_TR + "_" + Sufijo.ACCION, Schema = Esquemas.VENTA)]
    public class AccionesDeUnParteTrDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.PARTE_TR + "_" + Sufijo.TIPO, Schema = Esquemas.VENTA)]
    public class TipoDeParteTrDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDeParteTrDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public new EstadoDeUnParteTrDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public int? IdTipoFacturaEmt { get; set; }
        public TipoDeFacturaEmtDtm TipoFacturaEmt { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.ParteDeTrabajo;
    }

    public static partial class ModeloDeParteTr
    {
        internal static void EstadosDeUnParteTr(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnParteTrDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnParteTr(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnParteTrDtm, EstadoDeUnParteTrDtm>(modelBuilder);
        }

        internal static void AccionesDeUnParteTr(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnParteTrDtm, TransicionesDeUnParteTrDtm>(modelBuilder);
        }

        internal static void TipoDeParteTr(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeParteTrDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeParteTrDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeParteTrDtm>(modelBuilder, nameof(TipoDeParteTrDtm.Padre), nameof(TipoDeParteTrDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeParteTrDtm>(modelBuilder, nameof(TipoDeParteTrDtm.Estado), nameof(TipoDeParteTrDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDeParteTrDtm>(modelBuilder, nameof(TipoDeParteTrDtm.TipoFacturaEmt), nameof(TipoDeParteTrDtm.IdTipoFacturaEmt), ICampos.ID_TIPO_FACTURA,requerida:false, unico: false);
        }
    }
}
