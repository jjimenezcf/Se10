using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Logistica
{
    // Línea de una devolución de venta: lo que se devuelve de una línea de un documento de venta, al precio y descuento al que se vendió
    // (se copian de la línea vendida y no se modifican); el importe, sin iva, es lo que se abona
    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.LOGISTICA)]
    public class LineaDeUnaDevolucionDeVentaDtm : RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public DevolucionDeVentaDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }

        public int IdLineaDeVenta { get; set; }
        public LineaDeUnaVentaDtm LineaDeVenta { get; set; }

        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal? Descuento { get; set; }

        public decimal ImporteSinDto => Precio * Cantidad;
        public decimal ImporteDeDto => ImporteSinDto * (Descuento == null ? 0m : (decimal)Descuento / 100);
        public decimal Importe => ImporteSinDto - ImporteDeDto;

        public enumNegocio Negocio => enumNegocio.DevolucionVenta;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeDevolucionDeVenta
    {
        internal static void DatosDeLineaDeUnaDevolucionDeVenta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Ignore(x => x.ImporteSinDto);
            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Ignore(x => x.ImporteDeDto);
            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Ignore(x => x.Importe);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaDevolucionDeVentaDtm>(modelBuilder, nameof(LineaDeUnaDevolucionDeVentaDtm.Elemento), nameof(LineaDeUnaDevolucionDeVentaDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);

            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Property(nameof(LineaDeUnaDevolucionDeVentaDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaDevolucionDeVentaDtm>(modelBuilder, nameof(LineaDeUnaDevolucionDeVentaDtm.LineaDeVenta), nameof(LineaDeUnaDevolucionDeVentaDtm.IdLineaDeVenta), ICampos.ID_LINEA_VENTA, requerida: true, unico: false);

            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Property(nameof(LineaDeUnaDevolucionDeVentaDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Property(nameof(LineaDeUnaDevolucionDeVentaDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaDevolucionDeVentaDtm>().Property(nameof(LineaDeUnaDevolucionDeVentaDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineaDeUnaDevolucionDeVentaDtm>(modelBuilder);
        }
    }
}
