using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.LOGISTICA)]
    public class LineaDeUnPedidoDtm : RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public PedidoDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public enumTipoDeLinea TipoDeLinea { get; set; }
        public int? IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }
        public string Concepto { get; set; }

        public int? IdNaturaleza { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }

        public enumClaseUnitario? Clase { get; set; }

        public decimal? Precio { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? Descuento { get; set; }
        public decimal ImporteSinDto => (Precio == null || Cantidad == null) ? 0m : (decimal)Precio * (decimal)Cantidad;
        public decimal ImporteDeDto => ImporteSinDto * (Descuento == null ? 0m : (decimal)Descuento / 100);
        public decimal ImporteConDto => ImporteSinDto - ImporteDeDto;

        public decimal ImporteDeLinea => (Precio == null || Cantidad == null) ? 0m : ImporteConDto;

        public int? IdUnidad { get; set; }
        public UnidadDtm Unidad { get; set; }

        public string Anotacion { get; set; }
        public enumNegocio Negocio => enumNegocio.Pedido;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDePedido
    {
        internal static void DatosDeLineaDeUnaPedido(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Ignore(x => x.ImporteSinDto);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Ignore(x => x.ImporteDeDto);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Ignore(x => x.ImporteConDto);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Ignore(x => x.ImporteDeLinea);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPedidoDtm>(modelBuilder, nameof(LineaDeUnPedidoDtm.Elemento), nameof(LineaDeUnPedidoDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);

            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(nameof(LineaDeUnPedidoDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(nameof(LineaDeUnPedidoDtm.TipoDeLinea)).HasColumnName(ICampos.TIPO_LINEA).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPedidoDtm>(modelBuilder, nameof(LineaDeUnPedidoDtm.Unitario), nameof(LineaDeUnPedidoDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: false, unico: false);

            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(nameof(LineaDeUnPedidoDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(nameof(LineaDeUnPedidoDtm.Anotacion)).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(nameof(LineaDeUnPedidoDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(nameof(LineaDeUnPedidoDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(nameof(LineaDeUnPedidoDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            modelBuilder.Entity<LineaDeUnPedidoDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPedidoDtm>(modelBuilder, nameof(LineaDeUnPedidoDtm.Unidad), nameof(LineaDeUnPedidoDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPedidoDtm>(modelBuilder, nameof(LineaDeUnPedidoDtm.Naturaleza), nameof(LineaDeUnPedidoDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineaDeUnPedidoDtm>(modelBuilder);
        }

    }
}
