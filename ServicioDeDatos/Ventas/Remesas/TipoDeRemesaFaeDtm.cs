using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.REMESA_FAE + "_" + Sufijo.ESTADO, Schema = Esquemas.VENTA)]
    public class EstadoDeUnaRemesaFaeDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.RemesaFae;
    }

    [Table(Tablas.REMESA_FAE + "_" + Sufijo.TRANSICION, Schema = Esquemas.VENTA)]
    public class TransicionesDeUnaRemesaFaeDtm : TransicionDtm
    {
    }

    [Table(Tablas.REMESA_FAE + "_" + Sufijo.ACCION, Schema = Esquemas.VENTA)]
    public class AccionesDeUnaRemesaFaeDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.REMESA_FAE + "_" + Sufijo.TIPO, Schema = Esquemas.VENTA)]
    public class TipoDeRemesaFaeDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDeRemesaFaeDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public new EstadoDeUnaRemesaFaeDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.RemesaFae;
    }

    public static partial class ModeloDeRemesaFae
    {
        internal static void EstadosDeUnaRemesaFae(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaRemesaFaeDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaRemesaFae(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaRemesaFaeDtm, EstadoDeUnaRemesaFaeDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaRemesaFae(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaRemesaFaeDtm, TransicionesDeUnaRemesaFaeDtm>(modelBuilder);
        }

        internal static void TipoDeRemesaFae(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeRemesaFaeDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeRemesaFaeDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();


            ApiDeRegistroDtm.DefinirFk<TipoDeRemesaFaeDtm>(modelBuilder, nameof(TipoDeRemesaFaeDtm.Padre), nameof(TipoDeRemesaFaeDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeRemesaFaeDtm>(modelBuilder, nameof(TipoDeRemesaFaeDtm.Estado), nameof(TipoDeRemesaFaeDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
