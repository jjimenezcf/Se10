using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.LOGISTICA)]
    public class LineaDeUnPedidoDtm : RegistroDtm, IEsUnaLineaConCantidad, IAuditoria
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

    // Entregas de una línea de un pedido: cada entrega va a un movimiento del almacén (IdAlmacen) o a un movimiento
    // de la obra asociada al expediente (IdExpediente), nunca a ambos (ver CK_PEDIDO_LINEA_RECEPCION_DESTINO)
    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.LINEA) + "_" + nameof(Sufijo.RECEPCION), Schema = Esquemas.LOGISTICA)]
    public class RecepcionDeLineaDtm : RegistroDtm, IAuditoria
    {
        public int IdLinea { get; set; }
        public LineaDeUnPedidoDtm Linea { get; set; }

        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal? Descuento { get; set; }
        public decimal Importe { get; set; }

        public int? IdExpediente { get; set; }
        public ExpedienteDtm Expediente { get; set; }

        public int? IdAlmacen { get; set; }
        public AlmacenDtm Almacen { get; set; }

        public bool VaAlAlmacen => IdAlmacen != null;
        public bool VaALaObra => IdExpediente != null;

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
        internal static void RecepcionesDeLineaDeUnPedido(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecepcionDeLineaDtm>().Ignore(x => x.VaAlAlmacen);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Ignore(x => x.VaALaObra);

            ApiDeRegistroDtm.DefinirCampoFk<RecepcionDeLineaDtm>(modelBuilder, nameof(RecepcionDeLineaDtm.Linea), nameof(RecepcionDeLineaDtm.IdLinea), ICampos.ID_LINEA, requerida: true, unico: false);

            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Importe)).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<RecepcionDeLineaDtm>(modelBuilder, nameof(RecepcionDeLineaDtm.Expediente), nameof(RecepcionDeLineaDtm.IdExpediente), ICampos.ID_EXPEDIENTE, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<RecepcionDeLineaDtm>(modelBuilder, nameof(RecepcionDeLineaDtm.Almacen), nameof(RecepcionDeLineaDtm.IdAlmacen), ICampos.ID_ALMACEN, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<RecepcionDeLineaDtm>(modelBuilder);

            // la entrega va al almacén o a la obra del expediente, nunca a ambos ni a ninguno
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(RecepcionDeLineaDtm));
            modelBuilder.Entity<RecepcionDeLineaDtm>().ToTable(t => t.HasCheckConstraint($"CK_{tabla}_DESTINO",
                $"([{ICampos.ID_ALMACEN}] IS NOT NULL AND [{ICampos.ID_EXPEDIENTE}] IS NULL) OR ([{ICampos.ID_ALMACEN}] IS NULL AND [{ICampos.ID_EXPEDIENTE}] IS NOT NULL)"));
        }

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

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPedidoDtm>(modelBuilder, nameof(LineaDeUnPedidoDtm.Unidad), nameof(LineaDeUnPedidoDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPedidoDtm>(modelBuilder, nameof(LineaDeUnPedidoDtm.Naturaleza), nameof(LineaDeUnPedidoDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineaDeUnPedidoDtm>(modelBuilder);
        }

    }
}
