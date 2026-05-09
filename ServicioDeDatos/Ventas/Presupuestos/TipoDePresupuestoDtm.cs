using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ServicioDeDatos.Presupuesto
{
    public enum enumClaseDePresupuesto
    {
        [Description("Presupuesto de venta")]
        venta,
        [Description("Presupuesto de gasto")]
        gasto,
        [Description("Presupuesto de inversión")]
        inversion
    }

    [Table(Tablas.PRESUPUESTO + "_" + Sufijo.ESTADO, Schema = Esquemas.PRESUPUESTO)]
    public class EstadoDeUnPresupuestoDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Presupuesto;
    }

    [Table(Tablas.PRESUPUESTO + "_" + Sufijo.TRANSICION, Schema = Esquemas.PRESUPUESTO)]
    public class TransicionesDeUnPresupuestoDtm : TransicionDtm
    {
    }

    [Table(Tablas.PRESUPUESTO + "_" + Sufijo.ACCION, Schema = Esquemas.PRESUPUESTO)]
    public class AccionesDeUnPresupuestoDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.PRESUPUESTO + "_" + Sufijo.TIPO, Schema = Esquemas.PRESUPUESTO)]
    public class TipoDePresupuestoDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDePresupuestoDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public enumClaseDePresupuesto ClaseDePresupuesto { get; set; }
        public new EstadoDeUnPresupuestoDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public int? IdTipoFacturaEmt { get; set; }
        public TipoDeFacturaEmtDtm TipoFacturaEmt { get; set; }

        public int? IdTipoParteTr { get; set; }
        public TipoDeParteTrDtm TipoParteTr { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Presupuesto;
    }

    public static partial class ModeloDePresupuesto
    {
        internal static void EstadosDeUnPresupuesto(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnPresupuestoDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnPresupuesto(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnPresupuestoDtm, EstadoDeUnPresupuestoDtm>(modelBuilder);
        }

        internal static void AccionesDeUnPresupuesto(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnPresupuestoDtm, TransicionesDeUnPresupuestoDtm>(modelBuilder);
        }

        internal static void TipoDePresupuesto(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDePresupuestoDtm>(modelBuilder);

            modelBuilder.Entity<TipoDePresupuestoDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();
            modelBuilder.Entity<TipoDePresupuestoDtm>().Property(nameof(TipoDePresupuestoDtm.ClaseDePresupuesto)).HasColumnName(ICampos.CLASE_PPT).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDePresupuestoDtm>(modelBuilder, nameof(TipoDePresupuestoDtm.Padre), nameof(TipoDePresupuestoDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDePresupuestoDtm>(modelBuilder, nameof(TipoDePresupuestoDtm.Estado), nameof(TipoDePresupuestoDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDePresupuestoDtm>(modelBuilder, nameof(TipoDePresupuestoDtm.TipoFacturaEmt), nameof(TipoDePresupuestoDtm.IdTipoFacturaEmt), ICampos.ID_TIPO_FACTURA, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDePresupuestoDtm>(modelBuilder, nameof(TipoDePresupuestoDtm.TipoParteTr), nameof(TipoDePresupuestoDtm.IdTipoParteTr), ICampos.ID_TIPO_PARTE, requerida: false, unico: false);
        }
    }
}
