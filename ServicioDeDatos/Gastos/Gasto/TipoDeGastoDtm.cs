using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    [Table(Tablas.GASTO + "_" + Sufijo.ESTADO, Schema = Esquemas.GASTO)]
    public class EstadoDeUnGastoDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Gasto;
    }

    [Table(Tablas.GASTO + "_" + Sufijo.TRANSICION, Schema = Esquemas.GASTO)]
    public class TransicionesDeUnGastoDtm : TransicionDtm
    {
    }

    [Table(Tablas.GASTO + "_" + Sufijo.ACCION, Schema = Esquemas.GASTO)]
    public class AccionesDeUnGastoDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.GASTO + "_" + Sufijo.TIPO, Schema = Esquemas.GASTO)]
    public class TipoDeGastoDtm : TipoConFlujoDtm
    {
        public TipoDeGastoDtm Padre { get; set; }
        public new EstadoDeUnGastoDtm Estado { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Gasto;
        public string AsuntoPropuesto { get; set; }
        public int? IdNaturaleza { get; set; }
        public NaturalezaDtm NaturalezaPropuesta { get; set; }
        public decimal? MaximoDeducible { get; set; }
        public int? IdCuentaNoDeducible { get; set; }
        public CuentaDtm CuentaNoDeducible { get; set; }
        public bool PermiteContado { get; set; }
        public bool PermiteRemesar { get; set; }

    }

    public static partial class ModeloDeGasto
    {
        internal static void EstadosDeUnGasto(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnGastoDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnGasto(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnGastoDtm, EstadoDeUnGastoDtm>(modelBuilder);
        }

        internal static void AccionesDeUnGasto(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnGastoDtm, TransicionesDeUnGastoDtm>(modelBuilder);
        }

        internal static void TipoDeGasto(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeGastoDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeGastoDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeGastoDtm>(modelBuilder, nameof(TipoDeGastoDtm.Padre), nameof(TipoDeGastoDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeGastoDtm>(modelBuilder, nameof(TipoDeGastoDtm.Estado), nameof(TipoDeGastoDtm.IdEstado), ICampos.ID_ESTADO, unico: false);

            modelBuilder.Entity<TipoDeGastoDtm>().Property(nameof(TipoDeGastoDtm.AsuntoPropuesto)).HasColumnName(ICampos.ASUNTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDeGastoDtm>(modelBuilder, nameof(TipoDeGastoDtm.NaturalezaPropuesta), nameof(TipoDeGastoDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);

            modelBuilder.Entity<TipoDeGastoDtm>().Property(nameof(TipoDeGastoDtm.MaximoDeducible)).HasColumnName(ICampos.IMPORTE_MAXIMO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDeGastoDtm>(modelBuilder, nameof(TipoDeGastoDtm.CuentaNoDeducible), nameof(TipoDeGastoDtm.CuentaNoDeducible), ICampos.ID_CUENTA_NO_DEDUCIBLE, requerida: false, unico: false);

            modelBuilder.Entity<TipoDeGastoDtm>().Property(p => p.PermiteContado).HasColumnName(ICampos.PERMITIR_CONTADO).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TipoDeGastoDtm>().Property(p => p.PermiteRemesar).HasColumnName(ICampos.PERMITIR_REMESAR).HasColumnType(IDominio.BIT).IsRequired();
        }
    }
}
