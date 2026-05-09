using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    [Table(Tablas.FACTURA_REC + "_" + Sufijo.ESTADO, Schema = Esquemas.GASTO)]
    public class EstadoDeUnaFacturaRecDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.FacturaEmitida;
    }

    [Table(Tablas.FACTURA_REC + "_" + Sufijo.TRANSICION, Schema = Esquemas.GASTO)]
    public class TransicionesDeUnaFacturaRecDtm : TransicionDtm
    {
    }

    [Table(Tablas.FACTURA_REC + "_" + Sufijo.ACCION, Schema = Esquemas.GASTO)]
    public class AccionesDeUnaFacturaRecDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.FACTURA_REC + "_" + Sufijo.TIPO, Schema = Esquemas.GASTO)]
    public class TipoDeFacturaRecDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDeFacturaRecDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public new EstadoDeUnaFacturaRecDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.FacturaRecibida;        
    }

    public static partial class ModeloDeFacturaRec
    {
        internal static void EstadosDeUnaFacturaRec(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaFacturaRecDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaFacturaRec(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaFacturaRecDtm, EstadoDeUnaFacturaRecDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaFacturaRec(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaFacturaRecDtm, TransicionesDeUnaFacturaRecDtm>(modelBuilder);
        }

        internal static void TipoDeFacturaRec(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeFacturaRecDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeFacturaRecDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeFacturaRecDtm>(modelBuilder, nameof(TipoDeFacturaRecDtm.Padre), nameof(TipoDeFacturaRecDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeFacturaRecDtm>(modelBuilder, nameof(TipoDeFacturaRecDtm.Estado), nameof(TipoDeFacturaRecDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
