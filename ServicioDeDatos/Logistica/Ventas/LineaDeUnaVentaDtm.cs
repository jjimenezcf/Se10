using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.VENTA + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.LOGISTICA)]
    public class LineaDeUnaVentaDtm : RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public VentaDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public int IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }

        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal? Descuento { get; set; }

        public decimal ImporteSinDto => Precio * Cantidad;
        public decimal ImporteDeDto => ImporteSinDto * (Descuento == null ? 0m : (decimal)Descuento / 100);
        public decimal Importe => ImporteSinDto - ImporteDeDto;

        public enumNegocio Negocio => enumNegocio.Venta;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeVenta
    {
        internal static void DatosDeLineaDeUnaVenta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnaVentaDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnaVentaDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnaVentaDtm>().Ignore(x => x.ImporteSinDto);
            modelBuilder.Entity<LineaDeUnaVentaDtm>().Ignore(x => x.ImporteDeDto);
            modelBuilder.Entity<LineaDeUnaVentaDtm>().Ignore(x => x.Importe);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaVentaDtm>(modelBuilder, nameof(LineaDeUnaVentaDtm.Elemento), nameof(LineaDeUnaVentaDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);

            modelBuilder.Entity<LineaDeUnaVentaDtm>().Property(nameof(LineaDeUnaVentaDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaVentaDtm>(modelBuilder, nameof(LineaDeUnaVentaDtm.Unitario), nameof(LineaDeUnaVentaDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: true, unico: false);

            modelBuilder.Entity<LineaDeUnaVentaDtm>().Property(nameof(LineaDeUnaVentaDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaVentaDtm>().Property(nameof(LineaDeUnaVentaDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaVentaDtm>().Property(nameof(LineaDeUnaVentaDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineaDeUnaVentaDtm>(modelBuilder);
        }
    }
}
