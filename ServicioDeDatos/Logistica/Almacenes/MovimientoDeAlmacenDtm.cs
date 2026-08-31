using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Logistica
{
    public enum enumClaseDeMovimiento
    {
        [Description("Entrada")]
        Entrada,
        [Description("Salida")]
        Salida,
        [Description("Inicial")]
        Inicial,
        [Description("Ajuste")]
        Ajuste,
        [Description("Regularización")]
        Regularizacion,
        [Description("Nulo")]
        Nulo
    }

    public class ltrDeUnMovimientoDeAlmacen
    {
        public static string FiltroPorAlmacen => nameof(FiltroPorAlmacen);

        public static string FiltroParaAlmacenesAccesibles => nameof(FiltroParaAlmacenesAccesibles);
        public static string FiltroPorRealizadoEl => nameof(FiltroPorRealizadoEl);
        public static string FiltroPorTipoMovimiento => nameof(FiltroPorTipoMovimiento);
        public static string FiltroPorUnitario => nameof(FiltroPorUnitario);

        public static string EstoyBarriendo => nameof(EstoyBarriendo);

    }

    [Table(Tablas.ALMACEN_TIPO_MOVIMIENTO, Schema = Esquemas.LOGISTICA)]
    public class TipoMovimientoDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public enumClaseDeMovimiento ClaseMovimiento { get; set; }
    }

    [Table(Tablas.ALMACEN_MOVIMIENTO, Schema = Esquemas.LOGISTICA)]
    public class MovimientoDeAlmacenDtm : RegistroDtm, IAuditoria
    {
        public int IdAlmacen { get; set; }
        public AlmacenDtm Almacen { get; set; }

        public int IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }

        public int IdTipoMovimiento { get; set; }
        public TipoMovimientoDtm TipoMovimiento { get; set; }

        public decimal Cantidad { get; set; }
        public decimal Stock { get; set; }
        public decimal Precio { get; set; }
        public decimal Valor { get; set; }

        public System.DateTime RealizadoEl { get; set; }

        // Origen o destino del movimiento (según la ClaseMovimiento del TipoMovimiento). Todavía no existen los Dtm de estos negocios.
        public int? IdMovimiento { get; set; }
        public int? IdLineaAlbaran { get; set; }
        public int? IdLineaDevolucion { get; set; }
        public int? IdLineaInventario { get; set; }

        public int? IdPreasiento { get; set; }
        public PreasientoDtm Preasiento { get; set; }

        public int IdUsuaCrea { get; set; }
        public int? IdUsuaModi { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }
        public System.DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeAlmacen
    {
        public static void TipoDeMovimiento(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<TipoMovimientoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TipoMovimientoDtm>(modelBuilder, unico: true);

            modelBuilder.Entity<TipoMovimientoDtm>().Property(p => p.ClaseMovimiento).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_20).IsRequired(true);
        }

        public static void Movimiento(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoFk<MovimientoDeAlmacenDtm>(modelBuilder, nameof(MovimientoDeAlmacenDtm.Almacen), nameof(MovimientoDeAlmacenDtm.IdAlmacen), ICampos.ID_ALMACEN, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<MovimientoDeAlmacenDtm>(modelBuilder, nameof(MovimientoDeAlmacenDtm.Unitario), nameof(MovimientoDeAlmacenDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<MovimientoDeAlmacenDtm>(modelBuilder, nameof(MovimientoDeAlmacenDtm.TipoMovimiento), nameof(MovimientoDeAlmacenDtm.IdTipoMovimiento), ICampos.ID_TIPO_MOVIMIENTO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<MovimientoDeAlmacenDtm>(modelBuilder, nameof(MovimientoDeAlmacenDtm.Preasiento), nameof(MovimientoDeAlmacenDtm.IdPreasiento), ICampos.ID_PREASIENTO, requerida: false, unico: false);

            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.Cantidad).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.Stock).HasColumnName(ICampos.STOCK).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.Precio).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.Valor).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.DECIMAL).IsRequired(true);

            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.RealizadoEl).HasColumnName(ICampos.REALIZADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);

            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.IdMovimiento).HasColumnName(ICampos.ID_MOVIMIENTO).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.IdLineaAlbaran).HasColumnName(ICampos.ID_LINEA_ALBARAN).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.IdLineaDevolucion).HasColumnName(ICampos.ID_LINEA_DEVOLUCION).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<MovimientoDeAlmacenDtm>().Property(p => p.IdLineaInventario).HasColumnName(ICampos.ID_LINEA_INVENTARIO).HasColumnType(IDominio.INT).IsRequired(false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<MovimientoDeAlmacenDtm>(modelBuilder);
        }
    }
}
