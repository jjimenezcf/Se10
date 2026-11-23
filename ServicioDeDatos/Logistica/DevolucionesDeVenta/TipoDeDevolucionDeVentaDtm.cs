using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.DEVOLUCIONVENTA + "_" + Sufijo.ESTADO, Schema = Esquemas.LOGISTICA)]
    public class EstadoDeUnaDevolucionDeVentaDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.DevolucionVenta;
    }

    [Table(Tablas.DEVOLUCIONVENTA + "_" + Sufijo.TRANSICION, Schema = Esquemas.LOGISTICA)]
    public class TransicionesDeUnaDevolucionDeVentaDtm : TransicionDtm
    {
    }

    [Table(Tablas.DEVOLUCIONVENTA + "_" + Sufijo.ACCION, Schema = Esquemas.LOGISTICA)]
    public class AccionesDeUnaDevolucionDeVentaDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.DEVOLUCIONVENTA + "_" + Sufijo.TIPO, Schema = Esquemas.LOGISTICA)]
    public class TipoDeDevolucionDeVentaDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDeDevolucionDeVentaDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }
        public new EstadoDeUnaDevolucionDeVentaDtm Estado { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.DevolucionVenta;
    }

    public static partial class ModeloDeDevolucionDeVenta
    {
        internal static void EstadosDeUnaDevolucionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaDevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaDevolucionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaDevolucionDeVentaDtm, EstadoDeUnaDevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaDevolucionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaDevolucionDeVentaDtm, TransicionesDeUnaDevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void TipoDeDevolucionDeVenta(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeDevolucionDeVentaDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeDevolucionDeVentaDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeDevolucionDeVentaDtm>(modelBuilder, nameof(TipoDeDevolucionDeVentaDtm.Padre), nameof(TipoDeDevolucionDeVentaDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeDevolucionDeVentaDtm>(modelBuilder, nameof(TipoDeDevolucionDeVentaDtm.Estado), nameof(TipoDeDevolucionDeVentaDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
